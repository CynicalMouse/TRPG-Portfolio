using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance { get; private set; }

    [SerializeField] private bool aiDisabled;

    private enum State
    {
        WaitingForTurn,
        TakingTurn,
        Busy,
    }

    private State state;

    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple EnemyAI instances found!");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        state = State.WaitingForTurn; // Set initial state // could cause issue if player isnt supposed to go first
    }

    private void Start()
    {
        // Event subscription
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChange;
    }

    // Update is called once per frame
    void Update()
    {
        // Player turn, take no action
        if (TurnSystem.Instance.IsPlayerTurn()) return;

        // Enemy AI diabled, take no action and wait a couple secs
        if (aiDisabled)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                TurnSystem.Instance.NextTurn();
            }
            return;
        }

        switch (state)
        {
            case State.WaitingForTurn:
                // Do nothing
                break;

                // ADD BIGGER TIMER TO SKIP TURN IF AI BREAKS?
            case State.TakingTurn:
                // Little pause so AI doesnt do everything at once
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    state = State.Busy;

                    if (TryTakeEnemyAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    }
                    else
                    {
                        // No more available actions
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;

            case State.Busy:
                // Performing action, do nothing
                break;
        }

        
    }

    private void TurnSystem_OnTurnChange(object sender, EventArgs e)
    {
        // Not player turn, take turn
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.TakingTurn;
            timer = 1f;
        }
    }

    private void SetStateTakingTurn() // Just changes state
    {
        timer = 0.5f;
        state = State.TakingTurn;
    }

    private bool TryTakeEnemyAction(Action onEnemyActionComplete)
    {
        foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnits())
        {
            if (TryTakeEnemyAction(enemyUnit, onEnemyActionComplete))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryTakeEnemyAction(Unit enemyUnit, Action onEnemyActionComplete)
    {
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;

        foreach (BaseAction baseAction in enemyUnit.GetActionsArray())
        {
            // Cannot afford action
            if (!enemyUnit.CanSpendActionPoints(baseAction))
            {
                continue;
            }


            // First valid action, set as current best
            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;
            }
            else
            {
                EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();

                // Compare actions
                if (testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                {
                    // Higher score, new best
                    bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                    bestBaseAction = baseAction;
                }

            }
        }

        // Have an action and points, perform action
        if (bestEnemyAIAction != null && enemyUnit.TrySpendActionPoints(bestBaseAction) && bestBaseAction)
        {
            bestBaseAction.TakeAction(bestEnemyAIAction.gridPosition, onEnemyActionComplete);
            return true;
        }
        // No points or action to perform, no action
        else
        {
            return false;
        }
    }
}

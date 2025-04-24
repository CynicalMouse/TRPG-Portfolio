using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private HealthSystem healthSystem; // Health system for managing health values and death

    [SerializeField] private bool IsEnemy; // Flag for if unit is an enemy

    [SerializeField] private Transform rangedAttackTargetTransform; // Transform that range attacks aim for

    private GridPosition gridPosition; // Current grid position of unit

    [SerializeField] private Transform actionContainer; // Child object that holds all actions. For better organisation of unit scripts
    private BaseAction[] actions; // Actions that unit can perform

    [SerializeField] private int actionPoints = 2; // Number of actions that can be performed in this units turn
    private int maxActionPoints = 2; // Max for resetting

    // Events
    public static event EventHandler OnAnyActionPointChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;

    private void Awake()
    {
        maxActionPoints = actionPoints; // Set max ap for later resetting

        healthSystem = GetComponent<HealthSystem>(); // Get reference to health system

        GetActions(); // Gather unit's actions
    }

    private void Start()
    {
        // Event subscriptions
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChange;
        healthSystem.OnDeath += HealthSystem_OnDeath;
    }

    // Start of combat, setting up unit
    public void SetUpUnit()
    {
        // Get unit's grid position based on its world position
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

        // Add unit to grid 
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);

        // Fire event for unit's creation
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    // Get actions from components
    private void GetActions()
    {
        if (actionContainer == null)
        {
            Debug.LogError("Unit has no action container " + this);
        }

        actions = actionContainer.GetComponents<BaseAction>();

        if (actions.Length == 0)
        {
            Debug.LogError("Unit has no actions" + this);
        }
    }

    private void Update()
    {
        // Not in combat
        if (!PlayerStateController.Instance.IsCombat()) return;

        // Level grid has not been created
        if (!LevelGrid.Instance.GetGridCreated()) return;

        // Update unit's gridposition every frame
        UpdateGridPosition();
    }

    // Updates unit's stored grid position
    private void UpdateGridPosition()
    {
        // Get new position based on current world position
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

        // If new position
        if (newGridPosition != gridPosition)
        {
            // Unit changed position
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;

            // Update in grid
            LevelGrid.Instance.UnitMoveGridPosition(this, oldGridPosition, newGridPosition);
            
        }
    }

    // Tries to spend action points if any are available and if can afford action
    public bool TrySpendActionPoints(BaseAction baseAction)
    {
        if (CanSpendActionPoints(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointCost());
            return true;
        }
        else
        {
            return false;
        }
    }

    // Returns true if unit has enough ap to perform action
    public bool CanSpendActionPoints(BaseAction baseAction)
    {
        if (actionPoints >= baseAction.GetActionPointCost())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Decrement action points based on amount spent
    private void SpendActionPoints(int amountToSpend)
    {
        actionPoints -= amountToSpend;
        OnAnyActionPointChanged?.Invoke(this, EventArgs.Empty); // Event to update UI
    }

    // Resets unit's action points on it's team's turn
    private void TurnSystem_OnTurnChange(object sender, EventArgs e)
    {
           // If enemy and enemy turn, refresh enemy points
        if ((GetIsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) ||
           // If player and player turn, refresh player points
            (!GetIsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            actionPoints = maxActionPoints;
            OnAnyActionPointChanged?.Invoke(this, EventArgs.Empty); // Event to update UI
        }
        
    }

    // Triggered by health system. Removes unit from grid
    private void HealthSystem_OnDeath(object sender, EventArgs e)
    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        Destroy(gameObject);

        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
    }

    // Returns the requested action
    // Like GetComponent<ComponentType>()
    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction action in actions)
        {
            if (action is T)
            {
                return (T)action;
            }
        }
        return null;
    }

    // Send damage amount to health system
    // Pass through is cleaner than unit.healthsystem.takedamage(). This is just unit.takedamage()
    public void TakeDamage(int damage)
    {
        healthSystem.TakeDamage(damage);
    }

    // Bunch of getters
    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public Vector3 GetRangedAttackTargetPosition()
    {
        return rangedAttackTargetTransform.position;
    }

    public BaseAction[] GetActionsArray()
    {
        return actions;
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    public bool GetIsEnemy()
    {
        return IsEnemy;
    }

    public float GetHealthNormalized()
    {
        return healthSystem.GetHealthNormalized();
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}

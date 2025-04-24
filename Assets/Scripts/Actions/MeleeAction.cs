using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAction : BaseAction
{
    private int maxMeleeDistance = 1;
    [SerializeField] private int damage = 1;
    private enum State
    {
        BeforeHit,
        AfterHit,
    }

    private State state;
    private float stateTimer;

    private Unit targetUnit;

    public event EventHandler OnMeleeAttackStarted;
    public event EventHandler OnMeleeAttackEnded;

    private void Update()
    {
        if (!IsActive) return;

        // Cycle through states
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            // Aim at target
            case State.BeforeHit:
                // Rotation - Remove if using panel board sprites
                Vector3 moveDirection = (targetUnit.GetWorldPosition() - unitTransform.position).normalized;

                // Look at target pos
                unitTransform.forward = Vector3.Lerp(unitTransform.forward, moveDirection, Time.deltaTime * 25);
                break;

            // Shoot target
            case State.AfterHit:
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }

    }

    private void NextState()
    {
        switch (state)
        {
            case State.BeforeHit:
                state = State.AfterHit;
                float afterHitStateTime = 0.1f;
                stateTimer = afterHitStateTime;
                targetUnit.TakeDamage(damage);
                break;
            case State.AfterHit:
                OnMeleeAttackEnded?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;
        }
    }

    public override string GetActionName()
    {
        return "Melee";
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 200 // Highly likely to do
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validActionGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        // Check all grid positions around unit
        // Start at -maxMoveDistance to get the grid position to left/behind unit
        // 1 as its melee
        for (int x = -maxMeleeDistance; x <= maxMeleeDistance; x++)
        {
            for (int z = -maxMeleeDistance; z <= maxMeleeDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                // Skip if invalid grid position  (out of bounds)
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                //  or grid position has not got a unit on it
                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(testGridPosition);

                // Units on same team, skip
                if (targetUnit.GetIsEnemy() == unit.GetIsEnemy())
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if (testDistance > maxActionRange)
                {
                    continue;
                }

                // Add to valid action grid position list
                validActionGridPositionList.Add(testGridPosition);
            }
        }

        return validActionGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);
        state = State.BeforeHit;
        float beforeHitStateTime = 0.6f;
        stateTimer = beforeHitStateTime;

        OnMeleeAttackStarted?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RangedAttackAction : BaseAction
{
    [SerializeField] private int baseDamage = 1; // Damage attack does

    // State for going between shoot animation - Probably remove or change
    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }

    private State state;
    private float stateTimer;
    private bool canShoot;


    private Unit targetUnit; // Unit getting shot
    
    [SerializeField] private LayerMask obstacleLayer; // Objects that block ranged ability


    // Event and arguments
    public static EventHandler<OnRangedAttackEventArgs> OnAnyRangedAttack;
    public event EventHandler<OnRangedAttackEventArgs> OnRangedAttack;

    public class OnRangedAttackEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit actingUnit;
    }

    protected void Update()
    {
        // Action isnt active, don't act
        if (!IsActive) return;

        // Cycle through states
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            // Aim at target
            case State.Aiming:
                // Rotation - Remove if using panel board sprites
                Vector3 moveDirection = (targetUnit.GetWorldPosition() - unitTransform.position).normalized;

                // Look at target pos
                unitTransform.forward = Vector3.Lerp(unitTransform.forward, moveDirection, Time.deltaTime * 10);

                break;

            // Shoot target
            case State.Shooting:
                if (canShoot)
                {
                    canShoot = false;
                    Shoot();
                }
                break;

            // Briefly do nothing
            case State.Cooloff:
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
            case State.Aiming:
                state = State.Shooting;
                float shootingStateTime = 0.1f;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                float cooloffStateTime = 0.5f;
                stateTimer = cooloffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    // Perform the action
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        // Set target to shoot
        targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);

        // Set state for animation
        state = State.Aiming;
        float aimingStateTime = 1.0f;
        stateTimer = aimingStateTime;
        canShoot = true; // Allow shooting

        // Do the action
        ActionStart(onActionComplete);
    }

    private void Shoot()
    {
        // Fire off event
        OnRangedAttack?.Invoke(this, new OnRangedAttackEventArgs
        {
            targetUnit = targetUnit,
            actingUnit = unit
        });

        OnAnyRangedAttack?.Invoke(this, new OnRangedAttackEventArgs
        {
            targetUnit = targetUnit,
            actingUnit = unit
        });

        // Apply damage to target
        targetUnit.TakeDamage(baseDamage);
    }

    public override string GetActionName()
    {
        return "Ranged Attack";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> validActionGridPositionList = new List<GridPosition>();

        // Check all grid positions around unit
        // Start at -maxMoveDistance to get the grid position to left/behind unit
        for (int x = -maxActionRange; x <= maxActionRange; x++)
        {
            for (int z = -maxActionRange; z <= maxActionRange; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = gridPosition + offsetGridPosition;

                // Skip if invalid grid position  (out of bounds)
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if (testDistance > maxActionRange)
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

                // Raycast so dont shoot through walls
                float unitShoulderHeight = 1.8f;

                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);

                Vector3 shootDirection = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;

                if(Physics.Raycast(unitWorldPosition + Vector3.up * unitShoulderHeight, shootDirection, Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()), obstacleLayer))
                {
                    // Blocked by obstacle
                    continue;
                }

                // Add to valid action grid position list
                validActionGridPositionList.Add(testGridPosition);
            }
        }

        return validActionGridPositionList;
    }

    

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        // Get target's health
        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);

        targetUnit.GetHealthNormalized(); // health converted to float for accurate weighting

        return new EnemyAIAction
        {
            gridPosition = gridPosition,

            // Extremely high value, will always do if possible
            // Prioritize low health units
            actionValue = 100000 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100f),
        };
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }
}

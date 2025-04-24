using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static MoveAction;

public class MoveAction : BaseAction
{
    // Events
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotateSpeed = 15f;

    private List<Vector3> targetPositionList; // List of positions to move between, gotten from pathfinding
    private int currentPositionIndex; 

    protected void Update()
    {
        if (!IsActive) return;
        Move();
    }

    // Set target position and move unit
    public override void TakeAction(GridPosition targetPosition, Action moveCompleteDelegate)
    {
        // Get path to take and store as list of grid positions
        List<GridPosition> pathGridPositions = Pathfinding.Instance.FindPath(unit.GetGridPosition(), targetPosition, out int pathLength);


        // Reset index and create new list
        currentPositionIndex = 0;
        targetPositionList = new List<Vector3>();

        // Populate list with every position in path
        foreach (GridPosition gridPosition in pathGridPositions)
        {
            targetPositionList.Add(LevelGrid.Instance.GetWorldPosition(gridPosition));
        }

        // Fire event
        OnStartMoving?.Invoke(this, EventArgs.Empty);

        // Start action
        ActionStart(moveCompleteDelegate);
    }

    // Move unit towards target position
    private void Move()
    {
        // Store target position
        Vector3 targetPosition = targetPositionList[currentPositionIndex];

        // Rotation - Remove if using panel board sprites
        Vector3 moveDirection = (targetPosition - unitTransform.position).normalized;
        // Look at target pos
        unitTransform.forward = Vector3.Lerp(unitTransform.forward, moveDirection, Time.deltaTime * rotateSpeed);

        // Move towards target pos
        unitTransform.position = Vector3.MoveTowards(unitTransform.position, targetPosition, moveSpeed * Time.deltaTime);
    
        // If we reached the target, go to next point
        if (unitTransform.position == targetPosition)
        {
            currentPositionIndex++;

            // Final position, stop moving
            if (currentPositionIndex >= targetPositionList.Count)
            {
                ActionComplete();

                OnStopMoving?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    // Get all valid grid positions for moving
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validActionGridPositionList = new List<GridPosition>();

        GridPosition currentGridPosition = unit.GetGridPosition();

        // Check all grid positions around unit
        // Start at -maxMoveDistance to get the grid position to left/behind unit
        for (int x = -maxActionRange; x <= maxActionRange; x++)
        {
            for (int z = -maxActionRange; z <= maxActionRange; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);

                GridPosition testGridPosition = currentGridPosition + offsetGridPosition;

                // Skip if invalid grid position  (out of bounds)
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                //  or same grid position that unit is on
                if (currentGridPosition == testGridPosition)
                {
                    continue;
                }

                //  or grid position has unit on it
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }

                // Cannot walk on grid position that has obstacle
                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    continue;
                }

                // Grid Position has no path
                if (!Pathfinding.Instance.HasPath(currentGridPosition, testGridPosition))
                {
                    continue;
                }

                // Too far away, stops tiles through walls being valid 
                if ((Pathfinding.Instance.GetPathLength(currentGridPosition, testGridPosition) / 10) > maxActionRange)
                {
                    continue;
                }

                // Quick math to make it a circle instead of square
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

    public override string GetActionName()
    {
        return "Move";
    }

    // Determine if unit should move
    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        // Count of enemies that can be shot from current position
        int validTargetCount = unit.GetAction<RangedAttackAction>().GetTargetCountAtPosition(gridPosition);

        return new EnemyAIAction
        {
            gridPosition = gridPosition,

            // Prioritize area with lots of enemies rather than none
            actionValue = validTargetCount * 10,
        };
    }
}

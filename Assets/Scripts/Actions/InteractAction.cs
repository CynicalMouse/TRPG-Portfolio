using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractAction : BaseAction
{
    private void Update()
    {
        if (!IsActive) return;
    }

    public override string GetActionName()
    {
        return "Interact";
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validActionGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        // Check all grid positions around unit
        // Start at -maxMoveDistance to get the grid position to left/behind unit
        // 1 as its melee
        for (int x = -maxActionRange; x <= maxActionRange; x++)
        {
            for (int z = -maxActionRange; z <= maxActionRange; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

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

                // Change to interactable object (base class)
                InteractableObject interactableObject = LevelGrid.Instance.GetInteractableObjectAtGridPosition(testGridPosition);

                if (interactableObject == null)
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
        InteractableObject interactableObject = LevelGrid.Instance.GetInteractableObjectAtGridPosition(gridPosition);
        interactableObject.Interact(OnInteractComplete);

        ActionStart(onActionComplete);
    }

    private void OnInteractComplete()
    {
        ActionComplete();
    }
}

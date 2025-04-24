using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoE_Action : BaseAction
{
    [SerializeField] private Transform explosiveProjectilePrefab;
    [SerializeField] private int damageRadius = 1; // In grid positions
    [SerializeField] private int damage = 2;

    private void Update()
    {
        if (!IsActive) return;
    }

    public override string GetActionName()
    {
        return "Explosion";
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

                // Cannot through explosive on grid position with obstacle
                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
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
        Transform explosiveProjectileTransform = Instantiate(explosiveProjectilePrefab, unit.GetWorldPosition(), Quaternion.identity);
        ExplosiveProjectile projectile = explosiveProjectileTransform.GetComponent<ExplosiveProjectile>();

        //                             *2 for cell size        
        projectile.Setup(gridPosition, damageRadius * 2f, damage, onExplosiveBehaviourComplete);

        ActionStart(onActionComplete);
    }

    private void onExplosiveBehaviourComplete()
    {
        ActionComplete();
    }
}

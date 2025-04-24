using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAction : BaseAction
{
    float spinTimer = 0f;

    protected void Update()
    {
        if (!IsActive) return;

        float spinSpeed = 360f * Time.deltaTime;

        spinTimer += spinSpeed;
        unitTransform.eulerAngles += new Vector3(0, spinSpeed, 0);

        if(spinTimer >= 360f)
        {
            spinTimer = 0f;
            ActionComplete();
        }  
    }

    public override void TakeAction(GridPosition gridPosition, Action onSpinComplete)
    {
        ActionStart(onSpinComplete);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validActionGridPositionList = new List<GridPosition>();
        GridPosition currentGridPosition = unit.GetGridPosition();

        validActionGridPositionList.Add(currentGridPosition);

        return validActionGridPositionList;
    }

    public override string GetActionName()
    {
        return "Spin";
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,

            // Low value, will only do if literally nothing else can be done
            actionValue = 0,
        };
    }
}

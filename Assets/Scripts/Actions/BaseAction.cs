using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    // Events
    public static event EventHandler OnAnyActionStart;
    public static event EventHandler<OnAnyActionEndEventArgs> OnAnyActionEnd;

    public class OnAnyActionEndEventArgs : EventArgs
    {
        public BaseAction baseAction;
    }

    protected Transform unitTransform; // Transform of unit that is acting
    protected Unit unit; // Unit that is acting

    protected bool IsActive = false; // True for duration of action

    protected Action onActionComplete; // Delegate

    public int maxActionRange; // How far the action has an effect

    public int actionPointCost = 1; // How many points the action costs

    public GridSystemVisual.GridColor actionColor; // Color of action on selectable tiles
    public GridSystemVisual.GridColor actionRangeColor; // Color of action on tiles in range, but not selectable
    public bool IsActionRangeVisible; // Bool for if range should be enabled
    public bool IsCircle; // Bool to change the range from a square around unit to a circle





    protected virtual void Awake()
    {
        unit = GetComponentInParent<Unit>();
        unitTransform = unit.gameObject.transform;
    }

    protected void ActionStart(Action onActionComplete)
    {
        IsActive = true;
        this.onActionComplete = onActionComplete;

        OnAnyActionStart?.Invoke(this, EventArgs.Empty);
    }

    // Generic take action method

    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

    protected void ActionComplete()
    {
        IsActive = false;
        onActionComplete();

        OnAnyActionEnd?.Invoke(this, new OnAnyActionEndEventArgs
        {
            baseAction = this
        });
    }

    // Check if grid position is valid for action
    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();
        return validActionGridPositionList.Contains(gridPosition);
    }

    // Get all valid grid positions for actiob
    public abstract List<GridPosition> GetValidActionGridPositionList();

    public virtual int GetActionPointCost()
    {
        return actionPointCost;
    }

    // Get action name
    public abstract string GetActionName();

    public Unit GetUnit()
    {
        return unit;
    }


    // Chooses the best action for AI to take based on action weights
    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();

        foreach (GridPosition gridPosition in validActionGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetBestEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);
        }

        if (enemyAIActionList.Count > 0)
        {
            // Sort by action value, 0 is more valuable
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        }
        else
        {
            // No possible actions
            return null;
        }

        
    }

    public abstract EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition);
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }

    // Events
    public event EventHandler OnSelectedUnitChange;
    public event EventHandler<OnSelectedActionChangeEentArgs> OnSelectedActionChange;
    public class OnSelectedActionChangeEentArgs : EventArgs
    {
        public BaseAction baseAction;
    }

    public event EventHandler<bool> OnBusyChange;
    public event EventHandler OnActionPointsChange;

    [SerializeField] private Unit selectedUnit; // Unit currently selected by player
    private BaseAction selectedAction; // Action currently selected to be used
    [SerializeField] private LayerMask unitLayer; // Unit layermask 

    private bool isBusy; // Flag for if the unit is performing an action

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("UnitActionSystem already exists! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Event sub
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;

        //SetUpUnitActionSystem();
    }

    private void Update()
    {
        // Not in combat
        if (!PlayerStateController.Instance.IsCombat()) return;

        // Action currently running
        if (isBusy) return;

        // Enemy turn, stop player actions
        if (TurnSystem.Instance.IsPlayerTurn() == false) return;

        // If mouse is over UI, dont take action
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (InputManager.Instance.IsLeftMouseButtonDownThisFrame())
        {
            if (TryHandleUnitSelection()) return; // Change unit, don't perform action

            HandleSelectedAction();
        }
    }

    // Set up call for starting a combat instance
    public void SetUpUnitActionSystem(Unit startingUnit)
    {
        SetSelectedUnit(startingUnit);
    }

    // Try to select new unit
    private bool TryHandleUnitSelection()
    {
        // Ray from camera to mouse position on world
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());

        // Raycast to unit layer
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, unitLayer))
        {
            if (hit.transform.TryGetComponent<Unit>(out Unit unit))
            {
                // Unit already selected
                if (unit == selectedUnit) return false;

                // Unit is an enemy
                if (unit.GetIsEnemy() == true) return false;

                // Set new unit
                SetSelectedUnit(unit);
                return true;
            }
        }

        return false;
    }

    // Perform selected action
    private void HandleSelectedAction()
    {
        // Get grid position of mouse
        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetMousePosition());

        // Check if mouse is in valid grid position
        if (selectedAction.IsValidActionGridPosition(mouseGridPosition))
        {
            // Try to spend action points, if so spend them and take action
            if (selectedUnit.TrySpendActionPoints(selectedAction))
            {
                // Set busy to stop multiple actions at once
                SetBusy();

                // Perform the action
                selectedAction.TakeAction(mouseGridPosition, ClearBusy);
  
                OnActionPointsChange?.Invoke(this, EventArgs.Empty); // Update action point amount UI
            }  
        }
    }

    // Stop other actions from happening
    private void SetBusy()
    {
        isBusy = true;
        OnBusyChange?.Invoke(this, isBusy);
    }

    // Clear busy flag, allow other actions
    private void ClearBusy()
    {
        isBusy = false;
        OnBusyChange?.Invoke(this, isBusy);
    }

    // Set selected unit and invoke event to change visuals
    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;

        // Should only occur when exiting combat
        // If happens, check anywhere selected unit is being set
        if (selectedUnit == null)
        {
            Debug.LogWarning("UnitActionSystem: Selected unit is null! If combat is over, this is fine.");
            return;
        }

        SetSelectedAction(selectedUnit.GetAction<MoveAction>());

        OnSelectedUnitChange?.Invoke(this, EventArgs.Empty);
    }

    // Get selected unit
    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    // Set selected action
    public void SetSelectedAction(BaseAction action)
    {
        selectedAction = action;

        // invoke event, send action as event arg
        OnSelectedActionChange?.Invoke(this, new OnSelectedActionChangeEentArgs
        {
            baseAction = selectedAction
        });
    }

    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }

    // Triggered by unit, auto selects another unit
    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        if (selectedUnit == (Unit)sender)
        {
            SetSelectedUnit(UnitManager.Instance.GetPlayerUnits()[0]);
        }
    }
}

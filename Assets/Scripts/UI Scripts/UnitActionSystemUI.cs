using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitActionSystemUI : MonoBehaviour
{
    [SerializeField] private Transform actionButtonPrefab;
    [SerializeField] private Transform actionsContainer;
    [SerializeField] private TextMeshProUGUI actionPointsText;

    private List<ActionButton> actionButtons;

    private void Awake()
    {
        actionButtons = new List<ActionButton>();
    }

    private void Start()
    {
        // Event subscriptions
        UnitActionSystem.Instance.OnSelectedUnitChange += UnitActionSystem_OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChange += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnActionPointsChange += UnitActionSystem_OnActionPointsChange;
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChange;
        Unit.OnAnyActionPointChanged += Unit_OnAnyActionPointChanged;

        // Create initial buttons
        CreateUnitActionButtons();

        // Highlight selected action
        UpdateSelectedActionVisual();

        // Update action point cost to assigned amount
        UpdateActionPoints();
    }

    private void CreateUnitActionButtons()
    {
        // Not in combat, don't need created
        if (!PlayerStateController.Instance.IsCombat()) return;

        // Clear previous buttons
        ClearUnitActionButtons();

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();

        // Loop through each action unit has
        foreach (BaseAction action in selectedUnit.GetActionsArray())
        {
            // Create a new button
            Transform button = Instantiate(actionButtonPrefab, actionsContainer);
            ActionButton actionButton = button.GetComponent<ActionButton>();

            // Assign action to button
            actionButton.SetBaseAction(action);

            // Add to button list
            actionButtons.Add(actionButton);
        }
    }

    // Highlights selected action's button, unhighlights rest
    private void UpdateSelectedActionVisual()
    {
        foreach (ActionButton actionButton in actionButtons)
        {
            actionButton.UpdateSelectedVisual(false);

            if (UnitActionSystem.Instance.GetSelectedAction() == actionButton.GetBaseAction())
            {
                actionButton.UpdateSelectedVisual(true);
            }
        }
    }

    // Remove all action buttons
    private void ClearUnitActionButtons()
    {
        foreach (Transform child in actionsContainer)
        {
            Destroy(child.gameObject);
        }

        actionButtons.Clear();
    }

    // Update buttons when selected unit changes
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e)
    {
        CreateUnitActionButtons();
        UpdateSelectedActionVisual();
        UpdateActionPoints();
    }

    // Change action being highlighted
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateSelectedActionVisual();
    }

    // Update displayed action point count
    private void UpdateActionPoints()
    {
        // Not in combat, don't need updated
        if (!PlayerStateController.Instance.IsCombat()) return;

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        actionPointsText.text = "Actions Available: " + selectedUnit.GetActionPoints();
    }

    // Events to update displayed action point count
    private void UnitActionSystem_OnActionPointsChange(object sender, EventArgs e)
    {
        UpdateActionPoints();
    }

    private void TurnSystem_OnTurnChange(object sender, EventArgs e)
    {
        UpdateActionPoints();
    }

    private void Unit_OnAnyActionPointChanged(object sender, EventArgs e)
    {
        UpdateActionPoints();
    }
}

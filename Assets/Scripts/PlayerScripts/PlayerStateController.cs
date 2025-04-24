using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerStateController : MonoBehaviour
{
    public static PlayerStateController Instance { get; private set; }

    [SerializeField] private bool InCombat;

    [SerializeField] private Unit tempManualUnit;

    public event EventHandler OnCombatStarted; // also used by destructible objects
    public event EventHandler OnCombatEnded;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("PlayerStateController already exists! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Default combat start, only use for testing
    // Set width, height, cellSize and gridOrigin in LevelGrid inspector
    public void StartCombat()
    {
        InCombat = true;

        // Step 1 - Set up the grid
        LevelGrid.Instance.CreateNewGrid();

        // Step 2 - Set up pathfinding
        // Could call directly instead
        LevelGrid.Instance.SetUpPathFinding();

        // Step 3 - Set up grid visual
        GridSystemVisual.Instance.SetUpGridVisual();

        // Step 4 - Set up Unit Action System
        UnitActionSystem.Instance.SetUpUnitActionSystem(tempManualUnit);

        // Step 5 - Set up units
        Unit[] units = FindObjectsOfType<Unit>();

        foreach (Unit unit in units)
        {
            unit.SetUpUnit();
        }

        // Step 6 - Show initial grid visual
        GridSystemVisual.Instance.UpdateGridVisual(UnitActionSystem.Instance.GetSelectedAction());

        // Last things in no particular order
        TurnSystem.Instance.ResetTurnCount();
        CameraController.Instance.EnterCombat();

        // Fire event for camera changing
        OnCombatStarted?.Invoke(this, EventArgs.Empty);
    }

    // Starts a new combat instance, parameters set by combat trigger (hitbox)
    public void StartCombat(int width, int height, float cellSize, Vector3 gridOrigin, Unit[] units)
    {
        InCombat = true;

        // Step 1 - Set up the grid
        LevelGrid.Instance.CreateNewGrid(width, height, cellSize, gridOrigin);

        // Step 2 - Set up pathfinding
        // Could call directly instead
        LevelGrid.Instance.SetUpPathFinding();

        // Step 3 - Set up grid visual
        GridSystemVisual.Instance.SetUpGridVisual();

        // Step 4 - Set up Unit Action System
        UnitActionSystem.Instance.SetUpUnitActionSystem(tempManualUnit);

        // Step 5 - Set up units
        if (units == null) { units = FindObjectsOfType<Unit>(); }

        foreach (Unit unit in units)
        {
            unit.SetUpUnit();
        }

        // Step 6 - Show initial grid visual
        GridSystemVisual.Instance.UpdateGridVisual(UnitActionSystem.Instance.GetSelectedAction());

        // Last things in no particular order
        TurnSystem.Instance.ResetTurnCount();
        CameraController.Instance.EnterCombat();

        // Fire event for camera changing
        OnCombatStarted?.Invoke(this, EventArgs.Empty);
    }

    // Starts a new combat instance, parameters set by combat trigger (hitbox)
    public void StartCombat(int width, int height, float cellSize, Vector3 gridOrigin, Transform camerStartTransform)
    {
        // Already in combat, don't start another
        if (InCombat) { Debug.LogError("PlayerStateController: Tried to start another combat instance at - " + gridOrigin); return; }

        InCombat = true;

        // Step 1 - Set up the grid
        LevelGrid.Instance.CreateNewGrid(width, height, cellSize, gridOrigin);

        // Step 2 - Set up pathfinding
        // Could call directly instead
        LevelGrid.Instance.SetUpPathFinding();

        // Step 3 - Set up grid visual
        GridSystemVisual.Instance.SetUpGridVisual();

        // Step 4 - Set up Unit Action System
        UnitActionSystem.Instance.SetUpUnitActionSystem(tempManualUnit);

        // Step 5 - Set up units
        Unit[] units = FindObjectsOfType<Unit>();

        foreach (Unit unit in units)
        {
            unit.SetUpUnit();
        }

        // Step 6 - Show initial grid visual
        GridSystemVisual.Instance.UpdateGridVisual(UnitActionSystem.Instance.GetSelectedAction());

        // Last things in no particular order
        TurnSystem.Instance.ResetTurnCount();
        CameraController.Instance.EnterCombat(camerStartTransform);

        // Fire event for camera changing
        OnCombatStarted?.Invoke(this, EventArgs.Empty);
    }

    // Starts a new combat instance, parameters set by combat trigger (hitbox)
    public void StartCombat(int width, int height, float cellSize, Vector3 gridOrigin, Transform camerStartTransform, Unit[] units)
    {
        // Already in combat, don't start another
        if (InCombat) { Debug.LogError("PlayerStateController: Tried to start another combat instance at - " + gridOrigin); return; }

        InCombat = true;

        // Step 1 - Set up the grid
        LevelGrid.Instance.CreateNewGrid(width, height, cellSize, gridOrigin);

        // Step 2 - Set up pathfinding
        // Could call directly instead
        LevelGrid.Instance.SetUpPathFinding();

        // Step 3 - Set up grid visual
        GridSystemVisual.Instance.SetUpGridVisual();

        // Step 4 - Set up Unit Action System
        UnitActionSystem.Instance.SetUpUnitActionSystem(tempManualUnit);

        // Step 5 - Set up units
        foreach (Unit unit in units)
        {
            unit.gameObject.SetActive(true);
            unit.SetUpUnit();
        }

        // Step 6 - Show initial grid visual
        GridSystemVisual.Instance.UpdateGridVisual(UnitActionSystem.Instance.GetSelectedAction());

        // Last things in no particular order
        TurnSystem.Instance.ResetTurnCount();
        CameraController.Instance.EnterCombat(camerStartTransform);

        // Fire event for camera changing
        OnCombatStarted?.Invoke(this, EventArgs.Empty);
    }

    public void StartCombat(int width, int height, float cellSize, Vector3 gridOrigin, Transform camerStartTransform, Unit[] units, InteractableObject[] interactableObjects)
    {
        // Already in combat, don't start another
        if (InCombat) { Debug.LogError("PlayerStateController: Tried to start another combat instance at - " + gridOrigin); return; }

        InCombat = true;

        // Step 1 - Set up the grid
        LevelGrid.Instance.CreateNewGrid(width, height, cellSize, gridOrigin);

        // Step 2 - Set up pathfinding
        // Could call directly instead
        LevelGrid.Instance.SetUpPathFinding();

        // Step 3 - Set up grid visual
        GridSystemVisual.Instance.SetUpGridVisual();

        // Step 4 - Set up Unit Action System
        UnitActionSystem.Instance.SetUpUnitActionSystem(tempManualUnit);

        // Step 5 - Set up units
        if (units.Length == 0 || units == null)
        {
            Debug.LogError("PlayerStateController: No units assigned to combat trigger");
        }

        foreach (Unit unit in units)
        {
            unit.gameObject.SetActive(true);
            unit.SetUpUnit();
        }

        // Step 5.5 - Set up interactable objects
        if (interactableObjects.Length == 0 || interactableObjects == null)
        {
            Debug.LogError("PlayerStateController: No interactable objects assigned to combat trigger");
        }

        foreach (Door door in interactableObjects)
        {
            door.SetUp();
        }

        // Step 6 - Show initial grid visual
        GridSystemVisual.Instance.UpdateGridVisual(UnitActionSystem.Instance.GetSelectedAction());

        // Last things in no particular order
        TurnSystem.Instance.ResetTurnCount();
        CameraController.Instance.EnterCombat(camerStartTransform);

        // Fire event for camera changing
        OnCombatStarted?.Invoke(this, EventArgs.Empty);
    }

    // Exits combat and clears all combat functions for later use
    // Currently called by Unit manager when enemy units = 0
    public void ExitCombat()
    {
        // Already out of combat, don't do below
        if (!InCombat) { Debug.LogError("PlayerStateController: Tried to stop combat instance while not in combat"); return; }

        InCombat = false;

        // Clear previous combat
        // Go in reverse

        // Step 1 Fire an event for || Camera?
        OnCombatEnded?.Invoke(this, EventArgs.Empty);

        // Step 2 - Clean up Units
        UnitManager.Instance.ClearLists();

        // Step 3 - Disable Unit Action System
        UnitActionSystem.Instance.SetUpUnitActionSystem(null);

        // Step 4 - Clear visual grid
        GridSystemVisual.Instance.ClearGridVisuals();

        // Step 5 - Clear Pathfinding
        Pathfinding.Instance.Clear();

        // Step 6 - Clear level grid
        LevelGrid.Instance.ClearGrid();

    }

    public bool IsCombat()
    {
        return InCombat;
    }
}

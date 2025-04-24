using System;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance { get; private set; }

    [SerializeField] private Transform gridVisualParent; // Object to hold all the visuals
    [SerializeField] private GameObject gridVisualPrefab; // Prefab visual to make

    private GridSystemVisualSingle[,] gridVisualSingleArray; // Array of grid visual objects // 2D for x and z

    // Enum for setting color of grid visual
    // Long, but better than remembering numbers
    public enum GridColor
    {
        White,
        Red,
        Blue,
        Green,
        Yellow,
        RedSoft,
        BlueSoft,
        GreenSoft,
        YellowSoft
    }

    [Serializable]
    public struct GridVisualColorMaterial
    {
        public GridColor gridColor;
        public Material material;
    }

    [SerializeField] private List<GridVisualColorMaterial> gridVisualTypeMaterialList; // Materials to be assigned in inspector

    // Hacky way to fix strange unit grid bug
    private float timer = 0.0005f;
    private bool hasUpdatedGrid = false;

    [SerializeField] private LayerMask obstacleLayer; // Layer that blocks visual from being generated // Same as in pathfinding

    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GridSystemVisual already exists! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        //SetUpGridVisual();

        // Event subscriptions
        UnitActionSystem.Instance.OnSelectedActionChange += UnitActionSystem_OnSelectedActionChange;
        LevelGrid.Instance.OnAnyUnitMoveGridPosition += LevelGrid_OnAnyUnitMoveGridPosition;
        BaseAction.OnAnyActionEnd += BaseAction_OnAnyActionEnd;

        
    }

    private void Update()
    {
        // Remove, horrible solution. This is getting done every frame
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (timer <= 0 && !hasUpdatedGrid)
        {
            // Not in combat
            if (!PlayerStateController.Instance.IsCombat()) return;

            // Update to show initial selection
            UpdateGridVisual(UnitActionSystem.Instance.GetSelectedAction());
            hasUpdatedGrid = true; // Ensure it only runs once
        }
    }

    // Initial Set up
    public void SetUpGridVisual()
    {
        int width = LevelGrid.Instance.GetWidth();
        int height = LevelGrid.Instance.GetHeight();

        // Create 2D array for width and height of grid
        gridVisualSingleArray = new GridSystemVisualSingle[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetHeight()];

        // Loop through size of grid
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int Z = 0; Z < LevelGrid.Instance.GetHeight(); Z++)
            {
                // Create new grid position
                GridPosition gridPosition = new GridPosition(x, Z);

                // Get world position
                Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);

                // Create the grid visual object
                GameObject gridVisual = Instantiate(gridVisualPrefab, worldPosition, Quaternion.identity);

                // Set size of visual
                gridVisual.transform.GetChild(0).localScale = new Vector3(LevelGrid.Instance.GetCellSize(), LevelGrid.Instance.GetCellSize(), LevelGrid.Instance.GetCellSize());

                // Add visual to array
                gridVisualSingleArray[x, Z] = gridVisual.GetComponent<GridSystemVisualSingle>();

                // Set as child for better organisation
                gridVisual.transform.SetParent(gridVisualParent, true);
            }
        }

        
    }

    // Loops through all visuals and hides them
    public void HideAllGridPositions()
    {
        // Out of combat, visuals are already destroyed
        if (!PlayerStateController.Instance.IsCombat()) return;

        foreach (GridSystemVisualSingle gridVisual in gridVisualSingleArray)
        {
            gridVisual.Hide();
        }
    }

    // SELECTABLE LOCATIONS
    // Shows the positions in provided list and the color they should display
    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridColor gridColor)
    {
        //HideAllGridPositions();
        foreach (GridPosition gridPosition in gridPositionList)
        {
            GridSystemVisualSingle gridSystemVisualSingle = gridVisualSingleArray[gridPosition.x, gridPosition.z];

            gridSystemVisualSingle.Show(GetGridVisualTypeMaterial(gridColor));
        }
    }

    // RANGE OF ACTION // NOT SELECTABLE
    // Shows the range that an action affects, what color to display
    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridColor gridColor, BaseAction selectedAction)
    {
        // Create a new list of positions
        List<GridPosition> gridPositions = new List<GridPosition>();

        // Loop through height and width of grid
        for (int x = - range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                // Create new position to test based on provided position
                GridPosition testGridPosition = gridPosition + new GridPosition(x,z);

                // Skip if invalid grid position  (out of bounds)
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                // Skip if obstacle on grid position
                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    continue;
                }


                // Quick math to make it a circle rather than square
                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                // If distance exceeds range skip
                // And if labeled as circle
                // Skipped if action is sqaure
                if (testDistance > range && selectedAction.IsCircle == false)
                {
                    continue;
                }

                // RANGED ACTION, only show if no view blocking obstacles are in the way
                // Line of sight
                if (selectedAction is RangedAttackAction)
                {
                    // Raycast so dont shoot through walls
                    float unitShoulderHeight = 1.8f;

                    Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                    Vector3 targetGridPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);

                    Vector3 shootDirection = (targetGridPosition - unitWorldPosition).normalized;

                    if (Physics.Raycast(unitWorldPosition + Vector3.up * unitShoulderHeight, shootDirection, Vector3.Distance(unitWorldPosition, targetGridPosition), obstacleLayer))
                    {
                        // Blocked by obstacle
                        continue;
                    }
                }

                // NOT A RANGED ACTION, show if range is enough to move to grid position
                else if ((Pathfinding.Instance.GetPathLength(gridPosition, testGridPosition) / 10) > UnitActionSystem.Instance.GetSelectedAction().maxActionRange)
                {
                    continue;
                }

                // Do not have path to position
                if (!Pathfinding.Instance.HasPath(gridPosition, testGridPosition))
                {
                    continue;
                }

                // Passed all checks, add to list
                gridPositions.Add(testGridPosition);
            }
        }

        // Show list of positions
        ShowGridPositionList(gridPositions, gridColor);
    }

    // Updates the displayed visuals
    public void UpdateGridVisual(BaseAction baseAction)
    {
        // Not in combat, don't call. Stops errors during combat cleanup
        if (!PlayerStateController.Instance.IsCombat()) return;

        // Hide all previous positions
        HideAllGridPositions();

        // Get current unit and action being selected/performed
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

        // Get color action
        GridColor gridColor = selectedAction.actionColor;

        // Only show range if enabled
        if (selectedAction.IsActionRangeVisible)
        {
            // Show range of action
            ShowGridPositionRange(selectedUnit.GetGridPosition(), selectedAction.maxActionRange ,selectedAction.actionRangeColor, baseAction);
        }

        // OR
        // ShowGridPositionRange overload for pre determined action layout
        // ShowGridPositionRange(selectedUnit.GetGridPosition(), selectedAction.AffectLayout, selectedAction.actionRangeColor);

        // Show positions that can be selected
        ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridColor);
    }

    // Returns material if it is in the list, assigned in inspector
    private Material GetGridVisualTypeMaterial(GridColor gridColor)
    {
        // Search through materials
        foreach(GridVisualColorMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            // Material found
            if (gridVisualTypeMaterial.gridColor == gridColor)
            {
                return gridVisualTypeMaterial.material;
            }
        }

        Debug.LogError("GridVisual Color not found! " + gridColor);
        return null;
    }

    // Destroys all visual objects when combat is over
    public void ClearGridVisuals()
    {
        // Clear grid array
        if (gridVisualSingleArray != null)
        {
            Array.Clear(gridVisualSingleArray, 0, gridVisualSingleArray.Length);
        }

        if (gridVisualParent == null)
        {
            Debug.LogError("gridVisualParent is null!");
            return;
        }

        // Destroy all children
        foreach (Transform child in gridVisualParent)
        {
            Destroy(child.gameObject);
        }

        hasUpdatedGrid = false; // Reset for dumb start up check
    }

    // Events that change grid visual
    private void UnitActionSystem_OnSelectedActionChange(object sender, UnitActionSystem.OnSelectedActionChangeEentArgs e)
    {
        UpdateGridVisual(e.baseAction);
    }

    private void LevelGrid_OnAnyUnitMoveGridPosition(object sender, LevelGrid.OnAnyUnitMoveGridPositionEventArgs e)
    {
        UpdateGridVisual(e.baseAction);
    }

    private void BaseAction_OnAnyActionEnd(object sender, BaseAction.OnAnyActionEndEventArgs e)
    {
        UpdateGridVisual(e.baseAction);
    }
}

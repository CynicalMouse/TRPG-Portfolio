using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    // Event for when a unit moves position
    public event EventHandler<OnAnyUnitMoveGridPositionEventArgs> OnAnyUnitMoveGridPosition;

    public class OnAnyUnitMoveGridPositionEventArgs : EventArgs
    {
        public BaseAction baseAction;
    }

    private GridSystem<GridObject> gridSystem; // Grid system for interacting with positions and unit lists
    [SerializeField] private Transform debugPrefab; // Debug object for showing positions
    [SerializeField] private bool debugTiles = false; // Hide or show debug in inspector

    // Default values
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 2f; // Size in world of each cell
    [SerializeField] private Transform gridOrigin; // Origin (bottom left) of grid
    private Vector3 initialGridPosition; // Vector 3 of above

    // Check for entering and exiting combat
    private bool levelGridCreated;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Level already exists! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        initialGridPosition = gridOrigin.position;

        Instance = this;

        //CreateNewGrid();
    }

    private void Start()
    {
        //SetUpPathFinding();
    }

    // Set up for entering combat (Default Values)
    public void CreateNewGrid()
    {
        if (gridSystem != null)
        {
            Debug.LogWarning("Grid system already exists");
        }

        gridSystem = new GridSystem<GridObject>(
        width, height, cellSize, initialGridPosition,
        (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(gridPosition, g)
        );

        levelGridCreated = true;

        if (debugTiles)
        {
            gridSystem.CreateDebugObjects(debugPrefab);
        }
    }

    // Set up grid based on provided parameters
    // Set in combat triggers
    public void CreateNewGrid(int width, int height, float cellSize, Vector3 gridOrigin)
    {
        if (gridSystem != null)
        {
            Debug.LogWarning("Grid system already exists");
        }

        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.gridOrigin.position = gridOrigin;

        initialGridPosition = gridOrigin;

        gridSystem = new GridSystem<GridObject>(
        width, height, cellSize, initialGridPosition,
        (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(gridPosition, g)
        );

        levelGridCreated = true;

        if (debugTiles)
        {
            gridSystem.CreateDebugObjects(debugPrefab);
        }
    }

    // Deletes grid for later recreation
    // Setting to 0 doesnt seem right, look into later, may break
    public void ClearGrid()
    {
        levelGridCreated = false;

        //width = 0; 
        //height = 0;
        //cellSize = 0;
        gridOrigin.position = Vector3.zero;
        gridSystem = null;
    }

    // Sets up pathfinding
    public void SetUpPathFinding()
    {
        Pathfinding.Instance.SetUp(width, height, cellSize, initialGridPosition);
    }

    // Adds a unit to the positions list
    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        // Get the grid object of the current position
        GridObject gridObject = gridSystem.GetGridObject(gridPosition, out bool correctedPosition);

        // If unit's position had to be corrected, teleport it first. Should only occur before combat starts
        if (correctedPosition) unit.transform.position = GetWorldPosition(gridObject.GetGridPosition());

        // Add to list
        gridObject.AddUnit(unit);
    }

    // Returns entire list of units at position
    public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);

        return gridObject.GetUnitList();
    }

    // Removes provided unit from grid position
    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);

        gridObject.RemoveUnit(unit);
    }

    // Called when a unit moves between grid positions
    public void UnitMoveGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        // Add and remove unit from positions
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);

        // Fire off event
        OnAnyUnitMoveGridPosition?.Invoke(this, new OnAnyUnitMoveGridPositionEventArgs
        {
            baseAction = unit.GetAction<MoveAction>()
        });
    }

    // Updates origin position for setting up new combat instance
    public void SetGridOrigin(Vector3 newOrigin)
    {
        gridSystem.SetOrigin(newOrigin);
    }
    public Vector3 GetGridOrigin()
    {
        return gridOrigin.position;
    }

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);

    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);

    public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

    public int GetWidth() => gridSystem.GetWidth();
    public int GetHeight() => gridSystem.GetHeight();

    public float GetCellSize() => gridSystem.GetCellSize();

    public bool GetGridCreated()
    {
        return levelGridCreated;
    }

    public InteractableObject GetInteractableObjectAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetInteractableObject();
    }

    public void SetInteractableObjectAtGridPosition(GridPosition gridPosition, InteractableObject interactableObject)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetInteractableObject(interactableObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

public class GridSystem<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridObjectArray;

    private Vector3 originPosition;

    // Constructor
    public GridSystem(int width, int height, float cellSize, Vector3 originPosition, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridObjectArray = new TGridObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                gridObjectArray[x, z] = createGridObject(this, gridPosition);
            }
        }
    }

    // Returns the world position of a grid position
    // Example, movement of units
    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return originPosition + new Vector3(gridPosition.x * cellSize, 0, gridPosition.z * cellSize);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        Vector3 offsetPosition = worldPosition - originPosition; // Adjust for origin
        return new GridPosition(
            Mathf.RoundToInt(offsetPosition.x / cellSize), // Account for cellsize
            Mathf.RoundToInt(offsetPosition.z / cellSize)
        );
    }

    public void SetOrigin(Vector3 newOrigin)
    {
        originPosition = newOrigin;
    }

    // Creates debug objects (visible coordinates)
    public void CreateDebugObjects(Transform debugPrefab)
    {
        // For size of grid
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Create this grid position
                GridPosition gridPosition = new GridPosition(x, z);

                // Make debug object
                Transform debugGrid = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);

                // Get component and assign position
                GridDebugObject gridDebugObject = debugGrid.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }

    // Returns a grid object at a specific grid position
    // CORRECTION VERSION FOR WHEN A UNIT IS PLACED OUTSIDE OF THE GRID
    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= width || gridPosition.z < 0 || gridPosition.z >= height)
        {
            Debug.LogWarning($"GridPosition {gridPosition} is out of bounds! Correcting to nearest valid position.");
            gridPosition = CorrectOutOfBoundsGridPosition(gridPosition);
        }

        return gridObjectArray[gridPosition.x, gridPosition.z];
    }


    // Same as above
    // Outputs additional bool for teleporting units to corrected position in LevelGrid
    public TGridObject GetGridObject(GridPosition gridPosition, out bool correctedPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= width || gridPosition.z < 0 || gridPosition.z >= height)
        {
            gridPosition = CorrectOutOfBoundsGridPosition(gridPosition);
            correctedPosition = true;
        }
        else
        {
            correctedPosition = false;
        }

        return gridObjectArray[gridPosition.x, gridPosition.z];
    }


    // Ideally this never gets called. More of a debug function than anything else
    // Units will be on top of eachother, won't cause an issue with functionality, just visual clipping unitl they move
    private GridPosition CorrectOutOfBoundsGridPosition(GridPosition gridPosition)
    {
        if (!LevelGrid.Instance.IsValidGridPosition(gridPosition))
        {
            // Temp remove to reduce error count. Re add if issues occur
            //Debug.LogError($"GridPosition {gridPosition} is out of bounds! Adjusting to nearest valid position.");

            // Adjust to nearest valid position
            gridPosition.x = Mathf.Clamp(gridPosition.x, 0, LevelGrid.Instance.GetWidth() - 1);
            gridPosition.z = Mathf.Clamp(gridPosition.z, 0, LevelGrid.Instance.GetHeight() - 1);

            //Debug.Log($"Adjusted GridPosition: {gridPosition}");
        }

        return gridPosition;
    }


    // Checks if the grid position is within the grid's bounds
    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return gridPosition.x >= 0 && 
               gridPosition.x < width &&
               gridPosition.z >= 0 &&
               gridPosition.z < height;
    }

    public int GetWidth() => width;
    public int GetHeight() => height;

    public float GetCellSize() => cellSize;
}

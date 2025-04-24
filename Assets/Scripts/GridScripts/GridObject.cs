using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that holds info about a grid object
public class GridObject
{
    private GridPosition gridPosition; // This object's position
    private GridSystem<GridObject> gridSystem; // The gridsystem
    private List<Unit> unitList; // Unit's stored on this position (List for when units move over eachother // [0] is functional unit)
    private InteractableObject interactableObject;

    // Constructor
    public GridObject(GridPosition gridPosition, GridSystem<GridObject> gridSystem)
    {
        this.gridPosition = gridPosition;
        this.gridSystem = gridSystem;
        unitList = new List<Unit>();
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitList)
        {
            unitString += unit.ToString() + "\n";
        }
        return gridPosition.ToString() + "\n" + unitString;
    }

    // Setters and Getters
    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    // Remove unit from list
    // Triggers when unit is moved by action and when it temporarily passes over during movement
    public void RemoveUnit(Unit unit)
    {
        unitList.Remove(unit);
    }

    // Adds unit from list
    // Triggers when unit arrives at its target position and when unit temporarily passes over during movement
    public void AddUnit(Unit unit)
    {
        unitList.Add(unit);
    } 

    // Check if position is occupied
    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    // Returns unit occupying this position
    // index 0 as this is the actual occupying unit, not one that is passing over
    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        }

        return null;
    }

    // Return the grid position
    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    // Change to get interactable object
    public InteractableObject GetInteractableObject()
    {
        return interactableObject;
    }

    public void SetInteractableObject(InteractableObject interactableObject)
    {
        this.interactableObject = interactableObject;
    }
}

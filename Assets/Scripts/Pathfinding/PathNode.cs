using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to hold a nodes values and links

public class PathNode
{
    private GridPosition gridPosition;

    private int gCost;
    private int hCost;
    private int fCost;

    private PathNode previousFromPathNode;


    // Default, tiles are walkable
    private bool isWalkable = true;

    // Add environment cost?
    // Like fire adds 10 cost to move through it
    // Prioritize moving around fire but can move through it if necessary
    // based on enum? or all environment costs are the same? fire = gas = poison = mud for example
    // private int environmentCost;

    public PathNode(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public override string ToString()
    {
        return gridPosition.ToString();
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public int GetGCost()
    {
        return gCost;
    }

    public int GetHCost()
    {
        return hCost;
    }

    public int GetFCost()
    {
        return fCost;
    }

    public PathNode GetPreviousPathNode()
    {
        return previousFromPathNode;
    }

    public void SetPreviousPathNode(PathNode newPreviousNode)
    {
        this.previousFromPathNode = newPreviousNode;
    }

    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
    }

    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
    }

    public void UpdateFCost()
    {
        fCost = gCost + hCost;
    }

    public void ResetPreviousPathNode()
    {
        previousFromPathNode = null;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool walkable)
    {
        isWalkable = walkable;
    }
}

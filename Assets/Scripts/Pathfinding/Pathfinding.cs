using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }


    [SerializeField] private Transform debugPrefab;
    [SerializeField] private bool showDebug;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    // Size of grid, got from LevelGrid.SetupPathfinding
    private int width;
    private int height;
    private float cellSize;

    private GridSystem<PathNode> gridSystem;

    [SerializeField] private LayerMask obstacleLayer;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one Pathfinding! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; 
    }

    // Sets up pathfinding based on the provided parameters
    public void SetUp(int width, int height, float cellsize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellsize;

        gridSystem = new GridSystem<PathNode>(width, height, cellsize, originPosition, (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition)); // Creates own gridsystem for getting positions and creating nodes // Essentially a copy of the one in level grid

        // Shows debug object with f, g, and h cost and if walkable
        if (showDebug) gridSystem.CreateDebugObjects(debugPrefab);

        // Loop through size of grid
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Create new position and get its world position
                GridPosition gridPosition = new GridPosition(x, z);
                Vector3 gridPositionWorldPos = LevelGrid.Instance.GetWorldPosition(gridPosition);

                float raycastOffsetDistance = 5f;

                // Raycast check if there is an obstacle
                // Offset raycast down so it doesn't spawn inside the obstacle and not count it
                if( Physics.Raycast(gridPositionWorldPos + Vector3.down * raycastOffsetDistance, Vector3.up, raycastOffsetDistance * 2, obstacleLayer))
                {
                    GetNode(x, z).SetIsWalkable(false);
                }
            }
        }
    }

    // Clear instance of pathfinding after exiting combat
    public void Clear()
    {
        width = 0;
        height = 0;
        cellSize = 0;

        gridSystem = null;
    }

    // Finds a path using A* algorithm
    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        // Create lists to store nodes to search through and nodes that have been searched
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        // Generate start and end nodes
        PathNode startNode = gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = gridSystem.GetGridObject(endGridPosition);

        // Add start node to open list
        openList.Add(startNode);

        // Loop through length and height of grid
        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                // Create a node for each grid position
                // Defaults are Max gcost, 0 heuristic and no pervious node
                PathNode pathNode = gridSystem.GetGridObject(new GridPosition(x, z));
                pathNode.SetGCost(int.MaxValue); // Max ensures node won't accidentally be considered best path before its checked
                pathNode.SetHCost(0);
                pathNode.UpdateFCost();
                pathNode.ResetPreviousPathNode();
            }
        }

        // Set start node values
        // 0 gcost as thats where it starts
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.UpdateFCost();


        // While there are nodes to be searched, loop
        // Starts because openList contains the starting node, more nodes get added to list during loop
        // Essentially, first loop only has the startNode, then adds it's neighbours, then adds their neighbours etc
        while (openList.Count > 0)
        {
            // Get next node to search
            PathNode currentNode = GetLowestFCostPathNode(openList);

            // Check if current searched node is the end node
            if (currentNode == endNode)
            {
                // Reached final node
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            // Remove node from search list, add it to searched list
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Loop through neighbours of current node
            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                // Node already searched, skip
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                // Node isnt walkable, skip
                if (!neighbourNode.IsWalkable())
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                // Current cost + cost to neighbour
                // Add environment cost here?
                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition());

                // Tentative cost less than current cost, better path found
                if (tentativeGCost < neighbourNode.GetGCost())
                {
                    // Update node so that current node is path to neighbour
                    neighbourNode.SetPreviousPathNode(currentNode);
                    neighbourNode.SetGCost(tentativeGCost);
                    neighbourNode.SetHCost(CalculateDistance(neighbourNode.GetGridPosition(), endGridPosition));
                    neighbourNode.UpdateFCost();

                    // Neigbour not added to list yet, add it
                    // THIS is where the openList gets populated
                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // No path found
        pathLength = 0;
        return null;
    }

    // Calculates the distance between two grid points
    public int CalculateDistance(GridPosition a, GridPosition b)
    {
        GridPosition gridPositionDistance = a - b;

        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);

        return (xDistance + zDistance) * MOVE_STRAIGHT_COST;

        /* DIAGONAL MOVEMENT VERSION
         * 
         * GridPosition gridPositionDistance = a - b;
         * 
         * int xDistance = Mathf.Abs(gridPositionDistance.x);
         * int zDistance = Mathf.Abs(gridPositionDistance.z);
         * 
         * int remaining = Mathf.Abs(xDistance - zDistance);
         * 
         * return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
         * 
         */
    }

    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        // Set first node to check
        PathNode lowestFCostPathNode = pathNodeList[0];

        // Cycle through list
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            // If current node is lower than stored node
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                // New lowest cost
                lowestFCostPathNode = pathNodeList[i];
            }
        }

        // Return lowest cost node
        return lowestFCostPathNode;
    }

    // Get a node at a certian position
    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }


    // Gets list of nodes surrounding provided node
    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        // Currently always movse up first if possible
        // Not too noticable in proper environment
        // Awful in large open space
        // Could add random so starting move direction is different each time

        if (gridPosition.z + 1 < gridSystem.GetHeight())
        {
            // Up
            neighbourList.Add(gridSystem.GetGridObject(new GridPosition(gridPosition.x + 0, gridPosition.z + 1)));
        }

        if (gridPosition.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(gridSystem.GetGridObject(new GridPosition(gridPosition.x - 1, gridPosition.z + 0)));
        }

        if (gridPosition.x + 1 < gridSystem.GetWidth())
        {
            // Right
            neighbourList.Add(gridSystem.GetGridObject(new GridPosition(gridPosition.x + 1, gridPosition.z + 0)));
        }

        if (gridPosition.z - 1 >= 0)
        {
            // Down
            neighbourList.Add(gridSystem.GetGridObject(new GridPosition(gridPosition.x + 0, gridPosition.z - 1)));
        }

        // IF WANT DIAGONAL MOVEMENT, ADD UPLEFT, DOWNRIGHT ETC

        return neighbourList;
    }

    // Calculates a path of grid positions
    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathList = new List<PathNode>();

        // Add end node to list first
        pathList.Add(endNode);

        // Start on node at end of path
        PathNode currentNode = endNode;

        // Check if node has a node linked to it
        while (currentNode.GetPreviousPathNode() != null)
        {
            // Add linked node to path
            pathList.Add(currentNode.GetPreviousPathNode());

            // Set linked node to node to be searched, starting the loop again
            currentNode = currentNode.GetPreviousPathNode();
        }


        // List starts on end node, reverse it
        pathList.Reverse();

        // Create list of grid positions
        List<GridPosition> gridPositions = new List<GridPosition>();


        // Convert path node list to grid positions
        foreach (PathNode pathNode in pathList)
        {
            gridPositions.Add(pathNode.GetGridPosition());
        }

        return gridPositions;
    }

    // Check if position can be walked on
    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).IsWalkable();
    }

    // Check if position can be walked on
    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool IsWalkable)
    {
        gridSystem.GetGridObject(gridPosition).SetIsWalkable(IsWalkable);
    }

    // Returns true if a path to position can be made
    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    // Returns length of path
    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        // Path is getting calculated a second time here. Could be made better if path length is just a member variable that gets saved in pathfinding itself? probably not though
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
}

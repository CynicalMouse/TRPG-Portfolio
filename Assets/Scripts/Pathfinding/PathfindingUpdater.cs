using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingUpdater : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DestructibleObject.OnAnyDestructibleObjectDestroyed += DestructibleObject_OnAnyDestructibleObjectDestroyed;
    }

    private void DestructibleObject_OnAnyDestructibleObjectDestroyed(object sender, EventArgs e)
    {
        DestructibleObject destructibleObject = sender as DestructibleObject;

        // Update pathfinding node to is walkable
        Pathfinding.Instance.SetIsWalkableGridPosition(destructibleObject.GetGridPosition(), true);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    private List<Unit> unitList;
    private List<Unit> playerUnits;
    private List<Unit> enemyUnits;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one EnemyAI" + transform + Instance);
            Destroy(gameObject);
        }

        Instance = this;

        // Create lists
        unitList = new List<Unit>();
        playerUnits = new List<Unit>();
        enemyUnits = new List<Unit>();

        // Event subs
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    // Triggered by unit, adds new unit to lists
    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        if (unit.GetIsEnemy())
        {
            enemyUnits.Add(unit);
        }
        else
        {
            playerUnits.Add(unit);
        }

        unitList.Add(unit);
    }

    // Triggered by unit, removes unit from list
    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        if (unit.GetIsEnemy())
        {
            enemyUnits.Remove(unit);

            // All enemy units dead
            if (enemyUnits.Count <= 0)
            {
                // Custom logic here
                // Presumably send an event instead?
                // So can set up cutscenes etc

                // For now, end combat
                PlayerStateController.Instance.ExitCombat();
            }
        }
        else
        {
            playerUnits.Remove(unit);

            if (playerUnits.Count <= 0)
            {
                // All player units dead
                // Custom logic here
                // Presumably send an event instead?
                // So can set up cutscenes etc

                // For now, end combat
                PlayerStateController.Instance.ExitCombat();
            }
        }

        unitList.Remove(unit);
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public List<Unit> GetPlayerUnits()
    {
        return playerUnits;
    }

    public List<Unit> GetEnemyUnits()
    {
        return enemyUnits;
    }

    // Clears list for later new combat instance
    public void ClearLists()
    {
        unitList.Clear();
        playerUnits.Clear();
        enemyUnits.Clear();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }

    private int turnNumber = 0;
    [SerializeField] private bool isPlayerTurn = true;

    public event EventHandler OnTurnChanged;

    private void Awake()
    {
        // Ensure there's only one instance of TurnSystem
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TurnSystem instances found!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ResetTurnCount()
    {
        turnNumber = 0;
        OnTurnChanged?.Invoke(this, EventArgs.Empty); // Updates UI // ALSO TRIGGERS ENEMY AI TO CHECK ITS TURN, DOESNT CAUSE ISSUES RN BUT BE CAREFUL
    }

    // Go to next turn
    public void NextTurn()
    {
        turnNumber++;
        isPlayerTurn = !isPlayerTurn;
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
}

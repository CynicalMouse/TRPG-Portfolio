using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnCounter;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject turnIndicator;

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChange;
        endTurnButton.onClick.AddListener(EndTurn);
        UpdateTurnCounter();
        UpdateTurnIndicator();
        UpdateEndTurnButtonVisibility();
    }

    private void EndTurn()
    {
        TurnSystem.Instance.NextTurn();
    }

    private void TurnSystem_OnTurnChange(object sender, EventArgs e)
    {
        UpdateTurnCounter();
        UpdateTurnIndicator();
        UpdateEndTurnButtonVisibility();
    }

    private void UpdateTurnCounter()
    {
        turnCounter.text = "Turn: " + TurnSystem.Instance.GetTurnNumber();
    }

    private void UpdateTurnIndicator()
    {
        // Hide on player turn
        turnIndicator.SetActive(!TurnSystem.Instance.IsPlayerTurn());
    }

    private void UpdateEndTurnButtonVisibility()
    {
        // Show on player turn
        endTurnButton.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn());
    }
}

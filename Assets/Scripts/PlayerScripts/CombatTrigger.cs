using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    [SerializeField] private bool combatTriggered = false;

    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private Transform gridOrigin;
    [SerializeField] private Transform cameraStartTransform;

    [SerializeField] private Transform unitArrayParent;
    private Unit[] unitArray;

    [SerializeField] private Transform playerUnitSpawnPointParent;
    private Transform[] playerSpawnPoints;

    // Change to interactable objects
    [SerializeField] private Transform interactableObjectParent;
    private InteractableObject[] interactableObjects;

    private void Start()
    {
        // Validate all neccesarry data for combat trigger is assigned in inspector
        if (ValidateAllParameters() == false) return;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the "Player" tag
        {
            StartCombat();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Exit logic
        }
    }

    public void StartCombat()
    {
        if (combatTriggered) return;

        // One last error check
        if (ValidateAllParameters() == false) return;

        // Create unit array
        CreateUnitArray(); ;

        // Create interactable object array
        CreateInteractableObjectArray();

        // Not in combat, enable combat
        if (!PlayerStateController.Instance.IsCombat())
        {
            PlayerStateController.Instance.StartCombat(width, height, cellSize, gridOrigin.position, cameraStartTransform, unitArray, interactableObjects);
            combatTriggered = true; // Stops trigger from starting combat again
        }

        // Clear afterwards
        // Don't know if this is needed but it could avoid issues when units die
        ClearUnitArray();
    }


    private bool ValidateAllParameters()
    {
        if (width == 0 || height == 0)
        {
            Debug.LogError("Height or width of combat trigger are 0! " + gameObject.name + " " + gameObject.transform.position.ToString() + " (" + width + "," + height + ") \n Did you forget to assign them in the inspector?");
            return false;
        }

        if (cellSize == 0)
        {
            Debug.LogError("Cellsize of combat trigger is 0! " + gameObject.name + " " + gameObject.transform.position.ToString() + " (" + cellSize + ") \n Did you forget to assign it in the inspector?");
            return false;
        }

        if (gridOrigin == null)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + " has no grid origin! \n Did you forget to assign it in the inspector?");
            return false;
        }

        if (cameraStartTransform == null)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + " has no starting camera position! \n Did you forget to assign it in the inspector?");
            return false;
        }

        if (playerUnitSpawnPointParent == null)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + " has no player spawn point positions! \n Did you forget to assign it in the inspector?");
            return false;
        }

        // Populate spawn point array
        playerSpawnPoints = GetComponentsInChildren<Transform>();

        if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + "'s player spawn point object has no children!!! \n Did you forget to assign them in the inspector?");
            return false ;
        }

        return true;
    }

    private void CreateUnitArray()
    {
        // Activate each unit
        foreach (Transform child in unitArrayParent.transform)
        {
            child.gameObject.SetActive(true);
        }

        if (unitArrayParent == null)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + " has no no Unit Array object!!! \n Did you forget to assign it in the inspector?");
        }

        unitArray = unitArrayParent.GetComponentsInChildren<Unit>();

        if (unitArray.Length == 0)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + "'s Unit object has no children!!! \n Did you forget to assign it in the inspector?");
        }
    }

    private void ClearUnitArray()
    {
        Array.Clear(unitArray, 0, unitArray.Length);
    }

    private void CreateInteractableObjectArray()
    {
        // Collect all interactable objects
        if (interactableObjectParent == null)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + " has no no Unit Array object!!! \n Did you forget to assign it in the inspector?");
        }

        interactableObjects = interactableObjectParent.GetComponentsInChildren<InteractableObject>();

        if (interactableObjects.Length == 0)
        {
            Debug.LogError(gameObject.name + " " + gameObject.transform.position.ToString() + "'s Unit object has no children!!! \n Did you forget to assign it in the inspector?");
        }
    }
}

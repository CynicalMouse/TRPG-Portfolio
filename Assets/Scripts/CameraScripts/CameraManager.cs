using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject playerFollowCamObject;
    [SerializeField] private GameObject combatCameraObject;
    [SerializeField] private GameObject actionCameraGameobject;

    private void Start()
    {
        //BaseAction.OnAnyActionStart += BaseAction_OnAnyActionStart;
        //BaseAction.OnAnyActionEnd += BaseAction_OnAnyActionEnd;

        PlayerStateController.Instance.OnCombatStarted += PlayerStateController_OnCombatStarted;
        PlayerStateController.Instance.OnCombatEnded += PlayerStateController_OnCombatEnded;

        HideActionCamera();
    }

    // Swap between combat and exploration cameras
    private void EnterCombatCamera()
    {
        combatCameraObject.SetActive(true);
        playerFollowCamObject.SetActive(false);
    }

    private void ExitCombatCamera()
    {
        playerFollowCamObject.SetActive(true);
        combatCameraObject.SetActive(false);
        HideActionCamera();
    }

    // Show and hide action cam
    private void ShowActionCamera()
    {
        actionCameraGameobject.SetActive(true);
    }

    private void HideActionCamera()
    {
        actionCameraGameobject.SetActive(false);
    }

    // If action is ranged action
    // Look at target
    private void BaseAction_OnAnyActionStart(object sender, EventArgs e)
    {
        switch (sender)
        {
            case RangedAttackAction rangedAction:
                Unit actingUnit = rangedAction.GetUnit();
                Unit targetUnit = rangedAction.GetTargetUnit();

                Vector3 shootDirection = (targetUnit.GetWorldPosition() - actingUnit.GetWorldPosition()).normalized;

                Vector3 cameraCharacterHeight = Vector3.up * 1.7f; // So camera isnt in ground
                
                float shoulderOffsetAmount = 0.5f; // Slightly to right 
                Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDirection * shoulderOffsetAmount;

                Vector3 actionCameraPosition = actingUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + (shootDirection * -1);

                actionCameraGameobject.transform.position = actionCameraPosition;
                actionCameraGameobject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);
                ShowActionCamera();
                break;
        }
    }

    // If ranged action has ended
    // Hide action camera
    private void BaseAction_OnAnyActionEnd(object sender, EventArgs e)
    {
        switch (sender)
        {
            case RangedAttackAction rangedAction:
                HideActionCamera();
                break;
        }
    }

    // Event functions to flip between cameras
    private void PlayerStateController_OnCombatStarted(object sender, EventArgs e)
    {
        EnterCombatCamera();
    }

    private void PlayerStateController_OnCombatEnded(object sender, EventArgs e)
    {
        ExitCombatCamera();
    }
}

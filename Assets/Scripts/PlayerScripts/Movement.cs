using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float defualtMovementSpeed;
    private float sprintSpeed;
    private float movementSpeed;
    private Vector3 moveDirection;

    private void Start()
    {
        movementSpeed = defualtMovementSpeed;
        sprintSpeed = defualtMovementSpeed * 2.5f;
        PlayerStateController.Instance.OnCombatEnded += PlayerStateController_OnCombatEnded;
    }

    // Update is called once per frame
    void Update()
    {
        // In combat, don't allow free movement
        if (PlayerStateController.Instance.IsCombat()) return;

        ChangeMoveSpeed();

        Move();
    }

    private void Move()
    {
        // Calculate movement direction
        Vector2 moveVector = InputManager.Instance.GetMoveVector().normalized;

        moveDirection = new Vector3(moveVector.x, 0, moveVector.y);

        // Move the player
        transform.Translate(moveDirection * movementSpeed * Time.deltaTime);
    }

    private void ChangeMoveSpeed()
    {
        if (InputManager.Instance.IsHoldingShift())
        {
            movementSpeed = sprintSpeed;
        }
        else
        {
            movementSpeed = defualtMovementSpeed;
        }
    }

    private void PlayerStateController_OnCombatEnded(object sender, EventArgs e)
    {
        movementSpeed = defualtMovementSpeed;
    }
}

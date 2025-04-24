using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableObject
{
    [SerializeField] private bool IsOpen;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void SetUp()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetInteractableObjectAtGridPosition(gridPosition, this);
        
        if (IsOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    public override void Interact(Action onInteractComplete)
    {
        this.onInteractComplete = onInteractComplete;
        isActive = true;
        interactionActionTimer = 0.5f;

        if (IsOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        IsOpen = true;

        animator.SetBool("IsOpen", IsOpen);

        Pathfinding.Instance.SetIsWalkableGridPosition(gridPosition, IsOpen);
    }

    private void CloseDoor()
    {
        IsOpen = false;

        animator.SetBool("IsOpen", IsOpen);

        Pathfinding.Instance.SetIsWalkableGridPosition(gridPosition, IsOpen);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    protected GridPosition gridPosition;
    protected Action onInteractComplete;
    protected bool isActive;
    [SerializeField] protected float interactionActionTimer;

    public abstract void SetUp();
    public abstract void Interact(Action onInteractComplete);

    private void Update()
    {
        if (!isActive) return;

        interactionActionTimer -= Time.deltaTime;

        if (interactionActionTimer <= 0)
        {
            isActive = false;
            onInteractComplete();
        }
    }
}

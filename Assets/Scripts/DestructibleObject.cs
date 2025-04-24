using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public static event EventHandler OnAnyDestructibleObjectDestroyed;

    private GridPosition gridPosition;

    [SerializeField] private Transform DestroyedObject;

    private void Start()
    {
        PlayerStateController.Instance.OnCombatStarted += PlayerStateController_OnCombatStarted;
    }

    public void Damage()
    {
        Transform destroyedObject = Instantiate(DestroyedObject, transform.position, Quaternion.identity);

        ApplyExplosionToChildren(destroyedObject, 150f, transform.position, 10f);

        Destroy(gameObject);

        OnAnyDestructibleObjectDestroyed?.Invoke(this, EventArgs.Empty);
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    private void ApplyExplosionToChildren(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody ChildRb))
            {
                ChildRb.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }

            ApplyExplosionToChildren(child, explosionForce, explosionPosition, explosionRange);
        }
    }

    private void PlayerStateController_OnCombatStarted(object sender, EventArgs e)
    {
        if (LevelGrid.Instance.GetGridPosition(transform.position) == null)
        {
            // outside of grid bounds
            // skip this object
        }
        else
        {
            gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        }
    }
}

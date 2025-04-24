using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    
    [SerializeField] private float damageRadius = 4f; // 2 = 1 grid position, 4 = 2 etc
    [SerializeField] private int damage = 2;
    [SerializeField] private Transform explosiveVFXPrefab;

    private Vector3 targetPosition;
    [SerializeField] private AnimationCurve arcAnimationCurve;
    private float totalDistance;
    private Vector3 xzPosition;

    private Action onExplosiveBehaviourComplete;

    public static event EventHandler OnAnyExplosiveExploded;

    public void Setup(GridPosition targetGridPosition, float damageRadius, int damage, Action onExplosiveBehaviourComplete)
    {
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        totalDistance = Vector3.Distance(transform.position, targetPosition);
        xzPosition = this.transform.position;
        xzPosition.y = 0;

        this.damageRadius = damageRadius;
        this.damage = damage;

        this.onExplosiveBehaviourComplete = onExplosiveBehaviourComplete;
    }

    private void Update()
    {
        Movement();

        float destroyDistance = 0.1f;

        if (Vector3.Distance(transform.position, targetPosition) < destroyDistance)
        {
            // Get all collisions in an area, probably bad maybe change to a unit layer or something
            Collider[] colliders = Physics.OverlapSphere(targetPosition, damageRadius);

            foreach(Collider collider in colliders)
            {
                // Unit in damage radius
                if(collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.TakeDamage(damage);
                }

                if (collider.TryGetComponent<DestructibleObject>(out DestructibleObject destructibleObject))
                {
                    destructibleObject.Damage();
                }
            }

            // Destroy projectile object
            Destroy(gameObject);

            // Spawn explosion VFX
            Instantiate(explosiveVFXPrefab, targetPosition + Vector3.up * 1.0f, Quaternion.identity); 
            
            // Fire events
            OnAnyExplosiveExploded?.Invoke(this, EventArgs.Empty); // For camera shake
            onExplosiveBehaviourComplete();
        }
    }

    private void Movement()
    {
        Vector3 moveDirection = (targetPosition - xzPosition).normalized;
        float moveSpeed = 10f;

        xzPosition += moveDirection * Time.deltaTime * moveSpeed;

        float distance = Vector3.Distance(xzPosition, targetPosition);
        float distanceNormalised = 1 - distance / totalDistance;

        float maxHeight = totalDistance / 5f;
        float yPosition = arcAnimationCurve.Evaluate(distanceNormalised) * maxHeight;

        transform.position = new Vector3(xzPosition.x, yPosition, xzPosition.z);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    private void Awake()
    {
        MoveAction moveAction = GetComponentInChildren<MoveAction>();
        if (moveAction != null)
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }

        RangedAttackAction rangedAttackAction = GetComponentInChildren<RangedAttackAction>();
        if (rangedAttackAction != null)
        {
            rangedAttackAction.OnRangedAttack += RangedAttackAction_OnRangedAttack;
        }

        MeleeAction meleeAction = GetComponentInChildren<MeleeAction>();
        if (meleeAction != null)
        {
            meleeAction.OnMeleeAttackStarted += MeleeAction_OnMeleeAttackStarted;
            meleeAction.OnMeleeAttackEnded += MeleeAction_OnMeleeAttackEnded;
        }
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsMoving", true);
    }

    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsMoving", false);
    }

    private void RangedAttackAction_OnRangedAttack(object sender, RangedAttackAction.OnRangedAttackEventArgs e)
    {
        animator.SetTrigger("RangedAttack");

        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, bulletSpawnPoint.position, Quaternion.identity);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 targetPosition = e.targetUnit.GetRangedAttackTargetPosition();

        bulletProjectile.SetUp(e.targetUnit.GetRangedAttackTargetPosition());
    }

    private void MeleeAction_OnMeleeAttackStarted(object sender, EventArgs e)
    {
        animator.SetTrigger("MeleeAttack");
    }

    private void MeleeAction_OnMeleeAttackEnded(object sender, EventArgs e)
    {
        animator.ResetTrigger("MeleeAttack");
    }
}

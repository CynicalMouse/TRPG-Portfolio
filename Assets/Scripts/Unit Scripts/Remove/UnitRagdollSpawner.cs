using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdollSpawner : MonoBehaviour
{
    [SerializeField] private Transform ragdollPrefab;
    [SerializeField] private Transform ragdollRootBone;
    private HealthSystem healthSystem;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();

        healthSystem.OnDeath += HealthSystem_OnDeath;
    }

    private void HealthSystem_OnDeath(object sender, EventArgs e)
    {
        Transform ragdollTransform = Instantiate(ragdollPrefab, transform.position, transform.rotation);

        UnitRagdoll unitRagdoll = ragdollTransform.GetComponent<UnitRagdoll>();
        unitRagdoll.SetUp(ragdollRootBone);
    }

}

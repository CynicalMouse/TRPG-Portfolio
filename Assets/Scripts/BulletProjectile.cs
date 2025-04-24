using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private Transform hitVFX;

    private Vector3 targetPosition;
    [SerializeField] private float bulletSpeed = 35f;

    private void Awake()
    {
        // Destroy after 10s if below check fails
        Destroy(gameObject, 10f);   
    }

    public void SetUp(Vector3 targetPosition)
    {
       this.targetPosition = targetPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, bulletSpeed * Time.deltaTime);

        if (transform.position == targetPosition)
        {
            bulletTrail.transform.parent = null;
            Destroy(gameObject);

            Instantiate(hitVFX, targetPosition, Quaternion.identity);
        }
    }
}

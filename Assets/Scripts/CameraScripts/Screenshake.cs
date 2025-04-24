using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Screenshake : MonoBehaviour
{
    public static Screenshake Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();

        if (Instance != null)
        {
            Debug.LogError("Screenshake already exists! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        // Test
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    impulseSource.GenerateImpulse();
        //}
    }

    public void Shake(float intensity)
    {
        impulseSource.GenerateImpulse(intensity);
    }
}

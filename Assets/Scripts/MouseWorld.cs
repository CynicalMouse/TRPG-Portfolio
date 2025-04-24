using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWorld : MonoBehaviour
{
    private static MouseWorld instance;
    [SerializeField] LayerMask groundPlane; // Layer that can be clicked on

    private void Awake()
    {
        instance = this;
    }

    // Gets position that mouse clicks on by sending raycast from camera
    public static Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, instance.groundPlane.value))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
}

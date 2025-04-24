#define USE_NEW_INPUT_SYSTEM
// Comment out above to use old input system

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("More than one input manager! " + transform + " - " + Instance.name);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public Vector2 GetMouseScreenPosition()
    {
#if USE_NEW_INPUT_SYSTEM
        return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif
    }

    public bool IsLeftMouseButtonDownThisFrame()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.LeftClick.WasPressedThisFrame();
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    public Vector2 GetMoveVector()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.Movement.ReadValue<Vector2>();
#else
        Vector2 inputMoveDirection = new Vector3(0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            inputMoveDirection += new Vector2(0, 1);
        }

        if (Input.GetKey(KeyCode.S))
        {
            inputMoveDirection += new Vector2(0, -1);
        }

        if (Input.GetKey(KeyCode.A))
        {
            inputMoveDirection += new Vector2(-1, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            inputMoveDirection += new Vector2(1, 0);
        }

        return inputMoveDirection;
#endif
    }

    public bool RotateLeft()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraRotateLeft.triggered;
#else
        return Input.GetKeyDown(KeyCode.Q);
#endif
    }

    public bool RotateRight()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.CameraRotateRight.triggered;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    public int GetCameraZoomLevel(int currentZoom)
    {
#if USE_NEW_INPUT_SYSTEM
        if (playerInputActions.Player.CameraZoom.ReadValue<float>() > 0 && PlayerStateController.Instance.IsCombat())
        {
            currentZoom -= 1;

            // Clamp to lowest zoom
            if (currentZoom < 0)
            {
                currentZoom = 0;
            }
        }
        else if (playerInputActions.Player.CameraZoom.ReadValue<float>() < 0 && PlayerStateController.Instance.IsCombat())
        {
            currentZoom += 1;

            // Clamp to highest zoom
            if (currentZoom > 2)
            {
                currentZoom = 2;
            }
        }

        return currentZoom;
#else
        // Scroll down
        if (Input.mouseScrollDelta.y > 0 && PlayerStateController.Instance.IsCombat())
        {
            currentZoom -= 1;

            // Clamp to lowest zoom
            if (currentZoom < 0)
            {
                currentZoom = 0;
            }
        }
        // Scroll Up
        else if (Input.mouseScrollDelta.y < 0 && PlayerStateController.Instance.IsCombat())
        {
            currentZoom += 1;

            // Clamp to highest zoom
            if (currentZoom > 2)
            {
                currentZoom = 2;
            }
        }

        return currentZoom;
#endif
    }

    public bool IsHoldingShift()
    {
#if USE_NEW_INPUT_SYSTEM
        return playerInputActions.Player.HoldingSprint.IsPressed();
#else
        return Input.GetKey(KeyCode.LeftShift);
#endif
    }
}

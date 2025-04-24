using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Transform startingPosition; // Starting position of camera

    // Camera Speeds
    [SerializeField] private float moveSpeed = 5f;
    private float defaultMoveSpeed;
    private float fastMoveSpeed;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera; // Camera
    
    private CinemachineTransposer cinemachineTransposer; // For editing offsets
    //private CinemachineComposer cinemachineComposer;
    private Vector3 targetFollowOffset;

    // Various zoom levels
    private enum CameraZoomLevel
    {
        Close, 
        Medium,
        Far,
        Exploration,
    }

    [SerializeField] private CameraZoomLevel cameraZoomLevel;

    private Vector3 targetRotation;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("CameraController already exists! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        //cinemachineComposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineComposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;

        defaultMoveSpeed = moveSpeed;
        fastMoveSpeed = moveSpeed * 2.0f;
    }

    private void Update()
    {
        // In combat, full control
        if (PlayerStateController.Instance.IsCombat())
        {
            ChangeMoveSpeed();
            MoveCamera();
            RotateCamera();
            UpdateZoom();
        } 
    }

    private void LateUpdate()
    {
        if (!PlayerStateController.Instance.IsCombat()) return;
        
        float gridWidth = LevelGrid.Instance.GetWidth() * LevelGrid.Instance.GetCellSize();
        float gridHeight = LevelGrid.Instance.GetHeight() * LevelGrid.Instance.GetCellSize();

        float minX = LevelGrid.Instance.GetWorldPosition(new GridPosition(0, 0)).x;
        float maxX = LevelGrid.Instance.GetWorldPosition(new GridPosition(LevelGrid.Instance.GetWidth() - 1, 0)).x;
        float minZ = LevelGrid.Instance.GetWorldPosition(new GridPosition(0, 0)).z;
        float maxZ = LevelGrid.Instance.GetWorldPosition(new GridPosition(0, LevelGrid.Instance.GetHeight() - 1)).z;

        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        transform.position = newPosition;
    }


    private void ChangeMoveSpeed()
    {
        if (InputManager.Instance.IsHoldingShift())
        {
            moveSpeed = fastMoveSpeed;
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
        }
    }

    private void MoveCamera()
    {
        Vector2 inputMoveDirection = InputManager.Instance.GetMoveVector();

        Vector3 moveDirection = transform.forward * inputMoveDirection.y + transform.right * inputMoveDirection.x;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void RotateCamera()
    {
        // Update target rotation based on input
        if (InputManager.Instance.RotateLeft())
        {
            targetRotation.y -= 45f;
        }

        if (InputManager.Instance.RotateRight())
        {
            targetRotation.y += 45f;
        }

        // Normalize target rotation to stay within [0, 360)
        targetRotation.y = (targetRotation.y + 360f) % 360f;

        // Smoothly interpolate the camera's current rotation toward the target
        Quaternion targetQuaternion = Quaternion.Euler(targetRotation);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, Time.deltaTime * rotationSpeed);

        // Optional: Snap to the target rotation when close enough
        if (Quaternion.Angle(transform.rotation, targetQuaternion) < 0.1f)
        {
            transform.rotation = targetQuaternion;
        }
    }

    private void UpdateZoom()
    {
        cameraZoomLevel = (CameraZoomLevel)InputManager.Instance.GetCameraZoomLevel((int)cameraZoomLevel);

        // Change offset based on zoom level
        switch (cameraZoomLevel)
        {
            case CameraZoomLevel.Close:
                targetFollowOffset = new Vector3(0, 2, -9);
                break;

            case CameraZoomLevel.Medium:
                targetFollowOffset = new Vector3(0, 10, -8);
                break;

            case CameraZoomLevel.Far:
                targetFollowOffset = new Vector3(0, 15, -0.1f);
                break;
        }

        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * zoomSpeed); ;
    }

    public void EnterCombat() // Add input camera position?
    {
        // Set initial zoom level and camera position
        cameraZoomLevel = CameraZoomLevel.Medium;
        transform.position = startingPosition.position;
    }

    public void EnterCombat(Transform startPosition) // Add input camera position?
    {
        // Set initial zoom level and camera position
        cameraZoomLevel = CameraZoomLevel.Medium;
        transform.position = startPosition.position;
    }
}

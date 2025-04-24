using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraObstacleFader : MonoBehaviour
{
    [SerializeField] private bool enable;

    [SerializeField] private LayerMask obstacleLayer;
    //[SerializeField] private float fadeValue = 0.5f;
    [SerializeField] private float distanceToFade = 10f;
    private Camera mainCamera;

    private List<GameObject> affectedObjects = new List<GameObject>();


    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enable) return;

        // Raycast from the center of the camera's view
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, distanceToFade))
        {
            // Save the hit location
            Vector3 hitLocation = hit.point;

            // Apply fade effect to all objects within a certain radius from the hit location
            Collider[] obstaclesInRange = Physics.OverlapSphere(hitLocation, 3.0f, obstacleLayer);

            // List of objects to be reset (those that are outside the range)
            List<GameObject> objectsToReset = new List<GameObject>();

            // Check each object in the current range
            foreach (Collider obstacle in obstaclesInRange)
            {
                // Get the material of the obstacle
                Material material = obstacle.GetComponent<MeshRenderer>().material;

                if (material != null)
                {
                    // Calculate the distance from the hit location
                    float distance = Vector3.Distance(hitLocation, obstacle.transform.position);

                    // If the distance is within the threshold, apply the dissolve effect
                    if (material.HasProperty("_DissolveAmount"))
                    {
                        float fadeValue = Mathf.Clamp01(1.0f - (distance / distanceToFade)); // Fade effect based on distance
                        material.SetFloat("_DissolveAmount", fadeValue);
                        material.SetInt("_IgnoreDistance", 1); // Optional: Ignore distance flag

                        // Add to the affected objects list if not already there
                        if (!affectedObjects.Contains(obstacle.gameObject))
                        {
                            affectedObjects.Add(obstacle.gameObject);
                        }
                    }
                    else
                    {
                        Debug.Log("Obstacle does not have dissolve shader! Is it supposed to fade?");
                    }
                }
                else
                {
                    Debug.Log("Material not found for obstacle: " + obstacle.name);
                }
            }

            // To reset the objects that are no longer in range, we collect them into a list
            List<GameObject> objectsToRemove = new List<GameObject>();

            // Check for objects in affectedObjects that are no longer in range
            foreach (GameObject affectedObj in affectedObjects)
            {
                bool isStillInRange = false;

                // Check if the object is still in range
                foreach (Collider obstacle in obstaclesInRange)
                {
                    if (obstacle.gameObject == affectedObj)
                    {
                        isStillInRange = true;
                        break;
                    }
                }

                if (!isStillInRange)
                {
                    // Reset material properties for the object that left the range
                    Material material = affectedObj.GetComponent<MeshRenderer>().material;
                    if (material != null)
                    {
                        material.SetInt("_IgnoreDistance", 0);
                    }

                    // Add the object to the list of objects to be removed
                    objectsToRemove.Add(affectedObj);
                }
            }

            // Remove the objects after iteration to avoid modifying the list during the loop
            foreach (GameObject obj in objectsToRemove)
            {
                affectedObjects.Remove(obj);
            }
        }
    }

    /*
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, distanceToFade, obstacleLayer.value))
        {
            if (lastHitObject == hit.collider.gameObject)
            {
                // Nothing
            }
            else
            {
                if (lastHitObject != null)
                {
                    Material lastMaterial = lastHitObject.GetComponent<MeshRenderer>().material;
                    if (lastMaterial != null)
                    {
                        lastMaterial.SetInt("_IgnoreDistance", 0);
                    }
                }

                lastHitObject = hit.collider.gameObject;
            }

            Material material = hit.collider.GetComponent<MeshRenderer>().material;
            
            if (Vector3.Distance(Camera.main.gameObject.transform.position, hit.collider.gameObject.transform.position) < 10.0f) return;

            if (material != null)
            {
                if (material.HasProperty("_DissolveAmount"))
                {
                    material.SetInt("_IgnoreDistance", 1);
                    material.SetFloat("_DissolveAmount", fadeValue);
                }
                else
                {
                    Debug.Log("Obstacle does not have dissolve shader! Is it supposed to fade?");
                }

            }
            else
            {
                Debug.Log("Material not found");
            }
            
        }
    }*/
}

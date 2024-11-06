using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracked : MonoBehaviour
{
    private ARTrackedImageManager trackedImages;
    public GameObject[] ArPrefabs;  // Array of 3D prefabs that correspond to the tracked images

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();  // To store instantiated prefabs

    void Awake()
    {
        trackedImages = GetComponent<ARTrackedImageManager>();

        // Initialize the dictionary and deactivate all prefabs
        foreach (var prefab in ArPrefabs)
        {
            var newPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            newPrefab.SetActive(false);  // Disable all prefabs initially

            // Add RotateObject script to each prefab so it can rotate when touched
            if (newPrefab.GetComponent<RotateObject>() == null)
            {
                newPrefab.AddComponent<RotateObject>();  // Add the rotation script if not present
            }

            spawnedPrefabs[prefab.name] = newPrefab;
        }
    }

    void OnEnable()
    {
        trackedImages.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImages.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Handle added images (newly detected images)
        foreach (var trackedImage in eventArgs.added)
        {
            UpdateARObject(trackedImage);
        }

        // Handle updated images (position or tracking state updates)
        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateARObject(trackedImage);
        }

        // Handle removed images (no longer in view)
        foreach (var trackedImage in eventArgs.removed)
        {
            // Hide the corresponding prefab when the image is no longer tracked
            if (spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
            {
                spawnedPrefabs[trackedImage.referenceImage.name].SetActive(false);
            }
        }
    }

    // Method to update the AR object based on tracked image
    private void UpdateARObject(ARTrackedImage trackedImage)
    {
        // Check if the tracked image's name matches any prefab's name
        if (spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            // Get the corresponding prefab for the tracked image
            var prefab = spawnedPrefabs[trackedImage.referenceImage.name];

            // If the image is being actively tracked, update its position and show it
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                prefab.SetActive(true);  // Show the object
                prefab.transform.position = trackedImage.transform.position;  // Update position to match the image
                prefab.transform.rotation = trackedImage.transform.rotation;  // Update rotation to match the image
            }
            else
            {
                // Hide the object when it's no longer actively tracked
                prefab.SetActive(false);
            }
        }
    }
}

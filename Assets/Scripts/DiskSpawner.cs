using System.Collections.Generic;
using UnityEngine;

public class DiskSpawner : MonoBehaviour
{
    // The prefab used to instantiate each disk
    public GameObject diskPrefab;
    // The parent transform of Rod A - disks spawn here
    public Transform rodA;
    // How many disks to spawn (set via slider or inspector)
    public int numberOfDisks = 3;
    // Optional array of materials to color each disk differently
    public Material[] diskMaterials;

    void Start()
    {
        // Spawn disks automatically when the game starts
        SpawnDisks();
    }
    // Called the GameManager when the slider changes disk count
    public void SetDiskCount(int count)
    {
        numberOfDisks = count;
    }

    public void SpawnDisks()
    {
        // Get the Rod script from Rod A's parent GameObject
        Rod rodAScript = rodA.GetComponent<Rod>();
        if (rodAScript == null)
        {
            Debug.LogError("Rod.cs not found on rodA");
            return;
        }
        // Clear any existing disks from the stack before spawning new ones
        rodAScript.disks = new Stack<GameObject>();
        // The Y scale of each disk (controls how tall/thin each disk is)
        float diskHeight = 0.2f;

        // Largest disk is always 3.0 units, smallest always 0.5 units
        // Fits comfortably within 3.5 - unit pole spacing regardless of disk count
        float maxDiskSize = 3.0f;
        float minDiskSize = 0.5f;

        for (int i = 0; i < numberOfDisks; i++)
        {
            // Create a new disk from the prefab
            GameObject disk = Instantiate(diskPrefab);
            // Calculate t: a 0-1 value where 0 = smallest disk, 1 = largest disk
            // When there is only 1 disk, t = 0 to avoid division by zero
            // As i increases (going up the stack), t decreases (smaller disks on top)
            float t = (numberOfDisks == 1) ? 0f : (float)(numberOfDisks - i - 1) / (numberOfDisks - 1);
            // Lerp between minDiskSize and maxDiskSize based on t
            // Bottom disk gets maxDiskSize, top disk gets minDiskSize
            float sizeFactor = Mathf.Lerp(minDiskSize, maxDiskSize, t);
            // Apply scale - X and Z control width, Y controls height
            disk.transform.localScale = new Vector3(sizeFactor, diskHeight, sizeFactor);
            // Parent the disk to Rod A so GetComponentInParent<Rod>() can find it
            disk.transform.SetParent(rodA, true);
            // Add disk to Rod A's stack - this also handles world position
            rodAScript.AddDisk(disk);
            // Assign a material from the array if one is provided
            // Uses modulo so materials cycle if there are fewer materials than disks
            if (diskMaterials != null && diskMaterials.Length > 0)
            {
                Renderer r = disk.GetComponent<Renderer>();
                if (r != null)
                    r.material = diskMaterials[i % diskMaterials.Length];
            }
        }
    }
}
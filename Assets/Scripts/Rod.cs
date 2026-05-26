using System.Collections.Generic;
using UnityEngine;

public class Rod : MonoBehaviour
{
    // Stack of disk GameObjects on this rod - top of stack = top-most disk
    public Stack<GameObject> disks = new Stack<GameObject>();
    // The world-space height of each disk (must match DiskSpawner's diskHeight)
    private const float DISK_HEIGHT = 0.2f;
    // Returns true if the given disk is allowed to be placed on this rod
    // Rules: rod must be empty, or the incoming disk must be smaller than the current
    // top disk
    public bool CanPlace(GameObject disk)
    {
        if (disks.Count == 0) return true;
        return disk.transform.localScale.x < disks.Peek().transform.localScale.x;
    }
    // Places a disk onto this rod - handles parenting, positioning, and colliders
    public void AddDisk(GameObject disk)
    {
        // Re-parent the disk to this rod's transform
        // True = preserve world scale so parenting doesn't resize the disk
        disk.transform.SetParent(this.transform, true);
        // stackIndex = how many disks are already here before adding this one
        // Used to calculate the correct vertical position
        int stackIndex = disks.Count;
        // Stack disks from Y=0 upward:
        // First disk center is at DISK_HEIGHT (0.2), each subsequent disk is 2x DISK_HEIGHT higher
        float worldY = DISK_HEIGHT + stackIndex * (DISK_HEIGHT * 2f);
        // Position the disk centered on this rod's X/Z, at the correct height
        disk.transform.position = new Vector3(
            this.transform.position.x,
            worldY,
            this.transform.position.z
        );
        // Push onto the stack - top of stack is always the uppermost disk
        disks.Push(disk);
        // Refresh colliders so only the top disk is clickable
        UpdateColliders();
    }
    // Removes and returns the top disk from this rod
    public GameObject RemoveDisk()
    {
        if (disks.Count == 0) return null;
        // Pop the top disk off the stack
        GameObject removed = disks.Pop();
        // Detach from this rod's hierarchy so it can be re-parented to another rod
        removed.transform.SetParent(null);
        // Refresh colliders so the new top disk becomes clickable
        UpdateColliders();
        return removed;
    }
    // Ensures only the top disk on this rod has an active collider
    // This enforces the rule that only the uppermost disk can be selected
    private void UpdateColliders()
    {
        // Disable colliders on all disks on this rod
        foreach (GameObject d in disks)
        {
            Collider col = d.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
        // Re-enable only the top disk's collider so it can be clicked
        if (disks.Count > 0)
        {
            Collider topCol = disks.Peek().GetComponent<Collider>();
            if (topCol != null) topCol.enabled = true;
        }
    }
}

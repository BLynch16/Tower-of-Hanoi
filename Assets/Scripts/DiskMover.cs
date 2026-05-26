using UnityEngine;

public class DiskMover : MonoBehaviour
{
    // The disk the player has currently selected (null if none selected)
    private GameObject selectedDisk = null;
    // The rod that the selected disk came from
    private Rod selectedDiskRod = null;
    // Reference to GameManager for registering moves and checking win condition
    private GameManager gameManager;

    void Start()
    {
        // Find the GameManager in the scene automatically
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        // Check for left mouse click every frame
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        // Cast a ray from the camera through the mouse position into the scene
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // If the ray doesn't hit anything, do nothing
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;
        // Walk up the hierarchy from the hit object to find a Rod component
        // This works whether the player clicks a disk or the rod cylinder itself
        Rod clickedRod = hit.collider.GetComponentInParent<Rod>();
        // If the clicked object isn't part of a rod, do nothing
        if (clickedRod == null)
            return;

        // -- Pick up disk --
        // If no disk is currently selected, try to pick one up from the clicked rod
        if (selectedDisk == null)
        {
            // Can't pick up from an empty rod
            if (clickedRod.disks.Count == 0)
                return;
            // Select the top disk on this rod (Peek = look at top without removing)
            selectedDisk = clickedRod.disks.Peek();
            selectedDiskRod = clickedRod;
            return;
        }

        // -- Cancel selection --
        // If the player clicks the same rod the disk came from, deselect it
        if (clickedRod == selectedDiskRod)
        {
            selectedDisk = null;
            selectedDiskRod = null;
            return;
        }

        // -- Attempt move --
        // Check if the selected disk can legally be placed on the clicked rod
        // CanPlace returns false if the moving disk is larger than the top disk on the target rod
        if (clickedRod.CanPlace(selectedDisk))
        {
            // Remove the disk from its current rod
            selectedDiskRod.RemoveDisk();
            // Place it on the new rod (handles positioning and parenting)
            clickedRod.AddDisk(selectedDisk);
            // Notify GameManager to increment move counter and check for win
            if (gameManager != null)
            {
                gameManager.RegisterMove();
                gameManager.CheckForWin();
            }
            // Clear selection after a successful move
            selectedDisk = null;
            selectedDiskRod = null;
        }
        // If CanPlace returns false, the move is illegal - selection stays active
        // so the player can try clicking a different rod
    }
}

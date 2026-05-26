using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSolver : MonoBehaviour
{
    // The three rods - assign in inspector
    // rodA = starting rod, rodB = auxiliary rod, rodC = destination rod
    public Rod rodA;
    public Rod rodB;
    public Rod rodC;
    // Time in seconds between each AI move - controls solve animation speed
    public float moveDelay = 0.5f;

    private bool solving = false; // Prevents the solver from being started multiple times simultaneously
    private GameManager gameManager; // Reference to GameManager to read disk count and register moves

    void Start()
    {
        // Find GameManager in the scene automatically
        gameManager = FindFirstObjectByType<GameManager>();
    }
    // Called by GameManager.OnSolveClicked - starts the coroutine if not already solving
    public void SolveFromStart()
    {
        if (!solving)
            StartCoroutine(SolveFromStartCoroutine());
    }

    private IEnumerator SolveFromStartCoroutine()
    {
        solving = true;
        // Get the current disk count from GameManager
        int n = gameManager.numberOfDisks;
        // Generate the complete optimal move list using recursive Hanoi
        // All disks move from rodA to finalRod using rodB as the auxiliary
        List<(Rod from, Rod to)> moves = new List<(Rod, Rod)>();
        GenerateMoves(n, rodA, gameManager.finalRod, rodB, moves);
        // Execute each move on the real board with a delay between each
        foreach (var (from, to) in moves)
        {
            // Safety check - skip if the source rod is somehow empty
            if (from.disks.Count == 0) continue;
            // Peek at the top disk without removing it yet
            GameObject disk = from.disks.Peek();
            // Safety check - skip if the move would violate Tower of Hanoi rules
            if (!to.CanPlace(disk)) continue;
            // Perform the move
            from.RemoveDisk();
            to.AddDisk(disk);
            // Register the move and check if the puzzle is complete
            gameManager.RegisterMove();
            gameManager.CheckForWin();
            // Wait before the next move so the player can watch
            yield return new WaitForSeconds(moveDelay);
        }

        solving = false;
    }
    // Classic recursive Tower of Hanoi algorithm
    // Generates the optimal move sequence to move n disks from 'from' to 'to' using
    // 'aux' the sequence is always (2^n) - 1 moves [the mathematical minimum]
    // How it works:
    //    1. Move the top n-1 disks out of the way (from -> aux)
    //    2. Move the largest disk to the destination (from -> to)
    //    3. Move the n-1 disks from aux onto the destination (aux -> to)
    void GenerateMoves(int n, Rod from, Rod to, Rod aux, List<(Rod, Rod)> moves)
    {
        // Base case: no disks to move
        if (n == 0) return;
        // Step 1: Move n-1 disks from source to auxiliary (destination acts as temp)
        GenerateMoves(n - 1, from, aux, to, moves);
        // Step 2: Move the nth (largest remaining) disk directly to destination
        moves.Add((from, to));
        // Step 3: Move the n-1 disks from auxiliary to destination (source acts as temp)
        GenerateMoves(n - 1, aux, to, from, moves);
    }
}
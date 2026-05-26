using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Number of disks in the current game (controlled by slider)
    public int numberOfDisks = 3;
    // The rod that all disks must reach to win (assigned in inspector)
    public Rod finalRod;
    // UI Text references
    public Text movesText; // Displays current move count
    public Text minMovesText; // Displays the minimum possible moves
    public Text timerText; // Displays elapsed time
    public Text diskCountText; // Displays current disk count from slider
    // The solve button
    public Button solveButton;
    // Tracks how many moves the player has made
    private int moves = 0;
    // Tracks elapsed time in seconds
    private float timer = 0f;
    // Controls whether the timer is counting up
    private bool timerRunning = false;
    // Reference to the TowerSolver for the AI solve feature
    private TowerSolver solver;

    void Start()
    {
        // Find TowerSolver in the scene automatically
        solver = FindFirstObjectByType<TowerSolver>();
        // Initialize all UI elements with starting values
        UpdateUI();
    }

    void Update()
    {
        // Increment timer every frame while it's running
        if (timerRunning)
        {
            timer += Time.deltaTime;
            // Update timer display, formatted to 1 decimal place
            if (timerText != null)
                timerText.text = "Time: " + timer.ToString("F1");
        }
    }
    // Updates all UI elements to reflect current game state
    void UpdateUI()
    {
        // Minimum moves for Tower of Hanoi is always (2^n)-1
        int minMoves = (int)Mathf.Pow(2, numberOfDisks) - 1;
        if (minMovesText != null)
            minMovesText.text = "Minimum Moves: " + minMoves;
        if (movesText != null)
            movesText.text = "Moves: 0";
        if (diskCountText != null)
            diskCountText.text = "Disks: " + numberOfDisks;
    }
    // Called by DiskMover after every legal move
    public void RegisterMove()
    {
        // Start the timer on the player's first move
        if (!timerRunning)
            timerRunning = true;

        moves++;
        if (movesText != null)
            movesText.text = "Moves: " + moves;
    }
    // Stops the timer - called when the player wins
    public void StopTimer()
    {
        timerRunning = false;
    }
    // Called after every move to check if the player has won
    public void CheckForWin()
    {
        // Win condition: all disks are stacked on the final rod
        if (finalRod != null && finalRod.disks.Count == numberOfDisks)
        {
            StopTimer();
            Debug.Log("You won! Congratulations!");
        }
    }
    // Called by the slider's OnValueChanged event in the inspector
    public void OnSliderChanged(float value)
    {
        // Round the float slider value to the nearest integer
        numberOfDisks = Mathf.RoundToInt(value);
        if (diskCountText != null)
            diskCountText.text = "Disks: " + numberOfDisks;
        // Recalculate and display the new minimum moves for this disk count
        int minMoves = (int)Mathf.Pow(2, numberOfDisks) - 1;
        if (minMovesText != null)
            minMovesText.text = "Minimum Moves: " + minMoves;
    }
    // Called by the Reset button - clears the board and respawns disks
    public void ResetGame()
    {
        // Reset timer
        timer = 0f;
        timerRunning = false;
        if (timerText != null)
            timerText.text = "Time: 0.0";
        // Reset move counter
        moves = 0;
        if (movesText != null)
            movesText.text = "Moves: 0";
        // Refresh all UI labels
        UpdateUI();
        // Destroy every disk currently on every rod
        Rod[] rods = FindObjectsByType<Rod>(FindObjectsSortMode.None);
        foreach (Rod rod in rods)
        {
            while (rod.disks.Count > 0)
            {
                GameObject d = rod.RemoveDisk();
                Destroy(d);
            }
        }
        // Tell the spawner how many disks to create and respawn them
        DiskSpawner spawner = FindFirstObjectByType<DiskSpawner>();
        if (spawner != null)
        {
            spawner.SetDiskCount(numberOfDisks);
            spawner.SpawnDisks();
        }
    }
    // Called by the Solve button in the inspector
    public void OnSolveClicked()
    {
        StartCoroutine(ResetThenSolve());
    }
    // Resets the board, waits briefly so the player can see the starting state
    // Then hands control to the AI solver
    private IEnumerator ResetThenSolve()
    {
        // Clear the board and respawn all disks on Rod A
        ResetGame();
        // Pause for 1 second so the player can see all disks on Rod A
        // before the AI starts moving them
        yield return new WaitForSeconds(1.0f);
        // Start the AI solver
        if (solver != null)
            solver.SolveFromStart();
    }
}
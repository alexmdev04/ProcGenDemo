using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGen : MonoBehaviour
{
    public static MazeGen instance { get; private set; }
    public bool 
        refresh,
        debugCorrectPath,
        debugPathSlow;
    public float
        debugPathSlowSpeed = 1f,
        debugBacktrackCount = 0f;
    public Vector3Int 
        mazeSize = Vector3Int.one * 4;
    public Dictionary<Vector3Int, MazePiece> 
        mazePiecesLookup = new();
    public GameObject
        mazePiecePrefab;
    public int 
        mazePieceSize = 10;
    public List<MazePiece>
        mazePieces = new(),
        mazeCorrectPath = new();
    public MazePiece
        mazePieceDefault = new();
    [SerializeField] GameObject
        floor;
    int 
        minimumFinalPathLength = 10;

    public static Vector3Int[] directions = new Vector3Int[4] 
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.right
    };
    void Awake()
    {
        instance = this;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) || refresh)
        {
            refresh = false;
            ui.instance.uiFadeAlphaSet(1);
            MazeGenerate();
            Player.instance.TeleportInstant(
                new Vector3(5f, 1f, 5f),
                new Vector3(0f, mazePiecesLookup[Vector3Int.zero].debugDirection.VectorNormalToCardinal().Euler(), 0f));
        }
    }
    void MazeGenerate()
    {
        System.Diagnostics.Stopwatch timer = new();
        timer.Start();

        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(mazeSize.x * 10, 1f, mazeSize.z * 10);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);

        MazeGridReset();
        MazeGridNew();
        GenerateCorrectPath();
        GenerateRemainingMaze();
        MazeRenderer.instance.Reset_();
        MazeRenderer.instance.UpdateGrid();

        timer.Stop();
        Debug.Log("Maze Generation + Initial Render = " + timer.Elapsed.TotalMilliseconds + "ms");
    }
    void MazeGridReset()
    {
        debugBacktrackCount = 0;
        minimumFinalPathLength = (mazeSize.x * mazeSize.z) / 4;
        foreach (Transform loadedMazePieceObject in transform) { Destroy(loadedMazePieceObject.gameObject); }
        mazePieces.Clear();
        mazePiecesLookup.Clear();
        mazeCorrectPath.Clear();
    }
    void MazeGridNew()
    {
        // CREATES AN EMPTY GRID
        for (int x = 0; x < mazeSize.x; x++)
        {
            for (int z = 0; z < mazeSize.z; z++)
            {
                // CREATES EMPTY MazePiece CLASS AND ADDS IT TO THE DICTIONARY
                MazePiece mazePieceNewComponent = new();
                Vector3Int mazePieceGridPosition = new(x, 0, z);
                mazePieceNewComponent.gridPosition = mazePieceGridPosition;
                mazePieces.Add(mazePieceNewComponent);
                mazePiecesLookup.Add(mazePieceGridPosition, mazePieceNewComponent);
            }
        }
        // SETS EDGE PIECES AND GETS ADJACENT PIECES
        foreach (MazePiece mazePiece in mazePieces) { mazePiece.Refresh(); }
    }
    void GenerateCorrectPath()
    {
        // SETS THE START OF THE MAZE
        MazePiece startingPiece = mazePiecesLookup[Vector3Int.zero];
        startingPiece.passed = true;
        MazePiece currentPiece = NextInPath(startingPiece, true);
        int piecesPassed = 0;

        NextPiece:

        // CHECK FOR EDGE PIECE AND MINIMUM PATH LENGTH
        if ((currentPiece.gridPosition.x == 0
            || currentPiece.gridPosition.x == mazeSize.x
            || currentPiece.gridPosition.z == 0
            || currentPiece.gridPosition.z == mazeSize.z)
            && piecesPassed > minimumFinalPathLength)
        {
            // SUCCESSFUL PATH
            // currentPiece == EXIT
            currentPiece.OpenDirection(-currentPiece.fromDirection);
            return;
        }
        // NEXT PIECE IN PATH
        currentPiece = NextInPath(currentPiece, true);
        mazeCorrectPath.Add(currentPiece);
        piecesPassed++;
        goto NextPiece;
    }
    MazePiece NextInPath(MazePiece currentPiece, bool isCorrectPath = false)
    {
        //currentPiece.debugBoxColor = Color.red;

        Backtrack:
        // GETS DIRECTIONS THE PATH CAN GO
        List<Vector3Int> availableDirections = new();
        for (int i = 0; i < currentPiece.adjacentPieces.Count; i++)
        {
            if (currentPiece.adjacentPieces[i] == null) { continue; }
            if (!currentPiece.adjacentPieces[i].passed) availableDirections.Add(directions[i]);
        }
        
        // CHECK FOR DEAD END
        if (availableDirections.Count == 0)
        {
            if (!isCorrectPath) { return null; }   
            // RECURSIVE BACKTRACKING
            mazeCorrectPath.RemoveAt(mazeCorrectPath.Count - 1);
            currentPiece = mazeCorrectPath[^1];
            debugBacktrackCount++;
            goto Backtrack;
        }

        // GOES TO A RANDOM PIECE WITHIN THE AVAILABLE DIRECTIONS
        Vector3Int randomDirection = availableDirections[Game.instance.random.Next(0, availableDirections.Count)];
        MazePiece nextPiece = mazePiecesLookup[currentPiece.gridPosition + randomDirection];

        // OPENS THE PATHWAY BETWEEN THE PIECES
        currentPiece.OpenDirection(randomDirection);
        if (isCorrectPath) { currentPiece.debugDirection = randomDirection; }
        nextPiece.OpenDirection(-randomDirection);
        nextPiece.passed = true;
        nextPiece.fromDirection = -randomDirection;
        return nextPiece;
    }
    void GenerateRemainingMaze()
    {
        // STARTS NEW "SPINE" PATHS FROM EACH MazePiece (THAT HAVE 2 AVAILABLE DIRECTIONS) UNTIL THE MAZE IS FULL
        List<MazePiece> mazePiecesWith2AvailableDirections = new();
        foreach (MazePiece mazePiece in mazeCorrectPath)
        {
            int adjacentsNotPassed = 4;
            foreach (MazePiece adjacentPiece in mazePiece.adjacentPieces)
            {
                if (adjacentPiece == null) { continue; }
                if (adjacentPiece.passed) { adjacentsNotPassed--; }
            }
            if (adjacentsNotPassed >= 1)
            {
                mazePiecesWith2AvailableDirections.Add(mazePiece);
            }
        }

        NextSpinePath:
        if (mazePiecesWith2AvailableDirections.Count == 0) { return; }
        MazePiece randomStartPiece = mazePiecesWith2AvailableDirections[Game.instance.random.Next(0, mazePiecesWith2AvailableDirections.Count)];
        MazePiece currentPiece = NextInPath(randomStartPiece);
        mazePiecesWith2AvailableDirections.Remove(randomStartPiece);

        NextPiece:
        if (currentPiece == null) { goto NextSpinePath; } // DEAD END SPINE PATH
        currentPiece = NextInPath(currentPiece);
        goto NextPiece;
    }
    public void ToggleDebugCorrectPath()
    {
        // DISPLAYS THE CORRECT PATH OF THE MAZE USING ARROWS AND DEBUG BOXES
        debugCorrectPath = !debugCorrectPath;
        mazeCorrectPath.ForEach(mazePiece => mazePiece.debug = debugCorrectPath);
        if (debugCorrectPath) { mazeCorrectPath[^1].debugBoxColor = Color.cyan; }
    }
}
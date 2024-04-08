using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    void Start()
    {
        MazeGenerate();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) || refresh)
        {
            refresh = false;
            MazeGenerate();
        }
    }
    void MazeGenerate()
    {
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();

        // RESETTING THE GRID
        debugBacktrackCount = 0;
        minimumFinalPathLength = (mazeSize.x * mazeSize.z) / 4;
        foreach (Transform loadedMazePieceObject in transform) { Destroy(loadedMazePieceObject.gameObject); }
        mazePieces.Clear();
        mazePiecesLookup.Clear();
        mazeCorrectPath.Clear();

        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(mazeSize.x * 10, 1f, mazeSize.z * 10);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);

        // GENERATES AN EMPTY MAZE WITH EDGES
        MazeNew();

        // 
        GenerateCorrectPath();

        // 
        GenerateRemainingMaze();

        stopwatch.Stop();
        Debug.Log("Maze generation time: " + stopwatch.Elapsed.TotalMilliseconds + "ms");

        // 
        MazeRenderer.instance.UpdateGrid();
    }
    void MazeNew()
    {
        for (int x = 0; x < mazeSize.x; x++)
        {
            for (int z = 0; z < mazeSize.z; z++)
            {
                MazePiece mazePieceNewComponent = new();
                Vector3Int mazePieceGridPosition = new Vector3Int(x, 0, z);
                mazePieceNewComponent.gridPosition = mazePieceGridPosition;
                mazePieces.Add(mazePieceNewComponent);
                mazePiecesLookup.Add(mazePieceGridPosition, mazePieceNewComponent);
            }
        }
        foreach (MazePiece mazePiece in mazePieces) { mazePiece.Refresh(); }
    }
    void GenerateCorrectPath()
    {
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
        currentPiece.debugBoxColor = Color.red;

        Backtrack:
        List<Vector3Int> availableDirections = new();
        for (int i = 0; i < currentPiece.adjacentPieces.Count; i++)
        {
            if (currentPiece.adjacentPieces[i] == null) { continue; }
            if (!currentPiece.adjacentPieces[i].passed) availableDirections.Add(directions[i]);
        }
        if (availableDirections.Count == 0)
        {
            // RECURSIVE BACKTRACKING
            if (!isCorrectPath) { return null; }      
            mazeCorrectPath.RemoveAt(mazeCorrectPath.Count - 1);
            currentPiece = mazeCorrectPath[^1];
            debugBacktrackCount++;
            goto Backtrack;
        }
        Vector3Int randomDirection = availableDirections[Game.instance.random.Next(0, availableDirections.Count)];
        MazePiece nextPiece = mazePiecesLookup[currentPiece.gridPosition + randomDirection];

        currentPiece.OpenDirection(randomDirection);
        if (isCorrectPath) { currentPiece.debugDirection = randomDirection; }
        nextPiece.OpenDirection(-randomDirection);
        nextPiece.passed = true;
        nextPiece.fromDirection = -randomDirection;
        return nextPiece;
    }
    void GenerateRemainingMaze()
    {

        // 

        foreach (MazePiece mazePiece in mazePieces)
        {
            int adjacentsNotPassed = 4;
            foreach (MazePiece adjacentPiece in mazePiece.adjacentPieces)
            {
                if (adjacentPiece == null) { continue; }
                if (adjacentPiece.passed) { adjacentsNotPassed--; }
            }
            if (adjacentsNotPassed >= 2)
            {
                MazePiece currentPiece = NextInPath(mazePiece);

                NextPiece:
                if (currentPiece == null) { continue; } // DEAD END SPINE PATH
                currentPiece = NextInPath(currentPiece);
                goto NextPiece;
            }
        }
    }
    public void ToggleDebugCorrectPath()
    {
        debugCorrectPath = !debugCorrectPath;
        mazeCorrectPath.ForEach(mazePiece => mazePiece.debug = debugCorrectPath);
        if (debugCorrectPath) { mazeCorrectPath[^1].debugBoxColor = Color.cyan; }
    }
}
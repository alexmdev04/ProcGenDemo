using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MazeRenderer))]
public class MazeGen : MonoBehaviour
{
    public static MazeGen instance { get; private set; }
    public bool 
        refresh,
        mazeRenderAuto = true;
    public Vector3Int 
        mazeSize = Vector3Int.one * 10;
    public Dictionary<Vector3Int, MazePiece> 
        mazePiecesLookup = new();
    public GameObject
        mazePiecePrefab;
    public int 
        mazePieceSize = 10,
        mazeSizeCount,
        goalMinimumDistanceFromStart;
    public MazePiece[]
        mazePieces;
    public MazePiece
        mazePieceDefault { get; private set; } = new();
    public GameObject
        floor;
    MazePiece
        startingPiece;
    List<MazePiece> 
        mazeCurrentPath = new();
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
            Player.instance.TeleportInstant((startingPiece.gridPosition * mazePieceSize) + new Vector3(5f, 1.15f, 5f),
                new Vector3(0f, startingPiece.debugDirection.VectorNormalToCardinal().Euler(), 0f));
        }
    }
    void MazeGenerate()
    {
        if (mazePiecePrefab == null) { Debug.LogError("Maze Piece Prefab not assigned, check Assets/Models/"); return; }

        System.Diagnostics.Stopwatch timer = new();
        timer.Start();

        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(mazeSize.x * 10, 1f, mazeSize.z * 10);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);

        MazeGridReset();
        MazeGridNew();
        MazeAlgorithm();
        MazeRenderer.instance.Reset();
  
        timer.Stop();
        Debug.Log("Maze Generation Time = " + timer.Elapsed.TotalMilliseconds + "ms");

        if (startingPiece.gridPosition == Player.instance.gridPosition) { MazeRenderer.instance.MazeRenderUpdate(); }
    }
    void MazeGridReset()
    {
        mazeSizeCount = mazeSize.x * mazeSize.z;
        foreach (Transform loadedMazePieceObject in transform) { Destroy(loadedMazePieceObject.gameObject); }
        //mazePieces.Clear();
        mazePieces = new MazePiece[mazeSizeCount];
        mazePiecesLookup.Clear();
    }
    void MazeGridNew()
    {
        int index = 0;
        // CREATES AN EMPTY GRID
        for (int x = 0; x < mazeSize.x; x++)
        {
            for (int z = 0; z < mazeSize.z; z++)
            {
                // CREATES EMPTY MazePiece CLASS AND ADDS IT TO THE DICTIONARY
                MazePiece mazePieceNewComponent = new();
                Vector3Int mazePieceGridPosition = new(x, 0, z);
                mazePieceNewComponent.gridPosition = mazePieceGridPosition;
                mazePieces[index] = mazePieceNewComponent;
                index++;
                mazePiecesLookup.Add(mazePieceGridPosition, mazePieceNewComponent);
            }
        }
        // SETS EDGE PIECES AND GETS ADJACENT PIECES
        foreach (MazePiece mazePiece in mazePieces) { mazePiece.Refresh(); }
    }
    void MazeAlgorithm()
    {
        // SETS THE START OF THE MAZE
        startingPiece = mazePiecesLookup[new Vector3Int(Game.instance.random.Next(mazeSize.x), 0, Game.instance.random.Next(mazeSize.z))];
        startingPiece.passed = true;
        startingPiece.debugBoxColor = Color.green;
        startingPiece.debug = true;
        mazeCurrentPath.Add(startingPiece);
        MazePiece currentPiece = NextInPath(startingPiece);
        startingPiece.debugDirection = -currentPiece.fromDirection;

        NextPiece:
        // CHECK IF THE ALGORITHM HAS BACK TRACKED TO THE START
        if (currentPiece != startingPiece)
        {
            // NEXT PIECE IN PATH
            mazeCurrentPath.Add(currentPiece);
            currentPiece = NextInPath(currentPiece);
            goto NextPiece;
        }

        // END OF MAZE GENERATION
        int minDiff = goalMinimumDistanceFromStart;
        NewEndPiece:
        MazePiece[] potentialEndPieces = mazePieces
            .Where(a => a.walls.Count(wallActive => wallActive) == 3)
            .Where(b => Extensions.Diff(b.gridPosition.x, startingPiece.gridPosition.x) > minDiff 
                     && Extensions.Diff(b.gridPosition.z, startingPiece.gridPosition.z) > minDiff).ToArray();
        if (potentialEndPieces.Length == 0) { minDiff--; goto NewEndPiece; }
        Vector3Int mazeExitPosition = potentialEndPieces[Game.instance.random.Next(potentialEndPieces.Length)].gridPosition;
        MazePiece mazeExit = mazePiecesLookup[mazeExitPosition];
        mazeExit.passed = true;
        mazeExit.debugBoxColor = Color.red;
        mazeExit.debug = true;
        Debug.Log("New Maze Complete, Start @ " + startingPiece.gridPosition + ", Exit @ " + mazeExit.gridPosition);
        return;
    }
    MazePiece NextInPath(MazePiece currentPiece)
    {
        Backtrack:
        // GETS DIRECTIONS THE PATH CAN GO
        List<Vector3Int> availableDirections = currentPiece.AvailableDirections();
        
        // CHECK FOR DEAD END
        if (availableDirections.Count == 0)
        {
            // RECURSIVE BACKTRACKING
            mazeCurrentPath.RemoveAt(mazeCurrentPath.Count - 1);
            if (mazeCurrentPath.Count == 0) { return currentPiece; }
            currentPiece = mazeCurrentPath[^1];
            goto Backtrack;
        }

        // GOES TO A RANDOM PIECE WITHIN THE AVAILABLE DIRECTIONS
        Vector3Int randomDirection = availableDirections[Game.instance.random.Next(availableDirections.Count)];
        MazePiece nextPiece = mazePiecesLookup[currentPiece.gridPosition + randomDirection];

        // OPENS THE PATHWAY BETWEEN THE PIECES
        currentPiece.OpenDirection(randomDirection);
        nextPiece.OpenDirection(-randomDirection);
        nextPiece.passed = true;
        nextPiece.fromDirection = -randomDirection;
        return nextPiece;
    }
}
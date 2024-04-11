using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    public List<MazePiece>
        mazePieces = new();
    public MazePiece
        mazePieceDefault { get; private set; } = new();
    public GameObject
        floor;
    MazePiece
        startingPiece;
    List<MazePiece> 
        mazeCurrentPath = new(),
        deadEnds = new();
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
        if (mazePiecePrefab is null) { Debug.LogError("Maze Piece Prefab not assigned, check Assets/Models/"); return; }

        System.Diagnostics.Stopwatch timer = new();
        timer.Start();

        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(mazeSize.x * 10, 1f, mazeSize.z * 10);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);

        MazeGridReset();
        MazeGridNew();
        MazeAlgorithm();
        MazeRenderer.instance.Reset_();
        //MazeRenderer.instance.
        //MazeRenderer.instance.UpdateGrid();
        
        timer.Stop();
        Debug.Log("Maze Generation + Initial Render = " + timer.Elapsed.TotalMilliseconds + "ms");
    }
    void MazeGridReset()
    {
        foreach (Transform loadedMazePieceObject in transform) { Destroy(loadedMazePieceObject.gameObject); }
        mazePieces.Clear();
        mazePiecesLookup.Clear();
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
        mazeSizeCount = mazePieces.Count;
        // SETS EDGE PIECES AND GETS ADJACENT PIECES
        foreach (MazePiece mazePiece in mazePieces) { mazePiece.Refresh(); }
    }
    void MazeAlgorithm()
    {
        // SETS THE START OF THE MAZE
        startingPiece = mazePiecesLookup[new Vector3Int(Game.instance.random.Next(mazeSize.x), 0, Game.instance.random.Next(mazeSize.z))];
        startingPiece.debug = true;
        startingPiece.debugBoxColor = Color.green;
        startingPiece.passed = true;
        mazeCurrentPath.Add(startingPiece);
        MazePiece currentPiece = NextInPath(startingPiece);

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
        MazePiece mazeExit = deadEnds[Game.instance.random.Next(deadEnds.Count)];
        Debug.Log("start @ " + startingPiece.gridPosition + ", exit @ " + mazeExit.gridPosition);
        mazeExit.debug = true;
        mazeExit.debugBoxColor = Color.red;
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
            if (mazeCurrentPath.Count == 0) 
            { 
                deadEnds.Add(currentPiece);
                return currentPiece;
            }
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
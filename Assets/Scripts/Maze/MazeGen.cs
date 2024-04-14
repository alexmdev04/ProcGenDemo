using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEditor;
using System;

[RequireComponent(typeof(MazeRenderer))]
public class MazeGen : MonoBehaviour
{
    public static MazeGen instance { get; private set; }
    public bool 
        refresh,
        mazeRenderAuto = true;
    public GameObject
        mazePiecePrefab;
    public int 
        mazeSizeX = 10,
        mazeSizeZ = 10,
        mazePieceSize = 10,
        mazeSizeCount; 
    public MazePiece[]
        mazePieceGrid;
    public MazePiece
        mazePieceDefault { get; private set; } = new();
    public GameObject
        floor;
    public int 
        iterations = 0;
    int
        goalMinimumDistanceFromStart;
    MazePiece
        startPiece,
        exitPiece;
    List<int[]> 
        endPieces = new();
    List<MazePiece> 
        mazeCurrentPath = new();
    public static int[][] directions = new int[4][]
    {
        new int[2]{ 0, 1 },
        new int[2]{ 0 , -1 },
        new int[2]{ -1 , 0 },
        new int[2]{ 1 , 0 }
    };
    const string 
        str_mazeGenTime = "Maze Generation Time = ",
        str_prefabError = "Maze Piece Prefab not assigned, check Assets/Models/",
        str_ms = "ms";
    void Awake()
    {
        instance = this;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) | refresh)
        {
            refresh = false;
            Reset();
        }
        if (Player.instance.gridIndex.Equals(exitPiece?.gridIndex)) { Win(); }
    }
    public void Reset()
    {
        ui.instance.uiFadeAlphaSet(1);
        MazeGenerate();
        Player.instance.TeleportInstant(startPiece.gridIndex.Multiply(mazePieceSize).ToVector() + new Vector3(5f, 1.15f, 5f),
            new Vector3(0f, startPiece.toDirection.ToVector().VectorNormalToCardinal().Euler(), 0f));
        ui.instance.settings.resetMessage.SetActive(false);
    }
    void MazeGenerate()
    {
        if (mazePiecePrefab == null) { Debug.LogError(str_prefabError); return; }

        System.Diagnostics.Stopwatch timer = new();
        timer.Start();

        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(mazeSizeX * 10, 1f, mazeSizeZ * 10);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);

        goalMinimumDistanceFromStart = Mathf.CeilToInt(mazeSizeCount * 0.75f);

        mazeSizeCount = mazeSizeX * mazeSizeZ;
        MazeGridNew();
        //MazeGridSet(mazeSizeCount);
        MazeAlgorithm();
  
        timer.Stop();
        uiMessage.instance.New(new StringBuilder(str_mazeGenTime).Append(timer.Elapsed.TotalMilliseconds).Append(str_ms).ToString());

        MazeRenderer.instance.Reset();
        if (startPiece.gridIndex.EqualTo(Player.instance.gridIndex)) { MazeRenderer.instance.MazeRenderUpdate(); }
    }
    void MazeGridNew()
    {
        endPieces.Clear();
        mazePieceGrid = new MazePiece[mazeSizeCount];
        int index = 0;
        // CREATES AN EMPTY GRID
        for (int x = 0; x < mazeSizeX; x++)
        {           
            for (int z = 0; z < mazeSizeZ; z++)
            {
                // CREATES EMPTY MazePiece CLASS AND ADDS IT TO THE DICTIONARY
                MazePiece mazePieceNewComponent = new();
                int[] mazePieceGridIndex = new int[2]{ x, z };
                mazePieceNewComponent.gridIndex = mazePieceGridIndex;
                mazePieceNewComponent.EdgeCheck();
                mazePieceGrid[index] = mazePieceNewComponent;
                index++;
            }
        }
        // SETS EDGE PIECES AND GETS ADJACENT PIECES
        // START ADJACENT PIECE CHAIN
        for (int i = 0; i < mazePieceGrid.Length; i++)
        {
            if (TryGetMazePiece(mazePieceGrid[i].gridIndex.Plus(directions[0]), out MazePiece up))
            {
                mazePieceGrid[i].adjacentPieces[0] = up;
                up.adjacentPieces[1] = mazePieceGrid[i];
            }
            if (TryGetMazePiece(mazePieceGrid[i].gridIndex.Plus(directions[3]), out MazePiece right))
            {
                mazePieceGrid[i].adjacentPieces[3] = right;
                right.adjacentPieces[2] = mazePieceGrid[i];
            }
        }
    }
    // void MazeGridSet(int amount)
    // {
    //     endPieces.Clear();
    //     bool newMazePieceGrid = mazePieceGrid == null;
    //     int oldMazeGridCount = newMazePieceGrid ? 0 : mazePieceGrid.Length;
    //     int[] lastPieceGridIndex = newMazePieceGrid ? new int[2] { 0, 0 } : mazePieceGrid[^1].gridIndex;
    //     if (amount >= oldMazeGridCount) // EXPAND POOL
    //     {
    //         if (newMazePieceGrid)
    //         {
    //             mazePieceGrid = new MazePiece[amount];
    //         }
    //         Array.Resize(ref mazePieceGrid, amount);
    //         int index = oldMazeGridCount - 1;
    //         for (int x = lastPieceGridIndex[0]; x < mazeSizeX; x++)
    //         {           
    //             for (int z = lastPieceGridIndex[1]; z < mazeSizeZ; z++)
    //             {
    //                 // CREATES EMPTY MazePiece CLASS AND ADDS IT TO THE DICTIONARY
    //                 MazePiece mazePieceNewComponent = new();
    //                 int[] mazePieceGridIndex = new int[2]{ x, z };
    //                 mazePieceNewComponent.gridIndex = mazePieceGridIndex;
    //                 mazePieceNewComponent.EdgeCheck();
    //                 mazePieceGrid[index] = mazePieceNewComponent;
    //                 index++;
    //             }
    //         }
    //     }
    //     else if (amount < oldMazeGridCount) // SHRINK POOL
    //     {
    //         for (int i = 0; i < mazePieceGrid[(amount - 1)..].Length; i++)
    //         {
    //             mazePieceGrid[i] = null;
    //         }
    //         mazePieceGrid = mazePieceGrid[..(amount - 1)];
    //     }
    //     else { return; }
    // }
    void MazeAlgorithm()
    {
        // SETS THE START OF THE MAZE
        startPiece = mazePieceGrid[Game.instance.random.Next(mazeSizeCount)];
        startPiece.passed = true;
        startPiece.debugBoxColor = Color.green;
        startPiece.debug = true;
        mazeCurrentPath.Add(startPiece);
        MazePiece currentPiece = NextInPath(startPiece);
        startPiece.toDirection = currentPiece.fromDirection.Negative();

        NextPiece:
        // CHECK IF THE ALGORITHM HAS BACK TRACKED TO THE START
        if (currentPiece != startPiece)
        {
            // NEXT PIECE IN PATH
            mazeCurrentPath.Add(currentPiece);
            currentPiece = NextInPath(currentPiece);
            //if (currentPiece.IsEndPiece()) { endPieces.Add(currentPiece.gridIndex); }
            goto NextPiece;
        }

        // END OF MAZE GENERATION
        exitPiece = GridIndexToMazePiece(GetExitPiecePosition());
        exitPiece.passed = true;
        exitPiece.debugBoxColor = Color.red;
        exitPiece.debug = true;
        //Debug.Log("New Maze Complete, Start @ " + startingPiece.gridIndex.ToStringBuilder() + ", Exit @ " + mazeExit.gridIndex.ToStringBuilder());
        return;
    }
    MazePiece NextInPath(MazePiece currentPiece)
    {
        Backtrack:
        // GETS DIRECTIONS THE PATH CAN GO
        List<int[]> availableDirections = currentPiece.AvailableDirections();
        
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
        int[] randomDirection = availableDirections[Game.instance.random.Next(availableDirections.Count)];
        MazePiece nextPiece = GridIndexToMazePiece(currentPiece.gridIndex.Plus(randomDirection));

        // OPENS THE PATHWAY BETWEEN THE PIECES
        currentPiece.OpenDirection(randomDirection);
        nextPiece.OpenDirection(randomDirection.Negative());
        nextPiece.passed = true;
        nextPiece.fromDirection = randomDirection.Negative();
        return nextPiece;
    }
    int[] GetExitPiecePosition()
    {
        int minDiff = goalMinimumDistanceFromStart;

        for (int i = 0; i < mazePieceGrid.Length; i++)
        {
            if (mazePieceGrid[i].IsEndPiece()) { endPieces.Add(mazePieceGrid[i].gridIndex); }
        }

        List<int[]> endPiecesOutsideMinDiff = new(endPieces);
        NewEndPiece:
        for (int i = 0; i < endPiecesOutsideMinDiff.Count; i++)
        {
            if (Math.Abs(endPiecesOutsideMinDiff[i][0] - startPiece.gridIndex[0]) > minDiff 
              & Math.Abs(endPiecesOutsideMinDiff[i][1] - startPiece.gridIndex[1]) > minDiff)
            {
                endPiecesOutsideMinDiff.RemoveAt(i);
            }
        }
        
        // List<int[]> endPiecesOutsideMinDiff = new();
        // for (int i = 0; i < endPieces.Count; i++)
        // {
        //     if (Math.Abs(endPieces[i][0] - startPiece.gridIndex[0]) > minDiff 
        //       & Math.Abs(endPieces[i][1] - startPiece.gridIndex[1]) > minDiff)
        //     {
        //         endPiecesOutsideMinDiff.Add(endPieces[i]);
        //     }
        // }
        if (minDiff <= 0) { Debug.Log("no valid exit position"); return new int[2]{ 0, 0 }; }
        if (endPiecesOutsideMinDiff.Count == 0) { minDiff--; goto NewEndPiece; }
        return endPiecesOutsideMinDiff[Game.instance.random.Next(endPiecesOutsideMinDiff.Count)];
    }
    readonly string[] winMsgs = new string[] 
    { 
        "If I had a cookie I would not give it to you and pat you on the back instead.",
        "Now go again.",
        "If you win again you might just go insane!",
        ""
    };
    void Win()
    {
        uiMessage.instance.New("You Win! " + winMsgs[Game.instance.random.Next(winMsgs.Length)]);
        uiMessage.instance.SetTimer(5f);
        refresh = true;
    }
    public bool TryGetMazePiece(int[] gridIndex, out MazePiece mazePiece)
    {
        mazePiece = null;
        // RETURNS NULL IF gridIndex IS OUTSIDE THE MAZE
        if (gridIndex[0] > mazeSizeX - 1 | gridIndex[0] < 0 | gridIndex[1] > mazeSizeZ - 1 | gridIndex[1] < 0) { return false; }
        mazePiece = GridIndexToMazePiece(gridIndex);
        return true;
    }
    public int GridIndexToMazePieceIndex(int[] gridIndex) => (gridIndex[0] * mazeSizeX) + gridIndex[1];
    //public int[] MazePieceIndexToGridIndex(int mazePieceIndex) => (gridIndex[0] * mazeSizeX) + gridIndex[1];
    public MazePiece GridIndexToMazePiece(int[] gridIndex) 
    {
        int index = GridIndexToMazePieceIndex(gridIndex);
        if (index > mazePieceGrid.Length - 1 | index < 0) { return null; }
        return mazePieceGrid[index];
    }
}
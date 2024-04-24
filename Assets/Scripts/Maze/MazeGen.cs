using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using Unity.Jobs;

[RequireComponent(typeof(MazeRenderer))]
public class MazeGen : MonoBehaviour
{
    public static MazeGen instance { get; private set; }
    public bool 
        refresh,
        mazeRenderAuto = true;
    public GameObject
        mazePiecePrefab,
        enemy;
    public int 
        mazeSizeX = 10,
        mazeSizeZ = 10,
        mazeSizeXNew,
        mazeSizeZNew,
        mazePieceSize = 10,
        mazeSizeCount; 
    public MazePiece[]
        mazePieceGrid = new MazePiece[0];
    public MazePiece
        mazePieceDefault { get; private set; } = new();
    public GameObject
        floor,
        exitBeacon;
    public MeshRenderer 
        floorRenderer;
    int
        currentPathIndex;
    MazePiece
        startMazePiece,
        exitMazePiece;
    List<int[]> 
        endPieces = new(); 
    MazePiece[] 
        mazeCurrentPath;
    public bool 
        resetOnWin, won;
    public TimeSpan allocationTime { get; private set; }
    public TimeSpan algorithmTime { get; private set; }
    public static readonly int[][] directions = new int[4][]
    {
        new int[2]{ 0, 1 },
        new int[2]{ 0,-1 },
        new int[2]{-1, 0 },
        new int[2]{ 1, 0 }
    };
    const string 
        str_mazeAllocationTime = "New Maze;  Allocation Time = ",
        str_mazeAlgorithmTime = "ms, Algorithm Time = ",
        str_prefabError = "Maze Piece Prefab not assigned, check Assets/Models/",
        str_mazeGenTotalTime = "ms, Total = ",
        str_ms = "ms";
    void Awake()
    {
        instance = this;
        floorRenderer = floor.GetComponent<MeshRenderer>();
        mazeSizeXNew = mazeSizeX;
        mazeSizeZNew = mazeSizeZ;
    }
    void Update()
    {
        if (exitMazePiece is not null) {
            if (Game.instance.papersCollected >= Game.instance.paperCount & !won & Player.instance.gridIndex.EqualTo(exitMazePiece.gridIndex)) { Win(); } }
    }
    public void Reset()
    {
        if (mazePiecePrefab == null) { Debug.LogError(str_prefabError); return; }
        ui.instance.uiFadeAlphaSet(1);
        exitBeacon.SetActive(false);
        Game.instance.Pause(false);
        Game.instance.Reset();
        ui.instance.gameOver.gameObject.SetActive(false);
        ui.instance.settings.gameObject.SetActive(false);

        won = false;
         
        MazeGenerate();

        Player.instance.TeleportInstant(startMazePiece.gridIndex.GridIndexToWorldPosition() + new Vector3(5f, 1.15f, 5f),
            new Vector3(0f, directions[0].ToVector().VectorNormalToCardinal().Euler(), 0f));
        Game.instance.inGame = true;
        Player.instance.PlayerFreeze(false);
        Player.instance.lookActive = true;
        Player.instance.moveActive = true;
        enemy.SetActive(true);
    }
    void MazeGenerate()
    {       
        System.Diagnostics.Stopwatch 
            allocationTimer = new(),
            algorithmTimer = new();

        allocationTimer.Start();
        MazeGridSet();
        allocationTimer.Stop();
        allocationTime = allocationTimer.Elapsed;

        SetFloor();

        algorithmTimer.Start();
        MazeAlgorithm();
        algorithmTimer.Stop();
        algorithmTime = algorithmTimer.Elapsed;

        allocationTimer.Stop();
        Debug.Log(new StringBuilder(str_mazeAllocationTime).Append(allocationTimer.Elapsed.TotalMilliseconds)
            .Append(str_mazeAlgorithmTime).Append(algorithmTimer.Elapsed.TotalMilliseconds)
            .Append(str_mazeGenTotalTime).Append((allocationTimer.Elapsed + allocationTimer.Elapsed).TotalMilliseconds).Append(str_ms).ToString());

        MazeRenderer.instance.Reset();
        if (startMazePiece.gridIndex.EqualTo(Player.instance.gridIndex)) { MazeRenderer.instance.MazeRenderUpdate(); }
    }
    void SetFloor()
    {
        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(mazeSizeX * mazePieceSize, 1f, mazeSizeZ * mazePieceSize);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);
        floorRenderer.material.mainTextureScale = new Vector2(mazeSizeX, mazeSizeZ);
        MazeNavMesh.instance.Bake();
    }
    void MazeGridSet()
    {
        mazeSizeX = Math.Clamp(mazeSizeXNew, 2, 10000);
        mazeSizeZ = Math.Clamp(mazeSizeZNew, 2, 10000);
        mazeSizeCount = mazeSizeX * mazeSizeZ;
        int index = 0;
        if (mazeSizeCount == mazePieceGrid.Length)
        { // IF THE AMOUNT NEEDED IS ALREADY INSTANTIATED JUST RESET THE VALUES
            for (int x = 0; x < mazeSizeX; x++)
            {           
                for (int z = 0; z < mazeSizeZ; z++)
                {
                    mazePieceGrid[index].Reset();
                    mazePieceGrid[index].gridIndex = new int[2]{ x, z };
                    index++;
                }
            }
        }
        else
        {

            if (mazeSizeZ != mazeSizeX) { mazeSizeZ = mazeSizeX; }

            for (int i = 0; i < mazePieceGrid.Length; i++) { mazePieceGrid[i] = null; }

            mazePieceGrid = new MazePiece[mazeSizeCount];
            // CREATES AN EMPTY GRID
            for (int x = 0; x < mazeSizeX; x++)
            {           
                for (int z = 0; z < mazeSizeZ; z++)
                {
                    // CREATES EMPTY MazePiece CLASS ASSIGNS ITS GRID INDEX ADDS IT TO THE ARRAY
                    MazePiece mazePieceNew = new() {
                        gridIndex = new int[2] { x, z } };
                    mazePieceGrid[index] = mazePieceNew;
                    index++;
                }
            }
        }
        // GETS EDGE PIECES AND GETS ADJACENT PIECES
        GetEdgePieces();
        GetAdjacentMazePieces();
    }
    void GetEdgePieces()
    {
        for (int x = 0; x < mazeSizeX; x++)
        {
            GridIndexToMazePiece(x, 0).EdgeCheck();
            GridIndexToMazePiece(x, mazeSizeZ - 1).EdgeCheck();
        }
        for (int z = 0; z < mazeSizeZ; z++)
        {
            GridIndexToMazePiece(0, z).EdgeCheck();
            GridIndexToMazePiece(mazeSizeX - 1, z).EdgeCheck();
        }
    }
    void GetAdjacentMazePieces()
    {
        for (int i = 0; i < mazePieceGrid.Length; i++)
        {
            if (TryGetMazePiece(mazePieceGrid[i].gridIndex[0] + directions[0][0], mazePieceGrid[i].gridIndex[1] + directions[0][1], out MazePiece up))
            {
                mazePieceGrid[i].adjacentPieces[0] = up;
                up.adjacentPieces[1] = mazePieceGrid[i];
            }
            if (TryGetMazePiece(mazePieceGrid[i].gridIndex[0] + directions[3][0], mazePieceGrid[i].gridIndex[1] + directions[3][1], out MazePiece right))
            {
                mazePieceGrid[i].adjacentPieces[3] = right;
                right.adjacentPieces[2] = mazePieceGrid[i];
            }
        }
    }
    void MazeAlgorithm()
    {
        mazeCurrentPath = new MazePiece[mazeSizeCount];
        // SETS THE START OF THE MAZE
        startMazePiece = mazePieceGrid[Game.instance.random.Next(mazeSizeCount)];
        startMazePiece.passed = true;
        mazeCurrentPath[0] = startMazePiece;
        // STARTS THE ALGORITHM
        MazePiece currentMazePiece = NextInPath(startMazePiece);

        int iterations = 0, iterationInfiniteLoop = mazeSizeCount * 10;

        NextMazePiece:

        iterations++;
        if (iterations > iterationInfiniteLoop) { throw new Exception("Infinite Loop Detected @ NextMazePiece"); }

        // CHECK IF THE ALGORITHM HAS BACK TRACKED TO THE START    
        if (currentMazePiece != startMazePiece)
        {
            // NEXT PIECE IN PATH
            currentPathIndex++;
            mazeCurrentPath[currentPathIndex] = currentMazePiece;
            currentMazePiece = NextInPath(currentMazePiece);
            goto NextMazePiece;
        }

        // END OF MAZE GENERATION
        exitMazePiece = GridIndexToMazePiece(GetExitPiecePosition());
        exitMazePiece.passed = true;

        GetPaperPositions();
        //Debug.Log("New Maze Complete, Start @ " + startingPiece.gridIndex.ToStringBuilder() + ", Exit @ " + mazeExit.gridIndex.ToStringBuilder());
        return;
    }
    MazePiece NextInPath(MazePiece currentMazePiece)
    {
        int iterations = 0, iterationInfiniteLoop = mazeSizeCount * 10;

        Backtrack:

        iterations++;
        if (iterations > iterationInfiniteLoop) { throw new Exception("Infinite Loop Detected @ Backtrack"); }

        // CHECK IF ANY AVAILABLE ADJACENT PIECES
        if (!currentMazePiece.TryGetRandomAdjacentPiece(out MazePiece nextMazePiece, out int[] toDirection))
        {
            // RECURSIVE BACKTRACKING
            mazeCurrentPath[currentPathIndex] = null;
            currentPathIndex--;
            if (currentPathIndex <= 0) { return mazeCurrentPath[0]; }
            currentMazePiece = mazeCurrentPath[currentPathIndex];
            goto Backtrack;
        }

        // OPENS THE PATHWAY BETWEEN THE PIECES
        currentMazePiece.OpenDirection(toDirection);
        nextMazePiece.OpenDirection(-toDirection[0], -toDirection[1]);
        nextMazePiece.passed = true;
        return nextMazePiece;
    }
    void GetPaperPositions()
    {      
        Game.instance.GetPaperCount();
        int 
            desiredPaperCount = Game.instance.paperCount,
            currentPaperCount = 0;
        for (int i = 0; currentPaperCount < desiredPaperCount; i++)
        {
            MazePiece randomMazePiece = mazePieceGrid[Game.instance.random.Next(mazePieceGrid.Length)];
            if (!randomMazePiece.hasPaper 
               & randomMazePiece.WallsActiveIsGrEqTo(1) 
               & !randomMazePiece.gridIndex.EqualTo(startMazePiece.gridIndex)
               & !randomMazePiece.gridIndex.EqualTo(exitMazePiece.gridIndex))
            {
                randomMazePiece.hasPaper = true;
                currentPaperCount++;
            }
        }
    }
    int[] GetExitPiecePosition()
    // this is the only list element left to optimise 
    // but i did not have the time
    {
        int minDiff = Mathf.CeilToInt(mazeSizeCount * 0.75f);
        for (int i = 0; i < mazePieceGrid.Length; i++)
        {
            if (mazePieceGrid[i].WallsActiveIsEqTo()) { endPieces.Add(mazePieceGrid[i].gridIndex); }
        }

        List<int[]> endPiecesOutsideMinDiff = new(endPieces);
        NewEndPiece:
        for (int i = 0; i < endPiecesOutsideMinDiff.Count; i++)
        {
            if (Math.Abs(endPiecesOutsideMinDiff[i][0] - startMazePiece.gridIndex[0]) > minDiff 
              & Math.Abs(endPiecesOutsideMinDiff[i][1] - startMazePiece.gridIndex[1]) > minDiff)
            {
                endPiecesOutsideMinDiff.RemoveAt(i);
            }
        }
        
        if (minDiff <= 0) { Debug.LogWarning("no valid exit position"); return new int[2]{ 0, 0 }; }
        if (endPiecesOutsideMinDiff.Count == 0) { minDiff--; goto NewEndPiece; }
        return endPiecesOutsideMinDiff[Game.instance.random.Next(endPiecesOutsideMinDiff.Count)];
    }
    public void OpenExit()
    {
        uiMessage.instance.New("You've got all the pages!");
        uiMessage.instance.New("Look up for the exit beacon");
        uiMessage.instance.SetTimer(5);
        exitBeacon.transform.position = exitMazePiece.gridIndex.GridIndexToWorldPosition() + new Vector3(5f, 50f, 5f);
        exitBeacon.SetActive(true);
    }
    readonly string[] winMsgs = new string[] 
    { 
        "If I had a cookie I would not give it to you and pat you on the back instead.",
        "Now go again.",
        "If you win again you might just go insane!"
    };
    void Win()
    {
        won = true;
        ui.instance.gameOver.gameObject.SetActive(true);
    }
    public bool TryGetMazePiece(int[] gridIndex, out MazePiece mazePiece)
    {
        mazePiece = null;
        // RETURNS NULL IF gridIndex IS OUTSIDE THE MAZE
        if (gridIndex[0] > mazeSizeX - 1 | gridIndex[0] < 0 | gridIndex[1] > mazeSizeZ - 1 | gridIndex[1] < 0) { return false; }
        mazePiece = GridIndexToMazePiece(gridIndex);
        return true;
    }
    public bool TryGetMazePiece(int gridIndexX, int gridIndexZ, out MazePiece mazePiece)
    {
        mazePiece = null;
        // RETURNS NULL IF gridIndex IS OUTSIDE THE MAZE
        if (gridIndexX > mazeSizeX - 1 | gridIndexX < 0 | gridIndexZ > mazeSizeZ - 1 | gridIndexZ < 0) { return false; }
        mazePiece = GridIndexToMazePiece(gridIndexX, gridIndexZ);
        return true;
    }
    // A REVERSE INDEX EQUATION IF NECESSARY
    // public int[] MazePieceIndexToGridIndex(int mazePieceIndex)
    // { 
    //     int z = mazePieceIndex % mazeSizeX;
    //     int a = mazePieceIndex - z;
    //     int x = a < 0 ? 0 : a / mazeSizeX;
    //     return new int[2] { x, z };
    // }
    public MazePiece GridIndexToMazePiece(int[] gridIndex) 
    {
        int index = ((gridIndex[0] * mazeSizeX) + gridIndex[1]) - (mazeSizeX > mazeSizeZ ? gridIndex[0] : 0);
        if (index > mazePieceGrid.Length - 1 | index < 0) { return null; }
        return mazePieceGrid[index];
    }
    public MazePiece GridIndexToMazePiece(int gridIndexX, int gridIndexZ) 
    {
        int index = ((gridIndexX * mazeSizeX) + gridIndexZ) - (mazeSizeX > mazeSizeZ ? gridIndexX : 0);
        if (index > mazePieceGrid.Length - 1 | index < 0) { return null; }
        return mazePieceGrid[index];
    }
}
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
        goalMinimumDistanceFromStart,
        currentPathIndex;
    MazePiece
        startMazePiece,
        exitMazePiece;
    List<int[]> 
        endPieces = new();
    //List<MazePiece> 
    MazePiece[] 
        mazeCurrentPath;
    public bool 
        resetOnWin, won;
    public static readonly int[][] directions = new int[4][]
    {
        new int[2]{ 0, 1 },
        new int[2]{ 0,-1 },
        new int[2]{-1, 0 },
        new int[2]{ 1, 0 }
    };
    const string 
        str_mazeGenTime = "Maze Generation Time = ",
        str_prefabError = "Maze Piece Prefab not assigned, check Assets/Models/",
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
        if (Input.GetKeyDown(KeyCode.F5) | refresh)
        {
            refresh = false;
            Reset();
        }
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
        mazeSizeCount = mazeSizeX * mazeSizeZ;
        goalMinimumDistanceFromStart = Mathf.CeilToInt(mazeSizeCount * 0.75f);
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
        System.Diagnostics.Stopwatch timer = new();
        timer.Start();

        MazeGridSet(mazeSizeCount);
        SetFloor();
        MazeAlgorithm();

        timer.Stop();
        Debug.Log(new StringBuilder(str_mazeGenTime).Append(timer.Elapsed.TotalMilliseconds).Append(str_ms).ToString());

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
    void MazeGridSet(int amount)
    {
        //if (mazeSizeZ > mazeSizeX) { (mazeSizeZ, mazeSizeX) = (mazeSizeX, mazeSizeZ); }
        int index = 0;
        if (amount == mazePieceGrid.Length)
        {
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
            mazeSizeX = Math.Clamp(mazeSizeXNew, 2, 10000);
            mazeSizeZ = Math.Clamp(mazeSizeZNew, 2, 10000);
            if (mazeSizeZ != mazeSizeX) { mazeSizeZ = mazeSizeX; }

            for (int i = 0; i < mazePieceGrid.Length; i++) { mazePieceGrid[i] = null; }
            mazePieceGrid = new MazePiece[amount];

            // CREATES AN EMPTY GRID
            for (int x = 0; x < mazeSizeX; x++)
            {           
                for (int z = 0; z < mazeSizeZ; z++)
                {
                    // CREATES EMPTY MazePiece CLASS AND ADDS IT TO THE DICTIONARY
                    MazePiece mazePieceNewComponent = new();
                    int[] mazePieceGridIndex = new int[2]{ x, z };
                    mazePieceNewComponent.gridIndex = mazePieceGridIndex;
                    mazePieceGrid[index] = mazePieceNewComponent;
                    index++;
                }
            }
        }
        // SETS EDGE PIECES AND GETS ADJACENT PIECES
        GetEdgePieces();
        GetAdjacentMazePieces();
    }
    void GetEdgePieces()
    {
        for (int x = 0; x < mazeSizeX; x++)
        {
            GridIndexToMazePiece(new int[2]{ x, 0 }).EdgeCheck();
            GridIndexToMazePiece(new int[2]{ x, mazeSizeZ - 1 }).EdgeCheck();
        }
        for (int z = 0; z < mazeSizeZ; z++)
        {
            GridIndexToMazePiece(new int[2]{ 0, z }).EdgeCheck();
            GridIndexToMazePiece(new int[2]{ mazeSizeX - 1, z }).EdgeCheck();
        }
    }
    void GetAdjacentMazePieces()
    {
        for (int i = 0; i < mazePieceGrid.Length; i++)
        {
            if (TryGetMazePiece(GridIndexExt.Plus(mazePieceGrid[i].gridIndex, directions[0]), out MazePiece up))
            {
                mazePieceGrid[i].adjacentPieces[0] = up;
                up.adjacentPieces[1] = mazePieceGrid[i];
            }
            if (TryGetMazePiece(GridIndexExt.Plus(mazePieceGrid[i].gridIndex, directions[3]), out MazePiece right))
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
        startMazePiece.debugBoxColor = Color.green;
        startMazePiece.debug = true;
        mazeCurrentPath[0] = startMazePiece;
        // STARTS THE ALGORITHM
        MazePiece currentMazePiece = NextInPath(startMazePiece);
        //startMazePiece.toDirection = currentMazePiece.fromDirection.Negative();

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
        exitMazePiece.debugBoxColor = Color.red;
        exitMazePiece.debug = true;

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
        int[] fromDirection = GridIndexExt.Negative(toDirection);
        nextMazePiece.OpenDirection(fromDirection);
        nextMazePiece.passed = true;
        //nextMazePiece.fromDirection = fromDirection;
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
    {
        int minDiff = goalMinimumDistanceFromStart;

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
    public int GridIndexToMazePieceIndex(int[] gridIndex) 
    {    
        return ((gridIndex[0] * mazeSizeX) + gridIndex[1]) 
            - (mazeSizeX > mazeSizeZ ? gridIndex[0] : 0);
            //- (mazeSizeX > mazeSizeZ ? (mazeSizeX - mazeSizeZ) * mazeSizeZ : 0);
    }
    public int[] MazePieceIndexToGridIndex(int mazePieceIndex) 
    { 
        int z = mazePieceIndex % mazeSizeX;
        int a = mazePieceIndex - z;
        int x = a < 0 ? 0 : a / mazeSizeX;
        return new int[2] { x, z };
    }
    public MazePiece GridIndexToMazePiece(int[] gridIndex) 
    {
        int index = GridIndexToMazePieceIndex(gridIndex);
        if (index > mazePieceGrid.Length - 1 | index < 0) { return null; }
        return mazePieceGrid[index];
    }
    
    
    /*
    public struct MazeGenJob : IJob
    {
        public void Execute()
        {
            
        }
    }
    public struct MazePieceStruct
    {
        public bool
            passed,
            debug;
        public bool[] 
            walls;
        [NonSerialized] public MazePieceStruct?[]
            adjacentPieces;
        public int[] 
            gridIndex,
            fromDirection,
            toDirection;
        public Color? 
            debugBoxColor;
        public LoadedMazePiece
            loadedMazePiece;

        public MazePieceStruct(int[] gridIndex_)
        {
            passed = false;
            debug = false;
            walls = new bool[4]{ true, true, true, true };
            adjacentPieces = new MazePieceStruct?[4];
            gridIndex = gridIndex_;
            fromDirection = new int[2]{ 0, 0 };
            toDirection = new int[2]{ 0, 0 };
            debugBoxColor = Color.clear;
            loadedMazePiece = null;
        }
        public static MazePieceStruct? Null()
        {
            return new MazePieceStruct
            {
                passed = false,
                debug = false,
                walls = null,
                adjacentPieces = null,
                gridIndex = null,
                fromDirection = null,
                toDirection = null,
                debugBoxColor = null,
                loadedMazePiece = null
            };
        }
    }
    public static bool IsEndPiece(this MazePieceStruct mazePiece) 
    { 
        int wallsActive = 0;
        for (int i = 0; i < mazePiece.walls.Length; i++) { if (mazePiece.walls[i]) { wallsActive++; } }
        return wallsActive >= 3;
    }
    public static MazePieceStruct? RandomAdjacentPiece(this MazePieceStruct mazePiece, out int[] direction)
    {
        MazePieceStruct?[] adjacentPiecesAvailable = new MazePieceStruct?[4];
        int[][] adjacentPiecesAvailableDirections = new int[4][];
        int count = 0;
        for (int i = 0; i < mazePiece.adjacentPieces.Length; i++)
        {
            if (mazePiece.adjacentPieces[i] is null) { continue; }
            // if (!mazePiece.adjacentPieces[i]?.passed) 
            if (mazePiece.adjacentPieces[i] is MazePieceStruct piece && !piece.passed)
            { 
                adjacentPiecesAvailable[count] = mazePiece.adjacentPieces[i];
                adjacentPiecesAvailableDirections[count] = MazeGen.directions[i];
                count++;
            }
        }
        if (count == 0)
        {
            direction = new int[2]{ 0, 0 };
            return null;
        }
        int randomInt = Game.instance.random.Next(count);
        direction = adjacentPiecesAvailableDirections[randomInt];
        return adjacentPiecesAvailable[randomInt];
    }
    public static void OpenDirection(this MazePieceStruct mazePiece, int[] direction)
    {
        for (int i = 0; i < MazeGen.directions.Length; i++)
        {
            mazePiece.walls[i] = (direction[0] != MazeGen.directions[i][0] | direction[1] != MazeGen.directions[i][1]) & mazePiece.walls[i];
        }
    }
    public static void EdgeCheck(this MazePieceStruct mazePiece)
    {
        mazePiece.walls[0] |= mazePiece.gridIndex[1] == MazeGen.instance.mazeSizeZ - 1;
        mazePiece.walls[1] |= mazePiece.gridIndex[1] == 0;
        mazePiece.walls[2] |= mazePiece.gridIndex[0] == 0;
        mazePiece.walls[3] |= mazePiece.gridIndex[0] == MazeGen.instance.mazeSizeZ - 1;
    }
    public static void Reset(this MazePieceStruct mazePiece)
    {
        mazePiece.passed = false;
        mazePiece.debug = false;
        mazePiece.walls = new bool[4]{ true, true, true, true };
        mazePiece.adjacentPieces = new MazePieceStruct?[4];
        mazePiece.gridIndex = null;
        mazePiece.fromDirection = new int[2]{ 0, 0 };
        mazePiece.toDirection = new int[2]{ 0, 0 };
        mazePiece.debugBoxColor = Color.clear;
        mazePiece.loadedMazePiece = null;
    }*/
}
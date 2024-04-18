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
        //paperPrefab,
        enemy;
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
    public MeshRenderer 
        floorRenderer;
    int
        goalMinimumDistanceFromStart,
        currentPathIndex;
    MazePiece
        startMazePiece,
        exitMazePiece;
    // int[]
    //     startGridIndex,
    //     exitGridIndex;
    List<int[]> 
        endPieces = new();
    //List<MazePiece> 
    MazePiece[] 
        mazeCurrentPath;
    public bool 
        resetOnWin, won;
    public static readonly int[][] directions = new int[5][]
    {
        new int[2]{ 0, 1 },
        new int[2]{ 0,-1 },
        new int[2]{-1, 0 },
        new int[2]{ 1, 0 },
        new int[2]{ 0, 0 } 
    };
    public List<GameObject>
        paperObjects;
    const string 
        str_mazeGenTime = "Maze Generation Time = ",
        str_prefabError = "Maze Piece Prefab not assigned, check Assets/Models/",
        str_ms = "ms";
    void Awake()
    {
        instance = this;
        floorRenderer = floor.GetComponent<MeshRenderer>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) | refresh)
        {
            refresh = false;
            Reset();
        }
        if (exitMazePiece is not null & !won) {
            if (Player.instance.gridIndex.EqualTo(exitMazePiece.gridIndex)) { Win(); }}
    }
    public void Reset()
    {
        ui.instance.uiFadeAlphaSet(1);
        Game.instance.papersCollected = 0;
        MazeGenerate();
        Player.instance.TeleportInstant(startMazePiece.gridIndex.GridIndexToWorldPosition() + new Vector3(5f, 1.15f, 5f),
            new Vector3(0f, startMazePiece.toDirection.ToVector().VectorNormalToCardinal().Euler(), 0f));
        Game.instance.inGame = true;
        Player.instance.PlayerFreeze(false);
        Player.instance.lookActive = true;
        Player.instance.moveActive = true;
        enemy.SetActive(true);
    }
    void MazeGenerate()
    {
        if (mazePiecePrefab == null) { Debug.LogError(str_prefabError); return; }
        won = false;

        System.Diagnostics.Stopwatch timer = new();
        timer.Start();

        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(mazeSizeX * mazePieceSize, 1f, mazeSizeZ * mazePieceSize);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);
        floorRenderer.material.mainTextureScale = new Vector2(mazeSizeX, mazeSizeZ);

        goalMinimumDistanceFromStart = Mathf.CeilToInt(mazeSizeCount * 0.75f);

        mazeSizeCount = mazeSizeX * mazeSizeZ;
        //MazeGridNew();
        MazeGridSet(mazeSizeCount);
        //Debug.Break();
        MazeAlgorithm();
        timer.Stop();
        uiMessage.instance.New(new StringBuilder(str_mazeGenTime).Append(timer.Elapsed.TotalMilliseconds).Append(str_ms).ToString());

        MazeRenderer.instance.Reset();
        if (startMazePiece.gridIndex.EqualTo(Player.instance.gridIndex)) { MazeRenderer.instance.MazeRenderUpdate(); }
    }
    // void MazeGridNew()
    // {
    //     endPieces.Clear();
    //     mazePieceGrid = new MazePiece[mazeSizeCount];
    //     int index = 0;
    //     // CREATES AN EMPTY GRID
    //     for (int x = 0; x < mazeSizeX; x++)
    //     {           
    //         for (int z = 0; z < mazeSizeZ; z++)
    //         {
    //             // CREATES EMPTY MazePiece CLASS AND ADDS IT TO THE DICTIONARY
    //             MazePiece mazePieceNewComponent = new();
    //             int[] mazePieceGridIndex = new int[2]{ x, z };
    //             mazePieceNewComponent.gridIndex = mazePieceGridIndex;
    //             mazePieceNewComponent.EdgeCheck();
    //             mazePieceGrid[index] = mazePieceNewComponent;
    //             index++;
    //         }
    //     }
    //     GetAdjacentMazePieces();
    // }
    void MazeGridSet(int amount)
    {
        endPieces.Clear();
        for (int i = 0; i < paperObjects.Count; i++) { Destroy(paperObjects[i]); paperObjects.RemoveAt(i); }
        int oldMazeGridCount = mazePieceGrid.Length;
        int[] lastPieceGridIndex = mazePieceGrid.Length == 0 ? new int[2] { 0, 0 } : MazePieceIndexToGridIndex(oldMazeGridCount - 1);
        if (amount > oldMazeGridCount) // EXPAND POOL
        {
            Array.Resize(ref mazePieceGrid, amount);
            int index = Math.Clamp(oldMazeGridCount - 1, 0, int.MaxValue);
            for (int x = lastPieceGridIndex[0]; x < mazeSizeX; x++)
            {           
                for (int z = lastPieceGridIndex[1]; z < mazeSizeZ; z++)
                {
                    // CREATES EMPTY MazePiece CLASS AND ADDS IT TO THE DICTIONARY
                    MazePiece mazePieceNewComponent = new();
                    int[] mazePieceGridIndex = new int[2]{ x, z };
                    mazePieceNewComponent.gridIndex = mazePieceGridIndex;
                    mazePieceNewComponent.toDirection = directions[4];
                    mazePieceNewComponent.fromDirection = directions[4];
                    mazePieceGrid[index] = mazePieceNewComponent;
                    index++;
                }
            }
            Debug.Log("mazePieceGrid +" + (amount - oldMazeGridCount));
        }
        else if (amount < oldMazeGridCount) // SHRINK POOL
        {
            for (int i = amount; i < mazePieceGrid[amount..].Length; i++)
            {
                mazePieceGrid[i] = null;
            }
            Array.Resize(ref mazePieceGrid, amount);
            Debug.Log("mazePieceGrid " + (amount - oldMazeGridCount));
        }
        else 
        {
            for (int i = 0; i < mazePieceGrid.Length; i++)
            {
                mazePieceGrid[i].Reset();
                mazePieceGrid[i].gridIndex = MazePieceIndexToGridIndex(i);
            }
        }
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
            //if (TryGetMazePiece(mazePieceGrid[i].gridIndex.Plus(directions[0]), out MazePiece up))
            if (TryGetMazePiece(GridIndexExt.Plus(mazePieceGrid[i].gridIndex, directions[0]), out MazePiece up))
            {
                mazePieceGrid[i].adjacentPieces[0] = up;
                up.adjacentPieces[1] = mazePieceGrid[i];
            }
            //if (TryGetMazePiece(mazePieceGrid[i].gridIndex.Plus(directions[3]), out MazePiece right))
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
        currentPathIndex = 0;
        // SETS THE START OF THE MAZE
        startMazePiece = mazePieceGrid[Game.instance.random.Next(mazeSizeCount)];
        startMazePiece.passed = true;
        startMazePiece.debugBoxColor = Color.green;
        startMazePiece.debug = true;
        mazeCurrentPath[0] = startMazePiece;
        MazePiece currentMazePiece = NextInPath(mazeCurrentPath[0]);
        //startMazePiece.toDirection = currentMazePiece.fromDirection.Negative();
        startMazePiece.toDirection = GridIndexExt.Negative(currentMazePiece.fromDirection);

        int iterations = 0, iterationInfiniteLoop = mazeSizeCount * 10;

        NextMazePiece:

        iterations++;
        if (iterations > iterationInfiniteLoop) { throw new Exception("Infinite Loop Detected @ NextMazePiece"); }

        // CHECK IF THE ALGORITHM HAS BACK TRACKED TO THE START
        if (!currentMazePiece.gridIndex.EqualTo(startMazePiece.gridIndex))
        {
            // NEXT PIECE IN PATH
            currentPathIndex++;
            //mazeCurrentPath.Add(currentPiece);
            mazeCurrentPath[currentPathIndex] = currentMazePiece;
            currentMazePiece = NextInPath(currentMazePiece);
            goto NextMazePiece;
        }

        // END OF MAZE GENERATION
        //TryGetMazePiece(GetExitPiecePosition(), out exitPiece);
        exitMazePiece = GridIndexToMazePiece(GetExitPiecePosition());
        exitMazePiece.passed = true;
        exitMazePiece.debugBoxColor = Color.red;
        exitMazePiece.debug = true;

        //int[][] paperPositions = GetPaperPositions(out int count);
        GetPaperPositions();
        // for (int i = 0; i < count; i++)
        // {
        //     GridIndexToMazePiece(paperPositions[i]).hasPaper = true;
        // }
        //Debug.Log("New Maze Complete, Start @ " + startingPiece.gridIndex.ToStringBuilder() + ", Exit @ " + mazeExit.gridIndex.ToStringBuilder());
        return;
    }
    MazePiece NextInPath(MazePiece currentMazePiece)
    {
        int iterations = 0, iterationInfiniteLoop = mazeSizeCount * 10;

        Backtrack:

        iterations++;
        if (iterations > iterationInfiniteLoop) { throw new Exception("Infinite Loop Detected @ Backtrack"); }

        // GETS DIRECTIONS THE PATH CAN GO
        //MazePiece nextMazePiece = currentMazePiece.RandomAdjacentPiece(out int[] toDirection);
        //currentMazePiece.TryGetRandomAdjacentPiece(ref nextMazePiece, ref toDirection);
        
        // CHECK FOR DEAD END
        // if (nextMazePiece is null)
        // {
        //     // RECURSIVE BACKTRACKING
        //     mazeCurrentPath[currentPathIndex] = null;
        //     currentPathIndex--;
        //     if (currentPathIndex <= 0) { return mazeCurrentPath[0]; }
        //     currentMazePiece = mazeCurrentPath[currentPathIndex];
        //     goto Backtrack;
        // }

        if (!currentMazePiece.TryGetRandomAdjacentPiece(out MazePiece nextMazePiece, out int[] toDirection))
        { // uint and nuint
            // RECURSIVE BACKTRACKING
            mazeCurrentPath[currentPathIndex] = null;
            currentPathIndex--;
            if (currentPathIndex <= 0) { return mazeCurrentPath[0]; }
            currentMazePiece = mazeCurrentPath[currentPathIndex];
            goto Backtrack;
        }

        // OPENS THE PATHWAY BETWEEN THE PIECES
        currentMazePiece.OpenDirection(toDirection);
        nextMazePiece.OpenDirection(GridIndexExt.Negative(toDirection));
        nextMazePiece.passed = true;
        nextMazePiece.fromDirection = GridIndexExt.Negative(toDirection);
        return nextMazePiece;
    }
    //int[][] GetPaperPositions(out int count)
    void GetPaperPositions()
    {
        int 
            count = 0,
            paperCount = Math.Max(mazeSizeX, mazeSizeZ) - 2;
        // MazePiece[] 
        //     mazePieceGridTemp = new MazePiece[mazePieceGrid.Length];
        // int[][]
        //     paperPositions = new int[mazePieceGrid.Length][];
        //Array.Copy(mazePieceGrid, mazePieceGridTemp, mazePieceGrid.Length);
        
        int iterations = 0, iterationInfiniteLoop = mazeSizeCount * 10;

        NextPosition:

        iterations++;
        if (iterations > iterationInfiniteLoop) { throw new Exception("Infinite Loop Detected @ NextPosition"); }

        //int randInt = Game.instance.random.Next(mazePieceGridTemp.Length);
        int randInt = Game.instance.random.Next(mazePieceGrid.Length);
        //if (mazePieceGridTemp[randInt] is not null) 
        {
            //if (!mazePieceGridTemp[randInt].hasPaper & mazePieceGridTemp[randInt].WallsActiveIsGrEqTo(1))
            if (!mazePieceGrid[randInt].hasPaper & mazePieceGrid[randInt].WallsActiveIsGrEqTo(1))
            {
                // paperPositions[index] = mazePieceGridTemp[randInt].gridIndex;
                // mazePieceGridTemp[randInt] = null;
                // index++;
                mazePieceGrid[randInt].hasPaper = true;
                count++;
            }
        }
        if (count < paperCount) { goto NextPosition; }
        //count = index;
        //return paperPositions;
    }
    int[] GetExitPiecePosition()
    {
        int minDiff = goalMinimumDistanceFromStart;

        for (int i = 0; i < mazePieceGrid.Length; i++)
        {
            if (mazePieceGrid[i].WallsActiveIsGrEqTo()) { endPieces.Add(mazePieceGrid[i].gridIndex); }
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
    readonly string[] winMsgs = new string[] 
    { 
        "If I had a cookie I would not give it to you and pat you on the back instead.",
        "Now go again.",
        "If you win again you might just go insane!"
    };
    void Win()
    {
        return;
        uiMessage.instance.New("You Win! " + winMsgs[Game.instance.random.Next(winMsgs.Length)]);
        uiMessage.instance.SetTimer(5f);
        won = true;
        if (!resetOnWin) { return; }
        Reset();
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
    public int[] MazePieceIndexToGridIndex(int mazePieceIndex) 
    { 
        int z = mazePieceIndex % mazeSizeX;
        int a = mazePieceIndex - z;
        int x = a < 0 ? 0 : (mazePieceIndex - z) / mazeSizeX;
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
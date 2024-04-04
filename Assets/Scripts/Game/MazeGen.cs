using System.Collections;
using System.Collections.Generic;
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
    public float 
        mazePieceSize = 10f;
    [SerializeField] List<MazePiece>
        mazePieces = new(),
        mazeCorrectPath = new();
    [SerializeField] GameObject
        floor;

    int 
        minimumFinalPathLength = 10;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //MazeGenerate();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) || refresh)
        {
            refresh = false;

            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();

            MazeGenerate();

            stopwatch.Stop();
            Debug.Log("Maze generation time: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
        }
    }
    void MazeGenerate()
    {
        debugBacktrackCount = 0;
        minimumFinalPathLength = (mazeSize.x * mazeSize.z) / 4;
        //foreach (MazePiece mazePiece in mazePieces) { Destroy(mazePiece); }
        mazePieces.Clear();
        mazePiecesLookup.Clear();
        mazeCorrectPath.Clear();

        floor.transform.localScale = new Vector3(mazeSize.x * 10, 1f, mazeSize.z * 10);
        floor.transform.position = new Vector3((floor.transform.localScale.x / 2) - 5, 0.5f, (floor.transform.localScale.z / 2) - 5);

        MazeNew();

        foreach (MazePiece mazePiece in mazePieces) { mazePiece.Refresh(); }

        GenerateCorrectPath();
        //UnityEditor.EditorApplication.isPaused = true;
    }
    void MazeNew()
    {
        for (int a = 0; a < mazeSize.x; a++)
        {
            for (int b = 0; b < mazeSize.z; b++)
            {
                MazePiece mazePieceNewComponent = new();
                Vector3Int mazePieceGridPosition = new Vector3Int(a, 0, b);
                mazePieceNewComponent.gridPosition = mazePieceGridPosition;
                mazePieces.Add(mazePieceNewComponent);
                mazePiecesLookup.Add(mazePieceGridPosition, mazePieceNewComponent);

                //GameObject mazePieceNew = Instantiate(mazePiecePrefab);
                //SceneManager.MoveGameObjectToScene(mazePieceNew, gameObject.scene);
                //mazePieceNew.transform.parent = transform;
                //mazePieceNew.name = "mazePiece @ " + mazePieceGridPosition.ToString();
                //mazePieceNew.transform.position = (Vector3)mazePieceGridPosition * mazePieceSize;
            }
        }
    }
    void GenerateCorrectPath()
    {
        MazePiece startingPiece = mazePiecesLookup[Vector3Int.zero];
        startingPiece.passed = true;
        MazePiece currentPiece = NextInPath(startingPiece, true, true);
        int piecesPassed = 0;

        NextPiece:

        if (currentPiece == null)
        {
            Debug.LogError("currentPiece is null?");
        }

        // CHECK FOR EDGE PIECE AND MINIMUM PATH LENGTH
        if ((currentPiece.gridPosition.x == 0
            || currentPiece.gridPosition.x == mazeSize.x
            || currentPiece.gridPosition.z == 0
            || currentPiece.gridPosition.z == mazeSize.z)
            && piecesPassed > minimumFinalPathLength)
        {
            // SUCCESSFUL PATH
            currentPiece.OpenDirection(-currentPiece.fromDirection);
            GenerateRemainingMaze();
            return;
        }
        else
        {
            // NEXT PIECE IN PATH
            currentPiece = NextInPath(currentPiece, true, true);
            mazeCorrectPath.Add(currentPiece);
            piecesPassed++;
            goto NextPiece;
        }
    }
    MazePiece NextInPath(MazePiece currentPiece, bool refreshWhenStuck = false, bool isCorrectPath = false)
    {
        currentPiece.debugBoxColor = Color.red;

        Backtrack:
        List<Vector3Int> availableDirections = new();
        if (currentPiece.adjacentPieceFwd != null) { if (!currentPiece.adjacentPieceFwd.passed) { availableDirections.Add(Vector3Int.forward); } }
        if (currentPiece.adjacentPieceBack != null) { if (!currentPiece.adjacentPieceBack.passed) { availableDirections.Add(Vector3Int.back); } }
        if (currentPiece.adjacentPieceLeft != null) { if (!currentPiece.adjacentPieceLeft.passed) { availableDirections.Add(Vector3Int.left); } }
        if (currentPiece.adjacentPieceRight != null) { if (!currentPiece.adjacentPieceRight.passed) { availableDirections.Add(Vector3Int.right); } }

        if (availableDirections.Count == 0)
        {
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
        foreach (MazePiece mazePiece in mazePieces)
        {
            int adjacentsNotPassed = 4;
            List<MazePiece> adjacentsPresent = new();

            if (mazePiece.adjacentPieceFwd != null) { adjacentsPresent.Add(mazePiece.adjacentPieceFwd); }
            else if (mazePiece.adjacentPieceBack != null) { adjacentsPresent.Add(mazePiece.adjacentPieceBack); }
            else if (mazePiece.adjacentPieceLeft != null) { adjacentsPresent.Add(mazePiece.adjacentPieceLeft); }
            else if (mazePiece.adjacentPieceRight != null) { adjacentsPresent.Add(mazePiece.adjacentPieceRight); }
            foreach (MazePiece adjacentPiece in adjacentsPresent)
            {
                if (adjacentPiece.passed) { adjacentsNotPassed--; }
            }
            if (adjacentsNotPassed >= 2)
            {
                MazePiece currentPiece = NextInPath(mazePiece);

                NextPiece:

                if (currentPiece == null)
                {
                    // DEAD END SPINE PATH
                    continue;
                }

                currentPiece = NextInPath(currentPiece);

                goto NextPiece;
            }
        }
    }
    public void ToggleDebugCorrectPath()
    {
        debugCorrectPath = !debugCorrectPath;
        mazeCorrectPath.ForEach(mazePiece => mazePiece.debug = debugCorrectPath);
        if (debugCorrectPath)
        {
            mazeCorrectPath[^1].debugBoxColor = Color.cyan;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeGen : MonoBehaviour
{
    public static MazeGen instance { get; private set; }
    public bool refresh;
    public bool debugCorrectPath;
    public bool debugPathSlow;
    public float debugPathSlowSpeed = 1f;
    public enum mazePieceTypeEnum
    {
        one,
        two,
        three,
        four,
    }
    [SerializeField] GameObject mazePiecePrefab;
    public Vector3Int mazeSize = Vector3Int.one * 4;
    [SerializeField] Vector3 mazePieceSize = new Vector3(10f, 0f, 10f);
    [SerializeField] List<MazePiece>
        mazePieces = new(),
        mazeCorrectPath = new();
    public GameObject floor;
    public MazePiece mazePieceNull;
    public Dictionary<Vector3Int, MazePiece> mazePiecesLookup = new();
    public int minimumFinalPathLength = 10;

    private void Awake()
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
        minimumFinalPathLength = (mazeSize.x * mazeSize.z) / 4;
    }

    void MazeGenerate()
    {
        foreach (MazePiece mazePiece in mazePieces) { Destroy(mazePiece.gameObject); }
        mazePieces.Clear();
        mazePiecesLookup.Clear();
        mazeCorrectPath.Clear();

        floor.transform.localScale = new Vector3(mazeSize.x * 10, 1f, mazeSize.z * 10);
        floor.transform.position = new Vector3((floor.transform.localScale.x / 2) - 5, 0.5f, (floor.transform.localScale.z / 2) - 5);

        MazeNew();

        foreach (MazePiece mazePiece in mazePieces) { mazePiece.Refresh(); }

        if (debugPathSlow)
        {
            StartCoroutine(GenerateCorrectPathCR());
        }
        else
        {
            GenerateCorrectPath();
        }
    }
    void MazeNew()
    {
        for (int a = 0; a < mazeSize.x; a++)
        {
            for (int b = 0; b < mazeSize.z; b++)
            {
                GameObject mazePieceNew = Instantiate(mazePiecePrefab);
                SceneManager.MoveGameObjectToScene(mazePieceNew, gameObject.scene);
                mazePieceNew.transform.parent = transform;
                Vector3Int mazePieceGridPosition = new Vector3Int(a, 0, b);
                mazePieceNew.transform.position = mazePieceSize.Multiply(mazePieceGridPosition);

                MazePiece mazePieceNewComponent = mazePieceNew.GetComponent<MazePiece>();
                mazePieceNewComponent.gridPosition = mazePieceGridPosition;
                mazePieces.Add(mazePieceNewComponent);
                mazePiecesLookup.Add(mazePieceGridPosition, mazePieceNewComponent);
            }
        }
    }
    void GenerateCorrectPath()
    {
        MazePiece startingPiece = mazePiecesLookup[Vector3Int.zero];
        startingPiece.passed = true;
        MazePiece currentPiece = NextInPath(startingPiece, true);
        int piecesPassed = 0;

        NextPiece:

        if ((currentPiece.gridPosition.x == 0
            || currentPiece.gridPosition.x == mazeSize.x
            || currentPiece.gridPosition.z == 0
            || currentPiece.gridPosition.z == mazeSize.z)
            && piecesPassed > minimumFinalPathLength)
        {
            currentPiece.OpenDirection(-currentPiece.fromDirection);
            currentPiece.debugBoxColor = Color.cyan;
            GenerateRemainingMaze();
            return;
        }
        else
        {
            currentPiece.debugBoxColor = Color.green;
            currentPiece = NextInPath(currentPiece, true);
            mazeCorrectPath.Add(currentPiece);
            piecesPassed++;
            goto NextPiece;
        }
    }
    IEnumerator GenerateCorrectPathCR()
    {
        MazePiece startingPiece = mazePiecesLookup[Vector3Int.zero];
        startingPiece.passed = true;
        MazePiece currentPiece = NextInPath(startingPiece, true);
        int piecesPassed = 0;

        NextPieceCR:

        if ((currentPiece.gridPosition.x == 0
            || currentPiece.gridPosition.x == mazeSize.x
            || currentPiece.gridPosition.z == 0
            || currentPiece.gridPosition.z == mazeSize.z)
            && piecesPassed > minimumFinalPathLength)
        {
            currentPiece.OpenDirection(-currentPiece.fromDirection);
            currentPiece.debugBoxColor = Color.cyan;
            GenerateRemainingMaze();
            yield return null;
        }
        else
        {
            currentPiece.debugBoxColor = Color.green;
            yield return new WaitForSeconds(debugPathSlowSpeed);
            currentPiece = NextInPath(currentPiece, true);
            mazeCorrectPath.Add(currentPiece);
            piecesPassed++;
            goto NextPieceCR;
        }
    }

    MazePiece NextInPath(MazePiece currentPiece, bool refreshWhenStuck = false)
    {
        currentPiece.debugBoxColor = Color.red;
        List<Vector3Int> availableDirections = new();
        if (currentPiece.adjacentPieceFwd != null) { availableDirections.Add(Vector3Int.forward); }
        if (currentPiece.adjacentPieceBack != null) { availableDirections.Add(Vector3Int.back); }
        if (currentPiece.adjacentPieceLeft != null) { availableDirections.Add(Vector3Int.left); }
        if (currentPiece.adjacentPieceRight != null) { availableDirections.Add(Vector3Int.right); }

        int loops = 0;
        GetRandomPiece:
        Vector3Int randomDirection = availableDirections[UnityEngine.Random.Range(0, availableDirections.Count)];
        MazePiece nextPiece = mazePiecesLookup[currentPiece.gridPosition + randomDirection];

        if (nextPiece.passed)
        {
            //if (nextPiece.passed == )

            if (loops < 4)
            {
                loops++;
                goto GetRandomPiece;
            }
            else
            {
                Debug.Log("stuck");
                refresh.IfFalseIgnore(refreshWhenStuck);
                return null;
            }
        }
        else
        {
            currentPiece.OpenDirection(randomDirection);
            nextPiece.OpenDirection(-randomDirection);
            nextPiece.passed = true;
            nextPiece.fromDirection = -randomDirection;
            return nextPiece;
        }
    }
    void GenerateRemainingMaze()
    {
        foreach (MazePiece mazePiece in mazePieces)
        {
            //Debug.Log("start spine path check " + mazePiece.name);
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
                    //Debug.Log("spine path died");
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
        foreach (MazePiece mazePiece in mazeCorrectPath)
        {
            mazePiece.debug = debugCorrectPath;
        }
    }
}
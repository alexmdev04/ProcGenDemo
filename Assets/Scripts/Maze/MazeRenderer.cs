using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MazeGen))]
public class MazeRenderer : MonoBehaviour
{
    public static MazeRenderer instance {  get; private set; }
    public bool refresh;      
    int 
        renderDistance = 3;
    List<LoadedMazePiece> 
        loadedMazePieces = new(),
        mazePiecePool = new();
    Stack<LoadedMazePiece>
        mazePiecePoolAvailable = new();
    const string
        str_renderTime = "Render Update Time = ",
        str_ms = "ms",
        str_mazePiecePooled = "mazePiece (pooled)";
    void Awake()
    {
        instance = this;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7) || refresh)
        {
            refresh = false;
            UpdateGrid();
        }
    }
    public void Reset_()
    {
        loadedMazePieces.Clear();
        ResetPool();
        CreatePool(GetMazePiecePoolSize());
    }
    public void UpdateGrid()
    {      
        //return;
        System.Diagnostics.Stopwatch timer = new();
        timer.Start();
        // GETS ALL PIECES TO LOAD IN A 45 DEGREE ROTATED SQUARE GRID AROUND THE PLAYER
        // THE RENDER DISTANCE DETERMINES HOW MANY PIECES AHEAD OF YOU WILL BE LOADED (NOT ACCOUNTING FOR PLAYER ROTATION)
        List<MazePiece> mazePiecesToLoad = new();
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            int start = (((renderDistance * 2) + 1) - (Math.Abs(x) * 2) - 1) / 2;
            for (int z = start; z >= -start; z--)
            {
                if (z < -start) { break; }
                //Debug.Log("("+ x + ", " + z + ")" + ", numOfZ = " + numOfZ);
                if (MazeGen.instance.mazePiecesLookup.TryGetValue(new(Player.instance.gridPosition.x + x, 0, Player.instance.gridPosition.z + z), out MazePiece mazePieceToLoad))
                {
                    mazePiecesToLoad.Add(mazePieceToLoad);
                }
            }
        }

        // FINDS PIECES TO BE UNLOADED AND RETURNS THEM TO THE POOL
        // ONLY CURRENTLY LOADED PIECES THAT ARE NOT PART OF mazePiecesToLoad SHOULD BE UNLOADED
        // THIS MAY LOOK UNNECESSARILY LONG BUT THERE IS NO OTHER WAY I SWEAR
        foreach (Vector3Int mazePiecePosition in 
            loadedMazePieces.Select(loadedMazePiece => loadedMazePiece.mazePiece.gridPosition)
                            .Except(mazePiecesToLoad.Select(mazePiece => mazePiece.gridPosition))) 
        {
            ReturnToPool(MazeGen.instance.mazePiecesLookup[mazePiecePosition].loadedMazePiece);
        }

        loadedMazePieces.Clear();

        // LOADS MAZE PIECES, CAN INCLUDE ALREADY LOADED PIECES AS THEY WILL BE SKIPPED
        mazePiecesToLoad.ForEach(mazePiece => loadedMazePieces.Add(TakeFromPool(mazePiece)));
        timer.Stop();
        Debug.Log(new StringBuilder(str_renderTime).Append(timer.Elapsed.TotalMilliseconds).Append(str_ms).ToString());
    }
    public void SetRenderDistance(int renderDistanceNew)
    {
        renderDistance = renderDistanceNew;
        MazeGen.instance.mazePieces.ForEach(mazePiece => mazePiece.loadedMazePiece = null);
        Reset_();
        UpdateGrid();
    }
    void CreatePool(int poolSize, bool reset = false)
    {
        // CHECKS IF THE CURRENT POOL IS ALREADY BIG ENOUGH
        if (!reset && mazePiecePool.Count == poolSize) { return; }

        ResetPool();

        /*  CREATES A NEW POOL WITH THE SIZE EQUAL TO THE MAX AMOUNT OF PIECES THAT CAN BE LOADED AT ONCE...
            ...WITH THE CURRENT RENDER DISTANCE, WHILE ALSO NOT EXCEEDING THE TOTAL AMOUNT OF PIECES IN THE MAZE */
        for (int i = 0; i < poolSize; i++)
        {
            GameObject mazePieceNew = Instantiate(MazeGen.instance.mazePiecePrefab);
            SceneManager.MoveGameObjectToScene(mazePieceNew, gameObject.scene);
            mazePieceNew.transform.parent = gameObject.transform;
            mazePiecePool.Add(mazePieceNew.GetComponent<LoadedMazePiece>());
        }
        mazePiecePoolAvailable = new(mazePiecePool);
    }
    void ResetPool()
    {
        // DESTROYS THE OLD POOL
        mazePiecePool.ForEach(mazePiece => Destroy(mazePiece.gameObject));
        mazePiecePool.Clear();
        mazePiecePoolAvailable.Clear();
    }
    LoadedMazePiece TakeFromPool(MazePiece mazePiece)
    {
        // ASSIGNS A MazePiece TO A LoadedMazePiece FROM THE POOL, TO BE VISIBLE IN THE WORLD
        if (mazePiece.loadedMazePiece is not null) { return mazePiece.loadedMazePiece; }
        if (!mazePiecePoolAvailable.TryPop(out LoadedMazePiece loadedMazePieceNew))
        {
            Debug.LogError("pool exceeded");
            return null;
        }
        mazePiece.loadedMazePiece = loadedMazePieceNew;
        loadedMazePieceNew.mazePiece = mazePiece;
        loadedMazePieceNew.Refresh();
        loadedMazePieceNew.gameObject.SetActive(true);
        return loadedMazePieceNew;
    }
    void ReturnToPool(LoadedMazePiece loadedMazePiece)
    {
        // RESETS THE LoadedMazePiece AND RETURNS IT TO THE POOL
        loadedMazePiece.gameObject.name = str_mazePiecePooled;
        loadedMazePiece.gameObject.SetActive(false);
        loadedMazePiece.mazePiece.loadedMazePiece = null;
        loadedMazePiece.mazePiece = MazeGen.instance.mazePieceDefault;
        mazePiecePoolAvailable.Push(loadedMazePiece);
    }  
    int GetMazePiecePoolSize()
    {
        // GETS EXACTLY THE AMOUNT REQUIRED TO FILL THE RENDER DISTANCE WHILE NOT EXCEEDING THE MAZE SIZE
        int poolSize = 1;
        for (int i = 1; i <= renderDistance; i++) { poolSize += 4 * i; }
        return (poolSize > MazeGen.instance.mazeSizeCount) ? MazeGen.instance.mazeSizeCount : poolSize;
    }
}
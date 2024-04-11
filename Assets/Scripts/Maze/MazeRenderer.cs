using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeRenderer : MonoBehaviour
{
    public static MazeRenderer instance {  get; private set; }
    public bool refresh;
    public List<LoadedMazePiece> loadedMazePieces = new();
    public int renderDistanceReadOnly = 3;
    int renderDistance = 5;
    public List<LoadedMazePiece> mazePiecePool = new();
    public Stack<LoadedMazePiece> mazePiecePoolAvailable = new();
    public int poolAvailable;
    public int poolQueueSize;
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
        poolAvailable = mazePiecePoolAvailable.Count;
    }
    public void Reset_()
    {
        loadedMazePieces.Clear();
        ResetPool();
        CreatePool(GetMazePiecePoolSize());
    }
    public void UpdateGrid()
    {      
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

        // CREATES NEW POOL
        // THE POOL SIZE IS EQUAL TO HOW MANY PIECES CAN BE LOADED AT ONCE WITH THE SET RENDER DISTANCE
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
        if (mazePiece.loadedMazePiece != null) { return mazePiece.loadedMazePiece; }
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
        loadedMazePiece.gameObject.SetActive(false);
        loadedMazePiece.mazePiece.loadedMazePiece = null;
        loadedMazePiece.mazePiece = MazeGen.instance.mazePieceDefault;
        loadedMazePiece.gameObject.name = str_mazePiecePooled;
        mazePiecePoolAvailable.Push(loadedMazePiece);
    }  
    int GetMazePiecePoolSize()
    {
        int[] poolSizes = new int[7] { 0, 4, 16, 24, 40, 60, 84 };
        return poolSizes[Math.Clamp(renderDistance, 1, 6)] + 1;
    }
}
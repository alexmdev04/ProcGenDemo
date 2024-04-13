using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MazeGen))]
public class MazeRenderer : MonoBehaviour
{
    public static MazeRenderer instance { get; private set; }
    public bool refresh;
    public int 
        extraChecks = 0,
        renderDistance = 3;
    List<LoadedMazePiece> 
        loadedMazePieces = new(),
        mazePiecePool = new();
    Stack<LoadedMazePiece>
        mazePiecePoolAvailable = new();
    const string
        str_renderTime = "Update Maze Time = ",
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
            MazeRenderUpdate();
        }
    }
    public void Reset()
    {
        loadedMazePieces.Clear();
        ResetPool();
        SetPoolSize(GetPoolSize());
    }
    public void MazeRenderUpdate()
    {      
        extraChecks = 0;
        System.Diagnostics.Stopwatch timer = new();
        timer.Start();
        // GETS ALL PIECES TO LOAD IN A DIAMOND SHAPE GRID AROUND THE PLAYER
        // THE RENDER DISTANCE DETERMINES HOW MANY PIECES AHEAD OF YOU WILL BE LOADED (NOT ACCOUNTING FOR PLAYER ROTATION)
        List<MazePiece> mazePiecesToLoad = new();
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            int start = (((renderDistance * 2) + 1) - (Math.Abs(x) * 2) - 1) / 2;
            for (int z = start; z >= -start; z--)
            {
                if (z < -start) { break; }
                if (MazeGen.instance.TryGetMazePiece(Player.instance.gridIndex.Plus(new int[2]{ x, z }), out MazePiece mazePieceToLoad)) 
                { 
                    mazePiecesToLoad.Add(mazePieceToLoad);
                }
                else
                {
                    extraChecks++;
                }
            }
        }

        // FINDS PIECES TO BE UNLOADED AND RETURNS THEM TO THE POOL
        // ONLY CURRENTLY LOADED PIECES THAT ARE NOT PART OF mazePiecesToLoad SHOULD BE UNLOADED
        // THIS MAY LOOK UNNECESSARILY LONG BUT THERE IS NO OTHER WAY I SWEAR
        foreach (int[] mazePiecePosition in 
            loadedMazePieces.Select(loadedMazePiece => loadedMazePiece.mazePiece.gridIndex)
                            .Except(mazePiecesToLoad.Select(mazePiece => mazePiece.gridIndex))) 
        {
            ReturnToPool(MazeGen.instance.GridIndexToMazePiece(mazePiecePosition).loadedMazePiece);
        }

        loadedMazePieces.Clear();

        // LOADS MAZE PIECES, CAN INCLUDE ALREADY LOADED PIECES AS THEY WILL BE SKIPPED
        mazePiecesToLoad.ForEach(mazePiece => loadedMazePieces.Add(TakeFromPool(mazePiece)));
        timer.Stop();
        //Debug.Log(new StringBuilder(str_renderTime).Append(timer.Elapsed.TotalMilliseconds).Append(str_ms).ToString());
        //Debug.Break();
    }
    public void SetRenderDistance(int renderDistanceNew)
    {
        renderDistance = renderDistanceNew;
        foreach (MazePiece mazePiece in MazeGen.instance.mazePieceGrid) { mazePiece.loadedMazePiece = null; }
        Reset();
        MazeRenderUpdate();
    }
    void SetPoolSize(int poolSize)
    {
        // ADJUSTS THE SIZE OF THE POOL TO THE EXACT AMOUNT TO FILL THE RENDER DISTANCE OR THE ENTIRE MAZE
        AdjustPool(poolSize - mazePiecePool.Count);
        mazePiecePoolAvailable = new(mazePiecePool);
    }
    void AdjustPool(int amount)
    {
        if (amount > 0) // EXPAND POOL
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject mazePieceNew = Instantiate(MazeGen.instance.mazePiecePrefab);
                SceneManager.MoveGameObjectToScene(mazePieceNew, gameObject.scene);
                mazePieceNew.transform.parent = gameObject.transform;
                mazePiecePool.Add(mazePieceNew.GetComponent<LoadedMazePiece>());    
            }
        }
        else if (amount < 0) // SHRINK POOL
        {
            amount = Math.Abs(amount);
            for (int i = mazePiecePool.Count - 1; i < mazePiecePool.Count + amount; i++) { Destroy(mazePiecePool[i]); }
            mazePiecePool.RemoveRange(mazePiecePool.Count - amount - 1, amount);
        }
        else { return; }
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
        loadedMazePiece.gameObject.name = str_mazePiecePooled;
        loadedMazePiece.gameObject.SetActive(false);
        loadedMazePiece.mazePiece.loadedMazePiece = null;
        loadedMazePiece.mazePiece = MazeGen.instance.mazePieceDefault;
        mazePiecePoolAvailable.Push(loadedMazePiece);
    }  
    int GetPoolSize()
    {
        // GETS EXACTLY THE AMOUNT REQUIRED TO FILL THE RENDER DISTANCE WHILE NOT EXCEEDING THE MAZE SIZE
        int poolSize = 1;
        bool poolSizeExceededMazeSize = false;
        for (int i = 1; i <= renderDistance; i++) 
        { 
            poolSize += 4 * i;
            poolSizeExceededMazeSize = poolSize > MazeGen.instance.mazeSizeCount;
            if (poolSizeExceededMazeSize) { renderDistance = i; break; }
        }
        return poolSizeExceededMazeSize ? MazeGen.instance.mazeSizeCount : poolSize;
    }
}
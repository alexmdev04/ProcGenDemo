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
    public bool 
        refresh,
        initialRenderComplete;
    public int 
        extraChecks = 0,
        renderDistance = 3;
    public List<LoadedMazePiece> 
        loadedMazePieces = new(),
        mazePiecePool = new();
    Stack<LoadedMazePiece>
        mazePiecePoolAvailable;
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
        if (Input.GetKeyDown(KeyCode.F7) | refresh)
        {
            refresh = false;
            MazeRenderUpdate();
        }
    }
    public void Reset()
    {
        for (int i = 0; i < loadedMazePieces.Count; i++) { ReturnToPool(loadedMazePieces[i]); }
        loadedMazePieces.Clear();
        initialRenderComplete = false;
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
                if (MazeGen.instance.TryGetMazePiece(Player.instance.gridIndex[0] + x, Player.instance.gridIndex[1] + z, out MazePiece mazePieceToLoad)) 
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

        MazeNavMesh.instance.Bake();
        if (!initialRenderComplete) { Enemy.instance.RandomPosition(); initialRenderComplete = true; };

        timer.Stop();
    }
    public void SetRenderDistance(int renderDistanceNew)
    {
        renderDistance = renderDistanceNew;
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
            Debug.Log("mazePiecePool +" + amount);
        }
        else if (amount < 0) // SHRINK POOL
        {
            Debug.Log("mazePiecePool " + amount);
            amount = Math.Abs(amount);
            for (int i = mazePiecePool.Count - amount; i < mazePiecePool.Count; i++)
            { Destroy(mazePiecePool[i].gameObject); }
            mazePiecePool.RemoveRange(mazePiecePool.Count - amount, amount);
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
        if (!mazePiecePoolAvailable.TryPop(out LoadedMazePiece loadedMazePiece)) 
        {
            Debug.LogError("pool exceeded");
            return null;
        }
        mazePiece.loadedMazePiece = loadedMazePiece;
        loadedMazePiece.mazePiece = mazePiece;
        loadedMazePiece.Refresh();
        loadedMazePiece.gameObject.SetActive(true);
        return loadedMazePiece;
    }
    void ReturnToPool(LoadedMazePiece loadedMazePiece)
    {
        // RESETS THE LoadedMazePiece AND RETURNS IT TO THE POOL
        loadedMazePiece.gameObject.name = str_mazePiecePooled;
        loadedMazePiece.gameObject.SetActive(false);
        loadedMazePiece.mazePiece.loadedMazePiece = null;
        loadedMazePiece.mazePiece = null;
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
            if (poolSizeExceededMazeSize) { renderDistance = i + 1; break; }
        }
        return poolSizeExceededMazeSize ? MazeGen.instance.mazeSizeCount : poolSize;
    }
}
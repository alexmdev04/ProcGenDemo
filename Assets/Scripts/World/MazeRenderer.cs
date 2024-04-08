using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeRenderer : MonoBehaviour
{
    public static MazeRenderer instance {  get; private set; }
    public bool refresh;
    public List<LoadedMazePiece> loadedMazePieces = new();
    public int renderDistance = 3;
    public List<LoadedMazePiece> mazePiecePool = new();
    public Stack<LoadedMazePiece> mazePiecePoolAvailable = new();
    public int poolAvailable;
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
    public void UpdateGrid()
    {
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();

        int
            mazeGridCount = MazeGen.instance.mazeSize.x * MazeGen.instance.mazeSize.z,
            mazeRenderDistanceGridCount = (renderDistance + 2) * (renderDistance + 2),
            mazePiecePoolSize = 0;

        if (mazeGridCount == mazeRenderDistanceGridCount) { mazePiecePoolSize = mazeRenderDistanceGridCount - 4; }
        else if (mazeGridCount > mazeRenderDistanceGridCount) { mazePiecePoolSize = mazeRenderDistanceGridCount; }
        else if (mazeRenderDistanceGridCount > mazeGridCount) { mazePiecePoolSize = mazeGridCount; }

        CreatePool(mazePiecePoolSize);
        
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

        //List<LoadedMazePiece> mazePiecesAlreadyLoaded = new();
        //mazePiecesToLoad.ForEach(mazePiece => { if (mazePiece.loadedMazePiece != null) { mazePiecesAlreadyLoaded.Add(mazePiece.loadedMazePiece); } });
        //loadedMazePieces.Except(mazePiecesToLoad)

        // FINDS PIECES TO BE UNLOADED AND RETURNS THEM TO THE POOL        
        List<LoadedMazePiece> mazePiecesToUnload = new(loadedMazePieces);
        mazePiecesToLoad.ForEach(mazePiece => mazePiecesToUnload.Remove(mazePiece.loadedMazePiece));
        mazePiecesToUnload.ForEach(mazePiece => ReturnToPool(mazePiece));
        loadedMazePieces.Clear();

        // LOADS MAZE PIECES
        mazePiecesToLoad.ForEach(mazePiece => loadedMazePieces.Add(TakeFromPool(mazePiece)));

        stopwatch.Stop();
        Debug.Log("Maze render time: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
    }
    void CreatePool(int poolSize)
    {
        if (mazePiecePool.Count == poolSize) { return; }

        Debug.Log("mazePiecePool reset, new size = " + poolSize);

        mazePiecePool.ForEach(mazePiece => Destroy(mazePiece));
        mazePiecePool.Clear();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject mazePieceNew = Instantiate(MazeGen.instance.mazePiecePrefab);
            SceneManager.MoveGameObjectToScene(mazePieceNew, gameObject.scene);
            mazePieceNew.transform.parent = gameObject.transform;
            mazePiecePool.Add(mazePieceNew.GetComponent<LoadedMazePiece>());
        }
        mazePiecePoolAvailable = new(mazePiecePool);
    }
    LoadedMazePiece TakeFromPool(MazePiece mazePiece)
    {
        if (mazePiece.loadedMazePiece != null) { return mazePiece.loadedMazePiece; }
        //LoadedMazePiece loadedMazePieceNew = mazePiecePoolAvailable.Pop();
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
        loadedMazePiece.mazePiece.loadedMazePiece = null;
        loadedMazePiece.mazePiece = MazeGen.instance.mazePieceDefault;
        loadedMazePiece.gameObject.SetActive(false);
        loadedMazePiece.Refresh();
        mazePiecePoolAvailable.Push(loadedMazePiece);
    }
}
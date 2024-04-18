using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class MazeNavMesh : MonoBehaviour
{
    public static MazeNavMesh instance { get; private set; }
    public bool initialBakeComplete;
    NavMeshSurface navMeshSurface;
    //NavMeshModifierVolume navMeshVolume;
    void Awake()
    {
        instance = this;
        navMeshSurface = GetComponent<NavMeshSurface>();
        //navMeshVolume = GetComponent<NavMeshModifierVolume>();
    }
    void Start()
    {
        
    }
    void Update()
    {
        // navMeshSurface.center = Player.instance.gridIndex.ToWorldPosition();
        // navMeshSurface.size = new (
        //     (MazeRenderer.instance.renderDistance * 2 * MazeGen.instance.mazePieceSize) + MazeGen.instance.mazePieceSize,
        //     20,
        //     (MazeRenderer.instance.renderDistance * 2 * MazeGen.instance.mazePieceSize) + MazeGen.instance.mazePieceSize);
    }
    public void MazeRenderFinished()
    {
        // navMeshSurface.center = Player.instance.gridIndex.ToWorldPosition() + new Vector3Int(5, 0, 5);
        // navMeshSurface.size = new (
        //     MazeGen.instance.mazePieceSize * ((MazeRenderer.instance.renderDistance * 2) + 1) + 1,
        //     20,
        //     MazeGen.instance.mazePieceSize * ((MazeRenderer.instance.renderDistance * 2) + 1) + 1);
        
        navMeshSurface.center = Player.instance.gridIndex.GridIndexToWorldPosition() + new Vector3(MazeGen.instance.mazePieceSize / 2, 0, MazeGen.instance.mazePieceSize / 2);

        float navMeshSurfaceSize = ((MazeGen.instance.mazePieceSize * MazeRenderer.instance.renderDistance) * 2) + (MazeGen.instance.mazePieceSize / 2);
        
        navMeshSurface.size = new(navMeshSurfaceSize, 20, navMeshSurfaceSize);
        navMeshSurface.BuildNavMesh();
        initialBakeComplete = true;
    }
}

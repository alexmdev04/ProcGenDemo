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
        navMeshSurface.center = Player.instance.gridIndex.GridIndexToWorldPosition() 
            + new Vector3(MazeGen.instance.mazePieceSize / 2, 0, MazeGen.instance.mazePieceSize / 2);
    }
    public void Bake()
    {      
        float navMeshSurfaceSize = (MazeGen.instance.mazePieceSize * MazeRenderer.instance.renderDistance * 2) + (MazeGen.instance.mazePieceSize / 2);
        
        navMeshSurface.size = new(navMeshSurfaceSize, 20, navMeshSurfaceSize);
        navMeshSurface.BuildNavMesh();
        initialBakeComplete = true;
    }
}

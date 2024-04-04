using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    public bool refresh;
    public List<ActiveMazePiece> activeMazePieces = new();
    public int renderDistance = 3;
    public int iterations = 0;
    public int max;
    void Start()
    {
        UpdateGrid();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7) || refresh)
        {
            refresh = false;
            UpdateGrid();
        }
    }
    void UpdateGrid()
    {
        iterations = 0;
        activeMazePieces.ForEach(activeMazePiece => Destroy(activeMazePiece.gameObject));
        activeMazePieces.Clear();

        Vector3Int playerGridPosition = Vector3Int.FloorToInt(Player.instance.transform.position / MazeGen.instance.mazePieceSize);
        /*
        ActivateMazePiece(new(-renderDistance, 0));          // -3, 0

        ActivateMazePiece(new(-renderDistance + 1, 0 + 1));  // -2, 1
        ActivateMazePiece(new(-renderDistance + 1, 0));      // -2, 0
        ActivateMazePiece(new(-renderDistance + 1, 0 - 1));  // -2, -1

        ActivateMazePiece(new(-renderDistance + 2, 0 + 2));  // -1, 2
        ActivateMazePiece(new(-renderDistance + 2, 0 + 1));  // -1, 1
        ActivateMazePiece(new(-renderDistance + 2, 0));      // -1, 0
        ActivateMazePiece(new(-renderDistance + 2, 0 - 1));  // -1, -1
        ActivateMazePiece(new(-renderDistance + 2, 0 - 2));  // -1, -2

        ActivateMazePiece(new(-renderDistance + 3, 0 + 3));  // 0, 3  
        ActivateMazePiece(new(-renderDistance + 3, 0 + 2));  // 0, 2
        ActivateMazePiece(new(-renderDistance + 3, 0 + 1));  // 0, 1
        ActivateMazePiece(new(-renderDistance + 3, 0));      // 0, 0
        ActivateMazePiece(new(-renderDistance + 3, 0 - 1));  // 0, -1
        ActivateMazePiece(new(-renderDistance + 3, 0 - 2));  // 0, -2
        ActivateMazePiece(new(-renderDistance + 3, 0 - 3));  // 0, -3

        ActivateMazePiece(new(-renderDistance + 4, 0 + 2));  // 1, 2
        ActivateMazePiece(new(-renderDistance + 4, 0 + 1));  // 1, 1
        ActivateMazePiece(new(-renderDistance + 4, 0));      // 1, 0
        ActivateMazePiece(new(-renderDistance + 4, 0 - 1));  // 1, -1
        ActivateMazePiece(new(-renderDistance + 4, 0 - 2));  // 1, -2

        ActivateMazePiece(new(-renderDistance + 5, 0 + 1));  // 2, 1
        ActivateMazePiece(new(-renderDistance + 5, 0));      // 2, 0
        ActivateMazePiece(new(-renderDistance + 5, 0 - 1));  // 2, -1

        ActivateMazePiece(new(-renderDistance + 6, 0));      // 3, 0
        */

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            int numOfZ = ((renderDistance * 2) + 1) - (Math.Abs(x) * 2); // = 3
            int start = (numOfZ - 1) / 2;
            for (int z = start; z > -numOfZ; z--)
            {
                Debug.Log("z = " + z + ", -numOfZ = " + -numOfZ);
                if (z < -start) { break; }
                ActivateMazePiece(new(x, 0, z), numOfZ);
            }
        }
        // MazeGen.instance.mazePiecesLookup[]
    }
    void ActivateMazePiece(Vector3Int gridPosition, int numOfZ)
    {
        Debug.Log(gridPosition + ", numOfZ = " + numOfZ);
        iterations++;
        GameObject activeMazePieceNew = Instantiate(MazeGen.instance.mazePiecePrefab);
        activeMazePieceNew.transform.position = gridPosition * 10;
        activeMazePieces.Add(activeMazePieceNew.GetComponent<ActiveMazePiece>());

        int a = 0 + ((((renderDistance * 2) + 1) - (Math.Abs(-renderDistance) * 2) - 1) / 2);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MazePiece
{
    public bool
        passed = false,
        debug = false,
        wallFwdEnabled = true,
        wallBackEnabled = true,
        wallLeftEnabled = true,
        wallRightEnabled = true;
    public bool[] 
        walls;
    [NonSerialized] public MazePiece[]
        adjacentPieces;
    public Vector3Int 
        gridPosition = Vector3Int.zero,
        fromDirection = Vector3Int.zero,
        debugDirection = Vector3Int.zero;
    public MazePiece
        adjacentPieceFwd,
        adjacentPieceBack,
        adjacentPieceLeft,
        adjacentPieceRight;
    public Color 
        debugBoxColor = Color.red;
    public LoadedMazePiece
        loadedMazePiece;

    public void Refresh()
    {
        walls = new bool[4] { wallFwdEnabled, wallBackEnabled, wallLeftEnabled, wallRightEnabled };
        //RandomizeWalls();
        EdgeCheck();
        GetAdjacentPieces();
    }
    // void RandomizeWalls(int amount = 3, bool edgeCheck = true)
    // {
    //    amount = System.Math.Clamp(amount, 0, 3);
    //    List<GameObject> randomWalls = new(walls);
    //    randomWalls.ForEach(wall => wall.SetActive(true));
    //    for (int i = 0; i < amount; i++)
    //    {
    //        int randomIndex = UnityEngine.Random.Range(0, randomWalls.Count);
    //        randomWalls[randomIndex].SetActive(false);
    //        randomWalls.RemoveAt(randomIndex);
    //    }
    //    if (edgeCheck) { EdgeCheck(); }
    // }
    void EdgeCheck()
    {
        wallFwdEnabled |= gridPosition.z == MazeGen.instance.mazeSize.z - 1;
        wallBackEnabled |= gridPosition.z == 0;
        wallLeftEnabled |= gridPosition.x == 0;
        wallRightEnabled |= gridPosition.x == MazeGen.instance.mazeSize.x - 1;
    }
    void GetAdjacentPieces()
    {
        adjacentPieceFwd = GetAdjacentPiece(Vector3Int.forward);
        adjacentPieceBack = GetAdjacentPiece(Vector3Int.back);
        adjacentPieceLeft = GetAdjacentPiece(Vector3Int.left);
        adjacentPieceRight = GetAdjacentPiece(Vector3Int.right);
        adjacentPieces = new MazePiece[4]
        {
            adjacentPieceFwd,
            adjacentPieceBack,
            adjacentPieceLeft,
            adjacentPieceRight
        };
    }
    MazePiece GetAdjacentPiece(Vector3Int direction)
    {
        if (MazeGen.instance.mazePiecesLookup.TryGetValue(gridPosition + direction, out MazePiece mazePiece)) { return mazePiece; }
        else { return null; }
    }
    public void OpenDirection(Vector3Int direction)
    {
        if (direction == Vector3Int.forward) { wallFwdEnabled = false; }
        else if (direction == Vector3Int.back) { wallBackEnabled = false; }
        else if (direction == Vector3Int.left) { wallLeftEnabled = false; } 
        else if (direction == Vector3Int.right) { wallRightEnabled = false; }
        walls = new bool[4] { wallFwdEnabled, wallBackEnabled, wallLeftEnabled, wallRightEnabled };
    }
    public List<Vector3Int> AvailableDirections()
    {
        List<Vector3Int> availableDirections = new();
        for (int i = 0; i < adjacentPieces.Length; i++)
        {
            if (adjacentPieces[i] is null) { continue; }
            if (!adjacentPieces[i].passed) availableDirections.Add(MazeGen.directions[i]);
        }
        return availableDirections;
    }
}
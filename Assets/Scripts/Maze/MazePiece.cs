using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MazePiece
{
    public bool
        passed = false,
        debug = false;
    public bool[] 
        walls = new bool[4]{ true, true, true, true };
    [NonSerialized] public MazePiece[]
        adjacentPieces = new MazePiece[4];
    //public string[] adjacentPieceIndexes = new string[4];
    public int[] 
        gridIndex,
        fromDirection = new int[2]{ 0, 0 },
        toDirection = new int[2]{ 0, 0 };
    public Color 
        debugBoxColor = Color.red;
    public LoadedMazePiece
        loadedMazePiece;
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
    public void EdgeCheck()
    {
        walls[0] |= gridIndex[1] == MazeGen.instance.mazeSizeZ - 1;
        walls[1] |= gridIndex[1] == 0;
        walls[2] |= gridIndex[0] == 0;
        walls[3] |= gridIndex[0] == MazeGen.instance.mazeSizeZ - 1;
    }
    public void OpenDirection(int[] direction)
    {
        for (int i = 0; i < MazeGen.directions.Length; i++)
        {
            walls[i] = direction.EqualTo(MazeGen.directions[i]) ? false : walls[i];
        }
    }
    public List<int[]> AvailableDirections()
    {
        List<int[]> availableDirections = new();
        for (int i = 0; i < adjacentPieces.Length; i++)
        {
            if (adjacentPieces[i] == null) { continue; }
            if (!adjacentPieces[i].passed) { availableDirections.Add(MazeGen.directions[i]); }
        }
        return availableDirections;
    }
    public bool IsEndPiece() 
    { 
        int wallsActive = 0;
        for (int i = 0; i < walls.Length; i++) { if (walls[i]) { wallsActive++; } }
        return wallsActive >= 3;
    }
}
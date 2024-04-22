using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MazePiece
{
    public bool
        passed = false,
        debug = false,
        hasPaper = false;
    public bool[] 
        walls = new bool[4]{ true, true, true, true };
    [NonSerialized] public MazePiece[]
        adjacentPieces = new MazePiece[4];
    //public string[] adjacentPieceIndexes = new string[4];
    public int[] 
        gridIndex = null;
        //fromDirection = new int[2]{ 0, 0 },
        //toDirection = new int[2]{ 0, 0 };
    public Color 
        debugBoxColor = default;
    public LoadedMazePiece
        loadedMazePiece = null;
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
    public void Reset()
    {
        passed = false;
        debug = false;
        hasPaper = false;
        walls = new bool[4]{ true, true, true, true };
        adjacentPieces = new MazePiece[4];
        gridIndex = null;
        //fromDirection = new int[2]{ 0, 0 };
        //toDirection = new int[2]{ 0, 0 };
        debugBoxColor = Color.clear;
        loadedMazePiece = null;
    }
    public void EdgeCheck()
    {
        walls[0] |= gridIndex[1] == MazeGen.instance.mazeSizeZ - 1;
        walls[1] |= gridIndex[1] == 0;
        walls[2] |= gridIndex[0] == 0;
        walls[3] |= gridIndex[0] == MazeGen.instance.mazeSizeZ - 1;
    }
    public void OpenDirection(int[] direction)
    {
        if (direction == null) { return; }
        for (int i = 0; i < walls.Length; i++)
        {
            //walls[i] = (direction[0] != MazeGen.directions[i][0] | direction[1] != MazeGen.directions[i][1]) & walls[i];
            walls[i] = direction.EqualTo(MazeGen.directions[i]) ? false : walls[i];
        }
    }
    // public List<int[]> AvailableDirections()
    // {
    //     List<int[]> availableDirections = new();
    //     for (int i = 0; i < adjacentPieces.Length; i++)
    //     {
    //         if (adjacentPieces[i] == null) { continue; }
    //         if (!adjacentPieces[i].passed) { availableDirections.Add(MazeGen.directions[i]); }
    //     }
    //     return availableDirections;
    // }
    // public int[][] AvailableDirections()
    // {
    //     int[][] availableDirections = new int[4][];
    //     int index = 0;
    //     for (int i = 0; i < adjacentPieces.Length; i++)
    //     {
    //         if (adjacentPieces[i] == null) { continue; }
    //         if (!adjacentPieces[i].passed) { availableDirections[index] = MazeGen.directions[i]; index++; }
    //     }
    //     Array.Resize(ref availableDirections, index);
    //     return availableDirections;
    // }
    // public MazePiece[] AdjacentPiecesAvailable(out int count, out int[] direction)
    // {
    //     MazePiece[] availableDirections = new MazePiece[4];
    //     count = 0;
    //     for (int i = 0; i < adjacentPieces.Length; i++)
    //     {
    //         if (adjacentPieces[i] == null) { continue; }
    //         if (!adjacentPieces[i].passed) { availableDirections[count] = adjacentPieces[i]; count++; }
    //     }
    //     return availableDirections;
    // }
    // public MazePiece RandomAdjacentPiece(out int[] direction)
    // {
    //     MazePiece[] adjacentPiecesAvailable = new MazePiece[4];
    //     int[][] adjacentPiecesAvailableDirections = new int[4][];
    //     int count = 0;
    //     for (int i = 0; i < adjacentPieces.Length; i++)
    //     {
    //         if (adjacentPieces[i] == null) { continue; }
    //         if (!adjacentPieces[i].passed) 
    //         { 
    //             adjacentPiecesAvailable[count] = adjacentPieces[i];
    //             adjacentPiecesAvailableDirections[count] = MazeGen.directions[i];
    //             count++;
    //         }
    //     }
    //     if (count == 0)
    //     {
    //         direction = null;//new int[2]{ 0, 0 };
    //         return null;
    //     }
    //     int randomInt = Game.instance.random.Next(count);
    //     direction = adjacentPiecesAvailableDirections[randomInt];
    //     return adjacentPiecesAvailable[randomInt];
    // }
    public bool TryGetRandomAdjacentPiece(out MazePiece mazePiece, out int[] direction)
    {
        mazePiece = null;
        int randomInt;
        for (int i = 0; i < adjacentPieces.Length; i++)
        {
            randomInt = Game.instance.random.Next(adjacentPieces.Length);
            if (adjacentPieces[randomInt] == null) { continue; }
            if (adjacentPieces[randomInt].passed) { continue; }
            mazePiece = adjacentPieces[randomInt];
            direction = MazeGen.directions[randomInt];
            return true;
        }
        direction = null;
        return false;
    }
    public bool WallsActiveIsGrEqTo(int count = 3) 
    { 
        int wallsActive = 0;
        for (int i = 0; i < walls.Length; i++) { if (walls[i]) { wallsActive++; } }
        return wallsActive >= count;
    }
    public bool WallsActiveIsEqTo(int count = 3) 
    { 
        int wallsActive = 0;
        for (int i = 0; i < walls.Length; i++) { if (walls[i]) { wallsActive++; } }
        return wallsActive == count;
    }
}
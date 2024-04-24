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
        for (int i = 0; i < 4; i++) { walls[i] = true; adjacentPieces[i] = null; }
        gridIndex = null;
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
            if (direction.EqualTo(MazeGen.directions[i]))
            {
                walls[i] = false;
                break;
            }
        }
    }
    public void OpenDirection(int directionX, int directionZ)
    {
        for (int i = 0; i < walls.Length; i++)
        {
            if (directionX == MazeGen.directions[i][0] & directionZ == MazeGen.directions[i][1])
            {
                walls[i] = false;
                break;
            }
        }
    }
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
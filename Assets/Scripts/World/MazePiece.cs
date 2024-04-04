using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePiece
{
    public GameObject 
        debugArrow;
    public bool
        passed,
        debug,
        wallFwdEnabled,
        wallBackEnabled,
        wallLeftEnabled,
        wallRightEnabled;
    //List<GameObject>
    //    walls;
    //List<MazePiece>
    //    adjacentPieces;
    public Vector3Int 
        gridPosition,
        fromDirection,
        debugDirection;
    public MazePiece
        adjacentPieceFwd,
        adjacentPieceBack,
        adjacentPieceLeft,
        adjacentPieceRight;
    public Color 
        debugBoxColor = Color.red;
    //void Awake()
    //{
    //    walls = new List<GameObject>()
    //    {
    //        wallFwd,
    //        wallBack,
    //        wallLeft,
    //        wallRight
    //    };
    //}
    //void Update()
    //{
    //    //name = "mazePiece " + gridPosition.ToString();
    //    debugArrow.SetActive(passed && debug);
    //    if (passed && debug)
    //    { 
    //        Popcron.Gizmos.Bounds(new Bounds(transform.position + (Vector3Int.up * 10), Vector3.one * 10), debugBoxColor);
    //        debugArrow.transform.localEulerAngles = new Vector3(0f, (debugDirection).VectorNormalToCardinal().Euler(), 0f);
    //    }    
    //}
    public void Refresh()
    {
        //RandomizeWalls();
        EdgeCheck();
        GetAdjacentPieces();
    }
    //void RandomizeWalls(int amount = 3, bool edgeCheck = true)
    //{
    //    amount = Math.Clamp(amount, 0, 3);
    //    List<GameObject> randomWalls = new(walls);
    //    randomWalls.ForEach(wall => wall.SetActive(true));
    //    for (int i = 0; i < amount; i++)
    //    {
    //        int randomIndex = UnityEngine.Random.Range(0, randomWalls.Count);
    //        randomWalls[randomIndex].SetActive(false);
    //        randomWalls.RemoveAt(randomIndex);
    //    }
    //    if (edgeCheck) { EdgeCheck(); }
    //}
    void EdgeCheck()
    {
        wallFwdEnabled.IfFalseIgnore(gridPosition.z == MazeGen.instance.mazeSize.z - 1);
        wallBackEnabled.IfFalseIgnore(gridPosition.z == 0);
        wallLeftEnabled.IfFalseIgnore(gridPosition.x == 0);
        wallRightEnabled.IfFalseIgnore(gridPosition.x == MazeGen.instance.mazeSize.x - 1);
    }
    void GetAdjacentPieces()
    {
        adjacentPieceFwd = GetAdjacentPiece(Vector3Int.forward);    
        adjacentPieceBack = GetAdjacentPiece(Vector3Int.back);
        adjacentPieceLeft = GetAdjacentPiece(Vector3Int.left);
        adjacentPieceRight = GetAdjacentPiece(Vector3Int.right);
        //adjacentPieces = new()
        //{
        //    adjacentPieceFwd,
        //    adjacentPieceBack,
        //    adjacentPieceLeft,
        //    adjacentPieceRight
        //};
    }
    MazePiece GetAdjacentPiece(Vector3Int direction)
    {
        //Debug.Log("piece " + gridPosition + ", is getting piece at " + (gridPosition + direction));
        if (MazeGen.instance.mazePiecesLookup.TryGetValue(gridPosition + direction, out MazePiece mazePiece)) { return mazePiece; }
        else { return null; }
    }
    public void OpenDirection(Vector3Int direction)
    {
        wallFwdEnabled.IfFalseIgnore(direction == Vector3Int.forward, false);
        wallBackEnabled.IfFalseIgnore(direction == Vector3Int.back, false);
        wallLeftEnabled.IfFalseIgnore(direction == Vector3Int.left, false);
        wallRightEnabled.IfFalseIgnore(direction == Vector3Int.right, false);
    }
}
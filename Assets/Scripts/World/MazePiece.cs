using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePiece : MonoBehaviour
{
    public GameObject debugArrow;
    public bool
        wallFwdEnabled,
        wallBackEnabled,
        wallLeftEnabled,
        wallRightEnabled,
        passed,
        debug;
    [SerializeField] GameObject 
        wallFwd,
        wallBack,
        wallLeft,
        wallRight;
    public Vector3Int 
        gridPosition,
        fromDirection;
    public MazePiece
        adjacentPieceFwd,
        adjacentPieceBack,
        adjacentPieceLeft,
        adjacentPieceRight;
    public Color debugBoxColor = Color.red;
    void Awake()
    {

    }
    void Start()
    {
        
    }
    void Update()
    {
        name = "mazePiece " + gridPosition.ToString();
        wallFwd.SetActive(wallFwdEnabled);
        wallBack.SetActive(wallBackEnabled);
        wallLeft.SetActive(wallLeftEnabled);
        wallRight.SetActive(wallRightEnabled);
        debugArrow.SetActive(passed && debug);
        if (passed && debug)
        { 
            Popcron.Gizmos.Bounds(new Bounds(transform.position + (Vector3Int.up * 10), Vector3.one * 10), debugBoxColor);
            debugArrow.transform.localEulerAngles = new Vector3(0f, (-fromDirection).VectorNormalToCardinal().Euler(), 0f);
        }
    }
    public void Refresh()
    {
        //RandomizeWalls();
        EdgeCheck();
        GetAdjacentPieces();
    }
    void RandomizeWalls()
    {
        wallFwdEnabled = Extensions.RandomBool();
        wallBackEnabled = Extensions.RandomBool();
        wallLeftEnabled = Extensions.RandomBool();
        wallRightEnabled = Extensions.RandomBool();
    }
    void EdgeCheck()
    {
        wallFwdEnabled.IfFalseIgnore(gridPosition.z == MazeGen.instance.mazeSize.z - 1);
        wallBackEnabled.IfFalseIgnore(gridPosition.z == 0);
        wallLeftEnabled.IfFalseIgnore(gridPosition.x == 0);
        wallRightEnabled.IfFalseIgnore(gridPosition.x == MazeGen.instance.mazeSize.x - 1);
    }
    void GetAdjacentPieces()
    {
        adjacentPieceFwd = SetAdjacentPiece(Vector3Int.forward);
        adjacentPieceBack = SetAdjacentPiece(Vector3Int.back);
        adjacentPieceRight = SetAdjacentPiece(Vector3Int.right);
        adjacentPieceLeft = SetAdjacentPiece(Vector3Int.left);
    }
    MazePiece SetAdjacentPiece(Vector3Int direction)
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
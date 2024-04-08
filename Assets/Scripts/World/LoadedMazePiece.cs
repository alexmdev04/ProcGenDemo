using UnityEngine;

public class LoadedMazePiece : MonoBehaviour
{
    public MazePiece mazePiece;
    public GameObject
        debugArrow;
    public GameObject
        wallFwd,
        wallBack,
        wallLeft,
        wallRight;
    void Update()
    {
        //Refresh();
    }
    public void Refresh()
    {
        gameObject.transform.position = mazePiece.gridPosition * MazeGen.instance.mazePieceSize;
        name = "mazePiece " + mazePiece.gridPosition.ToString();
        wallFwd.SetActive(mazePiece.wallFwdEnabled);
        wallBack.SetActive(mazePiece.wallBackEnabled);
        wallLeft.SetActive(mazePiece.wallLeftEnabled);
        wallRight.SetActive(mazePiece.wallRightEnabled);
        debugArrow.SetActive(mazePiece.passed && mazePiece.debug);
        if (mazePiece.passed && mazePiece.debug)
        {
            Popcron.Gizmos.Bounds(new Bounds(transform.position + (Vector3Int.up * 10), Vector3.one * 10), mazePiece.debugBoxColor);
            debugArrow.transform.localEulerAngles = new Vector3(0f, mazePiece.debugDirection.VectorNormalToCardinal().Euler(), 0f);
        }
    }
}

using System.Text;
using UnityEngine;

public class LoadedMazePiece : MonoBehaviour
{
    public MazePiece 
        mazePiece;
    public GameObject
        debugArrow;
    public GameObject
        wallFwd,
        wallBack,
        wallLeft,
        wallRight;
    const string 
    str_mazePiece = "mazePiece ";
    public void Refresh()
    {
        gameObject.transform.position = mazePiece.gridPosition * MazeGen.instance.mazePieceSize;
        name = new StringBuilder(str_mazePiece).Append(mazePiece.gridPosition.ToStringBuilder()).ToString();
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

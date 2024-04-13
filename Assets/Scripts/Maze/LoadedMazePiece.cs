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
        gameObject.transform.position = mazePiece.gridIndex.Multiply(MazeGen.instance.mazePieceSize).ToVector();
        name = new StringBuilder(str_mazePiece).Append(mazePiece.gridIndex.ToStringBuilder()).ToString();
        wallFwd.SetActive(mazePiece.walls[0]);
        wallBack.SetActive(mazePiece.walls[1]);
        wallLeft.SetActive(mazePiece.walls[2]);
        wallRight.SetActive(mazePiece.walls[3]);
        debugArrow.SetActive(mazePiece.passed && mazePiece.debug);
    }
    void Update()
    {
        if (mazePiece.debug)
        {
            Popcron.Gizmos.Bounds(new Bounds(transform.position + new Vector3(5f, 10f, 5f), Vector3.one * 10f), mazePiece.debugBoxColor);
            debugArrow.transform.localEulerAngles = new Vector3(0f, mazePiece.toDirection.ToVector().VectorNormalToCardinal().Euler(), 0f);
        }
    }
}

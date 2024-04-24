using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

public class LoadedMazePiece : MonoBehaviour
{
    public MazePiece 
        mazePiece;
    public GameObject
        wallFwd,
        wallBack,
        wallLeft,
        wallRight;
    public Collectable
        paper;
    const string 
        str_mazePiece = "mazePiece ";
    public void Refresh()
    {
        gameObject.transform.position = mazePiece.gridIndex.GridIndexToWorldPosition();
        name = new StringBuilder(str_mazePiece).Append(mazePiece.gridIndex.ToStringBuilder()).ToString();
        wallFwd.SetActive(mazePiece.walls[0]);
        wallBack.SetActive(mazePiece.walls[1]);
        wallLeft.SetActive(mazePiece.walls[2]);
        wallRight.SetActive(mazePiece.walls[3]);
        paper.gameObject.SetActive(mazePiece.hasPaper);
    }
}

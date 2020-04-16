using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    [SerializeField]
    private int tileIndex;
    public int towerLevel;
    public Point GridPosition { get; set; }

    public Vector2 WorldPostion {
        get {
            return new Vector2(transform.position.x + GetComponent<SpriteRenderer>().bounds.size.x/3,
                transform.position.y - GetComponent<SpriteRenderer>().bounds.size.y / 3);
        }
    }

    public void Setup(Point gridPos, Vector3 worldPos, Transform parent) {
        GridPosition = gridPos;
        transform.position = worldPos;
        transform.SetParent(parent);
        LevelManager.Instance.Tiles.Add(gridPos, this);       
    }

    //Makes the UI visible when the tower spawn point is pressed
    private void OnMouseDown() {
        GameManager.Instance.ShowTowerTypeBtn(GridPosition, 
            LevelManager.Instance.Tiles[GridPosition].towerLevel );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int tileIndex;

    //Tower
    public int towerLevel;
    public bool towerLevelMax = false;

    public SpriteRenderer SpriteRenderer;
    public Point GridPosition { get; set; }
    public int x;
    public int y;

    public Vector2 WorldPostion {
        get {
            return new Vector2(transform.position.x + GetComponent<SpriteRenderer>().bounds.size.x/2,
                transform.position.y - GetComponent<SpriteRenderer>().bounds.size.y /2);
        }
    }

    private void Start() {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        x = GridPosition.x;
        y = GridPosition.y;
    }


    public void Setup(Point gridPos, Vector3 worldPos, Transform parent) {
        GridPosition = gridPos;
        transform.position = worldPos;
        transform.SetParent(parent);
        LevelManager.Instance.Tiles.Add(gridPos, this);       
    }
    
}

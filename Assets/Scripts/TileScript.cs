using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int tileIndex;

    //Tower
    public int TowerLevel;
    public bool towerLevelMax = false;

    public SpriteRenderer SpriteRenderer;
    public Point GridPosition { get; set; }

    public Vector3[] SoldierPos { get; set; }
    public Vector2 WorldPostion {
        get {
            return new Vector2(transform.position.x + GetComponent<SpriteRenderer>().bounds.size.x/2,
                transform.position.y - GetComponent<SpriteRenderer>().bounds.size.y /2);
        }
    }

    private void Start() {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SoldierPos = new Vector3[3];
    }


    public void Setup(Point gridPos, Vector3 worldPos, Transform parent) {
        GridPosition = gridPos;
        transform.position = worldPos;
        transform.SetParent(parent);
        LevelManager.Instance.Tiles.Add(gridPos, this);
    }

    public Vector3[] SetSoldierPos(Vector3 standardPos) {
        SoldierPos[0] = standardPos + new Vector3(-GetComponent<SpriteRenderer>().bounds.size.x / 3, GetComponent<SpriteRenderer>().bounds.size.y / 3,0);
        SoldierPos[1] = standardPos + new Vector3(0, -GetComponent<SpriteRenderer>().bounds.size.y / 3,0);
        SoldierPos[2] = standardPos + new Vector3(GetComponent<SpriteRenderer>().bounds.size.x / 3, GetComponent<SpriteRenderer>().bounds.size.y / 3,0);
        return SoldierPos;
    }
    
}

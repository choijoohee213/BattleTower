using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int tileIndex;

    //****This is only for debugging needs to be removed later!*****
    public int GScore;
    public int HScore;
    public int FScore;
    //*/

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

    /*
    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(pos, Vector2.zero);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);


            if (tileIndex != 2 || towerLevelMax.Equals(true))
                return;
            print("not return");

            TileScript tile;
            tile = hit.collider.gameObject.GetComponent<TileScript>();
            GameManager.Instance.ShowTowerTypeBtn(
                tile.GridPosition, tile.towerLevel);
        }
    }
    */

    public void Setup(Point gridPos, Vector3 worldPos, Transform parent) {
        GridPosition = gridPos;
        transform.position = worldPos;
        transform.SetParent(parent);
        LevelManager.Instance.Tiles.Add(gridPos, this);       
    }

    //Makes the UI visible when the tower spawn point is pressed
    private void OnMouseDown() {
        if (tileIndex != 2 || towerLevelMax.Equals(true))
            return;
        GameManager.Instance.ShowTowerTypeBtn(GridPosition, towerLevel);
    }
}

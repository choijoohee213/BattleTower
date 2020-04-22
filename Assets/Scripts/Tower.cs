using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public int towerPrice;

    [SerializeField]
    private Sprite[] towerSprites;

    private SpriteRenderer sr;
        
    public Point GridPosition { get; set; }

    public GameObject towerRange {
        get {
            return transform.GetChild(0).gameObject;
        }
    }

    private void Start() {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(Point gridPos, Vector3 worldPos) {
        GridPosition = gridPos;
        transform.position = worldPos + new Vector3(0,0.13f,0);
        GameManager.Instance.SetTowerParent(this);
        GameManager.Instance.Towers.Add(gridPos, this);
        LevelManager.Instance.Tiles[gridPos].towerLevel++;
    }  

    public void LevelUp() {
        int level = ++LevelManager.Instance.Tiles[GridPosition].towerLevel;
       
        //Stop upgrading when the tower level is at its maximum
        if (level.Equals(towerSprites.Length-1))
            LevelManager.Instance.Tiles[GridPosition].towerLevelMax = true;

        //Replace with next level sprite
        sr.sprite = towerSprites[level];      
    }
}

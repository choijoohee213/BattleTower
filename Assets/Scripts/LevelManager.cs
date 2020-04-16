using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class LevelManager : Singleton<LevelManager>
{
    [SerializeField]
    private GameObject[] tilePrefabs;

    [SerializeField]
    private CameraMovement cameraMovement;

    [SerializeField]
    private GameObject greenPortalPrefab, purplePortalPrefab, spawnTowerPrefab;

    [SerializeField]
    private Transform map, canvas;

    private Point greenSpawn, purpleSpawn;

    public Dictionary<Point, TileScript> Tiles { get; set; }
    //public Dictionary<Point, SpawnTower> SpawnTowerUI { get; set; }

    public float TileSize {
        get { return tilePrefabs[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x; }
    }

    void Start() {
        CreateLevel();
    }

    void CreateLevel(){
        Tiles = new Dictionary<Point, TileScript>();
        //SpawnTowerUI = new Dictionary<Point, SpawnTower>();

        string[] mapData = ReadLevelText();

        int mapX = mapData[0].ToCharArray().Length;
        int mapY = mapData.Length;

        Vector3 maxTile = Vector3.zero;

        //Calculates the world start point, this is the top left corner of the screen
        Vector3 worldStart = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height));
        
        for(int y = 0; y < mapY; y++) {  //The y positions
            char[] newTiles = mapData[y].ToCharArray();
            for(int x = 0; x < mapX; x++) {  //The x positions
                PlaceTile(newTiles[x].ToString(), x, y, worldStart);               
            }
        }
        maxTile = Tiles[new Point(mapX - 1, mapY - 1)].transform.position;
        cameraMovement.SetLimits(new Vector3(maxTile.x + TileSize, maxTile.y - TileSize));
        SpawnPortal(mapX-2, mapY-3);    
    }

    string[] ReadLevelText() {
        TextAsset bindData = Resources.Load("Level1") as TextAsset;
        string data = bindData.text.Replace(Environment.NewLine, string.Empty);
        return data.Split('-');
    }

    void PlaceTile(string tileType, int x, int y, Vector3 worldStart) {
        int tileIndex = int.Parse(tileType);

        //Creates a new tile and makes a reference to that tile in the newTile variable
        TileScript newTile = Instantiate(tilePrefabs[tileIndex]).GetComponent<TileScript>();
        newTile.Setup(new Point(x, y), new Vector3(worldStart.x + TileSize * x, worldStart.y - TileSize * y, 0), map);

        /*
        //In case of spawn tile, ui object is also created
        if (tileIndex.Equals(2)) {
            SpawnTower selectUI = Instantiate(spawnTowerPrefab).GetComponent<SpawnTower>();
            selectUI.Setup(new Point(x, y), Tiles[new Point(x,y)].WorldPostion + new Vector2(0.3f, -0.3f), canvas);
        }
        */
    }

    void SpawnPortal(int x, int y) {
        greenSpawn = new Point(0,0);
        Instantiate(greenPortalPrefab, Tiles[greenSpawn].GetComponent<TileScript>().WorldPostion + new Vector2(0,-0.17f), Quaternion.identity);

        purpleSpawn = new Point(x, y);
        Instantiate(purplePortalPrefab, Tiles[purpleSpawn].GetComponent<TileScript>().WorldPostion + new Vector2(0, -0.17f), Quaternion.identity);
    }
}

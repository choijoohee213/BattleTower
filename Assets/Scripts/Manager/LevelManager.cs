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
    private Transform map, canvas;

    public Point greenSpawn, purpleSpawn, mapSize;

    private Stack<Node> path;
    
    public Stack<Node> Path {
        get {
            if (path == null)
                GeneratePath();
            return new Stack<Node>(new Stack<Node>(path));
        }
    }

    public GameObject greenPortal, purplePortal;

    public Dictionary<Point, TileScript> Tiles { get; set; }

    public float TileSize = 1.0f;


    public void CreateLevel(){
        Tiles = new Dictionary<Point, TileScript>();

        string[] mapData = ReadLevelText();

        int mapX = mapData[0].ToCharArray().Length;
        int mapY = mapData.Length;

        mapSize = new Point(mapX, mapY);

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
        SpawnPortal();    
    }

    string[] ReadLevelText() {
        TextAsset bindData = Resources.Load("Level1") as TextAsset;
        string data = bindData.text.Replace(Environment.NewLine, string.Empty);
        return data.Split('-');
    }

    void PlaceTile(string tileType, int x, int y, Vector3 worldStart) {
        int tileIndex = int.Parse(tileType);
        if(tileIndex.Equals(8)) {
            greenSpawn = new Point(x, y);
            tileIndex = 1;
        }
        else if(tileIndex.Equals(9)) {
            purpleSpawn = new Point(x, y);
            tileIndex = 1;
        }

        //Creates a new tile and makes a reference to that tile in the newTile variable
        TileScript newTile = Instantiate(tilePrefabs[tileIndex]).GetComponent<TileScript>();
        newTile.Setup(new Point(x, y), new Vector3(worldStart.x + TileSize * x, worldStart.y - TileSize * y, 0), map);

        
    }

    void SpawnPortal() {
        greenPortal.transform.position = Tiles[greenSpawn].GetComponent<TileScript>().WorldPostion + new Vector2(0, 0.1f);
        purplePortal.transform.position = Tiles[purpleSpawn].GetComponent<TileScript>().WorldPostion + new Vector2(0, 0.1f);
    }
   
    
    public bool InBounds(Point position) {
        return position.x >= 0 && position.y >= 0
            && position.x < mapSize.x && position.y < mapSize.y;
    }

    public void GeneratePath() {
        path = AStar.GetPath(greenSpawn, purpleSpawn);
    }
}

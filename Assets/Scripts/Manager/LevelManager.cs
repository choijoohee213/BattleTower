using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : Singleton<LevelManager> {

    int gameLevel = 1;
    public int GameLevel { get => gameLevel; }

    Tilemap map;

    public TileScript Tile { get; set; }

    [SerializeField]
    new CameraMovement camera;

    public GameObject greenPortal, purplePortal;

    private Stack<Vector3> path;

    public Stack<Vector3> Path {
        get {
            if(path == null)
                GeneratePath();
            return new Stack<Vector3>(new Stack<Vector3>(path));
        }
    }

    public Dictionary<Point, TileScript> SpawnPoints;

    void Start() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        CreateLevel();       
    }

    void CreateLevel() {
        map = GameManager.Instance.objectManager.GetObject("Level"+gameLevel.ToString()).GetComponent<Tilemap>();
        map.transform.SetParent(transform);
        map.CompressBounds();

        //Camera Setting
        camera.SetLimits(map.CellToWorld(map.cellBounds.max), map.CellToWorld(map.cellBounds.min));
        
        map = map.transform.GetChild(0).GetComponent<Tilemap>();
        map.RefreshAllTiles();
        map.ResizeBounds();
        Tile = map.GetComponent<TileScript>();
        
        GetWorldTiles();
    }

    // Use this for initialization
    void GetWorldTiles() {
        SpawnPoints = new Dictionary<Point, TileScript>();
        foreach(Vector3Int pos in map.cellBounds.allPositionsWithin) {
            print(pos);
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if(!map.HasTile(localPlace))
                continue;
            var tile = new TileScript {
                LocalPlace = localPlace,
                WorldLocation = map.CellToWorld(localPlace),
                GridPosition = new Point(localPlace.x, localPlace.y),
                TileBase = map.GetTile(localPlace),
                TilemapMember = map,
                TowerLevel = 0,
                TowerLevelMax = false
            };
            SpawnPoints.Add(tile.GridPosition, tile);
            print("world" + tile.WorldLocation);
            print("yaya");

        }
        SpawnPortal();
    }

    void SpawnPortal() {
        greenPortal.transform.position = Tile.GreenPortalPos.position;
        purplePortal.transform.position = Tile.PurplePortalPos.position;
    }

    public void GeneratePath() {
        path = new Stack<Vector3>();
        foreach(Transform pos in Tile.CheckPoints) {
            path.Push(pos.position);
        }
    }




}

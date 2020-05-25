using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Paths {
    public Stack<Vector3> path;

    public Stack<Vector3> Path {
        get {
            return new Stack<Vector3>(new Stack<Vector3>(path));
        }
    }
}

public class LevelManager : Singleton<LevelManager> {

    int gameLevel;
    public int GameLevel { get => gameLevel; }

    Tilemap map, spawnMap;
    Grid grid;

    public TileScript Tile { get; set; }

    

    [SerializeField]
    new CameraMovement camera;

    [SerializeField]
    Animator Loading;

    public GameObject[] greenPortal;
    public GameObject purplePortal;

    public List<Paths> MapPaths;

    [SerializeField]
    private Sprite[] walkAbleTiles;

    public Dictionary<Point, TileScript> SpawnPoints;

    void Start() {
        grid = GetComponent<Grid>();
        Loading.SetTrigger("FadeIn");
        gameLevel = MainMenuManager.Instance.selectedLevel;
        MapPaths = new List<Paths>();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        CreateLevel();       
    }

    void CreateLevel() {
        map = GameManager.Instance.objectManager.GetObject("Level"+gameLevel.ToString()).GetComponent<Tilemap>();
        map.transform.SetParent(transform);
        map.CompressBounds();

        //Camera Setting
        camera.SetLimits(map.CellToWorld(map.cellBounds.max), map.CellToWorld(map.cellBounds.min));
        
        spawnMap = map.transform.GetChild(0).GetComponent<Tilemap>();
        Tile = spawnMap.GetComponent<TileScript>();
        GetWorldTiles();
    }

    // Use this for initialization
    void GetWorldTiles() {
        SpawnPoints = new Dictionary<Point, TileScript>();
        foreach(Vector3Int pos in spawnMap.cellBounds.allPositionsWithin) {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if(!spawnMap.HasTile(localPlace))
                continue;
            var tile = new TileScript {
                LocalPlace = localPlace,
                WorldLocation = spawnMap.CellToWorld(localPlace),
                GridPosition = new Point(localPlace.x, localPlace.y),
                TileBase = spawnMap.GetTile(localPlace),
                TilemapMember = spawnMap,
                TowerLevel = 0,
                TowerLevelMax = false
            };
            SpawnPoints.Add(tile.GridPosition, tile);
        }
        SpawnPortal();
    }

    void SpawnPortal() {
        for(int i = 0; i < Tile.GreenPortalPos.Length; i++) {
            greenPortal[i].gameObject.SetActive(true);
            greenPortal[i].transform.position = Tile.GreenPortalPos[i].position;
            greenPortal[i].transform.rotation = Tile.GreenPortalPos[i].rotation;
        }

        purplePortal.transform.position = Tile.PurplePortalPos.position;
        purplePortal.transform.rotation = Tile.PurplePortalPos.rotation;
    }

    public void GeneratePath(int wayIndex) {
        MapPaths.Add(new Paths());
        MapPaths[wayIndex].path = new Stack<Vector3>();
        foreach(Transform pos in Tile.MonsterWay[wayIndex].CheckPoints) {
            MapPaths[wayIndex].path.Push(pos.position);
        }
    }

    public bool IsWalkableTile(Vector3 pos) {
        var tilePos = grid.WorldToCell(pos);
        var _tileSprite = map.GetSprite(tilePos);
        foreach(Sprite tile in walkAbleTiles) {
            if(tile.Equals(_tileSprite)) {
                return true;
            }
        }
        return false;
    }


}

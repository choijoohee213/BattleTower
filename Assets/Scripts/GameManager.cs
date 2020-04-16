using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    int selectIndex;

    [SerializeField]
    private GameObject spawnTowerUI, okBtn;

    [SerializeField]
    private GameObject[] towerTypesBtn, towerPrefabs;

    [SerializeField]
    private Transform towerParent;

    public Point selectSpawnUI;

    public void SetTowerParent(Tower tower) {
        tower.transform.SetParent(towerParent);
    }

    public Dictionary<Point, Tower> Towers { get; set; }

    private void Start() {
        Towers = new Dictionary<Point, Tower>();
    }

    //Shows UI to select tower to build
    public void ShowTowerTypeBtn(Point pos, int level) {
        selectSpawnUI = pos;
        spawnTowerUI.transform.position = LevelManager.Instance.Tiles[pos].WorldPostion + new Vector2(0.3f, -0.3f);
        spawnTowerUI.SetActive(true);
       
        for (int i = 0; i < 4; i++)
            towerTypesBtn[i].SetActive(level.Equals(0));

        for (int i = 4; i < towerTypesBtn.Length; i++)
            towerTypesBtn[i].SetActive(!level.Equals(0));

        okBtn.SetActive(false);
    }

    //OK button and tower information are displayed when the tower type button is pressed
    public void PressTowerTypeBtn(int index) {
        selectIndex = index;
        towerTypesBtn[selectIndex].SetActive(false);
        okBtn.SetActive(true);
        okBtn.transform.position = towerTypesBtn[selectIndex].transform.position;
    }

    //Create a tower object when the OK button is pressed
    public void PressOkBtn() {
        Tower tower = Instantiate(towerPrefabs[selectIndex]).GetComponent<Tower>();
        tower.Setup(selectSpawnUI, spawnTowerUI.transform.position + new Vector3(0, 0.1f, 0));
        spawnTowerUI.SetActive(false);
    }
}

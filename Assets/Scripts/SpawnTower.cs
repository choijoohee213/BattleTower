using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTower : MonoBehaviour
{
    int selectIndex;
    
    [SerializeField]
    private GameObject spawnTowerUI, okBtn;

    [SerializeField]
    private GameObject[] towerTypesBtn, towerPrefabs;
    public Point GridPosition { get; set; }


    public void Setup(Point gridPos, Vector3 worldPos, Transform parent) {
        GridPosition = gridPos;
        transform.position = worldPos;
        transform.SetParent(parent);
        LevelManager.Instance.SpawnTowerUI.Add(gridPos, this);
    }


    //Shows UI to select tower to build
    public void ShowTowerTypeBtn() {
        spawnTowerUI.SetActive(true);
        for (int i = 0; i < towerTypesBtn.Length; i++)
            towerTypesBtn[i].SetActive(true);
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
        tower.Setup(GridPosition, transform.position + new Vector3(0, 0.1f, 0));
        spawnTowerUI.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {
    int selectBtnIndex;
    bool islevelChanged;

    [SerializeField]
    private GameObject spawnTowerUI, towerInformUI, okBtn, towerPriceUI, upgradePriceUI;

    [SerializeField]
    private GameObject[] towerTypesBtn, towerPrefabs;

    [SerializeField]
    private Transform towerParent;

    [SerializeField]
    private Text upgradePrice;

    [SerializeField]
    private Sprite[] towerSprites;

    [SerializeField]
    private SpriteRenderer towerImg, towerRangeImg;

    public ObjectManager objectManager;

    public Point selectSpawnPos;

    public Dictionary<Point, Tower> Towers { get; set; }


    private void Start() {
        Towers = new Dictionary<Point, Tower>();
    }


    /// <summary>
    /// The following is a function related to Tower Spawn
    /// </summary>

    public void SetTowerParent(Tower tower) {
        tower.transform.SetParent(towerParent);
    }

    //The UI for selecting the tower type is shown.
    public void ShowTowerTypeBtn(Point pos, int level) {
        spawnTowerUI.transform.position = LevelManager.Instance.Tiles[pos].WorldPostion;
        spawnTowerUI.SetActive(true);

        //Determine display UI    
        DisplayBuiltUI(level.Equals(0),pos);

        ChangeActivated(true);

        selectSpawnPos = pos;
    }

    //Tower object generation when OK button is pressed
    public void PressTowerTypeBtn(int index) {
        if(!islevelChanged)
            towerTypesBtn[selectBtnIndex].SetActive(true);
        
        selectBtnIndex = index;
        towerTypesBtn[selectBtnIndex].SetActive(false);

        ChangeActivated(false);
        okBtn.transform.position = towerTypesBtn[selectBtnIndex].transform.position;
        
        if (selectBtnIndex.Equals(4) || selectBtnIndex.Equals(5)) {
            towerImg.enabled = false;
            towerRangeImg.transform.localScale = Towers[selectSpawnPos].towerRange.transform.localScale + new Vector3(0.6f, 0.4f, 0);
            return;
        }
        towerImg.sprite = towerSprites[selectBtnIndex];
    }
   
    //Create, upgrade, or sell tower objects when you press the OK button.
    public void PressOkBtn() {
        if (selectBtnIndex.Equals(4)) { //Upgrade Tower
            Towers[selectSpawnPos].LevelUp();
            Towers[selectSpawnPos].towerRange.transform.localScale += new Vector3(0.6f, 0.4f, 0);
        } 
        else if (selectBtnIndex.Equals(5)) { //Sell Tower
            SellTower();
            return;
        } 
        else {  //Create(Buy) Tower
            Tower tower = Instantiate(towerPrefabs[selectBtnIndex]).GetComponent<Tower>();
            tower.Setup(selectSpawnPos, spawnTowerUI.transform.position + new Vector3(0, 0.1f, 0));
        }

        //Change Upgrade Price
        Towers[selectSpawnPos].towerPrice += 50;
        spawnTowerUI.SetActive(false);
        Towers[selectSpawnPos].towerRange.SetActive(false);
    }


    //Decide whether to show some UI depending on whether to build a tower
    void DisplayBuiltUI(bool levelZero,Point pos) {
        for (int i = 0; i < 4; i++)
            towerTypesBtn[i].SetActive(levelZero);
        for (int i = 4; i < towerTypesBtn.Length; i++)
            towerTypesBtn[i].SetActive(!levelZero);

        towerPriceUI.SetActive(levelZero);
        upgradePriceUI.SetActive(!levelZero);

        if (Towers.ContainsKey(selectSpawnPos)) 
            Towers[selectSpawnPos].towerRange.SetActive(false);           
        
        if (!levelZero) {
            upgradePrice.text = Towers[pos].towerPrice.ToString();
            Towers[pos].towerRange.SetActive(true);
        } else
            towerRangeImg.transform.localScale = new Vector3(6f, 4f);
    }

    void ChangeActivated(bool isActive) {
        islevelChanged = isActive;
        towerInformUI.SetActive(!isActive);
        towerRangeImg.enabled = !isActive;
        towerImg.enabled = !isActive;
        okBtn.SetActive(!isActive);
    }

    //If you sell the tower, the data of the spawn point is initialized.
    void SellTower() {
        Towers[selectSpawnPos].gameObject.SetActive(false); 
        Towers.Remove(selectSpawnPos);  //Delete from dictionary
        LevelManager.Instance.Tiles[selectSpawnPos].towerLevel = 0;
        LevelManager.Instance.Tiles[selectSpawnPos].towerLevelMax = false;
        spawnTowerUI.SetActive(false);
    }





    /// <summary>
    /// The following is a function related to Monster Spawn
    /// </summary>

    public void StartWave() {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave() {
        LevelManager.Instance.GeneratePath();

        int monsterIndex = Random.Range(0, 4);
        string type = string.Empty;
        switch (monsterIndex) {
            case 0:
                type = "YellowMonster";
                break;
            case 1:
                type = "GreyMonster";
                break;
            case 2:
                type = "BlueMonster";
                break;
            case 3:
                type = "RedMonster";
                break;
        }

        Monster monster = objectManager.GetObject(type).GetComponent<Monster>();
        monster.Spawn();

        yield return new WaitForSeconds(2.5f);
    }
}

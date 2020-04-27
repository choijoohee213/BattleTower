using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {
    int selectBtnIndex, wave, lives, health = 15;
    bool islevelChanged, gameOver = false;

    [SerializeField]
    private GameObject spawnTowerUI, towerInformUI, okBtn, towerPriceUI, upgradePriceUI, waveBtn, gameOverUI;

    [SerializeField]
    private GameObject[] towerTypesBtn, towerPrefabs;

    [SerializeField]
    private Transform towerParent;

    [SerializeField]
    private Text upgradePrice, waveText, livesText;

    [SerializeField]
    private Sprite[] towerSprites;

    [SerializeField]
    private SpriteRenderer towerImg, towerRangeImg;

    private List<Monster> activeMonsters = new List<Monster>();

    public bool WaveActive {
        get { return activeMonsters.Count > 0; }
    }

    public int Lives {
        get { return lives; }
        set {
            this.lives = value;

            if (lives <= 0) {
                this.lives = 0;
                GameOver();
            }
            livesText.text = value.ToString();
        }
    }

    public ObjectManager objectManager;

    public Point selectSpawnPos;

    public Dictionary<Point, Tower> Towers { get; set; }


    private void Start() {
        StartCoroutine(StartClickTile());
        Towers = new Dictionary<Point, Tower>();
        Lives = 10;
    }


    /// <summary>
    /// The following is a function related to Tower Spawn
    /// </summary>

    public void SetTowerParent(Tower tower) {
        tower.transform.SetParent(towerParent);
    }

    //Makes the UI visible when the tower spawn point is pressed
    IEnumerator StartClickTile() {
        while (true) {
            //Only if you don't click the UI with the left mouse button pressed
            if (Input.GetMouseButton(0) && !IsPointerOverUIObject(Input.mousePosition))
                ClickTile();
            yield return new WaitForSeconds(0.08f);
        }
    }

    void ClickTile() {
        int layerMask = 1 << LayerMask.NameToLayer("SpawnTower");  // Everything에서 해당 레이어만 충돌 체크함
        int layerMask2 = 1 << LayerMask.NameToLayer("TowerRange");

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0.1f, layerMask);
        RaycastHit2D hit2 = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0.1f, ~layerMask2);

        if (hit.collider != null) {
            TileScript tmp = hit.collider.GetComponent<TileScript>();
            if (tmp != null && tmp.tileIndex == 2 && tmp.towerLevelMax.Equals(false)) {
                ShowTowerTypeBtn(tmp.GridPosition, tmp.towerLevel, true);
            }
        }

        else if (hit2.collider != null && !hit2.collider.CompareTag("UI")) {
            spawnTowerUI.SetActive(false);
            if (Towers.ContainsKey(selectSpawnPos))
                Towers[selectSpawnPos].towerRange.enabled = false;
        }

    }

    public bool IsPointerOverUIObject(Vector2 mousePos) {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = mousePos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }


    //The UI for selecting the tower type is shown.
    public void ShowTowerTypeBtn(Point pos, int level, bool isActive) {
        spawnTowerUI.transform.position = LevelManager.Instance.Tiles[pos].WorldPostion;
        spawnTowerUI.SetActive(isActive);

        //Determine display UI    
        DisplayBuiltUI(level.Equals(0), pos);

        ChangeActivated(isActive);

        selectSpawnPos = pos;
    }


    //Tower object generation when OK button is pressed
    public void PressTowerTypeBtn(int index) {
        if (!islevelChanged)
            towerTypesBtn[selectBtnIndex].SetActive(true);

        selectBtnIndex = index;
        towerTypesBtn[selectBtnIndex].SetActive(false);

        ChangeActivated(false);
        okBtn.transform.position = towerTypesBtn[selectBtnIndex].transform.position;

        if (selectBtnIndex.Equals(4) || selectBtnIndex.Equals(5)) {
            towerImg.enabled = false;
            towerRangeImg.transform.localScale = Towers[selectSpawnPos].towerRange.transform.localScale + new Vector3(1f, 1f, 0);
            return;
        }
        towerImg.sprite = towerSprites[selectBtnIndex];
    }

    //Create, upgrade, or sell tower objects when you press the OK button.
    public void PressOkBtn() {
        if (selectBtnIndex.Equals(4)) { //Upgrade Tower
            Towers[selectSpawnPos].LevelUp();
            Towers[selectSpawnPos].towerRange.transform.localScale += new Vector3(1f, 1f, 0);
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
        Towers[selectSpawnPos].towerRange.enabled = false;
    }


    //Decide whether to show some UI depending on whether to build a tower
    void DisplayBuiltUI(bool levelZero, Point pos) {
        for (int i = 0; i < 4; i++)
            towerTypesBtn[i].SetActive(levelZero);
        for (int i = 4; i < towerTypesBtn.Length; i++)
            towerTypesBtn[i].SetActive(!levelZero);

        towerPriceUI.SetActive(levelZero);
        upgradePriceUI.SetActive(!levelZero);

        if (Towers.ContainsKey(selectSpawnPos))
            Towers[selectSpawnPos].towerRange.enabled = false;

        if (!levelZero) {
            upgradePrice.text = Towers[pos].towerPrice.ToString();
            Towers[pos].towerRange.enabled = true;
        }
        else
            towerRangeImg.transform.localScale = new Vector3(4f, 4f);
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
        wave++;
        waveText.text = "공격 " + wave.ToString() + "/10";
        StartCoroutine(SpawnWave());
        waveBtn.SetActive(false);
    }

    IEnumerator SpawnWave() {
        LevelManager.Instance.GeneratePath();

        for (int i = 0; i < wave; i++) {
            int monsterIndex = 0; //Random.Range(0, 4);
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
            monster.Spawn(health);



            if((wave % 3).Equals(0)) {
                health += 5;
            }

            activeMonsters.Add(monster);

            yield return new WaitForSeconds(2.5f);
        }
    }

    public void RemoveMonster(Monster monster) {
        activeMonsters.Remove(monster);
        if (!WaveActive && !gameOver) {
            waveBtn.SetActive(true);
        }
    }


    public void GameOver() {
        if (!gameOver) {
            gameOver = true;
            Time.timeScale = 0;
            gameOverUI.SetActive(true);
        }
    }

    public void GameRetry() {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameQuit() {
        Application.Quit();
    }
}

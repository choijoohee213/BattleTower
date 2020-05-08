using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : Singleton<GameManager> {
    int selectBtnIndex, wave, money, lives;
    bool islevelChanged, gameOver = false, levelZero = true;
    
    public int[] towerPrices = new int[4] { 70, 80, 90, 80 };
    bool[] purchasable = new bool[6] { true, true, true, true, true, true };

    [SerializeField]
    private GameObject spawnTowerUI, towerDescription, okBtn, towerPriceUI, upgradePriceUI, waveBtn, gameOverUI, towerInformUI, pauseUI, panel;

    [SerializeField]
    private GameObject[] towerTypesBtn, towerPrefabs;

    [SerializeField]
    private Transform towerParent;

    [SerializeField]
    private Text upgradePrice, waveText, moneyText, livesText;

    [SerializeField]
    private Text[] descriptionTexts, towerInformTexts;

    [SerializeField]
    private Sprite[] towerSprites;

    [SerializeField]
    private SpriteRenderer towerImg, towerRangeImg, towerIcon;

    private List<Monster> activeMonsters = new List<Monster>();

    public bool WaveActive {
        get { return activeMonsters.Count > 0; }
    }

    public int Lives {
        get { return lives; }
        set {
            lives = value;

            if (lives <= 0) {
                lives = 0;
                GameOver();
            }
            livesText.text = value.ToString();
        }
    }

    public int Money {
        get { return money; }
        set {
            money += value;
            moneyText.text = money.ToString();
        }
    }

    public ObjectManager objectManager;
    public DataManager dataManager;

    public Point selectSpawnPos;

    public Dictionary<Point, Tower> Towers { get; set; }



    private void Start() {
        StartCoroutine(StartUpdate());
        Towers = new Dictionary<Point, Tower>();

        Lives = 10;
        Money = 2000;

    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            ShowGameMenu();      
    }


    /// <summary>
    /// The following is a function related to Tower Spawn
    /// </summary>

    public void SetTowerParent(Tower tower) {
        tower.transform.SetParent(towerParent);
    }

    //Makes the UI visible when the tower spawn point is pressed
    IEnumerator StartUpdate() {
        while (true) {

            //Only if you don't click the UI with the left mouse button pressed
            if(Input.GetMouseButton(0)) {
                if(!EventSystem.current.IsPointerOverGameObject())
                    ClickTile();
            }
            
            yield return new WaitForSeconds(0.01f);
        }
    }

    void ClickTile() {
        int layerMask = 1 << LayerMask.NameToLayer("SpawnTower");  // Everything에서 해당 레이어만 충돌 체크함
        int layerMask2 = 1 << LayerMask.NameToLayer("TowerRange");
        
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0.1f, layerMask);
        RaycastHit2D hit2 = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0.1f, ~layerMask2);

        //When you click the tower spawn point
        if(hit.collider != null) {   
            TileScript tmp = hit.collider.GetComponent<TileScript>();
            if (tmp != null && tmp.tileIndex == 2) {
                    ShowTowerTypeBtn(tmp.GridPosition, tmp.towerLevel, true);
            }
        }

        //When you click a place other than the tower spawn point
        else if(hit2.collider != null && !hit2.collider.CompareTag("UI")) {
            spawnTowerUI.SetActive(false);
            if(Towers.ContainsKey(selectSpawnPos)) {
                Towers[selectSpawnPos].towerRange.enabled = false;
                towerInformUI.SetActive(false);
            }
        }

    }


    //The UI for selecting the tower type is shown.
    public void ShowTowerTypeBtn(Point pos, int level, bool isActive) {
        spawnTowerUI.transform.position = LevelManager.Instance.Tiles[pos].WorldPostion;
        spawnTowerUI.SetActive(isActive);
        levelZero = level.Equals(0);

        //Determine display UI    
        DisplayBuiltUI(levelZero, pos);
        ChangeActivated(isActive);

        selectSpawnPos = pos;
        //OnCurrencyChanged();
        PriceCheck();
    }

    
    //Tower object generation when OK button is pressed
    public void PressTowerTypeBtn(int index) {
        if (!islevelChanged)
            towerTypesBtn[selectBtnIndex].SetActive(true);

        selectBtnIndex = index;
        towerTypesBtn[selectBtnIndex].SetActive(false);

        ChangeActivated(false);
        TowerDescription();
        okBtn.transform.position = towerTypesBtn[selectBtnIndex].transform.position;
        
        if (!purchasable[selectBtnIndex]) okBtn.GetComponent<Image>().color = Color.grey;
        else okBtn.GetComponent<Image>().color = Color.white;


        if (selectBtnIndex.Equals(4) || selectBtnIndex.Equals(5)) {
            towerImg.enabled = false;
            towerRangeImg.transform.localScale = Towers[selectSpawnPos].towerRange.transform.localScale + new Vector3(1f, 1f, 0);
            return;
        }
        towerImg.sprite = towerSprites[selectBtnIndex];
    }


    //Create, upgrade, or sell tower objects when you press the OK button.
    public void PressOkBtn() {
        if (selectBtnIndex.Equals(5)) { //Sell Tower
            SellTower();
            return;
        }

        if (!purchasable[selectBtnIndex]) return;

        if (selectBtnIndex.Equals(4)) { //Upgrade Tower
            Money = -Towers[selectSpawnPos].towerPrice;
            Towers[selectSpawnPos].LevelUp();
            Towers[selectSpawnPos].towerRange.transform.localScale += new Vector3(1f, 1f, 0);
        }

        if(!selectBtnIndex.Equals(4) && !selectBtnIndex.Equals(5)){  //Create(Buy) Tower
            Money = -towerPrices[selectBtnIndex];
            Tower tower = objectManager.GetObject(dataManager.towerNamesENG[selectBtnIndex]).GetComponent<Tower>();
            tower.Setup(selectSpawnPos, spawnTowerUI.transform.position + new Vector3(0, 0.1f, 0));
        }

        
        Towers[selectSpawnPos].towerPrice += 50;   //Change Upgrade Price
        spawnTowerUI.SetActive(false);
        towerInformUI.SetActive(false);
        Towers[selectSpawnPos].towerRange.enabled = false;
    }


    //Determine if you can buy with your current money
    void PriceCheck() {
        if (levelZero) {
            for (int i = 0; i < 4; i++) {
                if (towerPrices[i] > money) {
                    towerTypesBtn[i].GetComponent<Image>().color = Color.grey;
                    purchasable[i] = false;
                }
                else {
                    towerTypesBtn[i].GetComponent<Image>().color = Color.white;
                    purchasable[i] = true;
                }
            }
        }

        else {
            if (Towers[selectSpawnPos].towerPrice > money) {
                towerTypesBtn[4].GetComponent<Image>().color = Color.grey;
                purchasable[4] = false;
            }
            else {
                towerTypesBtn[4].GetComponent<Image>().color = Color.white;
                purchasable[4] = true;
            }
        }
    }

    //Decide whether to show some UI depending on whether to build a tower
    void DisplayBuiltUI(bool levelZero, Point pos) {
        for (int i = 0; i < 4; i++)
            towerTypesBtn[i].SetActive(levelZero);
        for (int i = 4; i < towerTypesBtn.Length; i++)
            towerTypesBtn[i].SetActive(!levelZero);

        towerPriceUI.SetActive(levelZero);
        upgradePriceUI.SetActive(!levelZero);
        towerInformUI.SetActive(!levelZero);

        if (Towers.ContainsKey(selectSpawnPos))
            Towers[selectSpawnPos].towerRange.enabled = false;

        if (!levelZero) {
            upgradePrice.text = Towers[pos].towerPrice.ToString();
            Towers[pos].towerRange.enabled = true;
            TowerInformation(pos);
        }
        else 
            towerRangeImg.transform.localScale = new Vector3(4f, 4f);

        if(!levelZero && LevelManager.Instance.Tiles[pos].towerLevelMax) {
            towerTypesBtn[4].SetActive(false);
            upgradePriceUI.SetActive(false);
        }
    }

    //Click to set the tower information to show to the UI
    void TowerInformation(Point pos) {
        towerIcon.sprite = towerTypesBtn[Towers[pos].towerIndex].GetComponent<Image>().sprite;

        //Get tower info and Change text
        towerInformTexts[0].text = dataManager.towerNamesKR[Towers[pos].towerIndex];
        towerInformTexts[1].text = "공격력 : " + Towers[pos].Damage.ToString();
        towerInformTexts[2].text = "공격속도 : " + Towers[pos].AttackCoolDown.ToString();
    }

    //Change various states
    void ChangeActivated(bool isActive) {
        islevelChanged = isActive;
        towerDescription.SetActive(!isActive);
        towerRangeImg.enabled = !isActive;
        towerImg.enabled = !isActive;
        okBtn.SetActive(!isActive);
    }

    void TowerDescription() {
        if (levelZero) {
            descriptionTexts[0].text = dataManager.towerNamesKR[selectBtnIndex];
            descriptionTexts[1].text = dataManager.towerDescriptions[selectBtnIndex];
            descriptionTexts[2].text = "공격력 : <color=#F68519>" + dataManager.towerOffensePower[selectBtnIndex].ToString() + "</color>";
            descriptionTexts[3].text = "공격속도 : <color=#F68519>" + dataManager.attackCoolDown[selectBtnIndex].ToString() + "</color>";
        }
        else {
            if (selectBtnIndex.Equals(4)) {
                descriptionTexts[0].text = dataManager.towerNamesKR[Towers[selectSpawnPos].towerIndex];
                descriptionTexts[1].text = dataManager.towerDescriptions[Towers[selectSpawnPos].towerIndex];
                descriptionTexts[2].text = "공격력 : <color=#F68519>" + (Towers[selectSpawnPos].Damage+1f).ToString() + "</color>";
                descriptionTexts[3].text = "공격속도 : <color=#F68519>" + Towers[selectSpawnPos].AttackCoolDown.ToString() + "</color>";
            }
            else {
                descriptionTexts[0].text = "타워 판매";
                descriptionTexts[1].text = "타워 가격의 1/2 만큼 반환됩니다.";
                descriptionTexts[2].text = "";
                descriptionTexts[3].text = "";
            }

        }
    }

    //If you sell the tower, the data of the spawn point is initialized.
    void SellTower() {
        Money = Towers[selectSpawnPos].towerPrice / 2;
        Towers[selectSpawnPos].Release();
               
        spawnTowerUI.SetActive(false);
        towerInformUI.SetActive(false);
    }



    /// <summary>
    /// The following is a function related to Monster Spawn
    /// </summary>

    public void StartWave() {
        wave++;
        waveText.text = "공격 <color=yellow>" + wave.ToString() + "</color>/10";
        StartCoroutine(SpawnWave());
        waveBtn.SetActive(false);
    }

    IEnumerator SpawnWave() {
        LevelManager.Instance.GeneratePath();

        for (int i = 0; i < wave; i++) {
            int monsterIndex = 0;//Random.Range(0,20);
            string type = dataManager.MonsterType(monsterIndex);

            //Create monsters and health bars
            Monster monster = objectManager.GetObject(type).GetComponent<Monster>();           
            GameObject bar = objectManager.GetObject("HealthBar");
            
            monster.gaugeBar = bar.transform.GetChild(0).GetComponent<Image>();
            monster.healthBar = bar;
            monster.Spawn();


            //if ((wave % 3).Equals(0)) {
            //    health += 5;
            //}

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





    /// <summary>
    /// The following is a function related to Game State
    /// </summary>
    /// 

    public void ShowGameMenu() {
        pauseUI.SetActive(!pauseUI.activeSelf);
        panel.SetActive(!panel.activeSelf);
        if (!pauseUI.activeSelf) {
            if (!WaveActive && !waveBtn.activeSelf) 
                waveBtn.SetActive(true);
            Time.timeScale = 1;          
        }
        else {
            if (!WaveActive && !gameOver && waveBtn.activeSelf) 
                waveBtn.SetActive(false);
            Time.timeScale = 0;   
        }
    }

    public void GameOver() {
        if (!gameOver) {
            gameOver = true;
            Time.timeScale = 0;
            panel.SetActive(!panel.activeSelf);
            gameOverUI.SetActive(true);
            waveBtn.SetActive(false);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {
    int selectBtnIndex, wave, lives, money, waveMax;
    bool islevelChanged, gameOver = false, levelZero = true, checkPointing = false;
    
    public int TowerLevelMax { get; set; }

    public int[] towerPrices = new int[4] { 70, 80, 90, 80 };
    bool[] purchasable = new bool[6] { true, true, true, true, true, true };

    [SerializeField]
    private GameObject spawnTowerUI, towerDescription, okBtn, towerPriceUI, upgradePriceUI, waveBtn, gameCompleteUI, gameOverUI, towerInformUI, pauseUI, panel;

    [SerializeField]
    private GameObject[] towerTypesBtn, towerPrefabs, soldierPosSign;
    public GameObject SoldierPosSign { get { return soldierPosSign[1]; } }

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

            if(lives <= 0) {
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
        Time.timeScale = 1;
        StartCoroutine(StartUpdate());
        Towers = new Dictionary<Point, Tower>();
    }



    /// <summary>
    // The following is a function related to Tower Spawn
    /// </summary>

    //Makes the UI visible when the tower spawn point is pressed
    IEnumerator StartUpdate() {
        while(true) {
            //Only if you don't click the UI with the left mouse button pressed
            if(Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())   //mobile : Input.GetTouch(0).fingerId
                ClickTile();
            if(Input.GetKeyDown(KeyCode.Escape))
                ShowGameMenu();
            yield return new WaitForSeconds(0.01f);
        }
    }

    void ClickTile() {
        int layerMask = 1 << LayerMask.NameToLayer("SpawnTower");  // Everything에서 해당 레이어만 충돌 체크함
        int layerMask2 = 1 << 9;  //TowerRange

        if(checkPointing) {
            RaycastHit2D hit3 = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0.1f, layerMask2);
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //클릭한 곳이 타워 범위 안인지를 확인
            if(hit3.collider != null && hit3.collider.name.Contains("TowerRange") && hit3.collider.transform.parent.gameObject.Equals(Towers[selectSpawnPos].gameObject)) {
                RaycastHit2D hit4 = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0.1f, 1 << 12);
                if(hit4.collider != null) {  //클릭한 곳이 타일맵인지 확인
                    //TileBase hitTile = hit4.collider.GetComponent<TileBase>();
                    if(LevelManager.Instance.IsWalkableTile(new Vector3(point.x, point.y, 0))) {  //클릭한 곳이 타일맵 중에서 갈수 있는 타일인지 확인
                        soldierPosSign[1].SetActive(true);
                        soldierPosSign[1].transform.position = new Vector3(point.x + 0.2f, point.y + 0.5f, 0);

                        Towers[selectSpawnPos].ChangeSoldierPos(point + new Vector3(0, 0, 10));
                        checkPointing = false;
                        Towers[selectSpawnPos].towerRange.enabled = false;
                    }
                    else ShowXSign(point);
                }
                else ShowXSign(point);
            }
            else ShowXSign(point);
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.zero, 0.1f, layerMask);
        RaycastHit2D hit2 = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, ~layerMask2);
        if(hit.collider != null ) {  //Display UI
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var worldPoint = new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), 0);
            TileScript tile =LevelManager.Instance.SpawnPoints[new Point(worldPoint.x, worldPoint.y)];
            //_tile.TilemapMember.SetTileFlags(_tile.LocalPlace, TileFlags.None);
            ShowTowerTypeBtn(tile.GridPosition, tile.TowerLevel, true);
           
        }
        else if(hit2.collider != null) {  //Hide UI
            spawnTowerUI.SetActive(false);
            if(Towers.ContainsKey(selectSpawnPos)) {
                Towers[selectSpawnPos].towerRange.enabled = false;
                towerInformUI.SetActive(false);
            }
        }
    }

    void ShowXSign(Vector3 point) {
        soldierPosSign[0].SetActive(true);
        soldierPosSign[0].transform.position = new Vector3(point.x + 0.2f, point.y + 0.2f, 0);
        soldierPosSign[0].GetComponent<Animator>().SetTrigger("XSign");
    }


    //The UI for selecting the tower type is shown.
    public void ShowTowerTypeBtn(Point pos, int level, bool isActive) {
        spawnTowerUI.transform.position = LevelManager.Instance.SpawnPoints[pos].WorldPostion;
        spawnTowerUI.SetActive(isActive);
        levelZero = level.Equals(0);

        //Determine display UI    
        DisplayBuiltUI(levelZero, pos);
        ChangeActivated(isActive);

        selectSpawnPos = pos;
        PriceCheck();
    }


    //Tower object generation when OK button is pressed
    public void PressTowerTypeBtn(int index) {
        if(!islevelChanged)
            towerTypesBtn[selectBtnIndex].SetActive(true);

        selectBtnIndex = index;
        towerTypesBtn[selectBtnIndex].SetActive(false);

        if(index.Equals(6)) {
            checkPointing = true;
            spawnTowerUI.SetActive(false);
            towerInformUI.SetActive(false);
            return;
        }

        ChangeActivated(false);
        TowerDescription();
        okBtn.transform.position = towerTypesBtn[selectBtnIndex].transform.position;

        if(!purchasable[selectBtnIndex])
            okBtn.GetComponent<Image>().color = Color.grey;
        else
            okBtn.GetComponent<Image>().color = Color.white;


        if(selectBtnIndex.Equals(4) || selectBtnIndex.Equals(5)) {
            towerImg.enabled = false;
            towerRangeImg.transform.localScale = Towers[selectSpawnPos].towerRange.transform.localScale + new Vector3(0.5f, 0.5f, 0);
            return;
        }
        towerImg.sprite = towerSprites[selectBtnIndex];
    }


    //Create, upgrade, or sell tower objects when you press the OK button.
    public void PressOkBtn() {
        if(selectBtnIndex.Equals(5)) { //Sell Tower
            SellTower();
            return;
        }

        if(!purchasable[selectBtnIndex])
            return;

        if(selectBtnIndex.Equals(4)) { //Upgrade Tower
            Money = -Towers[selectSpawnPos].towerPrice;
            Towers[selectSpawnPos].LevelUp();
            Towers[selectSpawnPos].towerRange.transform.localScale += new Vector3(0.5f, 0.5f, 0);
        }

        if(!selectBtnIndex.Equals(4) && !selectBtnIndex.Equals(5)) {  //Create(Buy) Tower
            Money = -towerPrices[selectBtnIndex];
            Tower tower = objectManager.GetObject(dataManager.TowerNamesENG[selectBtnIndex]).GetComponent<Tower>();
            tower.transform.SetParent(towerParent);
            tower.Setup(selectSpawnPos, spawnTowerUI.transform.position + new Vector3(0, 0.1f, 0));
        }


        Towers[selectSpawnPos].towerPrice += 50;   //Change Upgrade Price
        spawnTowerUI.SetActive(false);
        towerInformUI.SetActive(false);
        Towers[selectSpawnPos].towerRange.enabled = false;
    }


    //Determine if you can buy with your current money
    void PriceCheck() {
        if(levelZero) {
            for(int i = 0; i < 4; i++) {
                if(towerPrices[i] > money) {
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
            if(Towers[selectSpawnPos].towerPrice > money) {
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
        for(int i = 0; i < 4; i++)
            towerTypesBtn[i].SetActive(levelZero);
        for(int i = 4; i < towerTypesBtn.Length; i++)
            towerTypesBtn[i].SetActive(!levelZero);

        towerPriceUI.SetActive(levelZero);
        upgradePriceUI.SetActive(!levelZero);
        towerInformUI.SetActive(!levelZero);

        if(Towers.ContainsKey(selectSpawnPos))
            Towers[selectSpawnPos].towerRange.enabled = false;

        if(!levelZero) {
            upgradePrice.text = Towers[pos].towerPrice.ToString();
            Towers[pos].towerRange.enabled = true;
            TowerInformation(pos);
            towerTypesBtn[6].SetActive(Towers[pos].ElementType.Equals(Element.BARRACKS));
        }
        else
            towerRangeImg.transform.localScale = new Vector3(4f, 4f);

        if(LevelManager.Instance.SpawnPoints[pos].TowerLevelMax) {
            towerTypesBtn[4].SetActive(false);
            upgradePriceUI.SetActive(false);
        }
    }

    //Click to set the tower information to show to the UI
    void TowerInformation(Point pos) {
        towerIcon.sprite = towerTypesBtn[Towers[pos].towerIndex].GetComponent<Image>().sprite;

        //Get tower info and Change text
        towerInformTexts[0].text = dataManager.TowerNamesKR[Towers[pos].towerIndex];
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
        if(levelZero) {
            descriptionTexts[0].text = dataManager.TowerNamesKR[selectBtnIndex];
            descriptionTexts[1].text = dataManager.TowerDescriptions[selectBtnIndex];
            descriptionTexts[2].text = "공격력 : <color=#F68519>" + dataManager.TowerOffensePower[selectBtnIndex].ToString() + "</color>";
            descriptionTexts[3].text = "공격속도 : <color=#F68519>" + dataManager.AttackCoolDown[selectBtnIndex].ToString() + "</color>";
        }
        else {
            if(selectBtnIndex.Equals(4)) {
                descriptionTexts[0].text = dataManager.TowerNamesKR[Towers[selectSpawnPos].towerIndex];
                descriptionTexts[1].text = dataManager.TowerDescriptions[Towers[selectSpawnPos].towerIndex];
                descriptionTexts[2].text = "공격력 : <color=#F68519>" + (Towers[selectSpawnPos].Damage + 1f).ToString() + "</color>";
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
        waveText.text = "공격 <color=yellow>" + wave.ToString() + "</color>/" + waveMax.ToString();
        StartCoroutine(SpawnWave());
        waveBtn.SetActive(false);
    }

    IEnumerator SpawnWave() {
        Queue<int> monsterData = dataManager.waveData.Dequeue();
        for(int i=0; i< LevelManager.Instance.Tile.MonsterWay.Count; i++)
            LevelManager.Instance.GeneratePath(i);

        int count = monsterData.Count;
        int wayIndex = 0;
        for(int i=0; i<count; i++) {
            if(i==0 || i%2 == 0) {
                wayIndex = monsterData.Dequeue();
            }
            else {
                int monsterIndex = monsterData.Dequeue();
                string type = dataManager.MonsterType(monsterIndex);

                //Create monsters and health bars
                Monster monster = objectManager.GetObject(type).GetComponent<Monster>();
                HealthBar bar = objectManager.GetObject("HealthBar").GetComponent<HealthBar>();
                monster.healthBar = bar;
               
                monster.Spawn(wayIndex);
                activeMonsters.Add(monster);
            }
            
            yield return new WaitForSeconds(Random.Range(1f,2.5f));
        }     
    }

    public void RemoveMonster(Monster monster) {
        activeMonsters.Remove(monster);
        if(!WaveActive && !gameOver) {
            if(wave.Equals(waveMax) && activeMonsters.Count.Equals(0)) {
                GameComplete();
                return;
            }
            waveBtn.SetActive(true);
        }
    }





    /// <summary>
    /// The following is a function related to Game State
    /// </summary>
    /// 
    public void GameStateInitial(int money, int waveMax, int towerLevelMax) {
        Lives = 20;
        Money = money;
        this.waveMax = waveMax;
        waveText.text = "공격 <color=yellow>0</color>/" + waveMax.ToString();
        TowerLevelMax = towerLevelMax;

    }

    public void ShowGameMenu() {
        pauseUI.SetActive(!pauseUI.activeSelf);
        panel.SetActive(!panel.activeSelf);
        if(!pauseUI.activeSelf) {
            if(!WaveActive && !waveBtn.activeSelf)
                waveBtn.SetActive(true);
            Time.timeScale = 1;
        }
        else {
            if(!WaveActive && !gameOver && waveBtn.activeSelf)
                waveBtn.SetActive(false);
            Time.timeScale = 0;
        }
    }

    void GameComplete() {
        panel.SetActive(!panel.activeSelf);
        gameCompleteUI.SetActive(true);
        if(PlayerPrefs.GetInt("AchieveLevel").Equals(LevelManager.Instance.GameLevel)){
            PlayerPrefs.SetInt("AchieveLevel", LevelManager.Instance.GameLevel + 1);
        }
    }

    public void GameOver() {
        if(!gameOver) {
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
        Destroy(MainMenuManager.Instance.dontDestory);
        SceneManager.LoadScene(1);
        //Application.Quit();
    }
}
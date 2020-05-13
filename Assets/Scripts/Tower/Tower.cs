using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public enum Element { ARCHER, WIZARD, BOMB, BARRACKS, NONE }

public class Tower : MonoBehaviour {
    float attackTimer;
    bool canAttack;
    public int towerIndex, towerPrice;

    private float projectileSpeed, attackCooldown;
    public float ProjectileSpeed { get { return projectileSpeed; } }
    public float AttackCoolDown { get => attackCooldown; }

    [SerializeField]
    private string projectileType;
    public string Projectile { get; set; }

    [SerializeField]
    private Sprite[] towerSprites;

    private SpriteRenderer sr;
    private TowerRange range;

    private Monster target;
    public Monster Target { get { return target; } }
    public List<Monster> monsterList;

    //Soldier
    public Dictionary<int, Soldier> Soldiers { get; set; }
    private Vector3 soldierStandardPos;

    private float damage, distance;
    public float Damage { get => damage; }

    [SerializeField]
    private Element elementType;
    public Element ElementType { get => elementType; }

    public SpriteRenderer towerRange;

    public Point GridPosition { get; set; }




    /// <summary>
    /// 
    /// </summary>

    private void Start() {
        sr = GetComponent<SpriteRenderer>();
        canAttack = true;
        monsterList = new List<Monster>();
    }

    private void Update() {
        if(!ElementType.Equals(Element.BARRACKS))
            Attack();
    }

    public void Setup(Point gridPos, Vector3 worldPos) {
        GridPosition = gridPos;
        transform.position = worldPos + new Vector3(0, 0.13f, 0);

        GameManager.Instance.SetTowerParent(this);
        GameManager.Instance.Towers.Add(gridPos, this);
        LevelManager.Instance.Tiles[gridPos].TowerLevel++;
        Projectile = projectileType + "1";
        GameManager.Instance.dataManager.Initilaize(towerIndex, ref damage, ref projectileSpeed, ref attackCooldown);
        towerPrice = GameManager.Instance.towerPrices[towerIndex];

        range = transform.GetChild(0).GetComponent<TowerRange>();
        range.GridPosition = GridPosition;
        towerRange.transform.localScale = new Vector3(4, 4.5f, 1);
        Soldiers = new Dictionary<int, Soldier>();

        //Create Soldiers if it is the BARRACKS
        if(ElementType.Equals(Element.BARRACKS)) {
            distance = 10000;
            CheckNeighborTiles(GridPosition);
            Vector3[] posArray = LevelManager.Instance.Tiles[GridPosition].SetSoldierPos(soldierStandardPos);
            for(int i = 0; i < 3; i++)
                StartCoroutine(CreateSoldier(i, posArray[i], false, false, null));
        }
    }

    public void LevelUp() {
        int level = ++LevelManager.Instance.Tiles[GridPosition].TowerLevel;
        damage++;
        DetermineProjectile();

        //Stop upgrading when the tower level is at its maximum
        if(level.Equals(towerSprites.Length))
            LevelManager.Instance.Tiles[GridPosition].towerLevelMax = true;

        //Replace with next level sprite
        sr.sprite = towerSprites[level - 1];

        if(ElementType.Equals(Element.BARRACKS)) {
            int count = Soldiers.Count;
            for(int i = 0; i < count; i++) {
                if(Soldiers[i].gameObject.activeSelf)
                    Soldiers[i].Release(false, true);
            }
        }
    }


    void Attack() {
        if(!canAttack) {
            attackTimer += Time.deltaTime;
            if(attackTimer >= attackCooldown) {
                canAttack = true;
                attackTimer = 0;

            }
        }

        if(target == null && monsterList.Count > 0) {
            target = monsterList[0];
            monsterList.Remove(target);
        }

        if(target != null && target.gameObject.activeSelf) {
            if(canAttack) {
                Shoot();
                canAttack = false;
            }
        }
        else if(monsterList.Count > 0) {
            target = monsterList[0];
            monsterList.Remove(target);
        }

        if((target != null && target.isDie) || (target != null && !target.gameObject.activeSelf)) {
            target = null;
        }
    }

    void Shoot() {
        Projectile proj = GameManager.Instance.objectManager.GetObject(Projectile).GetComponent<Projectile>();
        proj.transform.position = GameManager.Instance.Towers[GridPosition].transform.position;
        proj.Initialize(this);
    }

    private void CheckNeighborTiles(Point towerPos) {
        for(int x = -1; x <= 1; x++) {
            for(int y = -1; y <= 1; y++) {
                Point neighborPos = new Point(towerPos.x - x, towerPos.y - y);
                if(LevelManager.Instance.Tiles[neighborPos].tileIndex.Equals(1)) {
                    float _distance = Vector3.Distance(transform.position, LevelManager.Instance.Tiles[neighborPos].WorldPostion);
                    if(distance > _distance) {
                        distance = _distance;
                        soldierStandardPos = LevelManager.Instance.Tiles[neighborPos].WorldPostion;
                    }
                }
            }
        }
    }

    public void RecreateSoldier(int index, bool haveTimer, bool isLevelUp, Monster target) {
        StartCoroutine(CreateSoldier(index, LevelManager.Instance.Tiles[GridPosition].SoldierPos[index], haveTimer, isLevelUp, target));
    }

    IEnumerator CreateSoldier(int index, Vector3 pos, bool haveTimer, bool isLevelUp, Monster target) {
        if(haveTimer && !isLevelUp)
            yield return new WaitForSeconds(attackCooldown);
        Soldier soldier = GameManager.Instance.objectManager.GetObject(Projectile).GetComponent<Soldier>();
        GameObject bar = GameManager.Instance.objectManager.GetObject("HealthBar");
        soldier.transform.position = transform.position;

        if(isLevelUp && target != null) {
            soldier.Target = target;
            soldier.isMoved = true;
        }
        soldier.healthBar = bar;
        soldier.SoldierIndex = index;
        soldier.SpawnPos = pos;

        Soldiers.Add(index, soldier);
        soldier.Initialize(this, !isLevelUp);
    }

    //void ChangeSoldierPos() {
    //    int count = soldierPos.Count;
    //    if(count < 3) {
    //        for(int i=0; i < 3- count; i++) {
    //            CreateSoldier
    //        }
    //    }
    //}

    void DetermineProjectile() {
        print(LevelManager.Instance.Tiles[GridPosition].TowerLevel);
        Projectile = projectileType +
            (GameManager.Instance.dataManager.ProjectileType(towerIndex, LevelManager.Instance.Tiles[GridPosition].TowerLevel)).ToString();       
    }

    public void MonsterInRange(Monster monster) { monsterList.Add(monster); }

    public void MonsterOutRange(Monster monster, bool isTarget) {
        monsterList.Remove(monster);
        if(isTarget) target = null;
    }

    public bool IsMonsterInRange(Monster monster) { return monsterList.Contains(monster); }

    public void Release() {
        GameManager.Instance.objectManager.ReleaseObject(gameObject);
        
        if(ElementType.Equals(Element.BARRACKS)) {
            int count = Soldiers.Count;
            for(int i=0; i< count; i++) {
                if(Soldiers[i].gameObject.activeSelf)
                    Soldiers[i].Release(false, false);
            }
        }
        monsterList.Clear();
        Soldiers.Clear();

        sr.sprite = towerSprites[0];
        LevelManager.Instance.Tiles[GridPosition].TowerLevel = 0;
        LevelManager.Instance.Tiles[GridPosition].towerLevelMax = false;             
        GameManager.Instance.Towers.Remove(GridPosition);  //Delete from dictionary   

    }
}

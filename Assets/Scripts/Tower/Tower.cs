using System.Collections;
using System.Collections.Generic;
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
    public string projectile;

    [SerializeField]
    private Sprite[] towerSprites;

    private SpriteRenderer sr;
    private TowerRange range;

    public Monster target;
    public Monster Target { get { return target; } }
    public List<Monster> monsterList = new List<Monster>();

    public List<Soldier> soldiers = new List<Soldier>();

    private float damage;
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
        if(ElementType.Equals(Element.BARRACKS))
            CreateSoldier();
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
        LevelManager.Instance.Tiles[gridPos].towerLevel++;
        projectile = projectileType + "1";
        GameManager.Instance.dataManager.Initilaize(towerIndex, ref damage, ref projectileSpeed, ref attackCooldown);
        towerPrice = GameManager.Instance.towerPrices[towerIndex];

        range = transform.GetChild(0).GetComponent<TowerRange>();
        range.GridPosition = GridPosition;
        towerRange.transform.localScale = new Vector3(4, 4.5f, 1);
    }

    public void LevelUp() {
        int level = ++LevelManager.Instance.Tiles[GridPosition].towerLevel;
        damage++;
        DetermineProjectile();

        //Stop upgrading when the tower level is at its maximum
        if(level.Equals(towerSprites.Length))
            LevelManager.Instance.Tiles[GridPosition].towerLevelMax = true;

        //Replace with next level sprite
        sr.sprite = towerSprites[level - 1];
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
            print(target);
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
            print(target);
        }

        if((target != null && target.isDie) || (target != null && !target.gameObject.activeSelf)) {
            target = null;
        }
    }

    void Shoot() {
        Projectile proj = GameManager.Instance.objectManager.GetObject(projectile).GetComponent<Projectile>();
        proj.transform.position = GameManager.Instance.Towers[GridPosition].transform.position;
        proj.Initialize(this);
    }

    void CreateSoldier() {
        for(int i=0; i<3; i++) {
            Soldier soldier = GameManager.Instance.objectManager.GetObject(projectile).GetComponent<Soldier>();
            GameObject bar = GameManager.Instance.objectManager.GetObject("HealthBar");

            soldiers.Add(soldier);
            soldier.transform.position = GameManager.Instance.Towers[GridPosition].transform.position;
            soldier.healthBar = bar;
            soldier.Initialize(this);
        }
        
    }

    void DetermineProjectile() {
        print(LevelManager.Instance.Tiles[GridPosition].towerLevel);
        projectile = projectileType +
            (GameManager.Instance.dataManager.ProjectileType(towerIndex, LevelManager.Instance.Tiles[GridPosition].towerLevel)).ToString();       
    }

    public void MonsterInRange(Monster monster) {
        monsterList.Add(monster);
        print("add"+monster);
    }

    public void MonsterOutRange(Monster monster, bool isTarget) {
        monsterList.Remove(monster);
        print("리무브 :"+monster);
        if(isTarget) target = null;
    }

    public bool IsMonsterInRange(Monster monster) { return monsterList.Contains(monster); }

    public void Release() {      
        if(ElementType.Equals(Element.BARRACKS)) {
            soldiers[0].Release();
            soldiers[1].Release();
            soldiers[2].Release();
        }
        monsterList.Clear();
        soldiers.Clear();
        
        LevelManager.Instance.Tiles[GridPosition].towerLevel = 0;
        LevelManager.Instance.Tiles[GridPosition].towerLevelMax = false;       
        
        GameManager.Instance.Towers.Remove(GridPosition);  //Delete from dictionary       
        GameManager.Instance.objectManager.ReleaseObject(gameObject);
    }
}

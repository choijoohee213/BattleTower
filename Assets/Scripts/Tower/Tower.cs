using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element { ARCHER, WIZARD, BOMB, BARRACKS, NONE }

public class Tower : MonoBehaviour {
    float attackTimer;
    bool canAttack = true;
    public int towerIndex, towerPrice;

    private float projectileSpeed, attackCooldown;
    public float ProjectileSpeed { get { return projectileSpeed; } }
    public float AttackCoolDown { get => attackCooldown; }

    [SerializeField]
    private string projectileType;
    private string projectile;

    [SerializeField]
    private Sprite[] towerSprites;

    private SpriteRenderer sr;
    private TowerRange range;

    private Monster target;
    public Monster Target { get { return target; } }

    private Queue<Monster> monsters = new Queue<Monster>();

    private float damage;
    public float Damage { get => damage; }

    [SerializeField]
    private Element elementType;
    public Element ElementType { get => elementType; }

    public Point GridPosition { get; set; }

    public SpriteRenderer towerRange;



    /// <summary>
    /// 
    /// </summary>

    private void Start() {
        sr = GetComponent<SpriteRenderer>();
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

        range = transform.GetChild(0).GetComponent<TowerRange>();
        range.GridPosition = GridPosition; 
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
        if(target == null && monsters.Count > 0) {
            target = monsters.Dequeue();
        }
        if(target != null && target.gameObject.activeSelf) {
            if(canAttack) {
                Shoot();
                canAttack = false;
            }
        }
        else if(monsters.Count > 0) {
            target = monsters.Dequeue();
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
        Soldier soldier = GameManager.Instance.objectManager.GetObject(projectile).GetComponent<Soldier>();
        soldier.transform.position = GameManager.Instance.Towers[GridPosition].transform.position;
        soldier.Initialize(this);
    }

    void DetermineProjectile() {
        print(LevelManager.Instance.Tiles[GridPosition].towerLevel);
        projectile = projectileType +
            (GameManager.Instance.dataManager.ProjectileType(towerIndex, LevelManager.Instance.Tiles[GridPosition].towerLevel)).ToString();       
    }

    public void MonsterInRange(Monster monster) {
        monsters.Enqueue(monster);
    }

    public void MonsterOutRange() {
        target = null;
    }
}

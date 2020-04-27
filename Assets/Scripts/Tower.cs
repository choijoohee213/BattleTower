using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    private float attackTimer;
    private bool canAttack = true;

    public int towerPrice;

    [SerializeField]
    private float projectileSpeed, attackCooldown;
    public float ProjectileSpeed { get { return projectileSpeed; } }

    [SerializeField]
    private string projectileType;
    
    [SerializeField]
    private Sprite[] towerSprites;

    private SpriteRenderer sr;
    private TowerRange range;
    
    private Monster target;
    public Monster Target { get { return target; } }

    private Queue<Monster> monsters = new Queue<Monster>();

    [SerializeField]
    private float damage;
    public float Damage { get => damage; }

    public Point GridPosition { get; set; }

    public SpriteRenderer towerRange {
        get { return range.GetComponent<SpriteRenderer>(); }
    }



    private void Start() {
        sr = GetComponent<SpriteRenderer>();       
    }

    private void Update() {
        Attack();
    }

    public void Setup(Point gridPos, Vector3 worldPos) {
        GridPosition = gridPos;
        transform.position = worldPos + new Vector3(0,0.13f,0);
        GameManager.Instance.SetTowerParent(this);
        GameManager.Instance.Towers.Add(gridPos, this);
        LevelManager.Instance.Tiles[gridPos].towerLevel++;
        range = transform.GetChild(0).GetComponent<TowerRange>();
        range.GridPosition = GridPosition;
    }  

    public void LevelUp() {
        int level = ++LevelManager.Instance.Tiles[GridPosition].towerLevel;
       
        //Stop upgrading when the tower level is at its maximum
        if (level.Equals(towerSprites.Length-1))
            LevelManager.Instance.Tiles[GridPosition].towerLevelMax = true;

        //Replace with next level sprite
        sr.sprite = towerSprites[level];      
    }

    void Attack() {
        if (!canAttack) {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown) {
                canAttack = true;
                attackTimer = 0;

            }
        }

        if (target == null && monsters.Count > 0) {
            target = monsters.Dequeue();
        }
        if (target != null && target.gameObject.activeSelf) {           
            if (canAttack) {
                Shoot();
                canAttack = false;
            }
        }
        else if(monsters.Count > 0) {
            target = monsters.Dequeue();
        }

        if (target != null && target.isDie) {
            target = null;
        }

    }

    void Shoot() {
        Projectile projectile = GameManager.Instance.objectManager.GetObject(projectileType).GetComponent<Projectile>();
        projectile.transform.position = GameManager.Instance.Towers[GridPosition].transform.position;
        projectile.Initialize(this);
    }

    public void MonsterInRange(Monster monster) {
        monsters.Enqueue(monster);
    }

    public void MonsterOutRange() {
        target = null;
    }
}

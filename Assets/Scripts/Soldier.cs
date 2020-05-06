using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    [SerializeField]
    int level;
    bool isSoldierDie = false, canAttack = true, spawn = false, isMoved = false;
    float attackTimer, hitPower, attackCooldown, createCooldown;
    Vector3 spawnPos;

    public Monster target;
    public Monster Target { get { return target; } }

    Queue<Monster> monsters = new Queue<Monster>();

    Tower parent;
    Animator myAnimator;
    Element elementType;

    void Start() {
        //myAnimator = GetComponent<Animator>();
    }

    private void Update() {
        Attack();
    }

    public void Initialize(Tower parent) {
        this.parent = parent;
        elementType = parent.ElementType;
        hitPower = parent.Damage;
        createCooldown = parent.AttackCoolDown;
        attackCooldown = parent.ProjectileSpeed;
    }

    IEnumerator Spawn(Vector3 pos) {
        while(!transform.position.Equals(pos) && !isMoved) { 
            transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * 5f);
            yield return new WaitForSeconds(0.01f);
        }
    }

    void Attack() {
        if(!spawn) return;

        if(!canAttack) {
            attackTimer += Time.deltaTime;
            if(attackTimer >= attackCooldown) {
                canAttack = true;
                attackTimer = 0;
            }
        }

        if(target != null && target.gameObject.activeSelf) {
            target.moveStop = true;
            if(!isMoved) MovetoTarget();
            if(canAttack && isMoved) {
                Hit();
                canAttack = false;
            }
        }

        //Monster Die
        if((target != null && target.isDie) || (target != null && !target.gameObject.activeSelf)) {
            target = null;
            isMoved = false;           
            StartCoroutine(Spawn(spawnPos));
        }
    }

    void MovetoTarget() {
        //몬스터에게 가고 때린다
        Vector3 targetPos = target.transform.position + new Vector3(0.3f, 0, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 3f);
        if(transform.position.Equals(targetPos)) {
            isMoved = true;
            transform.rotation = Quaternion.Euler(0, target.transform.rotation.y - 180f, 0);
        }
    }

    void Hit() {
        target.TakeDamage(hitPower, elementType);       
    }

    /*몬스터에게서 맞는다
    public void TakeDamage(float damage, Element dmgSource) {
        if(dmgSource.Equals(elementType))
            damage /= 2;
        currentHealth -= damage;

        //Monster Die
        if(currentHealth <= 0) {
            currentHealth = 0;
            GameManager.Instance.Money = price;
            GameManager.Instance.objectManager.ReleaseObject(healthBar);
            anim.SetTrigger("MonsterDie");
        }
        gaugeBar.fillAmount = currentHealth / health;
    }
    */

    void CreateSoldier() {

    }

    private void OnTriggerStay2D(Collider2D collision) {
        if(collision.CompareTag("Soil") && !spawn) {
            spawnPos = collision.GetComponent<TileScript>().WorldPostion;
            StartCoroutine(Spawn(spawnPos));
            spawn = true;
        }

        if(target == null && collision.CompareTag("Monster")) {
            target = collision.GetComponent<Monster>();
        }
    }
}

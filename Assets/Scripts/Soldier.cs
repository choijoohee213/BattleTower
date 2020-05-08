using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Soldier : MonoBehaviour
{
    [SerializeField]
    int level;
    bool spawn = false, isMoved = false;
    public bool IsDie { get { return currentHealth <= 0; }}

    float attackTimer, hitPower, attackCooldown, createCooldown;
    Vector3 spawnPos;

    public Monster target;
    public Monster Target { get { return target; } }

    [SerializeField]
    private float health;
    private float currentHealth;

    public GameObject healthBar;
    public Image gaugeBar;

    Tower parent;
    Animator anim;
    Element elementType;

    void Start() {
        anim = GetComponent<Animator>();       
        healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
    }

    public void Initialize(Tower parent) {
        this.parent = parent;
        elementType = parent.ElementType;
        hitPower = parent.Damage;
        createCooldown = parent.AttackCoolDown;
        attackCooldown = parent.ProjectileSpeed;

        //Initialization for Health Information
        currentHealth = health;
        gaugeBar = healthBar.transform.GetChild(0).GetComponent<Image>();
        gaugeBar.fillAmount = 1;

        StartCoroutine(Attack());
    }

    IEnumerator Spawn(Vector3 pos) {
        while(!transform.position.Equals(pos) && !isMoved && target == null && !IsDie) {
            transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * 3f);
            healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
            transform.rotation = Quaternion.Euler(0, 180, 0);
            anim.SetBool("SoldierMove", true);
            yield return new WaitForSeconds(0.01f);
        }
        anim.SetBool("SoldierMove", false);
    }

    IEnumerator Attack() {
        while(true) {
            if(spawn) {
                //Attack when the active monster is within range
                if(target != null && target.gameObject.activeSelf && !IsDie) {
                    target.moveStop = true;
                    target.StartAttack(this);
                    if(!isMoved)
                        MovetoTarget();                    
                    if(isMoved) { 
                        yield return new WaitForSeconds(attackCooldown); 
                        Hit(); 
                    }
                }

                //Monster Die
                if((target != null && target.isDie) || (target != null && !target.gameObject.activeSelf)) {
                    target = null;
                    isMoved = false;
                    StartCoroutine(Spawn(spawnPos));
                }
            }
            yield return null;
        }
    }

    void MovetoTarget() {
        //몬스터에게 가고 때린다
        Vector3 targetPos = target.transform.position + new Vector3(0.3f, 0, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 2f);
        healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
        anim.SetBool("SoldierMove", true);
        if(transform.position.Equals(targetPos)) {
            isMoved = true;
            transform.rotation = Quaternion.Euler(0, target.transform.rotation.y - 180f, 0);
        }
    }

    void Hit() {
        anim.SetBool("SoldierMove", false);
        target.TakeDamage(hitPower, elementType);
        anim.SetTrigger("SoldierAttack");
    }

  
    public void TakeDamage(float damage) {
        currentHealth -= damage;

        //Soldier Die
        if(currentHealth <= 0 ) {
            currentHealth = 0;
            GameManager.Instance.objectManager.ReleaseObject(healthBar);
            anim.SetTrigger("SoldierDie");
        }
        gaugeBar.fillAmount = currentHealth / health;
    }
    

    void CreateSoldier() {

    }

    private void OnTriggerStay2D(Collider2D collision) {
        if(collision.CompareTag("Soil") && !spawn) {
            spawnPos = collision.GetComponent<TileScript>().WorldPostion;
            StartCoroutine(Spawn(spawnPos));
            spawn = true;
        }

        if(target == null && collision.CompareTag("Monster") && parent.IsMonsterInRange(collision.GetComponent<Monster>())) {
            target = collision.GetComponent<Monster>();
        }
    }

    public void Release() {
        GameManager.Instance.objectManager.ReleaseObject(healthBar);
        GameManager.Instance.objectManager.ReleaseObject(gameObject);
        spawn = false; isMoved = false;
        transform.position = parent.transform.position;
    }
}

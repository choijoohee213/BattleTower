using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Soldier : MonoBehaviour
{
    [SerializeField]
    int level;
    public int SoldierIndex { get; set; }
    public bool spawn = false, isMoved = false;
    public bool IsDie { get { return currentHealth <= 0; }}

    public float hitPower, attackCooldown;
    public Vector3 SpawnPos { get; set; }

    public Monster Target; // { get { return target; } set { target = value; } }

    [SerializeField]
    float health;
    float currentHealth;

    public GameObject healthBar;
    public Image gaugeBar;

    Tower parent;
    Animator anim;
    Element elementType;

    void Start() {
        healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
    }


    public void Initialize(Tower parent, bool move) {
        this.parent = parent;
        elementType = parent.ElementType;
        hitPower = parent.Damage;
        attackCooldown = parent.ProjectileSpeed;

        //Initialization for Health Information
        currentHealth = health;
        gaugeBar = healthBar.transform.GetChild(0).GetComponent<Image>();
        gaugeBar.fillAmount = 1;

        anim = GetComponent<Animator>();

        StartSpawn(move);
        StartCoroutine(Attack());
    }

    public void StartSpawn(bool move) {
        if(!spawn) {
            if(move) StartCoroutine(Spawn(SpawnPos));
            else {
                if(Target != null) transform.position = Target.transform.position + new Vector3(-0.3f, 0, 0);
                else transform.position = SpawnPos;
                }
            spawn = true;
        }
    }

    IEnumerator Spawn(Vector3 pos) {
        while(!transform.position.Equals(pos) && !isMoved && Target == null && !IsDie) {
            transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * 2f);
            healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
            transform.rotation = Quaternion.Euler(0, 180, 0);
            anim.SetBool("SoldierMove", true);
            yield return null;
        }
        anim.SetBool("SoldierMove", false);
    }

    IEnumerator Attack() {
        //WaitForSeconds wait = new WaitForSeconds(0.01f);
        while(true) {
            if(spawn) {
                //Attack when the active monster is within range
                if(Target != null && Target.gameObject.activeSelf && !IsDie && !Target.isDie) {
                    Target.MoveStop = true;
                    Target.StartAttack(this);
                    if(!isMoved)
                        MovetoTarget();
                    if(isMoved && !Target.isDie) {
                        yield return new WaitForSeconds(attackCooldown);
                        if(!Target.isDie || Target.gameObject.activeSelf)
                            Hit();
                    }
                }
                if(IsDie) {
                    GameManager.Instance.objectManager.ReleaseObject(healthBar);
                    anim.SetTrigger("SoldierDie");
                }


                //Monster Die
                if((Target != null && Target.isDie) || (Target != null && !Target.gameObject.activeSelf)) {
                    print(SoldierIndex);
                    Target = null;
                    isMoved = false;
                    StartCoroutine(Spawn(SpawnPos));
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }


    //After moving to the monster, attack the monster
    void MovetoTarget() {
        Vector3 targetPos;
        if(!SoldierIndex.Equals(2))
            targetPos = Target.transform.position + new Vector3(-0.3f, 0, 0);
        else
            targetPos = Target.transform.position + new Vector3(0.3f, 0, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 2f);
        healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
        anim.SetBool("SoldierMove", true);
        if(transform.position.Equals(targetPos)) {
            isMoved = true;
            anim.SetBool("SoldierMove", false);
            transform.rotation = Quaternion.Euler(0, Target.transform.rotation.y - 180f, 0);
        }
    }

    void Hit() {
        if(Target == null || Target.isDie) return;
        anim.SetTrigger("SoldierAttack");
        Target.TakeDamage(hitPower, elementType);
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

    private void OnTriggerStay2D(Collider2D collision) {
        if(Target == null && collision.CompareTag("Monster") && parent.IsMonsterInRange(collision.GetComponent<Monster>())) {
            Target = collision.GetComponent<Monster>();
        }
    }

    public void Release(bool haveTimer, bool isLevelUp) {
        GameManager.Instance.objectManager.ReleaseObject(healthBar);
        if(Target != null && Target.TargetSoldiers.Contains(this)) {
            if(!isLevelUp)
                Target.MoveStop = false;
            Target.TargetSoldiers.Remove(this);
        }
        spawn = false; isMoved = false;
        parent.Soldiers.Remove(SoldierIndex);
        if(parent.gameObject.activeSelf) {
            parent.RecreateSoldier(SoldierIndex, haveTimer, isLevelUp, Target);
        }
        Target = null;
        transform.position = parent.transform.position;
        GameManager.Instance.objectManager.ReleaseObject(gameObject);
    }
}

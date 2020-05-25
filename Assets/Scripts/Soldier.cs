using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour {
    [SerializeField]
    int level;
    public int SoldierIndex { get; set; }
    public bool spawn = false, isMoved = false;
    public bool IsDie { get { return currentHealth <= 0; } }

    public float hitPower, attackCooldown;
    public Vector3 SpawnPos { get; set; }

    public Monster Target;

    [SerializeField]
    float health;
    float currentHealth;

    public HealthBar healthBar;
    public Coroutine stopAttack;

    Tower parent;
    Animator anim;
    Element elementType;

    void Start() {
        healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
    }


    public void Initialize(Tower parent, bool levelUp) {
        this.parent = parent;
        elementType = parent.ElementType;
        hitPower = parent.Damage;
        attackCooldown = parent.ProjectileSpeed;

        //Initialization for Health Information
        currentHealth = health;
        healthBar.GaugeBar.fillAmount = 1;

        anim = GetComponent<Animator>();

        StartSpawn(levelUp);
        StartCoroutine(Attack());
    }

    public void ChangePos() {
        spawn = false;
        isMoved = false;
        StartSpawn(true);

        if(Target == null) {
            StartCoroutine(Attack());
        }
    }

    public void StartSpawn(bool move) {
        if(!spawn) {
            if(move)
                StartCoroutine(Spawn(SpawnPos));
            else {
                if(Target != null) {
                    if(Target.transform.rotation.Equals(Quaternion.Euler(0, 180, 0))) {  //left
                        transform.position = Target.transform.position + new Vector3(-0.3f, 0, 0);
                    }
                    else {
                        transform.position = Target.transform.position + new Vector3(0.3f, 0, 0);
                        transform.rotation = Quaternion.Euler(0, -180, 0);
                    }
                }
                else {
                    transform.position = SpawnPos;
                }
            }
            spawn = true;

        }
    }

    IEnumerator Spawn(Vector3 pos) {
        while(!transform.position.Equals(pos) && Target == null && !IsDie) {
            transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * 2f);
            healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
            transform.rotation = Quaternion.Euler(0, 180, 0);
            anim.SetBool("SoldierMove", true);
            yield return null;
        }
        anim.SetBool("SoldierMove", false);
        GameManager.Instance.SoldierPosSign.SetActive(false);

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
                        if(Target != null && !Target.isDie && Target.gameObject.activeSelf)
                            Hit();
                    }
                }
                if(IsDie) {
                    GameManager.Instance.objectManager.ReleaseObject(healthBar.gameObject);
                    healthBar.ParentObj = null;
                    anim.SetTrigger("SoldierDie");
                }


                //Monster Die
                if((Target != null && Target.isDie) || (Target != null && !Target.gameObject.activeSelf)) {
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
        if(Target.transform.eulerAngles.y.Equals(180f))  //몬스터가 왼쪽 방향으로 가고있을 때
            targetPos = Target.transform.position + new Vector3(-0.3f, 0, 0);
        else  //몬스터가 오른쪽 방향으로 가고있을 때
            targetPos = Target.transform.position + new Vector3(0.3f, 0, 0);
        
        transform.LookAt(Target.transform);
        transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.y, 0));
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 2f);
        
        healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
        
        anim.SetBool("SoldierMove", true);
        
        if(transform.position.Equals(targetPos)) {
            isMoved = true;
            anim.SetBool("SoldierMove", false);
        }
    }

    void Hit() {
        if(Target == null || Target.isDie)
            return;
        anim.SetTrigger("SoldierAttack");
        Target.TakeDamage(hitPower, elementType);
        
    }


    public void TakeDamage(float damage) {
        currentHealth -= damage;

        //Soldier Die
        if(currentHealth <= 0) {
            currentHealth = 0;
            anim.SetTrigger("SoldierDie");
        }
        healthBar.GaugeBar.fillAmount = currentHealth / health;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if(Target == null && collision.CompareTag("Monster") && parent.IsMonsterInRange(collision.GetComponent<Monster>())) {
            Target = collision.GetComponent<Monster>();
        }
    }

    public void Release(bool haveTimer, bool isLevelUp) {
        healthBar.ParentObj = null;
        GameManager.Instance.objectManager.ReleaseObject(healthBar.gameObject);

        if(Target != null && Target.TargetSoldiers.Contains(this)) {
            if(!isLevelUp)
                Target.MoveStop = false;
            Target.TargetSoldiers.Remove(this);
        }
        spawn = false;
        isMoved = false;
        parent.Soldiers.Remove(SoldierIndex);
        GameManager.Instance.objectManager.ReleaseObject(gameObject);
        if(parent.gameObject.activeSelf) {
            parent.RecreateSoldier(SoldierIndex, haveTimer, isLevelUp, Target);
        }
        Target = null;
    }
}

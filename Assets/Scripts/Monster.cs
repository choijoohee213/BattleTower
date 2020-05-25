using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour {
    [SerializeField]
    private float speed;
    private bool canAttack = true;
    private bool moveStop = false;
    public bool MoveStop { get { return moveStop; } set { moveStop = value; } }

    [SerializeField]
    private int price;

    [SerializeField]
    private Element elementType;

    private Stack<Vector3> path;
    private Vector3 destination;
    private Animator anim;

    [SerializeField]
    private float health, damage, attackCoolDown;
    private float currentHealth;

    public HealthBar healthBar;
    public List<Soldier> TargetSoldiers;

    public bool isDie {
        get { return currentHealth <= 0; }
    }

    public Vector3 MonsterPos { get; set; }




    /// <summary>
    /// 
    /// </summary>
    /// 
    private void Awake() {
        anim = GetComponent<Animator>();

    }

    //Spawns the monster in our world
    public void Spawn(int wayIndex) {
        //Sets the monsters path
        SetPath(LevelManager.Instance.MapPaths[wayIndex].Path, wayIndex);


        //Initialization for Health Information
        currentHealth = health;
        healthBar.GaugeBar.fillAmount = 1;


        //Starts to scale the monsters
        StartCoroutine(Scale(new Vector3(0.1f, 0.1f), new Vector3(1, 1), true));

        

        StartCoroutine(MonsterMove());
    }

    //Sclaes a monster up or down
    public IEnumerator Scale(Vector3 from, Vector3 to, bool isActive) {
        float progress = 0;

        //As long as the progress is less than 1, than we need to keep scaling
        while(progress <= 1) {
            transform.localScale = Vector3.Lerp(from, to, progress);
            progress += Time.deltaTime * 1.3f;
            yield return new WaitForSeconds(0.01f);
        }
        //Make sure that is has the correct scale after scaling
        transform.localScale = to;


        if(!isActive)
            Release();
        else
            gameObject.SetActive(true);
    }


    //Gives the monster a path to walk on
    void SetPath(Stack<Vector3> newPath, int wayIndex) {
        if(newPath != null) {
            path = new Stack<Vector3>();
            MonsterPos = new Vector3(0, 0, 0);

            path = newPath;
            Animate(transform.position, path.Peek());
            transform.position = path.Peek();
            MonsterPos = path.Peek();
            destination = path.Pop();
        }
    }


    //Makes the monster move along the given path
    IEnumerator MonsterMove() {
        while(!isDie && !MoveStop) {
            transform.position = Vector2.MoveTowards(transform.position, destination, speed);
            healthBar.transform.position = transform.position + new Vector3(0, 0.43f, 0);
            anim.SetBool("MonsterIdle", false);

            //Checks if monster arrived at the destination
            if(transform.position == destination) {
                if(path != null & path.Count > 0) {
                    Animate(MonsterPos, path.Peek());

                    //Sets the new GirdPosition and destination
                    MonsterPos = path.Peek();
                    destination = path.Pop();
                }
                else
                    break;
            }
            yield return new WaitForSeconds(0.03f);
        }
        if(MoveStop) {
            Animate(MonsterPos, MonsterPos);
        }

        //Monsters arrive at the edge of the map without dying
        if(!isDie && MonsterPos.x.Equals(LevelManager.Instance.purplePortal.transform.position.x)) {
            StartCoroutine(Scale(new Vector3(1, 1), new Vector3(0.1f, 0.1f), false));
            GameManager.Instance.Lives--;
        }
    }

    public void StartAttack(Soldier soldier) {
        if(!TargetSoldiers.Contains(soldier)) {
            TargetSoldiers.Add(soldier);
        }
        if(canAttack) {
            StartCoroutine(Attack());
            canAttack = false;
        }
    }

    IEnumerator Attack() {
        while(TargetSoldiers.Count > 0 && MoveStop) {
            if(!TargetSoldiers[0].IsDie && TargetSoldiers[0].gameObject.activeSelf) {
                yield return new WaitForSeconds(attackCoolDown);
                if(TargetSoldiers.Count > 0 && !TargetSoldiers[0].IsDie && TargetSoldiers[0].gameObject.activeSelf) {
                    anim.SetTrigger("MonsterAttack");
                    TargetSoldiers[0].TakeDamage(damage);
                }
            }
            if(TargetSoldiers.Count > 0 && TargetSoldiers[0].IsDie) {
                TargetSoldiers.Remove(TargetSoldiers[0]);
            }
        }
        moveStop = false;
        canAttack = true;
        StartCoroutine(MonsterMove());
    }


    void Animate(Vector3 currentPos, Vector3 newPos) {
        if(MoveStop) {
            anim.SetBool("MonsterIdle", true);
            anim.SetBool("MonsterDown", false);
            return;
        }
        else if(!MoveStop) {
            anim.SetBool("MonsterIdle", false);
        }

        if(currentPos.y >= newPos.y) { //Moving down
            anim.SetBool("MonsterDown", true);
        }
        else if(currentPos.y < newPos.y) {  //Moving up
            anim.SetBool("MonsterDown", false);
        }

        if(currentPos.x > newPos.x) {  //Move to left
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if(currentPos.x < newPos.x) {  //Move to right
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

    }

    public void TakeDamage(float damage, Element dmgSource) {
        if(dmgSource.Equals(elementType))
            damage /= 2;
        currentHealth -= damage;

        //Monster Die
        if(currentHealth <= 0) {
            currentHealth = 0;
            GameManager.Instance.Money = price;
            anim.SetTrigger("MonsterDie");
        }
        healthBar.GaugeBar.fillAmount = currentHealth / health;
    }

    public void Release() {
        healthBar.ParentObj = null;
        GameManager.Instance.objectManager.ReleaseObject(healthBar.gameObject);

        canAttack = true;
        moveStop = false;
        TargetSoldiers.Clear();
        GameManager.Instance.RemoveMonster(this);
        GameManager.Instance.objectManager.ReleaseObject(gameObject);
    }
}

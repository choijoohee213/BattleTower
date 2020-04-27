using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour {   
    [SerializeField]
    private float speed;

    private Stack<Node> path;
    private Vector3 destination;
    private Animator anim;

    [SerializeField]
    private float health;
    public float currentHealth;

    [SerializeField]
    private Image gaugeBar;
    private GameObject canvas;

    public bool isDie {
        get { return currentHealth <= 0; }
    }

    public Point GridPosition { get; set; }




    /// <summary>
    /// 
    /// </summary>
    /// 
    private void Awake() {
        anim = GetComponent<Animator>();
        canvas = transform.GetChild(0).gameObject;
    }

    //Spawns the monster in our world
    public void Spawn(int _health) {
        transform.position = LevelManager.Instance.greenPortal.transform.position;

        //Initialization for Health Information
        health = _health;
        currentHealth = _health;
        gaugeBar.fillAmount = 1;


        //Starts to scale the monsters
        StartCoroutine(Scale(new Vector3(0.1f, 0.1f), new Vector3(1, 1), true));
        
        //Sets the monsters path
        SetPath(LevelManager.Instance.Path);
        
        StartCoroutine(MonsterMove());
    }

    //Sclaes a monster up or down
    public IEnumerator Scale(Vector3 from, Vector3 to, bool isActive) {
        float progress = 0;

        //As long as the progress is less than 1, than we need to keep scaling
        while (progress <= 1) {
            transform.localScale = Vector3.Lerp(from, to, progress);
            progress += Time.deltaTime;
            yield return null;
        }
        //Make sure that is has the correct scale after scaling
        transform.localScale = to;


        if (!isActive)
            Release();
        else
            gameObject.SetActive(true);
    }


    //Gives the monster a path to walk on
    void SetPath(Stack<Node> newPath) {
        if (newPath != null) {
            path = newPath;
            Animate(GridPosition, path.Peek().GridPosition);
            GridPosition = path.Peek().GridPosition;
            destination = path.Pop().WorldPosition;
        }
    }

    //Makes the monster move along the given path
    IEnumerator MonsterMove() {
        while (!isDie) {
            transform.position = Vector2.MoveTowards(transform.position, destination, speed);

            //Checks if monster arrived at the destination
            if (transform.position == destination) {
                if (path != null & path.Count > 0) {
                    Animate(GridPosition, path.Peek().GridPosition);

                    //Sets the new GirdPosition and destination
                    GridPosition = path.Peek().GridPosition;
                    destination = path.Pop().WorldPosition;
                }
                else
                    break;
            }
            yield return new WaitForSeconds(0.03f);
        }

        //Monsters arrive at the edge of the map without dying
        if (!isDie && GridPosition == LevelManager.Instance.purpleSpawn) {
            StartCoroutine(Scale(new Vector3(1, 1), new Vector3(0.1f, 0.1f), false));
            GameManager.Instance.Lives--;
        }
    }


    void Animate(Point currentPos, Point newPos) {
        if (currentPos.y < newPos.y) { //Moving down
            anim.SetBool("MonsterDown", true);
        }
        else if (currentPos.y > newPos.y) {  //Moving up
            anim.SetBool("MonsterDown", false);
        }

        if (currentPos.y == newPos.y) {
            anim.SetBool("MonsterDown", true);
            if (currentPos.x > newPos.x) {  //Move to left
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (currentPos.x < newPos.x) {  //Move to right
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    public void TakeDamage(float damage) {
        currentHealth -= damage;        
        if(currentHealth <= 0) {  //Monster death
            currentHealth = 0;
            canvas.SetActive(false);
            anim.SetTrigger("MonsterDie");
        }
        gaugeBar.fillAmount = currentHealth / health;
    }

    public void Release() {
        GridPosition = LevelManager.Instance.greenSpawn;
        canvas.SetActive(true);
        GameManager.Instance.objectManager.ReleaseObject(gameObject);
        GameManager.Instance.RemoveMonster(this);
    }
}

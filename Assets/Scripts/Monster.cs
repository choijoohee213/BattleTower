using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField]
    private float speed;

    private Stack<Node> path;
    private Vector3 destination;
    private Animator anim;

    public Point GridPosition { get; set; }


    //Spawns the monster in our world
    public void Spawn() {
        transform.position = LevelManager.Instance.greenPortal.transform.position;
        anim = GetComponent<Animator>();
        StartCoroutine(Scale(new Vector3(0.1f,0.1f), new Vector3(1,1),true));
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
        gameObject.SetActive(isActive);
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
        while (true) {
            transform.position = Vector2.MoveTowards(transform.position, destination, speed);
            
            //Checks if monster arrived at the destination
            if (transform.position == destination) {
                if (path != null & path.Count > 0) {
                    Animate(GridPosition, path.Peek().GridPosition);

                    //Sets the new GirdPosition and destination
                    GridPosition = path.Peek().GridPosition;
                    destination = path.Pop().WorldPosition;
                }
                else break;
            }
            yield return new WaitForSeconds(0.03f);
        }

        
        if(GridPosition == LevelManager.Instance.purpleSpawn) {
            StartCoroutine(Scale(new Vector3(1, 1), new Vector3(0.1f, 0.1f), false));
        }
    }
    

    void Animate(Point currentPos, Point newPos) {
        if(currentPos.y < newPos.y) { //Moving down
            anim.SetBool("MonsterDown", true);
        }
        else if(currentPos.y > newPos.y) {  //Moving up
            anim.SetBool("MonsterDown", false);
        }

        if (currentPos.y == newPos.y) {
            anim.SetBool("MonsterDown", true);
            if (currentPos.x > newPos.x) {  //Move to left
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if(currentPos.x < newPos.x) {  //Move to right
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}

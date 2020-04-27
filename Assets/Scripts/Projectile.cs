using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Monster target;
    private Tower parent;
    private Animator myAnimator;

    void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveToTarget();
    }

    public void Initialize(Tower parent) {
        this.parent = parent;
        this.target = parent.Target;
   

    }

    void MoveToTarget() {
        if(target != null && target.gameObject.activeSelf) {
            if(target.isDie)
                GameManager.Instance.objectManager.ReleaseObject(gameObject);
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * parent.ProjectileSpeed);

            Vector2 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else if (!target.gameObject.activeSelf) {
            GameManager.Instance.objectManager.ReleaseObject(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Monster")) {
            if (target.gameObject.Equals(collision.gameObject)) {
                target.TakeDamage(parent.Damage);
                myAnimator.SetTrigger("Impact");
            }
        }
    }
}

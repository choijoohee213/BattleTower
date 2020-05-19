using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    private bool collideMonster = false;

    [SerializeField]
    private int level;

    private Monster target;
    private Tower parent;
    private Animator myAnimator;
    private Element elementType;

    void Start() {
        myAnimator = GetComponent<Animator>();
    }

    void Update() {
        MoveToTarget();
    }

    public void Initialize(Tower parent) {
        this.parent = parent;
        target = parent.Target;
        elementType = parent.ElementType;
        collideMonster = false;
    }

    void MoveToTarget() {
        if(target != null && target.gameObject.activeSelf) {
            if(target.isDie)
                GameManager.Instance.objectManager.ReleaseObject(gameObject);

            Vector2 dir = target.transform.position - transform.position;
            if(elementType.Equals(Element.BOMB))
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position - new Vector3(0, 0.2f, 0), Time.deltaTime * parent.ProjectileSpeed);
            else
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * parent.ProjectileSpeed);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if(!(elementType.Equals(Element.BOMB) && level.Equals(6)))
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else if(!target.gameObject.activeSelf)
            GameManager.Instance.objectManager.ReleaseObject(gameObject);

        if(collideMonster)
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Monster")) {
            if(target.gameObject.Equals(collision.gameObject)) {
                collideMonster = true;
                target.TakeDamage(parent.Damage, elementType);
                myAnimator.SetTrigger("Impact");
            }
        }
    }
}

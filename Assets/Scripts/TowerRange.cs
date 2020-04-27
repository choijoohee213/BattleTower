using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRange : MonoBehaviour
{
    public Point GridPosition { get; set; }


    void OnTriggerEnter2D(Collider2D collision) {
        //Adds new monsters to the queue when they enter the range
        if (collision.CompareTag("Monster")) {
            GameManager.Instance.Towers[GridPosition].MonsterInRange(collision.GetComponent<Monster>());
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Monster")) {
            GameManager.Instance.Towers[GridPosition].MonsterOutRange();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRange : MonoBehaviour
{
    private bool isBarracks;

    public Point GridPosition { get; set; }

    private void Start() {
        isBarracks = GameManager.Instance.Towers[GridPosition].ElementType.Equals(Element.BARRACKS);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        //Adds new monsters to the queue when they enter the range
        if (collision.CompareTag("Monster"))
            GameManager.Instance.Towers[GridPosition].MonsterInRange(collision.GetComponent<Monster>());
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Monster") ) {
            if(!isBarracks) GameManager.Instance.Towers[GridPosition].MonsterOutRange(collision.GetComponent<Monster>(), collision.GetComponent<Monster>().Equals(GameManager.Instance.Towers[GridPosition].Target));
            else GameManager.Instance.Towers[GridPosition].MonsterOutRange(collision.GetComponent<Monster>(), false);
        }
    }

}

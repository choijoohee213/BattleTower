using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private Transform towerParent;

    public Point selectSpawnUI;

    public void SetTowerParent(Tower tower) {
        tower.transform.SetParent(towerParent);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public string[] towerNames, towerDescriptions;
    public float[] towerOffensePower, projectileSpeed, attackCoolDown;

    private void Start() {
        towerNames = new string[4] { "궁수 타워", "마법사 타워", "대포", "민병대" };
       
        towerDescriptions = new string[4] {
        "궁수들은 멀리서 적들을 사냥하도록 대기합니다.",
        "마법사들은 물리 피해를 무시하는 마법탄을 발사합니다.",
        "대포를 쏘아 지상의 적을 공격해 피해를 줍니다.",
        "적을 차단하며 싸우는 튼튼한 민병대의 훈련소입니다."};
       
        towerOffensePower = new float[4] { 1f, 2f, 3f, 2f };
        projectileSpeed = new float[4] { 3f, 2.5f, 2.3f, 1f };
        attackCoolDown = new float[4] { 1f, 2f, 3f, 3f };
    }

    public void Initilaize(int towerIndex, ref float power, ref float speed, ref float coolTime) {
        power = towerOffensePower[towerIndex];
        speed = projectileSpeed[towerIndex];
        coolTime = attackCoolDown[towerIndex];
    }

    public int ProjectileType(int towerIndex, int towerLevel) {
        int typeLevel = 0;
        switch (towerIndex) {
            case 0 :  //Archer Tower : 1,3,5,7
                if(towerLevel <= 2) typeLevel = 1;
                else if(towerLevel <= 4) typeLevel = 3;
                else if(towerLevel <= 6) typeLevel = 5;
                else typeLevel = towerLevel;
                break;
            case 1:  //Wizard Tower : 1,3,5,6,7
                if (towerLevel <= 2) typeLevel = 1;
                else if (towerLevel <= 4) typeLevel = 3;
                else typeLevel = towerLevel;
                break;
            case 2:  //Bomb Tower : 1,3,6,7
                if (towerLevel <= 2) typeLevel = 1;
                else if (towerLevel <= 5) typeLevel = 3;
                else typeLevel = towerLevel;
                break;
            case 3:  //Barracks
                typeLevel = towerLevel;
                break;
        }

        return typeLevel;
    }


    public string MonsterType(int monsterIndex) {
        string type = string.Empty;
        switch (monsterIndex) {
                case 0: type = "YellowMonster"; break;
                case 1: type = "GreyMonster"; break;
                case 2: type = "BlueMonster"; break;
                case 3: type = "RedMonster"; break;
                case 4: type = "YellowRobot"; break;
                case 5: type = "GreyRobot"; break;
                case 6: type = "BlueRobot"; break;
                case 7: type = "YellowArcher"; break;
                case 8: type = "GreyArcher"; break;
                case 9: type = "BlueArcher"; break;
                case 10: type = "YellowBomber"; break;
                case 11: type = "GreyBomber"; break;
                case 12: type = "BlueBomber"; break;
                case 13: type = "YellowKnight"; break;
                case 14: type = "GreyKnight"; break;
                case 15: type = "BlueKnight"; break;
                case 16: type = "YellowMagician"; break;
                case 17: type = "GreyMagician"; break;
                case 18: type = "BlueMagician"; break;
                case 19: type = "RedHammer"; break;
                case 20: type = "RedMute"; break;
        }
        return type;
    }
}

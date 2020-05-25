using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DataManager : MonoBehaviour {
    public Queue<Queue<int>> waveData;
   
    public string[] TowerNamesKR { get; set; }
    public string[] TowerNamesENG { get; set; }
    public string[] TowerDescriptions { get; set; }

    public float[] TowerOffensePower { get; set; }
    public float[] ProjectileSpeed { get; set; }
    public float[] AttackCoolDown { get; set; }

    private void Start() {
        waveData = new Queue<Queue<int>>();
        LevelDataLoad();

        TowerNamesKR = new string[4] { "궁수 타워", "마법사 타워", "대포", "민병대" };
        TowerNamesENG = new string[4] { "Archer", "Wizard", "Bomb", "Barracks" };

        TowerDescriptions = new string[4] {
        "궁수들은 멀리서 적들을 사냥하도록 대기합니다.",
        "마법사들은 물리 피해를 무시하는 마법탄을 발사합니다.",
        "대포를 쏘아 지상의 적을 공격해 피해를 줍니다.",
        "적을 차단하며 싸우는 튼튼한 민병대의 훈련소입니다."};
       
        TowerOffensePower = new float[4] { 1f, 2f, 3f, 2f };
        ProjectileSpeed = new float[4] { 3f, 2.5f, 2.3f, 2f };
        AttackCoolDown = new float[4] { 1f, 2f, 3f, 8f };
    }

    public void Initilaize(int towerIndex, ref float power, ref float speed, ref float coolTime) {
        power = TowerOffensePower[towerIndex];
        speed = ProjectileSpeed[towerIndex];
        coolTime = AttackCoolDown[towerIndex];
    }

    //There is an order of monsters to appear in the level, and the level status information reads the text file on the last line.
    private void LevelDataLoad() {
        TextAsset textAsset = Resources.Load<TextAsset>("Level" + LevelManager.Instance.GameLevel.ToString());
        string[] lines = textAsset.text.Split('\n'); //Read line by line in text file

        //Read one word per line
        for(int i=0; i<lines.Length; i++) {
            Queue<int> monsterData = new Queue<int>();
            string[] words = lines[i].Split('\t');
            if(i.Equals(lines.Length - 1)) {
                //Level States : Money, Monster wave max, Tower level max
                GameManager.Instance.GameStateInitial(int.Parse(words[0]), int.Parse(words[1]), int.Parse(words[2]));
                return;
            }

            foreach(string index in words) 
                monsterData.Enqueue(int.Parse(index));
            waveData.Enqueue(monsterData);
        }
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
            case 3:  //Barracks : 1,3,5
                if(towerLevel <=2) typeLevel = 1;
                else if(towerLevel <= 4) typeLevel = 2;
                else if(towerLevel <= 6) typeLevel = 3;
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

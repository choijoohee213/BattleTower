using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : Singleton<MainMenuManager>
{
    public int selectedLevel;
    public GameObject dontDestory;

    [SerializeField]
    Animator Loading;

    [SerializeField]
    GameObject[] LevelBtns;


    private void Start() {
        Time.timeScale = 1;
        if(PlayerPrefs.GetInt("AchieveLevel").Equals(0))
            PlayerPrefs.SetInt("AchieveLevel", 1);

        if(SceneManager.GetActiveScene().buildIndex.Equals(1)){
            if(dontDestory == null && gameObject.activeInHierarchy) {
                DontDestroyOnLoad(gameObject);
                dontDestory = gameObject;
            }
            else {
                Destroy(dontDestory);
            }
            ShowLevelMap();
        }
    }

    public void ShowLevelMap() {
        for(int i=0; i< PlayerPrefs.GetInt("AchieveLevel"); i++) {
            LevelBtns[i].SetActive(true);
        }
    }

    public void LoadScene(int index) {
        SceneManager.LoadScene(index);
    }

    public void GameStart(int level) {
        selectedLevel = level;
        Loading.SetTrigger("FadeOut");
    }
}

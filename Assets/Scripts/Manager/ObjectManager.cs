using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] objectPrefabs;

    List<GameObject> pooledObjs = new List<GameObject>();

    [SerializeField]
    Transform monster, projectile, canvas, soldier;

    private GameObject Generate(string type, bool isActive) {
        for(int i = 0; i < objectPrefabs.Length; i++) {
            if(objectPrefabs[i].name.Equals(type)) {
                GameObject newObject = Instantiate(objectPrefabs[i]);
                newObject.SetActive(isActive);

                if(objectPrefabs[i].CompareTag("Monster"))
                    newObject.transform.SetParent(monster);
                else if(objectPrefabs[i].CompareTag("Projectile"))
                    newObject.transform.SetParent(projectile);
                else if(objectPrefabs[i].name.Contains("Bar"))
                    newObject.transform.SetParent(canvas);
                else if(objectPrefabs[i].CompareTag("Soldier"))
                    newObject.transform.SetParent(soldier);

                pooledObjs.Add(newObject);
                newObject.name = type;
                return newObject;
            }
        }
        return null;
    }


    public GameObject GetObject(string type) {
        foreach(GameObject obj in pooledObjs) {
            if(obj.name.Equals(type) && !obj.activeInHierarchy) {
                obj.SetActive(true);
                return obj;
            }
        }

        return Generate(type, true);       
    }

    public void ReleaseObject(GameObject gameObject) {
        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] objectPrefabs;

    List<GameObject> pooledObjs = new List<GameObject>();

    [SerializeField]
    Transform monster, projectile;

   
    public GameObject GetObject(string type) {
        foreach(GameObject obj in pooledObjs) {
            if(obj.name.Equals(type) && !obj.activeInHierarchy) {
                obj.SetActive(true);
                return obj;
            }
        }

        for(int i=0; i<objectPrefabs.Length; i++) {
            if(objectPrefabs[i].name.Equals(type)) {
                GameObject newObject = Instantiate(objectPrefabs[i]);
                
                if (objectPrefabs[i].name.Contains("Monster"))
                    newObject.transform.SetParent(monster);
                else if (objectPrefabs[i].name.Contains("Projectile"))
                    newObject.transform.SetParent(projectile);
                
                pooledObjs.Add(newObject);
                newObject.name = type;
                return newObject;
            }
        }
        return null;
    }

    public void ReleaseObject(GameObject gameObject) {
        gameObject.SetActive(false);
    }
}

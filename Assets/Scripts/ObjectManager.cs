using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] objectPrefabs;

    [SerializeField]
    Transform monster;

    public GameObject GetObject(string type) {
        
        for(int i=0; i<objectPrefabs.Length; i++) {
            if(objectPrefabs[i].name == type) {
                GameObject newObject = Instantiate(objectPrefabs[i],monster);
                newObject.name = type;
                return newObject;
            }
        }
        return null;
    }
}

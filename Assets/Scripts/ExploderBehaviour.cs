using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploderBehaviour : MonoBehaviour
{

    public GameObject[] prefabsToSpawn = new GameObject[0];

    // Start is called before the first frame update
    void Start()
    {
        foreach(var go in prefabsToSpawn)
        {
            Instantiate<GameObject>(go, transform.position, Quaternion.identity);
            //give it a kick...
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

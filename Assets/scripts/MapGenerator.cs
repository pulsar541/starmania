using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MapGenerator : NetworkBehaviour
{ 
    LevelController levelController; 
    public static bool levelAlreadyGenerated = false;
 

    void Awake()
    {
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
          
    }
    void Start()
    {   
        if(levelAlreadyGenerated)
            return; 

        int seed = (int)transform.position.x  +  100 * ((int)transform.position.y  +  (100 * (int)transform.position.z)) ; 
        levelController.GenerateLevel(seed);
        levelController.Build(); 

        levelAlreadyGenerated = true;
    } 

    public static void Reset() 
    {
        levelAlreadyGenerated = false;
    }
}

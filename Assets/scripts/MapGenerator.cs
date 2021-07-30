using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MapGenerator : NetworkBehaviour
{ 
    LevelController levelController;
    GameObject player;

    public static bool levelAlreadyGenerated = false;

    void Awake()
    {
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
          
    }
    void Start()
    {   
        if(levelAlreadyGenerated)
            return;

        player  = GameObject.Find("Player(Clone)");

        int seed = (int)transform.position.x * (int)transform.position.y * (int)transform.position.z; 
        levelController.GenerateLevel(seed);
        levelController.Build();
        levelController.BindPlayerGameObject(player);

        levelAlreadyGenerated = true;
    } 
}

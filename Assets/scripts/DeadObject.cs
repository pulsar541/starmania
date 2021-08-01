using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadObject : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 

        foreach(Transform child in transform) { 
            Light light = child.GetComponent<Light>(); 
            if(light != null) {
                    light.gameObject.SetActive(false);
            }
        
        }

        foreach (var item in Physics.OverlapBox(transform.position, new Vector3(transform.localScale.x/3 ,transform.localScale.y/4 ,transform.localScale.z/3  )))
        {
            Player player = item.GetComponent<Player>();
            if (player)
            {
                player.Health = 0;

                // player.transform.position = LevelController.mapCenter;  


                foreach(Transform child in transform) { 
                    Light light = child.GetComponent<Light>(); 
                    if(light != null) {
                         light.gameObject.SetActive(true);
                    }
             
                }


            } 
        }   
    }
}

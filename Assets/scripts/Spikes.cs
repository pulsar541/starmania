using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {  
        foreach (var item in Physics.OverlapBox(transform.position, new Vector3(transform.localScale.x/3 ,transform.localScale.y/5 ,transform.localScale.z/3  )))
        {
            Player player = item.GetComponent<Player>();
            if (player)
            {
                player.Health = 0;  
            } 
        }   
    }
}

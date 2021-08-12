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
        foreach (var item in Physics.OverlapBox(transform.position, 
        new Vector3(transform.localScale.x*0.55f ,transform.localScale.y*0.55f ,transform.localScale.z*0.55f )))
        {
            Player player = item.GetComponent<Player>();
            if (player)
            {
                player.Health = 0;  
            } 
        }   
    }
}

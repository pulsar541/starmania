using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Cable : NetworkBehaviour
{
  //  [SerializeField] private GameObject cablePrefab = null; 
    
    //Vector3 nextDuplicatePos;
    // Start is called before the first frame update
    void Start()
    {
    }

    List<Player> _tmpPlayers = new List<Player>();

    // Update is called once per frame
    void FixedUpdate()
    {
        // Vector3 point0 = transform.position  + new Vector3(0, -1,0);
        // Vector3 point1 = transform.position  + new Vector3(0, 1,0);
        
    
        // // foreach(Player pl in _tmpPlayers)
        // // {
        // //     if(pl.gravity == 0) {
        // //         pl.gravity = Player.STANDART_GRAVITY;
        // //         _tmpPlayers.Remove(pl);
        // //         break;
        // //     }
        // // }

        // bool wasPlayerCollision = false;
        // foreach (var item in Physics.OverlapCapsule (point0, point1, 0.2f))
        // {
        //     Player player = item.GetComponent<Player>();
        //     if (player)
        //     {   
        //        // _tmpPlayers.Add(player);
        //         player.inCable = true;  
                
        //         wasPlayerCollision = true;
        //     }  
        // }


        // if(!wasPlayerCollision) 
        // {
        //     foreach (var item in Physics.OverlapCapsule (point0, point1, 3))
        //     {
        //         Player player = item.GetComponent<Player>();
        //         if (player)
        //         {    
        //             player.inCable = false;   
        //         }  
        //     }
            
        // }


    }
 

}

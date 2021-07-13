using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Fireball : NetworkBehaviour
{ 
    uint owner;
    bool inited;
 
    Vector3 dir;
    Vector3 start;
    float speed = 0;

    public void Init(uint owner, Vector3 start, Vector3 dir, float speed)
    {
        this.owner = owner; //кто сделал выстрел
        this.dir = dir; //куда должна лететь пуля
        transform.position = this.start = start;
        this.speed = speed;
        inited = true;
        
    }
 
    // Update is called once per frame
    void Update()
    {
        if (inited && isServer)
        {
            transform.Translate(dir.normalized * speed * Time.deltaTime);

            foreach (var item in Physics.OverlapSphere(transform.position, transform.localScale.x))
            {
                Player player = item.GetComponent<Player>();
                if (player)
                {
                    if (player.netId != owner)
                    {
                        player.ChangeHealthValue(player.Health - 10); //отнимаем одну жизнь по аналогии с примером SyncVar 
                        NetworkServer.Destroy(gameObject);  
                    }
                } 
            }

            foreach (var item in Physics.OverlapBox(transform.position, new Vector3(0,0,0), Quaternion.identity))
            { 
                Cube cube = item.GetComponent<Cube>();
                if(cube) {
                    NetworkServer.Destroy(gameObject);    
                 //   NetworkServer.Destroy(cube.gameObject);
                }
            }

            if (Vector3.Distance(transform.position, start) > 50)  
            {
                NetworkServer.Destroy(gameObject); 
            }
        }   
    }
}

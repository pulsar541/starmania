using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Fireball : NetworkBehaviour
{ 


    [SerializeField] private LightManager lightManagerGO = null;

    uint owner;
    bool inited;
 
    Vector3 _movement;
    Vector3 start; 

    Vector3 oldPos;
    BoxCollider boxColl;
    Color expandLightColor;


    int _type = 0;
   
 
     
    public void Awake()
    {
        boxColl = GetComponent<BoxCollider>(); 
        boxColl.enabled = false;
    }
    public void Init(uint owner, Vector3 start, Vector3 movement, int type = 0)
    {
        this.owner = owner; //кто сделал выстрел
        this._movement = movement; //куда должна лететь пуля
        oldPos = transform.position = this.start = start; 
        inited = true;

        _type = type;

        expandLightColor = LightManager.GetLampColorByPosition(start) ;  

        if(_type == 1) 
        {
            foreach(Transform child in transform) { 
                Light light = child.GetComponent<Light>(); 
                if(light != null) {
                    light.color = Color.yellow;   
                } 
            }

            Material mat = GetComponent<Renderer>().material;
            mat.SetColor("_EmissionColor", Color.yellow);
        }   

    }

    
    [SerializeField] private GameObject fireballExpandedPrefab = null;

    public void Expand()
    { 
        if(isServer) 
        {
            MakeExpand();
        }
        else
        {
            CmdMakeExpand();
        }
 
    }


    [Server]
    public void MakeExpand()
    {    
        transform.localScale  = new Vector3(0.2f, 0.05f, 0.2f);   
        boxColl.enabled = true;   

 
        foreach(Transform child in transform) { 
            Light light = child.GetComponent<Light>(); 
            if(light != null) {
                //light.color = LightManager.GetLampColorByPosition(light.transform.position);
                //light.range = 5;
                
                Destroy(light.gameObject);
            }
    
            ParticleSystem partSys = child.GetComponent<ParticleSystem>(); 
            if(partSys != null) { 
                Destroy(partSys.gameObject);
            }
        }

 
       // Vector3 = new Vector3((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
       
        // if(transform.position.y < LevelController.CUBES_J / 2 - 1)
        //     lightManagerGO.InsertLight(transform.position, LightManager.GetLampColorByPosition(transform.position));
        // else  {
        //     Material mat = GetComponent<Renderer>().material;
        //     mat.SetColor("_EmissionColor", Color.black);
        // }
 
        GameObject fireballEx = Instantiate(fireballExpandedPrefab, transform.position, Quaternion.identity); //Создаем локальный объект пули на сервере
        NetworkServer.Spawn(fireballEx); //отправляем информацию о сетевом объекте всем игрокам. 

        NetworkServer.Destroy(gameObject);
    }

    [Command]
    public void CmdMakeExpand()
    { 
        MakeExpand();
    }
  

 
 
    // Update is called once per frame
    void Update()
    {
         //lightManagerGO.ActivateLight(transform.position, 3);

        if ( inited /*&& isServer */)
        {
            transform.Translate(_movement * Time.deltaTime);
           // Vector3 lpos = new Vector3((int)transform.position.x, (int)transform.position.y,(int)transform.position.z);
           // lightManagerGO.GetComponent<LightManager>().InsertLight(lpos, expandLightColor);
 
            foreach (var item in Physics.OverlapSphere(transform.position, transform.localScale.x))
            {
                Player player = item.GetComponent<Player>();
                if (player)
                {
                    if (player.netId != owner)
                    {
                        if(isServer)
                            player.ChangeHealthValue(player.Health - 50);  
                        else
                            player.CmdChangeHealth(player.Health - 50);   


                        player.score++;
                        player.CmdScoreUp(player.score);
                        player.UpdateData();

 
                    }
                } 

                if(_type == 1)
                {
                    Enemy enemy = item.GetComponent<Enemy>();
                    if (enemy)
                    {
                        if (enemy.netId != owner)
                        {
                            if(isServer)
                                enemy.ChangeHealth(enemy.Health - 20); 
                            else  
                                enemy.CmdChangeHealth(enemy.Health - 20);  

                            NetworkServer.Destroy(gameObject);            
                        }
                    } 
                }

                Cable cable = item.GetComponent<Cable>();
                if(cable)
                {   //Vector3 cablePos = cable.gameObject.transform.position;
                    //LevelController.control.hasCable[Mathf.RoundToInt(cablePos.x), Mathf.RoundToInt(cablePos.y), Mathf.RoundToInt(cablePos.z)] = false;
                   // NetworkServer.Destroy(cable.gameObject); 
                    NetworkServer.Destroy(gameObject);  
                }
            }

            foreach (var item in Physics.OverlapBox(transform.position, transform.localScale/2))
            {
                FireballEx fireballEx = item.GetComponent<FireballEx>();
                if (fireballEx)
                {
                    
                        ///player.ChangeHealthValue(player.Health - 10); //отнимаем одну жизнь по аналогии с примером SyncVar 
                        NetworkServer.Destroy(gameObject); 
                        NetworkServer.Destroy(fireballEx.gameObject);  
                        lightManagerGO.GetComponent<LightManager>().DestroyLight(transform.position);
                     
                } 
            }

            foreach (var item in Physics.OverlapBox(transform.position, transform.localScale/2, Quaternion.identity))
            { 
                Cube cube = item.GetComponent<Cube>();
                if(cube) { 
                    if(_type == 0)
                        Expand(); 

                }

            } 

             foreach (var item in Physics.OverlapSphere(transform.position, transform.localScale.x))
             { 
                Fireball fireball = item.GetComponent<Fireball>();
                if (fireball && fireball.netId != owner)
                {
                    if(_type == 0)
                        Expand(); 
                }
             }

            if (Vector3.Distance(transform.position, start) > 50)  
            {
                NetworkServer.Destroy(gameObject); 
            }
  
        }   
    }

    // void FixedUpdate()
    // {
    //     if(_type == 1) 
    //     {

    //     }   
    // }
  
}

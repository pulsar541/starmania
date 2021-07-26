using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Fireball : NetworkBehaviour
{ 


    [SerializeField] private LightManager lightManagerGO = null;

    uint owner;
    bool inited;
 
    Vector3 dir;
    Vector3 start;
    float _speed = 0;

    Vector3 oldPos;
    BoxCollider boxColl;
    Color expandLightColor;
    public void Awake()
    {
        boxColl = GetComponent<BoxCollider>(); 
        boxColl.enabled = false;
    }
    public void Init(uint owner, Vector3 start, Vector3 dir, float speed)
    {
        this.owner = owner; //кто сделал выстрел
        this.dir = dir; //куда должна лететь пуля
        oldPos = transform.position = this.start = start;
        this._speed = speed;
        inited = true;

        expandLightColor = LightManager.GetLampColorByPosition(start) ;    
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
        _speed = 0; 

 
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
        if ( inited && isServer )
        {
            transform.Translate(dir.normalized * _speed * Time.deltaTime);
           // Vector3 lpos = new Vector3((int)transform.position.x, (int)transform.position.y,(int)transform.position.z);
           // lightManagerGO.GetComponent<LightManager>().InsertLight(lpos, expandLightColor);
             
            foreach (var item in Physics.OverlapSphere(transform.position, transform.localScale.x))
            {
                Player player = item.GetComponent<Player>();
                if (player)
                {
                    if (player.netId != owner)
                    {
                        ///player.ChangeHealthValue(player.Health - 10); //отнимаем одну жизнь по аналогии с примером SyncVar 
                       // NetworkServer.Destroy(gameObject);  
                    }
                } 
            }

            foreach (var item in Physics.OverlapBox(transform.position, new Vector3(0,0,0), Quaternion.identity))
            { 
                Cube cube = item.GetComponent<Cube>();
                if(cube) {
                    NetworkServer.Destroy(gameObject);    
                    //NetworkServer.Destroy(cube.gameObject);

                    //      Expand(); 

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

            if (Vector3.Distance(transform.position, start) > 50)  
            {
                NetworkServer.Destroy(gameObject); 
            }
  
        }   
    }


    
  
}

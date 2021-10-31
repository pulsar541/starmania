using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Enemy : NetworkBehaviour
{

    LevelController levelController;

    Vector3 movement = new Vector3();

    private GameObject _localPlayer;


    [SerializeField] private GameObject fireballPrefab = null;

    int dir = 0;
    [SyncVar(hook = nameof(SyncDir))]  
    int _SyncDir;

    
    public int Health = 100;
    [SyncVar(hook = nameof(SyncHealth))]  
    int _SyncHealth;
 

    float _changeModeInterval = 0;

    bool _stopingMode = false;


    float __fireInterval = 0;

    void Awake()
    {
        levelController  = (LevelController)GameObject.Find("LevelController").GetComponent<LevelController>();
    }

    // Start is called before the first frame update
    void Start()
    {   
        Vector3 pos = transform.position;
        levelController.enemyTrigger[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z)] = false;
        movement = new Vector3(1,0,0); 
        Health = 100;
    }

    // Update is called once per frame
    void Update()
    { 

        if(SceneController.pause)
            return;
      
        if (isServer)
        {

            if(levelController.isType( transform.position + movement * 0.5f , LevelController.CubeType.WALL)
              //|| levelController.isType( transform.position + movement * 1.5f , LevelController.CubeType.WALL)
            )
            { 
                int d = Random.Range(0,6);
               
                ChangeDir(d); 

                switch(dir)
                {
                    case 0: movement = new Vector3(1,0,0); break;
                    case 1: movement = new Vector3(-1,0,0); break;
                    case 2: movement = new Vector3(0,0,1); break;        
                    case 3: movement = new Vector3(0,0,-1); break;  
                    case 4: movement = new Vector3(0,1,0); break;        
                    case 5: movement = new Vector3(0,-1,0); break;   

                }
            }
            else 
            {
                if(!_stopingMode)
                    transform.position += movement * Time.deltaTime * 1.0f;
               // transform.position = new Vector3(Mathf.Round(transform.position .x),  Mathf.Round(transform.position .y), Mathf.Round(transform.position .z));
            }

        }
 
        foreach (var item in Physics.OverlapSphere(transform.position, transform.localScale.x/2 ))
        {
            Player player = item.GetComponent<Player>();
            if (player)
            {
                player.Health = 0;   
            } 
        }

        

      //  if(_localPlayer && Vector3.Distance(_localPlayer.transform.position, transform.position) > 3)
       //     gameObject.SetActive(false);
       // else 
         //   gameObject.SetActive(true);     

 

        Vector3 playerPos = new Vector3();
        foreach (var item in Physics.OverlapSphere(transform.position, 10 ))
        {
            Player player = item.GetComponent<Player>();
            if (player)
            { 
                
               // if(isServer)
              //  {
                    _changeModeInterval += Time.deltaTime;
                    if(_changeModeInterval > 5.0f)
                    { 
                        _changeModeInterval = 0; 
                        _stopingMode = !_stopingMode; 
                    }

                    if (_stopingMode)
                    {
                        if (item.GetComponent<CharacterController>())
                            playerPos = item.GetComponent<CharacterController>().transform.position;
                    }
             //   }
 
            } 
        } 

 

        if(_stopingMode)
        {
            __fireInterval += Time.deltaTime;
            if(__fireInterval > 1.0f)
            { 
                Vector3 weaponMovement = Vector3.Normalize(playerPos - transform.position);  

                __fireInterval = 0;

                if (isServer)
                    Fire(netId, transform.position, weaponMovement);
                else
                    CmdFire(netId, transform.position, weaponMovement); 

            }      
        } 



    }


    
    [Server]
    public void Fire(uint owner, Vector3 startPos, Vector3 dir)
    { 
            GameObject fireballGo = Instantiate(fireballPrefab, startPos, Quaternion.identity); //Создаем локальный объект пули на сервере
            NetworkServer.Spawn(fireballGo); //отправляем информацию о сетевом объекте всем игрокам.
            Vector3 fbMovement = dir.normalized * 6.0f + movement;
            fireballGo.GetComponent<Fireball>().Init(owner, startPos, fbMovement, 1); //инициализируем поведение пули
                                                                                      //    _lastFireballGo = fireballGo; 
    }


    [Command]
    public void CmdFire(uint owner, Vector3 startPos, Vector3 dir)
    {
        Fire(owner, startPos, dir);
    }

    void FixedUpdate()
    {
        if(Health <= 0)
            NetworkServer.Destroy(gameObject);  
    }


    [Server]  
    public void ChangeDir(int newValue)
    {
        _SyncDir = newValue; 
    } 

    void SyncDir(int oldValue, int newValue)  
    {
        dir = newValue;
    }

    [Command]  
    public void CmdChangeDir(int newValue)  
    {
        ChangeDir(newValue); 
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////

  
    [Server]
    public void ChangeHealth(int newHealth)
    {
        _SyncHealth = newHealth;
    }

    [Command]
    public void CmdChangeHealth(int newHealth)
    {
        ChangeHealth(newHealth);
    }

    void SyncHealth(int oldValue, int newValue)
    {
        Health = newValue;
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////
    public void BindLocalPlayerGameObject(GameObject playerGO)
    { 
        _localPlayer = playerGO;
    }

}

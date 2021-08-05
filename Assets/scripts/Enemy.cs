using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Enemy : NetworkBehaviour
{

    LevelController levelController;

    Vector3 movement = new Vector3();

    private GameObject _localPlayer;



    int dir = 0;
    [SyncVar(hook = nameof(SyncDir))]  
    int _SyncDir;

    
    public int Health = 100;
    [SyncVar(hook = nameof(SyncHealth))]  
    int _SyncHealth;
 

    void Awake()
    {
        levelController  = (LevelController)GameObject.Find("LevelController").GetComponent<LevelController>();
    }

    // Start is called before the first frame update
    void Start()
    {   
        Vector3 pos = transform.position;
        levelController.enemyTrigger[(int)pos.x, (int)pos.y, (int)pos.z] = false;
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

            if(!levelController.isType( transform.position + movement, LevelController.CubeType.VOID))
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
                transform.position += movement * Time.deltaTime * 3.0f;
            }

        }
 
        foreach (var item in Physics.OverlapSphere(transform.position, transform.localScale.x ))
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

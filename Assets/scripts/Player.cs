﻿using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
//[AddComponentMenu("Control Script/Player")]

public class Player : NetworkBehaviour
{

    //SyncList<Vector3> _SyncVector3Vars = new SyncList<Vector3>();
 
    public int Health;  
    [SyncVar(hook = nameof(SyncHealth))] //задаем метод, который будет выполняться при синхронизации переменной
    int _SyncHealth;



     public int Seed; 
    [SyncVar(hook = nameof(SyncSeed))] //задаем метод, который будет выполняться при синхронизации переменной
    int _SyncSeed;


    public int Score;  
    [SyncVar(hook = nameof(SyncScore))] //задаем метод, который будет выполняться при синхронизации переменной
    int _SyncScore;



    public bool IsLocal {
        get {return isLocalPlayer;}
    }

    public List<Vector3> Vector3Vars;


    private CharacterController _charController;

    [SerializeField] private GameObject fireballPrefab = null;
 
    [SerializeField] private GameObject _head = null;
    
    LightManager lightManager;

    LevelController levelController;

    private ControllerColliderHit _contact;
 
    private Camera _fpsCamera;

    // private Rigidbody _rigidBody;

    public float gravity = 0; //-9.81f;
 
    public float speed = 2.0f;

    private float _vertSpeed;
    public const float baseSpeed = 6.0f;

    // public GameObject cameraFPS;
    public float jumpSpeed = 7.0f;
    public float terminalVelocity = -10.0f;
    public float minFall = -1.5f;


    //public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensivityHor = 9.0f;
    public float sensivityVert = 9.0f;
    public float minVert = -90.0f;
    public float maxVert = 90.0f;

    private float _rotationX = 0;
    private float _rotationY = 0;

    private bool _isMoveEnable = false;
 
    bool wasFirstJump = true;


    float shiftMulSpeeed = 1;

    float _walkCounter = 0;

    GameObject _lastFireballGo = null;

    bool fireBallExpandMode = false;

    void Awake()
    { 
        levelController = (LevelController)GameObject.Find("LevelController").GetComponent<LevelController>();
        lightManager  = (LightManager)GameObject.Find("LightManager").GetComponent<LightManager>();
        _charController = GetComponent<CharacterController>();
       
    }
    void OnDestroy()
    { 
    }

    // Start is called before the first frame update
    void Start()
    { 
         _fpsCamera = (Camera)GameObject.Find("Camera").GetComponent<Camera>(); 
        //  _rigidBody = GetComponent<Rigidbody>(); 
        _vertSpeed = 0;  
        ChangeHealthValue(100);

        DateTime uniDT =   DateTime.Now.ToUniversalTime(); 
        ChangeSeedValue( uniDT.DayOfYear * (uniDT.Hour+1) * (uniDT.Minute+1) * (uniDT.Second+1)); 
       
        if(!levelController.generated) 
        {
            levelController.GenerateLevel(_SyncSeed); 
            levelController.Build();
            levelController.BindPlayerGameObject(gameObject);
        }

        ChangeScoreValue(0);
        
    }
 
    float dist(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    // Update is called once per frame
    void Update()
    { 
        if (!isLocalPlayer) 
            return;

        if(SceneController.pause)
            return;
      
        if (hasAuthority)
        {
 
            if(!SceneController.pause) 
            {
                _rotationX -= Input.GetAxis("Mouse Y") * sensivityVert;
                _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);
                float delta = Input.GetAxis("Mouse X") * sensivityHor;
                _rotationY = _charController.transform.localEulerAngles.y + delta;
                transform.localEulerAngles = new Vector3(0/*_rotationX*/, _rotationY, 0);
            }
        
            //lightManager.SetNonDestroy(transform.position);

            Color lightColor = LightManager.GetLampColorByPosition(transform.position);


           // if(isServer) 
            {
                lightManager.TryInsertLight(transform.position,lightColor, 4); 
                lightManager.TryInsertLight(transform.position + transform.TransformDirection(new Vector3(0,0,3)), lightColor, 4);
            }
          //  else {
          //      lightManager.CmdTryInsertLight(transform.position,lightColor, 4); 
           //     lightManager.CmdTryInsertLight(transform.position + transform.TransformDirection(new Vector3(0,0,3)), lightColor, 4);                
          //  }


            _head.transform.localEulerAngles = new Vector3(_rotationX,0,0);

  
             if (Input.GetKeyUp(KeyCode.LeftShift)) 
            {
                shiftMulSpeeed = 1;
            }

            if (Input.GetKey(KeyCode.LeftShift) && (_charController.isGrounded || gravity == 0))
            {
                shiftMulSpeeed = 2.0f;
            } 

            float deltaX = Input.GetAxis("Horizontal") * speed * shiftMulSpeeed;
            float deltaZ = Input.GetAxis("Vertical") * speed * shiftMulSpeeed;
            if(SceneController.pause) {
                deltaX = 0;
                deltaZ = 0;
            }

            Vector3 movement = new Vector3(deltaX, 0, deltaZ);

            movement = Vector3.ClampMagnitude(movement, speed * shiftMulSpeeed);
            movement = _charController.transform.TransformDirection(movement);

            int px = (int)_charController.transform.position.x;
            int py = (int)_charController.transform.position.y;            
            int pz = (int)_charController.transform.position.z;            


              bool allowWallJump = false;// (levelController.cubes[px+1, py, pz] == LevelController.CubeType.WALL
            //                      ||  levelController.cubes[px, py, pz+1] == LevelController.CubeType.WALL
            //                      ||  levelController.cubes[px-1, py, pz] == LevelController.CubeType.WALL
            //                      ||  levelController.cubes[px, py, pz-1] == LevelController.CubeType.WALL) && !_charController.isGrounded ;

            if (_charController.isGrounded)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    _vertSpeed = jumpSpeed;
                    wasFirstJump = true;
                    allowWallJump = true;
                }
                else
                {
                    _vertSpeed = minFall;
                }
 
            } 
            else
            {
                //  allowWallJump = (levelController.cubes[px+1, py, pz] == LevelController.CubeType.WALL && dist(new Vector3(px+1, py, pz), _charController.transform.position) <= 1.05f
                //                    ||  levelController.cubes[px, py, pz+1] == LevelController.CubeType.WALL && dist(new Vector3(px, py, pz+1), _charController.transform.position) <= 1.05f
                //                    ||  levelController.cubes[px-1, py, pz] == LevelController.CubeType.WALL && dist(new Vector3(px-1, py, pz), _charController.transform.position) <= 1.05f
                //                    ||  levelController.cubes[px, py, pz-1] == LevelController.CubeType.WALL && dist(new Vector3(px, py, pz-1), _charController.transform.position) <= 1.05f)  ;

                /////allowWallJump = levelController.WallsHorizontAroundCount(_charController.transform.position) >= 2;

                if(allowWallJump && Input.GetButtonDown("Jump")) 
                {
                    _vertSpeed = jumpSpeed*0.75f;
                } 
                else if(wasFirstJump && Input.GetButtonDown("Jump")) {
                    _vertSpeed = jumpSpeed;
                     wasFirstJump = false;
                }
                // else if(allowWallJump){
                //     if (Input.GetButtonDown("Jump"))
                //     {
                //         _vertSpeed = jumpSpeed; 
                //         allowWallJump = false;
                //     }      
                // }   


                else {
                    _vertSpeed += gravity * Time.deltaTime;

                    if (_vertSpeed < terminalVelocity)
                    {
                        _vertSpeed = terminalVelocity;
                    }
                } 

            }
 
            bool hitCeiling = false;
            RaycastHit hit;
            Vector3 rayStart = _charController.transform.position + new Vector3(0, _charController.height * 0.5f, 0);
            if (_vertSpeed > 0 && Physics.Raycast(rayStart, Vector3.up * 10.0f, out hit))
            {
                float check = (_head.transform.localScale.y) ;
                hitCeiling = hit.distance <= check ; 
            }
            if (hitCeiling)
                _vertSpeed = 0;
 

            if(gravity != 0)
                movement.y = _vertSpeed;
         
            movement *= Time.deltaTime;

            if (_isMoveEnable) {
                _charController.Move(movement); 

                // if(movement.magnitude > 0) {
                //     lightManager.InsertLight(transform.position);
                // }
            }


            
            if(_charController.transform.position.y < 0) {
               _charController.transform.position = LevelController.mapCenter; 
               _vertSpeed = 0;
               movement.y = 0;
            }


            if(_fpsCamera) {
                _fpsCamera.transform.position = _head.transform.position;  
                if(_charController.isGrounded && (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))) {
                       _walkCounter +=  speed * (shiftMulSpeeed > 1?1.5f:1.0f) * Time.deltaTime * 5.0f; 

                }
                _fpsCamera.transform.position += new Vector3(0,Math.Abs(Mathf.Sin(_walkCounter))*0.035f,0);  
                _fpsCamera.transform.position +=  _fpsCamera.transform.TransformDirection(new Vector3(-Mathf.Cos(_walkCounter)*0.02f,0,0)); 
                _fpsCamera.transform.rotation = _head.transform.rotation; 
                _fpsCamera.transform.localEulerAngles += new Vector3((Mathf.Sin(_walkCounter*2))*0.1f ,0,0);  
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 spawnPos = transform.position;
                float weaponSpeed = 5.0f;
                Vector3 weaponMovement = new Vector3(0, 0, 1);
                weaponMovement = _head.transform.TransformDirection(weaponMovement); //* weaponSpeed;
                spawnPos += _charController.transform.TransformDirection(new Vector3(0.07f, -0.07f, 0));
 
                if(isServer)
                    Fire(netId, spawnPos, weaponMovement, weaponSpeed);
                else 
                    CmdFire(netId, spawnPos, weaponMovement, weaponSpeed);
            }



            if(levelController.Builded) {
                 _isMoveEnable = true; 
            }
  

            if(IsLocal) {
                foreach(Renderer r in this.gameObject.GetComponentsInChildren<Renderer>()) { 
                    r.enabled = false; 
                    }
            }


            


        } // if hasAuthority

    }
 

    private void OnSpeedChanged(float value)
    {
        speed = baseSpeed * value;
    }


    public void SetReady()
    {
        _isMoveEnable = true;
    }


    [Server]
    public void  Fire(uint owner, Vector3 startPos, Vector3 dir, float speed)
    {
        if(fireBallExpandMode && _lastFireballGo != null) 
        { 
            _lastFireballGo.GetComponent<Fireball>().Expand();
            fireBallExpandMode = false;
        }
        else 
        {
            GameObject fireballGo = Instantiate(fireballPrefab, startPos, Quaternion.identity); //Создаем локальный объект пули на сервере
            NetworkServer.Spawn(fireballGo); //отправляем информацию о сетевом объекте всем игрокам.
            fireballGo.GetComponent<Fireball>().Init(owner, startPos, dir, speed); //инициализируем поведение пули
            _lastFireballGo = fireballGo;
            fireBallExpandMode = true;
        }
 
    }
   

    [Command]
    public void CmdFire(uint owner, Vector3 startPos, Vector3 dir, float speed)
    {
        Fire(owner, startPos, dir, speed); 
    }

  



  /////////////////////////////////////////////////////////////////////////////////////
    void SyncHealth(int oldValue, int newValue) //обязательно делаем два значения - старое и новое. 
    {
        Health = newValue;
    }

    [Server] //обозначаем, что этот метод будет вызываться и выполняться только на сервере
    public void ChangeHealthValue(int newValue)
    {
        _SyncHealth = newValue;

        if (_SyncHealth <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    } 

    [Command] //обозначаем, что этот метод должен будет выполняться на сервере по запросу клиента
    public void CmdChangeHealth(int newValue) //обязательно ставим Cmd в начале названия метода
    {
        ChangeHealthValue(newValue); //переходим к непосредственному изменению переменной
    }
  /////////////////////////////////////////////////////////////////////////////////////


    [Server] //обозначаем, что этот метод будет вызываться и выполняться только на сервере
    public void ChangeSeedValue(int newValue)
    {
        _SyncSeed = newValue; 
    } 

    void SyncSeed(int oldValue, int newValue) //обязательно делаем два значения - старое и новое. 
    {
        Seed = newValue;
    }

    [Command] //обозначаем, что этот метод должен будет выполняться на сервере по запросу клиента
    public void CmdChangeSeed(int newValue) //обязательно ставим Cmd в начале названия метода
    {
        ChangeSeedValue(newValue); //переходим к непосредственному изменению переменной
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////



    [Server] //обозначаем, что этот метод будет вызываться и выполняться только на сервере
    public void ChangeScoreValue(int newValue)
    {
        _SyncScore = newValue; 
    } 

    void SyncScore(int oldValue, int newValue) //обязательно делаем два значения - старое и новое. 
    {
        Score = newValue;
    }

    [Command] //обозначаем, что этот метод должен будет выполняться на сервере по запросу клиента
    public void CmdChangeScore(int newValue) //обязательно ставим Cmd в начале названия метода
    {
        ChangeScoreValue(newValue); //переходим к непосредственному изменению переменной
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////




    // public override void OnStartClient()
    // {
    //     base.OnStartClient();
    //     _SyncVector3Vars.Callback += SyncVector3Vars; //вместо hook, для SyncList используем подписку на Callback 
    //     Vector3Vars = new List<Vector3>();//(_SyncVector3Vars.Count); //так как Callback действует только на изменение массива,  
    //     for (int i = 0; i < _SyncVector3Vars.Count; i++) //а у нас на момент подключения уже могут быть какие-то данные в массиве, нам нужно эти данные внести в локальный массив
    //     {
    //         Vector3Vars.Add(_SyncVector3Vars[i]);
    //     }


    //     Debug.Log("OnStartClient. _SyncVector3Vars "  +_SyncVector3Vars.Count);


    //     if(Vector3Vars.Count > 0) {
    //        // for (int i = 0; i < Vector3Vars.Count; i++) 
    //         levelController.ImportLevel(Vector3Vars);     
    //         levelController.Build(); 
    //     }
 
    // }

    // [Server]
    // void ChangeVector3Vars(Vector3 newValue)
    // {
    //     _SyncVector3Vars.Add(newValue);
    // }
    //  [Command]
    //  public void CmdChangeVector3Vars(Vector3 newValue)
    //  {
    //      ChangeVector3Vars(newValue);
    //  }


    // void SyncVector3Vars(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem)
    // {
    //     switch (op)
    //     {
    //         case SyncList<Vector3>.Operation.OP_ADD:
    //             {
    //                 Vector3Vars.Add(newItem);
    //                 break;
    //             }
    //         case SyncList<Vector3>.Operation.OP_CLEAR:
    //             {
    //                 // Vector3Vars.Clear();
    //                 break;
    //             }
    //         case SyncList<Vector3>.Operation.OP_INSERT:
    //             {

    //                 break;
    //             }
    //         case SyncList<Vector3>.Operation.OP_REMOVEAT:
    //             {

    //                 break;
    //             }
    //         case SyncList<Vector3>.Operation.OP_SET:
    //             {
    //                 break;
    //             }
    //     }
    // }
}

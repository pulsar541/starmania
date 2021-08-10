using Mirror;
//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
//[AddComponentMenu("Control Script/Player")]

public class Player : NetworkBehaviour
{

    //SyncList<Vector3> _SyncVector3Vars = new SyncList<Vector3>();

    public int Health = 100;

    public int Seed;
    [SyncVar(hook = nameof(SyncSeed))] //задаем метод, который будет выполняться при синхронизации переменной
    int _SyncSeed;


    public int Score;
    [SyncVar(hook = nameof(SyncScore))] //задаем метод, который будет выполняться при синхронизации переменной
    int _SyncScore;



    public bool IsLocal
    {
        get { return isLocalPlayer; }
    }

    public List<Vector3> Vector3Vars;


    private List<Vector3> pickupCheckpoints;

    private CharacterController _charController;

    [SerializeField] private GameObject cablePrefab = null;
    [SerializeField] private GameObject mapGenPrefab = null;
    [SerializeField] private GameObject fireballPrefab = null;
    [SerializeField] private GameObject enemyPrefab = null;
    [SerializeField] private GameObject playerUIPrefab = null;
    [SerializeField] private GameObject _head = null;
    [SerializeField] private AudioSource soundSourceFire;
    [SerializeField] private AudioClip fireSound;

    [SerializeField] private GameObject headCameraGO = null;

    [SerializeField] private GameObject _mapMarkerGO = null;
    
   // private GameObject _mapCameraGO = null;   

    LightManager lightManager;

    LevelController levelController;

    private ControllerColliderHit _contact;

    private GameObject _goFpsCamera;
  //  private Camera _fpsCamera;

    // private Rigidbody _rigidBody;

    public static float STANDART_GRAVITY = -9.81f;
    private float gravity = STANDART_GRAVITY; //-9.81f;

    public float speed = 2.0f;

    private float _vertSpeed = 0;
    public const float baseSpeed = 6.0f;

    // public GameObject cameraFPS;
    public float jumpSpeed = 7.0f;
    public float terminalVelocity = -20.0f;

    public float deathVertSpeed = -9.0f;
    public float minFall = -1.5f;


    //public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensivityHor = 10.0f;
    public float sensivityVert = 10.0f;
    public float minVert = -90.0f;
    public float maxVert = 90.0f;

    private float _rotationX = 0;
    private float _rotationY = 0;

    private bool _isMoveEnable = true;

    bool wasFirstJump = true;


    float shiftMulSpeeed = 1;

    float _walkCounter = 0;

    GameObject _lastFireballGo = null;

    bool fireBallExpandMode = false;


    private int score = 0;

    Vector3 movement = new Vector3();

    public bool inCable = false;

    private int _viewMode = 0;

    /////////////////////////////////////

    public event System.Action<int> OnPlayerNumberChanged;
    public event System.Action<Color32> OnPlayerColorChanged;
    public event System.Action<int> OnPlayerDataChanged;
    // Players List to manage playerNumber
    internal static readonly List<Player> playersList = new List<Player>();

    internal static void ResetPlayerNumbers()
    {
        int playerNumber = 0;
        foreach (Player player in playersList)
        {
            player.playerNumber = playerNumber++;
        }
    }

    [Header("SyncVars")]

    /// <summary>
    /// This is appended to the player name text, e.g. "Player 01"
    /// </summary>
    [SyncVar(hook = nameof(PlayerNumberChanged))]
    public int playerNumber = 0;

    /// <summary>
    /// This is updated by UpdateData which is called from OnStartServer via InvokeRepeating
    /// </summary>
    [SyncVar(hook = nameof(PlayerDataChanged))]
    public int playerData = 0;

    /// <summary>
    /// Random color for the playerData text, assigned in OnStartServer
    /// </summary>
    [SyncVar(hook = nameof(PlayerColorChanged))]
    public Color32 playerColor = Color.white;

    // This is called by the hook of playerNumber SyncVar above
    void PlayerNumberChanged(int _, int newPlayerNumber)
    {
        OnPlayerNumberChanged?.Invoke(newPlayerNumber);
    }

    // This is called by the hook of playerData SyncVar above
    void PlayerDataChanged(int _, int newPlayerData)
    {
        OnPlayerDataChanged?.Invoke(newPlayerData);
    }

    // This is called by the hook of playerColor SyncVar above
    void PlayerColorChanged(Color32 _, Color32 newPlayerColor)
    {
        OnPlayerColorChanged?.Invoke(newPlayerColor);
    }


    GameObject playerUI = null;
    GameObject goDirLight = null;

    static bool isCameraConnected = false;

    [Command]
    public void CmdScoreUp(int newScore)
    {
        // Server say all clients, your score
        RpcScoreUp(newScore);
    }

    [ClientRpc]
    public void RpcScoreUp(int newScore)
    {
        // You dont need do this action again, will be do it only your instance on all clients
        if (!isLocalPlayer)
        {
            score = newScore;
            //scorePlayer1.text = score.ToString();
            UpdateData();
        }

    }

    /////////////////////////////////////////



    void Awake()
    {
        levelController = (LevelController)GameObject.Find("LevelController").GetComponent<LevelController>();
        lightManager = (LightManager)GameObject.Find("LightManager").GetComponent<LightManager>();
        _charController = GetComponent<CharacterController>();
      //   _mapCameraGO = GameObject.Find("CameraMap");
       // _mapCameraGO.SetActive(false);
        _mapMarkerGO = GameObject.Find("MapMarker");
        goDirLight = GameObject.Find("Directional Light");
        goDirLight.SetActive(false);
    }


    void Start()
    {
        
      


        //  _rigidBody = GetComponent<Rigidbody>(); 
        _vertSpeed = 20;
        Health = 100;

        if (isServer)
            MapGenerator(netId);
        else
            CmdMapGenerator(netId);


        if (IsLocal)
        {
            levelController.BindPlayerGameObject(gameObject);
        }

        pickupCheckpoints = new List<Vector3>();

        ChangeScoreValue(0);

        
    }




    ////////////////////////////////////////////////////////



    public override void OnStartServer()
    {
        base.OnStartServer();

        // Add this to the static Players List
        playersList.Add(this);

        // set the Player Color SyncVar
        //playerColor = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);
         playerColor = new Color(Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
        // Start generating updates
        //InvokeRepeating(nameof(UpdateData), 1, 1);

        ReSpawn(LevelController.mapCenter);



        foreach(Transform child in transform) { 
            Renderer ren = child.GetComponent<Renderer>(); 
            if(ren != null) { 
                ren.material.color = playerColor;
                break;
            }
        }
     
    }


    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer()
    {
        CancelInvoke();
        playersList.Remove(this);
    }

    // This only runs on the server, called from OnStartServer via InvokeRepeating
    [ServerCallback]
    void UpdateData()
    {
        playerData = score;
    }

    public override void OnStartClient()
    {
        // Activate the main panel
        ((NetMan)NetworkManager.singleton).mainPanel.gameObject.SetActive(true);

        playerUI = Instantiate(playerUIPrefab, ((NetMan)NetworkManager.singleton).playersPanel); // Instantiate the player UI as child of the Players Panel

        playerUI.GetComponent<PlayerUI>().SetPlayer(this, isLocalPlayer); // Set this player object in PlayerUI to wire up event handlers

        // Invoke all event handlers with the current data
        OnPlayerNumberChanged.Invoke(playerNumber);
        OnPlayerColorChanged.Invoke(playerColor);
        OnPlayerDataChanged.Invoke(playerData);

        ReSpawn(LevelController.mapCenter);

       // RenderSettings.ambientLight = playerColor;


        foreach(Transform child in transform) { 
            Renderer ren = child.GetComponent<Renderer>(); 
            if(ren != null) { 
                ren.material.color = playerColor;
                break;
            }
        }
     
 

    }

    public override void OnStopClient()
    {
        // Remove this player's UI object
        Destroy(playerUI);

        // Disable the main panel for local player
        if (isLocalPlayer)
            ((NetMan)NetworkManager.singleton).mainPanel.gameObject.SetActive(false);
    }

    ////////////////////////////////////////////////////////

    float dist(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
    }


    public void ReSpawn(Vector3 spawnPos)
    {
        Health = 100;
        transform.position = spawnPos;
        _vertSpeed = 0;
        transform.localEulerAngles = new Vector3(0, 0, 0);

    }

    public float GetSpeed()
    {
        return speed;
    }
    public float GetShuftMultiplier()
    {
        return shiftMulSpeeed;
    }

    // Update is called once per frame
    void Update()
    {
        // if (!isLocalPlayer) 
        //      return;

        //  if (SceneController.pause)
        //       return;


        if (hasAuthority)
        {
            if (!SceneController.pause)
            {
                _rotationX -= Input.GetAxis("Mouse Y") * sensivityVert;
                _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);

               // float delta =;
                  
                _rotationY +=  Input.GetAxis("Mouse X") * sensivityHor ;
                transform.localEulerAngles = new Vector3(0, _rotationY, 0);
               
                //transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0);
            }

            //lightManager.SetNonDestroy(transform.position);

            Color lightColor = LightManager.GetLampColorByPosition(transform.position);


            lightManager.ActivateLight(transform.position, 3);


           

            Vector3 newEnemySpawnPos = levelController.TryActivateEnemy(transform.position, 7);
            if (newEnemySpawnPos.x > 0)
            {
                if (isServer)
                    EnemySpawn(netId, newEnemySpawnPos);
                else
                    CmdEnemySpawn(netId, newEnemySpawnPos);

                levelController.enemyTrigger[(int)newEnemySpawnPos.x, (int)newEnemySpawnPos.y, (int)newEnemySpawnPos.z] = false;
            }


            _head.transform.localEulerAngles = new Vector3(_rotationX, 0, 0);


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

            if (SceneController.pause || Health <= 0)
            {
                deltaX = 0;
                deltaZ = 0;
            }

            movement = new Vector3(deltaX, 0, deltaZ);

            movement = Vector3.ClampMagnitude(movement, speed * shiftMulSpeeed);
            movement = _head.transform.TransformDirection(movement);

            int px = (int)Mathf.Round(_charController.transform.position.x);
            int py = (int)Mathf.Round(_charController.transform.position.y);
            int pz = (int)Mathf.Round(_charController.transform.position.z); 



            LevelController.control.AreaExplored(_charController.transform.position);


            bool allowWallJump = false;// (levelController.cubes[px+1, py, pz] == LevelController.CubeType.WALL
            //                      ||  levelController.cubes[px, py, pz+1] == LevelController.CubeType.WALL
            //                      ||  levelController.cubes[px-1, py, pz] == LevelController.CubeType.WALL
            //                      ||  levelController.cubes[px, py, pz-1] == LevelController.CubeType.WALL) && !_charController.isGrounded ;

            if (_charController.isGrounded)
            {
 
                if (Input.GetButtonDown("Jump") && Health > 0)
                {
                    _vertSpeed = jumpSpeed;
                    //wasFirstJump = true;
                    //allowWallJump = true;
                }
                else
                {
                    _vertSpeed = minFall;
                }

            }
            else if(inCable)
            {
                _vertSpeed = 0;
                gravity = 0;
            }
            else
            {
                                        //  allowWallJump = (levelController.cubes[px+1, py, pz] == LevelController.CubeType.WALL && dist(new Vector3(px+1, py, pz), _charController.transform.position) <= 1.05f
                                        //                    ||  levelController.cubes[px, py, pz+1] == LevelController.CubeType.WALL && dist(new Vector3(px, py, pz+1), _charController.transform.position) <= 1.05f
                                        //                    ||  levelController.cubes[px-1, py, pz] == LevelController.CubeType.WALL && dist(new Vector3(px-1, py, pz), _charController.transform.position) <= 1.05f
                                        //                    ||  levelController.cubes[px, py, pz-1] == LevelController.CubeType.WALL && dist(new Vector3(px, py, pz-1), _charController.transform.position) <= 1.05f)  ;

                                        /////allowWallJump = levelController.WallsHorizontAroundCount(_charController.transform.position) >= 2;

                // if (allowWallJump && Input.GetButtonDown("Jump") && Health > 0)
                // {
                //     _vertSpeed = jumpSpeed * 0.75f;
                // }
                // else if (wasFirstJump && Input.GetButtonDown("Jump"))
                // {
                //     _vertSpeed = jumpSpeed;
                //     wasFirstJump = false;
                // }
                                        // else if(allowWallJump){
                                        //     if (Input.GetButtonDown("Jump"))
                                        //     {
                                        //         _vertSpeed = jumpSpeed; 
                                        //         allowWallJump = false;
                                        //     }      
                                        // }   


              //  else
              //  {
                    gravity = Player.STANDART_GRAVITY;
                    _vertSpeed += gravity * Time.deltaTime;

                    if (_vertSpeed < terminalVelocity)
                    {
                        _vertSpeed = terminalVelocity;
                    }
             //   }

            }

            bool hitCeiling = false;
            RaycastHit hit;
            Vector3 rayStart = _charController.transform.position + new Vector3(0, _charController.height * 0.5f, 0);
            if (_vertSpeed > 0 && Physics.Raycast(rayStart, Vector3.up * 10.0f, out hit))
            {
                float check = (_head.transform.localScale.y);
                hitCeiling = hit.distance <= check;
            }
            if (hitCeiling)
                _vertSpeed = 0;







            if (gravity != 0)
                movement.y = _vertSpeed;

            movement *= Time.deltaTime;



            _isMoveEnable = levelController.Builded;

            if (levelController.Builded)
            {
                _charController.Move(movement);

                // if(movement.magnitude > 0) {
                //     lightManager.InsertLight(transform.position);
                // }
            }




            if (transform.position.y < 0)
            {
                Health = 0;
            }

            if (_charController.isGrounded && _vertSpeed < deathVertSpeed)
            {
                Health = 0; ;
            }


            headCameraGO.GetComponent<PlayerHeadCamera>().SetIsPlayerWalking(
                _charController.isGrounded && (Input.GetButton("Horizontal") || Input.GetButton("Vertical")));
   
 
            if(Input.GetKeyDown(KeyCode.Tab)) 
            {
                _viewMode = 1 - _viewMode;

                GameObject sceneController = GameObject.Find("SceneController");

                switch (_viewMode)
                {
                    case 0:
                        headCameraGO.SetActive(true);
                        _mapMarkerGO.SetActive(false);  
                    break;
                    case 1: 
                        
                        headCameraGO.SetActive(false);
                        _mapMarkerGO.SetActive(true);
                    break; 
                }
                
                //viewMode = 1 - viewMode;  
                goDirLight.SetActive(_viewMode == 1);
                //StartCoroutine(LoadAsyncScene("MapScene",false));
            }
 
             Vector3 discretePos = new Vector3( 
                Mathf.Round(transform.position.x),  
                Mathf.Round(transform.position.y), 
                Mathf.Round(transform.position.z));

           _mapMarkerGO.transform.position = discretePos * 0.01f + new Vector3(0,1000,0); 
  
           _mapMarkerGO.transform.localEulerAngles = new Vector3(
               _head.transform.localEulerAngles.x,
               _charController.transform.localEulerAngles.y,
               0              
            );    


            // if (_fpsCamera)
            // {
            //     _fpsCamera.transform.localPosition =  Vector3.zero;
            //     if(SceneController.viewMode == SceneController.ViewModes.ViewModeFPS){
            //         _fpsCamera.transform.localPosition =  Vector3.zero;
            //     }
            //     else { 
            //        // _fpsCamera.transform.position = _head.transform.position * 0.01f + new Vector3(0, 1000, 0); 
            //           _fpsCamera.transform.localPosition += new Vector3(1000,0,-0.5f);
            //         // _fpsCamera.
            //     }
                
            //     if (Health <= 0)
            //         _fpsCamera.transform.position +=_fpsCamera.transform.TransformDirection(new Vector3(0, 0, -0.5f));

            //     if (_charController.isGrounded && (Input.GetButton("Horizontal") || Input.GetButton("Vertical")))
            //     {
            //         _walkCounter += speed * (shiftMulSpeeed > 1 ? 1.5f : 1.0f) * Time.deltaTime * 5.0f;

            //     } 

            //     if(SceneController.viewMode == 0) {
            //         _fpsCamera.transform.position += new Vector3(0, Mathf.Abs(Mathf.Sin(_walkCounter)) * 0.04f, 0);
            //         _fpsCamera.transform.position += transform.TransformDirection(new Vector3(-Mathf.Cos(_walkCounter) * 0.02f, 0, 0));
            //     } 
            //     else
            //     {
            //         _fpsCamera.transform.localPosition +=  new Vector3(0, 0, -3.0f);  
            //     }

            //     //_fpsCamera.transform.rotation = _head.transform.rotation;
            //     _fpsCamera.transform.localEulerAngles = Vector3.zero;
            //     _fpsCamera.transform.localEulerAngles += new Vector3((Mathf.Sin(_walkCounter * 2)) * 0.1f, 0, (-Mathf.Sin(_walkCounter)) * 0.2f);


            // }

            if (Input.GetKeyDown(KeyCode.Mouse0) && Health > 0)
            {

                Vector3 spawnPos = transform.position;
                Vector3 weaponMovement = new Vector3(0, 0, 1);
                weaponMovement = _head.transform.TransformDirection(weaponMovement);
                spawnPos += _charController.transform.TransformDirection(new Vector3(0.035f, 0.01f, 0));

                soundSourceFire.PlayOneShot(fireSound);

                if (isServer)
                    Fire(netId, spawnPos, weaponMovement);
                else
                    CmdFire(netId, spawnPos, weaponMovement);


            }



            if (Input.GetKeyDown(KeyCode.Mouse1) && Health > 0)
            {
                if ((playerNumber % 2) == 0)
                { 
                    Vector3 pos = new Vector3(px,py,pz);
                    Vector3 spawnPos = transform.position +  transform.TransformDirection(new Vector3(0, 0, 0.85f));  

 
                    if(!LevelController.control.HasCable(spawnPos) &&  LevelController.control.isType(pos + new Vector3(0,-1,0), LevelController.CubeType.VOID))
                        StartCoroutine(MakeCable(spawnPos, isServer, netId));  

                } 

               
                // for(float ii  = 48; ii<=52; ii+=0.1f)
                // for(float kk  = 48; kk<=52; kk+=0.1f)   
                // {

                //     if(Mathf.Round(ii) == 50 &&  Mathf.Round(kk) == 50)
                //     {
                //         StartCoroutine(MakeCable(new Vector3(ii,50,kk), isServer, netId)); 
                //     }                    
                // }
                                 
                
/*

            Vector3 p = transform.position;

            if(LevelController.control._cubeGO[(int)p.x, (int)(p.y -0.5f), (int)p.z])
                Debug.Log("TYPE=" + LevelController.control._cubeGO[(int)p.x, (int)(p.y-0.5f), (int)p.z].GetInstanceID());
*/
            }


            ///////////////////////////////////////////////////// 
            foreach (GameObject checkpointGo in levelController.checkPointsGO)
            {
                if (dist(checkpointGo.transform.position, transform.position) < 0.5f && !pickupCheckpoints.Contains(checkpointGo.transform.position))
                {
                    pickupCheckpoints.Add(checkpointGo.transform.position);
                    checkpointGo.GetComponent<Checkpoint>().SetPickup();
                    score++;
                    CmdScoreUp(score);
                    UpdateData();
                }
            }

            //////////////////////////////////////


            if (IsLocal)
            {
                foreach (Renderer r in this.gameObject.GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false;
                    if (Health <= 0)
                        r.enabled = true;
                }


            }


 

            // /////////////////////
            //     if (Health <= 0)
            //     {
            //         _vertSpeed = 0;
            //         transform.localEulerAngles = new Vector3(0, _rotationY, -25);

            //         foreach (Transform child in transform)
            //         {
            //             Light light = child.GetComponent<Light>();
            //             if (light != null)
            //             {
            //                 light.gameObject.SetActive(true);
            //             }
            //         }

            //     }
            //     else
            //     {
            //         foreach (Transform child in transform)
            //         {
            //             Light light = child.GetComponent<Light>();
            //             if (light != null)
            //             {
            //                 light.gameObject.SetActive(false);
            //             }
            //         }

            //     }

            //     if (Health <= 0)
            //         if (Input.GetKeyDown(KeyCode.Mouse0))
            //         {
            //             ReSpawn(LevelController.mapCenter);
            //         }
        //////////////////////////////////////
 


                Vector3 point0 = transform.position  + new Vector3(0, -0.2f,0);
                Vector3 point1 = transform.position  + new Vector3(0, 0.2f,0); 
                inCable = false;
                foreach (var item in Physics.OverlapCapsule (point0, point1, 0.25f))
                {
                    Cable cable = item.GetComponent<Cable>();
                    if (cable)
                    {    
                        inCable = true;   
                    }  
                }
                        



        } // if hasAuthority



    }
  

    void FixedUpdate()
    {
 
        if(hasAuthority)
        {
        /////////////////////
            if (Health <= 0)
            {
                _vertSpeed = 0;
                transform.localEulerAngles = new Vector3(0, _rotationY, -25);

                foreach (Transform child in transform)
                {
                    Light light = child.GetComponent<Light>();
                    if (light != null)
                    {
                        light.gameObject.SetActive(true);
                    }
                }

            }
            else
            {
                foreach (Transform child in transform)
                {
                    Light light = child.GetComponent<Light>();
                    if (light != null)
                    {
                        light.gameObject.SetActive(false);
                    }
                }

            }

            if (Health <= 0)
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    ReSpawn(LevelController.mapCenter);
                }
    //////////////////////////////////////

            }
    }

 

    private void OnSpeedChanged(float value)
    {
        speed = baseSpeed * value;
    }


    //////////////////////////////////////////////////////////////////////////////////
    [Server]
    public void EnemySpawn(uint owner, Vector3 startPos)
    {
        GameObject goEnemy = Instantiate(enemyPrefab, startPos, Quaternion.identity);
        NetworkServer.Spawn(goEnemy);
    }


    [Command]
    public void CmdEnemySpawn(uint owner, Vector3 startPos)
    {
        EnemySpawn(owner, startPos);
    }

    //////////////////////////////////////////////////////////////////////////////////


    [Server]
    public void Fire(uint owner, Vector3 startPos, Vector3 dir)
    {
        if ((playerNumber % 2) == 0)
        {

            //   if (fireBallExpandMode && _lastFireballGo != null)
            //  {
            //      _lastFireballGo.GetComponent<Fireball>().Expand();
            //      fireBallExpandMode = false;
            //  }
            //  else
            //  {

            GameObject fireballGo = Instantiate(fireballPrefab, startPos, Quaternion.identity); //Создаем локальный объект пули на сервере
            NetworkServer.Spawn(fireballGo); //отправляем информацию о сетевом объекте всем игрокам.
            Vector3 fbMovement = dir.normalized * 3.0f + movement;
            fireballGo.GetComponent<Fireball>().Init(owner, startPos, fbMovement, 0); //инициализируем поведение пули
                                                                                      //    _lastFireballGo = fireballGo;
                                                                                      //    fireBallExpandMode = true;

            //  }
        }
        else
        {
            Vector3 fbMovement = dir.normalized * 6.0f + movement;
            GameObject fireballGo = Instantiate(fireballPrefab, startPos, Quaternion.identity); //Создаем локальный объект пули на сервере
            NetworkServer.Spawn(fireballGo); //отправляем информацию о сетевом объекте всем игрокам.
            fireballGo.GetComponent<Fireball>().Init(owner, startPos, fbMovement, 1); //инициализируем поведение пули            
        }

    }


    [Command]
    public void CmdFire(uint owner, Vector3 startPos, Vector3 dir)
    {
        Fire(owner, startPos, dir);
    }

    //////////////////////////////////////////////////////////////////////////////////


    [Server]
    public void MapGenerator(uint owner)
    {
        Vector3 startPos = new Vector3(Random.Range(1, 256), Random.Range(1, 256), Random.Range(1, 256));
        GameObject mapGeneratorGO = Instantiate(mapGenPrefab, startPos, Quaternion.identity);
        NetworkServer.Spawn(mapGeneratorGO);
    }


    [Command]
    public void CmdMapGenerator(uint owner)
    {
        MapGenerator(owner);
    }



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



    [Server]
    public void HangCable(uint owner, Vector3 startPos)
    {
        GameObject cableGo = Instantiate(cablePrefab, startPos, Quaternion.identity);
        NetworkServer.Spawn(cableGo);
    }

    [Command]
    public void CmdHangCable(uint owner, Vector3 startPos)
    {
        HangCable(owner, startPos);
    }

    //////////////////////////////////////////////////////////////////////////////////


  
    IEnumerator MakeCable(Vector3 startPos, bool isServer, uint netId)
    { 
        Vector3 pos = startPos;  
        while(
            _charController.isGrounded
            && LevelController.control.isType(pos, LevelController.CubeType.VOID)       
            && !LevelController.control.HasCable(pos))
        {   
            LevelController.control.hasCable[(int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y), (int)Mathf.Round(pos.z)] = true;
            if(isServer)
            {
                HangCable(netId, pos);
            }
            else 
            {
                CmdHangCable(netId, pos);
            }            
            pos += new Vector3(0,-1,0);  
            yield return new WaitForSeconds(0.5f);
        } 
        yield return null; 
    }


}

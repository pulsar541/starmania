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
    [SyncVar(hook = nameof(SyncHealth))] //задаем метод, который будет выполняться при синхронизации переменной
    int _SyncHealth;


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

    GameObject _headCameraGO = null;

    [SerializeField] private GameObject _mapMarkerGO = null;

    // private GameObject _mapCameraGO = null;   

    LightManager lightManager;

    LevelController levelController;

    //   private ControllerColliderHit _contact;

    GameObject audioListener = null;


    private Transform _body = null;

    private GameObject _fpsCameraGO;

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


    public int score = 1000000;

    Vector3 movement = new Vector3();

    public bool inCable = false;

    private int _viewMode = 0;

    private Vector3 _spawnPosition;

    public bool isWin = false;
    

    Quaternion  quatAng;
    Quaternion  quatAngHead;


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

    GameObject goLocalLight = null;
    
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
    public int TeamNumber()
    {
        return (playerNumber % 2) == 0 ? 0 : 1;
    }
    /////////////////////////////////////////



    void Awake()
    {
        levelController = (LevelController)GameObject.Find("LevelController").GetComponent<LevelController>();
        lightManager = (LightManager)GameObject.Find("LightManager").GetComponent<LightManager>();
        _charController = GetComponent<CharacterController>(); 
        _mapMarkerGO = GameObject.Find("MapMarker");
        _headCameraGO = GameObject.Find("Camera");
      

        foreach (Transform child in transform)
        {
            if (child.name.IndexOf("Body") >= 0)
            {
                _body = child;
                break;
            }
        }

        audioListener = GameObject.Find("AudioListener");

        LevelController.control.Notify += InitPosition;
   
    }


    void InitPosition(Vector3[] teamPositions)
    {
        _spawnPosition = teamPositions[TeamNumber()];
        _charController.transform.position = teamPositions[TeamNumber()];
    }

    void Start()
    {
        //  _rigidBody = GetComponent<Rigidbody>(); 
        _vertSpeed = 20;
        Health = 100;
        isWin = false;

        if (isServer)
            InitMapGenerator(netId);
        else
            CmdInitMapGenerator(netId);

        if (IsLocal)
        {
            levelController.BindPlayerGameObject(gameObject);
            goDirLight = GameObject.Find("Directional Light");
            goLocalLight = GameObject.Find("LocalPlayerLight");

            if(goDirLight)
                goDirLight.SetActive(false);

            if(_headCameraGO && _head) 
            {
                _headCameraGO.transform.parent = _head.transform;
                _headCameraGO.transform.localPosition = Vector3.zero;
                _headCameraGO.SetActive(true);
            }

            if(_mapMarkerGO)
                _mapMarkerGO.SetActive(false);


            
           
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
        playerColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        // Start generating updates
        //InvokeRepeating(nameof(UpdateData), 1, 1);

        ReSpawn();



        foreach (Transform child in transform)
        {
            Renderer ren = child.GetComponent<Renderer>();
            if (ren != null)
            {
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
    public void UpdateData()
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

        ReSpawn();

        // RenderSettings.ambientLight = playerColor;


        foreach (Transform child in transform)
        {
            Renderer ren = child.GetComponent<Renderer>();
            if (ren != null)
            {
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


    public void ReSpawn()
    {
        Health = 100;
        _charController.transform.position = _spawnPosition;
        transform.position = _spawnPosition;
        _vertSpeed = 0;
        _body.transform.localEulerAngles = new Vector3(0, 0, 0);

    }

    public float GetSpeed()
    {
        return speed;
    }
    public float GetShuftMultiplier()
    {
        return shiftMulSpeeed;
    }


    void FixedUpdate()
    {
        if (hasAuthority)
        {
            score = (int) Vector3.Distance( transform.position, LevelController.control.exitPosition );
                
           // if(score == 1)
           //     isWin = true;

            CmdScoreUp(score);
            UpdateData();
        }

    }
    // Update is called once per frame
    void Update()
    {
      //  if (!isLocalPlayer)
         //   return;

        //  if (SceneController.pause)
        //       return;


        if (hasAuthority)
        {


            // if (Health <= 0) 
            // {   Health = 100;
            //     StartCoroutine(WhitebeforeRespawn(0.5f));
            //     //ReSpawn();
            // }


            if (!SceneController.pause)
            {
                _rotationX -= Input.GetAxis("Mouse Y") * sensivityVert;
                _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);

                // float delta =;

                _rotationY += Input.GetAxis("Mouse X") * sensivityHor;

               // Vector3 localEuAng = new Vector3(0, _rotationY, 0);
                Quaternion quat =  Quaternion.Euler(0, _rotationY, 0); 
                quatAng = Quaternion.Lerp(quatAng, quat, 15.0f*Time.deltaTime); 
                transform.localEulerAngles = new Vector3(0, quatAng.eulerAngles.y, 0);
  
                //transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0);


                Quaternion quat2 =  Quaternion.Euler(_rotationX, 0 , 0); 
                quatAngHead = Quaternion.Lerp(quatAngHead, quat2, 15.0f*Time.deltaTime); 
                _head.transform.localEulerAngles = new Vector3(quatAngHead.eulerAngles.x, 0, 0);


            }

      


            //lightManager.SetNonDestroy(transform.position);

            //Color lightColor = LightManager.GetLampColorByPosition(transform.position);


            lightManager.ActivateLight(transform.position, 10);
            goLocalLight.transform.position = transform.position;


            // goLocalLight.GetComponent<Light>().color = new Color( 
            //             0.5f + Mathf.Sin( 0.01f * (transform.position.x * transform.position.y) ), 
            //             0.5f + Mathf.Sin( 0.01f * (transform.position.y * transform.position.z) ),  
            //             0.5f + Mathf.Sin( 0.01f * (transform.position.z * transform.position.x) )
            //         );



 
            Vector3 newEnemySpawnPos = levelController.TryActivateEnemy(transform.position, 7);
            if (newEnemySpawnPos.x > 0)
            {
                if (isServer)
                    EnemySpawn(netId, newEnemySpawnPos);
                else
                    CmdEnemySpawn(netId, newEnemySpawnPos);

                levelController.enemyTrigger[(int)newEnemySpawnPos.x, (int)newEnemySpawnPos.y, (int)newEnemySpawnPos.z] = false;
            }



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
            else if (inCable)
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
    
            if (_vertSpeed > 0 )
                foreach (var item in Physics.OverlapSphere(_head.transform.position + new Vector3(0,0.1f,0), 0.1f))
                { 
                    Cube cube = item.GetComponent<Cube>();
                    if (cube) 
                    _vertSpeed = 0; 
                }


            if (gravity != 0)
                movement.y = _vertSpeed;

            movement *= Time.deltaTime;



            _isMoveEnable = levelController.Builded;

            if (levelController.Builded)
            {
                _charController.Move(movement);
            }


            if (transform.position.y < 0)
            {
                Health = 0;
            }

            if (_charController.isGrounded && _vertSpeed < deathVertSpeed)
            {
                Health = 0;
            }
 


            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _viewMode = 1 - _viewMode;
 
                switch (_viewMode)
                {
                    case 0:
                        _headCameraGO.SetActive(true);
                        _mapMarkerGO.SetActive(false);
                        break;
                    case 1:

                        _headCameraGO.SetActive(false);
                        _mapMarkerGO.SetActive(true);
                        break;
                }
 
                goDirLight.SetActive(_viewMode == 1); 
            }

            Vector3 discretePos = new Vector3(
                Mathf.Round(transform.position.x),
                Mathf.Round(transform.position.y),
                Mathf.Round(transform.position.z)
            );

            if(_mapMarkerGO)
            {
                _mapMarkerGO.transform.position = discretePos * 0.01f + new Vector3(0, 1000, 0); 
                _mapMarkerGO.transform.localEulerAngles = new Vector3(
                    _head.transform.localEulerAngles.x,
                    _charController.transform.localEulerAngles.y,
                    0
                );
            }

            if (_body)
            {
                if (_charController.isGrounded && (Input.GetButton("Horizontal") || Input.GetButton("Vertical")))
                {
                    _walkCounter += speed * (shiftMulSpeeed > 1 ? 1.5f : 1.0f) * Time.deltaTime * 5.0f;
                    _body.transform.localPosition = new Vector3(0, 0, 0);
                    _body.transform.localPosition += new Vector3(0, (Mathf.Sin(_walkCounter*2.0f)) * 0.02f, 0);
                    _body.transform.localPosition += new Vector3(-Mathf.Cos(_walkCounter) * 0.015f, 0, 0);
                    _body.transform.localEulerAngles = new Vector3((Mathf.Sin(_walkCounter * 2)) * 0.15f, 0, (Mathf.Sin(_walkCounter)) * 0.3f);
                }

            }

            if (audioListener)
            {
                audioListener.transform.position = _head.transform.position;
                audioListener.transform.rotation = _head.transform.rotation;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && Health > 0)
            {

                Vector3 spawnPos = transform.position;
                Vector3 weaponMovement = new Vector3(0, 0, 1);
                weaponMovement = _head.transform.TransformDirection(weaponMovement);
                spawnPos += _charController.transform.TransformDirection(new Vector3(0, 0.01f, 0));

                soundSourceFire.PlayOneShot(fireSound);

                if (isServer)
                    Fire(netId, spawnPos, weaponMovement);
                else
                    CmdFire(netId, spawnPos, weaponMovement);


            }



            if (Input.GetKeyDown(KeyCode.Mouse1) && Health > 0 &&  _charController.isGrounded)
            {
              //  if ((playerNumber % 2) == 0)
               // {
                    Vector3 pos = new Vector3(px, py, pz);
                    Vector3 cableBeginPos = transform.position + _charController.transform.TransformDirection(new Vector3(0, 0, 0.85f));
                    Vector3 cableCheckVoidPos = cableBeginPos + new Vector3(0, -0.5f, 0);

                    if (!LevelController.control.HasCable(cableBeginPos) && LevelController.control.isType(cableCheckVoidPos, LevelController.CubeType.VOID))
                        StartCoroutine(MakeCable(cableBeginPos, isServer, netId));

               // }
 
            }


            ///////////////////////////////////////////////////// 
            foreach (GameObject checkpointGo in levelController.checkPointsGO)
            {
                
                if (dist(checkpointGo.transform.position, transform.position) < 0.5f /*&& !pickupCheckpoints.Contains(checkpointGo.transform.position)*/)
                {
                     foreach (GameObject checkpointGo2 in levelController.checkPointsGO) 
                     {  if(checkpointGo2 != checkpointGo)
                          checkpointGo.GetComponent<Checkpoint>().UnsetPickup();
                     }

                    //pickupCheckpoints.Add(checkpointGo.transform.position);
                    checkpointGo.GetComponent<Checkpoint>().SetPickup();
                    _spawnPosition = transform.position; 
                }
            }

            //////////////////////////////////////



           
            


            if (IsLocal)
            {
                _head.GetComponent<Renderer>().enabled = Health > 0;
                foreach (Renderer r in this.gameObject.GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false; // !inCable;
                    if (Health <= 0)
                        r.enabled = true;
                }

            }



 
        if (hasAuthority)
        {
            /////////////////////
            if (Health <= 0)
            {
              //  _vertSpeed = 0;
                _body.transform.localEulerAngles = new Vector3(0, _rotationY, -45);

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
            {  
                Health = 100;
                StartCoroutine(WhitebeforeRespawn(0.75f)); 
            }
            //////////////////////////////////////

        }

            Vector3 point0 = transform.position + new Vector3(0, -0.2f, 0);
            Vector3 point1 = transform.position + new Vector3(0, 0.2f, 0);
            inCable = false;
            foreach (var item in Physics.OverlapCapsule(point0, point1, 0.25f))
            {
                Cable cable = item.GetComponent<Cable>();
                if (cable)
                {
                    inCable = true;
                }
            }




        } // if hasAuthority



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
        // if ((playerNumber % 2) == 0)
        // {

            //   if (fireBallExpandMode && _lastFireballGo != null)
            //  {
            //      _lastFireballGo.GetComponent<Fireball>().Expand();
            //      fireBallExpandMode = false;
            //  }
            //  else
            //  {

            GameObject fireballGo = Instantiate(fireballPrefab, startPos, Quaternion.identity); //Создаем локальный объект пули на сервере
            NetworkServer.Spawn(fireballGo); //отправляем информацию о сетевом объекте всем игрокам.
            Vector3 fbMovement = dir.normalized * 6.0f + movement;
            fireballGo.GetComponent<Fireball>().Init(owner, startPos, fbMovement, 0); //инициализируем поведение пули
                                                                                      //    _lastFireballGo = fireballGo;
                                                                                      //    fireBallExpandMode = true;

            //  }
        // }
        // else
        // {
        //     Vector3 fbMovement = dir.normalized * 6.0f + movement;
        //     GameObject fireballGo = Instantiate(fireballPrefab, startPos, Quaternion.identity); //Создаем локальный объект пули на сервере
        //     NetworkServer.Spawn(fireballGo); //отправляем информацию о сетевом объекте всем игрокам.
        //     fireballGo.GetComponent<Fireball>().Init(owner, startPos, fbMovement, 1); //инициализируем поведение пули            
        // }

    }


    [Command]
    public void CmdFire(uint owner, Vector3 startPos, Vector3 dir)
    {
        Fire(owner, startPos, dir);
    }

    //////////////////////////////////////////////////////////////////////////////////


    [Server]
    public void InitMapGenerator(uint owner)
    {
        Vector3 startPos = new Vector3(Random.Range(1, 256), Random.Range(1, 256), Random.Range(1, 256));
        GameObject mapGeneratorGO = Instantiate(mapGenPrefab, startPos, Quaternion.identity);
        NetworkServer.Spawn(mapGeneratorGO);
    }


    [Command]
    public void CmdInitMapGenerator(uint owner)
    {
        InitMapGenerator(owner);
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


    [Server] //обозначаем, что этот метод будет вызываться и выполняться только на сервере
    public void ChangeHealthValue(int newValue)
    {
        _SyncHealth = newValue;
    }

    void SyncHealth(int oldValue, int newValue) //обязательно делаем два значения - старое и новое. 
    {
        Health = newValue;
    }

    [Command] //обозначаем, что этот метод должен будет выполняться на сервере по запросу клиента
    public void CmdChangeHealth(int newValue) //обязательно ставим Cmd в начале названия метода
    {
        ChangeHealthValue(newValue); //переходим к непосредственному изменению переменной
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////

    IEnumerator MakeCable(Vector3 startPos, bool isServer, uint netId)
    {
        Vector3 pos = startPos;
        while (  LevelController.control.isType(pos, LevelController.CubeType.VOID)
            && !LevelController.control.HasCable(pos))
        {
            LevelController.control.hasCable[(int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y), (int)Mathf.Round(pos.z)] = true;
            if (isServer)
            {
                HangCable(netId, pos);
            }
            else
            {
                CmdHangCable(netId, pos);
            }
            pos += new Vector3(0, -1, 0);
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }


    IEnumerator WhitebeforeRespawn(float witeTime)
    { 
       
        yield return new WaitForSeconds(witeTime);
        ReSpawn();
        yield return null;
        
    }




}

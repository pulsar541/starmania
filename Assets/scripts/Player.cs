using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[RequireComponent(typeof(CharacterController))]
//[AddComponentMenu("Control Script/Player")]

public class Player : NetworkBehaviour
{

    SyncList<Vector3> _SyncVector3Vars = new SyncList<Vector3>();

    public List<Vector3> Vector3Vars;


    private CharacterController _charController;

    [SerializeField] private GameObject fireballPrefab;

    LevelController levelController;

    private ControllerColliderHit _contact;

    private GameObject _fireball;

    // private Rigidbody _rigidBody;

    private float gravity = -9.81f;

    private float _distanceMove = 0.0f;
    public float speed = 1.0f;

    private float _vertSpeed;
    public const float baseSpeed = 6.0f;

    // public GameObject cameraFPS;
    public float jumpSpeed = 15.0f;
    public float terminalVelocity = -10.0f;
    public float minFall = -1.5f;


    //public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensivityHor = 9.0f;
    public float sensivityVert = 9.0f;
    public float minVert = -70.0f;
    public float maxVert = 70.0f;

    private float _rotationX = 0;
    private float _rotationY = 0;

    private bool _isMoveEnable = false;

    void Awake()
    {
        //  Messenger<float>.AddListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);


        levelController = (LevelController)GameObject.Find("LevelController").GetComponent<LevelController>();
    }
    void OnDestroy()
    {
        //   Messenger<float>.RemoveListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
    }

    // Start is called before the first frame update
    void Start()
    {
        _charController = GetComponent<CharacterController>();

        //  _rigidBody = GetComponent<Rigidbody>();

        _vertSpeed = 0;

        //cameraFPS = GameObject.Find("Main Camera");


        if (isServer)
        {
            levelController.GenerateLevel();

            List<Vector3> lst = levelController.GetCubesIJKs();
            Vector3Vars = new List<Vector3>();
            foreach (Vector3 item in lst)
            {
                Vector3Vars.Add(item);
                ChangeVector3Vars(item);
            }

            levelController.Build();
        }

    }

    float dist(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    // Update is called once per frame
    void Update()
    {

        /*
        //float speed = (Input.GetKey(KeyCode.LeftShift) ? 2.0f:1.0f) * baseSpeed ; 
    	float deltaX = Input.GetAxis("Horizontal") * speed;
    	float deltaZ = Input.GetAxis("Vertical") * speed;


    	Vector3 movement = new Vector3(deltaX, 0, deltaZ);
    	movement = Vector3.ClampMagnitude(movement, speed);
    	

        _distanceMove +=  Mathf.Sqrt(deltaX*deltaX + deltaZ*deltaZ) ;
     //   movement.y = gravity;
 
 
        cameraFPS.transform.localPosition = new Vector3(
            Mathf.Sin(_distanceMove* 0.01f)*0.025f ,  
            1.0f + Mathf.Cos(_distanceMove* 0.015f)*0.025f  ,  
            0  );

    	movement *= Time.deltaTime;
    	movement = transform.TransformDirection(movement);
    	_charController.Move(movement);

*/

        if (hasAuthority)
        {

            _rotationX -= Input.GetAxis("Mouse Y") * sensivityVert;
            _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);
            float delta = Input.GetAxis("Mouse X") * sensivityHor;
            _rotationY = transform.localEulerAngles.y + delta;
            transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0);

            float shiftMulSpeeed = 1;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                shiftMulSpeeed = 1.75f;
            }

            float deltaX = Input.GetAxis("Horizontal") * speed * shiftMulSpeeed;
            float deltaZ = Input.GetAxis("Vertical") * speed * shiftMulSpeeed;
            Vector3 movement = new Vector3(deltaX, 0, deltaZ);

            movement = Vector3.ClampMagnitude(movement, speed * shiftMulSpeeed);
            movement = transform.TransformDirection(movement);

            if (_charController.isGrounded)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    _vertSpeed = jumpSpeed;
                }
                else
                {
                    _vertSpeed = minFall;
                }
            }

            else
            {
                _vertSpeed += gravity * Time.deltaTime;

                if (_vertSpeed < terminalVelocity)
                {
                    _vertSpeed = terminalVelocity;
                }
            }


            bool hitCeiling = false;
            RaycastHit hit;
            if (_vertSpeed > 0 && Physics.Raycast(transform.position, Vector3.up, out hit))
            {
                float check = (_charController.height + _charController.radius) / 1.9f;
                hitCeiling = hit.distance <= check;
            }
            if (hitCeiling)
                _vertSpeed = 0;



            movement.y = _vertSpeed;
            movement *= Time.deltaTime;

            if (_isMoveEnable)
                _charController.Move(movement);


            if (Input.GetMouseButtonDown(0))
            {

                if (_isMoveEnable)
                {
                    _fireball = (GameObject)Instantiate(fireballPrefab);
                    _fireball.transform.position = transform.position;
                    float weaponSpeed = 5.0f;
                    Vector3 weaponMovement = new Vector3(0, 0, 1);
                    weaponMovement = transform.TransformDirection(weaponMovement) * weaponSpeed;
                    _fireball.transform.position += transform.TransformDirection(new Vector3(0.15f, -0.15f, 0));
                    _fireball.GetComponent<WeaponLogic>().SetMovement(weaponMovement);

                }
                else
                {
                    _isMoveEnable = true;
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


    public override void OnStartClient()
    {
        base.OnStartClient();

        _SyncVector3Vars.Callback += SyncVector3Vars; //вместо hook, для SyncList используем подписку на Callback

        Vector3Vars = new List<Vector3>(_SyncVector3Vars.Count); //так как Callback действует только на изменение массива,  
        for (int i = 0; i < _SyncVector3Vars.Count; i++) //а у нас на момент подключения уже могут быть какие-то данные в массиве, нам нужно эти данные внести в локальный массив
        {
            Vector3Vars.Add(_SyncVector3Vars[i]);
        }

        if (Vector3Vars.Count > 0)
        {
            levelController.ImportLevel(Vector3Vars);
            levelController.Build();

            Debug.Log("_SyncVector3Vars " + _SyncVector3Vars.Count);
            Debug.Log("Vector3Vars " + Vector3Vars.Count);
        }

    }

    [Server]
    void ChangeVector3Vars(Vector3 newValue)
    {
        _SyncVector3Vars.Add(newValue);
    }
    [Command]
    public void CmdChangeVector3Vars(Vector3 newValue)
    {
        ChangeVector3Vars(newValue);
    }



    void SyncVector3Vars(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem)
    {
        switch (op)
        {
            case SyncList<Vector3>.Operation.OP_ADD:
                {
                    Vector3Vars.Add(newItem);
                    break;
                }
            case SyncList<Vector3>.Operation.OP_CLEAR:
                {

                    break;
                }
            case SyncList<Vector3>.Operation.OP_INSERT:
                {

                    break;
                }
            case SyncList<Vector3>.Operation.OP_REMOVEAT:
                {

                    break;
                }
            case SyncList<Vector3>.Operation.OP_SET:
                {
                    //  Vector3Vars = new List<Vector3>(levelController.GetCubesIJKs());
                    break;
                }
        }
    }
}

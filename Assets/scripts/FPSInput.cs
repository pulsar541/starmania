﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]

public class FPSInput : MonoBehaviour
{
    private CharacterController _charController;


   // private Rigidbody _rigidBody;

    public float gravity = 0;
    private float _distanceMove = 0.0f;
    public float speed = 6.0f;

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

    private bool _inputEnable = true;

    void Awake()
    {
        //  Messenger<float>.AddListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
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

        _vertSpeed = minFall;

        //cameraFPS = GameObject.Find("Main Camera");
    }

    float dist(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    public void SetInputEnable(bool value)
    {
        _inputEnable = value;
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
                if (_inputEnable)
                {
                    _rotationX -= Input.GetAxis("Mouse Y") * sensivityVert;
                    _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);
                    float delta = Input.GetAxis("Mouse X") * sensivityHor;
                    _rotationY = transform.localEulerAngles.y + delta;
                    transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0);


                    float deltaX = Input.GetAxis("Horizontal") * speed;
                    float deltaZ = Input.GetAxis("Vertical") * speed;
                    Vector3 movement = new Vector3(deltaX, 0, deltaZ);

                    movement = Vector3.ClampMagnitude(movement, speed); 
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
                        _vertSpeed += gravity * 5 * Time.deltaTime;
                        if (_vertSpeed < terminalVelocity)
                        {
                            _vertSpeed = terminalVelocity;
                        }
                    }


                    movement.y = _vertSpeed;
                    movement *= Time.deltaTime; 
                    _charController.Move(movement); 
                    
                     }

         


/*

        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;

        if (Input.GetButtonDown("Jump"))
        {
            
            _rigidBody.AddForce(new Vector3(0,10,0), ForceMode.VelocityChange);
        }
 */
  
}

private void OnSpeedChanged(float value)
{
    speed = baseSpeed * value;
}

 
}
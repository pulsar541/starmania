using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMap : MonoBehaviour
{
    // // Update is called once per frame
    // public void UpdateByPlayer(Transform playerTransform)
    // {

    //    transform.position = playerTransform.position + new Vector3(0,1000,0);
    //    //transform.position += playerTransform.TransformDirection(new Vector3(0,0,-0.5f));
    //    transform.localPosition = new Vector3(0,0,-0.5f);
    //    transform.localEulerAngles = playerTransform.eulerAngles;
    // }
    private float zoomSpeed = 0.25f;
 
    private float zMin = -0.35f;
    private float zMax = -0.05f;
    private Camera myCamera;

    private float _sourceRadius = 0;

    private float farZ = 0;

    void Awake()
    { 
       myCamera = GetComponent<Camera>();
    } 
    void Update()
    {
        // if (myCamera.orthographic)
        // {
        //     if (Input.GetAxis("Mouse ScrollWheel") < 0)
        //     {
        //         myCamera.orthographicSize += zoomSpeed;
        //     }
        //     if (Input.GetAxis("Mouse ScrollWheel") > 0)
        //     {
        //         myCamera.orthographicSize -= zoomSpeed;
        //     }
        //     myCamera.orthographicSize = Mathf.Clamp(myCamera.orthographicSize, orthographicSizeMin, orthographicSizeMax);
        // }
        // else
        // {
        //     if (Input.GetAxis("Mouse ScrollWheel") < 0)
        //     {
        //         myCamera.fieldOfView += zoomSpeed;
        //     }
        //     if (Input.GetAxis("Mouse ScrollWheel") > 0)
        //     {
        //         myCamera.fieldOfView -= zoomSpeed;
        //     }
        //     myCamera.fieldOfView = Mathf.Clamp(myCamera.fieldOfView, fovMin, fovMax);
        // }
            float locDist = Vector3.Magnitude( myCamera.transform.localPosition );

            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                farZ -= zoomSpeed * locDist;
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                farZ += zoomSpeed * locDist;
            }

           
            farZ = Mathf.Clamp(farZ, zMin, zMax);

            myCamera.transform.localPosition = new Vector3(0,0,farZ);
    }
}

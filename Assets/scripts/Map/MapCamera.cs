using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public float sensivityHor = 5.0f;
    public float sensivityVert = 5.0f;
    public float minVert = -90.0f;
    public float maxVert = 90.0f;

    private float _rotationX = 0;
    private float _rotationY = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * sensivityVert;
        _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);
        float delta = Input.GetAxis("Mouse X") * sensivityHor;

        _rotationY = transform.localEulerAngles.y + delta;
        transform.localEulerAngles = new Vector3(0, _rotationY, 0); 
    
    }
}

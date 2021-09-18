using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerLight : MonoBehaviour
{
    // Start is called before the first frame update
    Light light;
    void Start()
    {
        
    }

    void Awake()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        light.intensity = Mathf.Clamp(transform.position.y / 50.0f, 0.0f, 1.0f); 
    }
}

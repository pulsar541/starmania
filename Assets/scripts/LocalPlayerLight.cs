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
        light.intensity = transform.position.y / 100.0f; 
    }
}

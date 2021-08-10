using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerBody : MonoBehaviour
{
    private float _sourceRadius = 0;
    private float _msek = 0;

    // Start is called before the first frame update
    void Start()
    {
       _sourceRadius = transform.localScale.x; 
    }

    // Update is called once per frame
    void Update()
    {
        _msek += Time.deltaTime;
        if(_msek > 180.0f)
        {
            _msek = 0; 
        } 
        float newRad =  _sourceRadius + Mathf.Sin(_msek * 10.0f) * _sourceRadius* 0.15f;
        transform.localScale = new Vector3(newRad, newRad, newRad);
    }
}

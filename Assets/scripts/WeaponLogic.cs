using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLogic : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 _movement;

    public void SetMovement(Vector3 movement) {
        _movement = movement;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        transform.position += _movement * Time.deltaTime;
    }
}

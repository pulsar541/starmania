using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{

    Player _player;
    // Start is called before the first frame update

    float _msek = 0;
    void Awake()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
        // if(_player == null) {
        //     if(_msek > 0.5f) {
        //         _player = (Player)GameObject.FindObjectOfType(typeof(Player));
        //         _msek = 0; 
        //     }
        //     _msek += Time.deltaTime;
        // } 
        // else if(_player.IsLocal) {
        //     transform.position = _player.transform.position;
        //     transform.rotation = _player.transform.rotation;
        // }

       
    }
}

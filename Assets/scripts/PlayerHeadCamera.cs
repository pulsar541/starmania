using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadCamera : MonoBehaviour
{ 
    [SerializeField] private GameObject playerGO = null;
    Player player = null;
    public class ViewModes
    {
        public const int FP = 0;
        public const int MAP = 1;
    }
    
    private int _mode = 0;
    private bool _playerIsWalking = false;
    private float _walkCounter = 0;
    public void SetIsPlayerWalking(bool isWalking)
    {
        _playerIsWalking = isWalking;
    }

    void Awake() 
    {
        player = playerGO.GetComponent<Player>();
    }

    public void ToggleViewMode()
    {
        _mode = 1 - _mode;     
         


    }

    public void UpdateByPlayer(Transform playerTransform)
    {    

      
        //if (Health <= 0)
        //    _fpsCamera.transform.position +=_fpsCamera.transform.TransformDirection(new Vector3(0, 0, -0.5f));

        if (_playerIsWalking)
        {
            _walkCounter +=  player.GetSpeed() * (player.GetShuftMultiplier() > 1 ? 1.5f : 1.0f) * Time.deltaTime * 5.0f;

        } 

        if(_mode == 0) {
            transform.position =  playerTransform.position + new Vector3(0, Mathf.Abs(Mathf.Sin(_walkCounter)) * 0.04f, 0);
            transform.position += playerTransform.TransformDirection(new Vector3(-Mathf.Cos(_walkCounter) * 0.02f, 0, 0));
            transform.localPosition = Vector3.zero;
        } 
        else
        { 
            transform.position =  playerTransform.position;
            transform.localPosition =  new Vector3(0, 0, -3.0f);  
        }

        // //_fpsCamera.transform.rotation = _head.transform.rotation;
        // transform.localEulerAngles = Vector3.zero;
         transform.localEulerAngles = new Vector3((Mathf.Sin(_walkCounter * 2)) * 0.1f, 0, (-Mathf.Sin(_walkCounter)) * 0.2f);


        
    }
}

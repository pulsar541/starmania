using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public static bool pause = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneController.pause = true;   
    }

    public static void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneController.pause = false;   
    }


    // Update is called once per frame
    void Update()
    {
  
 
    }
}

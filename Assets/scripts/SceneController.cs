using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static bool pause = false;


     

    public static SceneController Instance
    {
        get
        {
            return _instance;
        }
    }

    private static SceneController _instance = null;


    GameObject goDirLight;

     

   // public static int viewMode = ViewModes.ViewModeFPS; 

    void Awake()
    {      
        if(_instance)
        {
            DestroyImmediate(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(transform.gameObject);
         

        goDirLight = GameObject.Find("Directional Light");  
    }

    // Start is called before the first frame update
    void Start()
    {
        Settings.Init();
         
        //#if SHOW_DEBUG_MESSAGES
        //Debug.Log ("SceneCOntroller started");
        //#endif
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
        // if(Input.GetKeyDown(KeyCode.Tab)) 
        // {

        //     viewMode = 1 - viewMode;  
        //     goDirLight.SetActive(viewMode == 1);
        //     //StartCoroutine(LoadAsyncScene("MapScene",false));
        // }
    }
 
    // IEnumerator LoadAsyncScene(string name, bool waitBefore = false)
    // {
    //     if (waitBefore)
    //         yield return new WaitForSeconds(3.0f);

    //     AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/" + name);
    //     while (!asyncLoad.isDone)
    //     {
    //         yield return null;
    //     }
    // }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static bool pause = false;

    public static SceneController control;

    void Awake()
    {
        control = this;
        DontDestroyOnLoad(transform.gameObject);
    }

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
        if(Input.GetKeyDown(KeyCode.Tab)) 
        {
            StartCoroutine(LoadAsyncScene("MapScene",true));
        }
    }
 
    IEnumerator LoadAsyncScene(string name, bool waitBefore = false)
    {
        if (waitBefore)
            yield return new WaitForSeconds(3.0f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/" + name);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

}

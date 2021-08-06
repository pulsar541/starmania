using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MapSceneController : MonoBehaviour
{

    private GameObject[,,] _cubeGO = new GameObject[LevelController.CUBES_I, LevelController.CUBES_J, LevelController.CUBES_K]; 

    public GameObject cubeSinglePrefab; 
    private LevelController  levelController;
    private SceneController  sceneController;


    void Awake()
    {
        levelController = LevelController.control;
        sceneController = SceneController.control;
    } 
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BuildMiniMap());
    }

    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape)) 
        {
            StartCoroutine(LoadAsyncScene("LevelScene",true));
        }     
    }

    IEnumerator BuildMiniMap()
    {
        Debug.Log("BuildMiniMap");



        //for(int delta = 0; delta < 50; delta++) 
        {
        for (int i = 0; i < LevelController.CUBES_I; i++)
        {
            for (int j = 0; j < LevelController.CUBES_J; j++)
            {
                for (int k = 0; k < LevelController.CUBES_K; k++)
                { 
                    if (levelController.cubes[i, j, k] == (int)LevelController.CubeType.VOID)
                    { 
                      _cubeGO[i, j, k] = (GameObject)Instantiate(cubeSinglePrefab);  
                    } 
                    
                    if (_cubeGO[i, j, k])
                    {
                        _cubeGO[i, j, k].transform.position = new Vector3(i, j, k); 
                    } 
 
                }

            }
            yield return null;
            // yield break;
        }
        
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

using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuSceneController : MonoBehaviour
{
    // Start is called before the first frame update

    private Scene scene;
    float msek = 0;
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (msek >= 3)
        {
            var parameters = new LoadSceneParameters(LoadSceneMode.Single); 
            scene = SceneManager.LoadScene("Scenes/LevelScene", parameters);
        }
        else
            msek += Time.deltaTime;

    }
}

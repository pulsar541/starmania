using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FireballEx : NetworkBehaviour
{
    // Start is called before the first frame update

    LightManager lightManager;
    void Awake()
    {
        lightManager  = (LightManager)GameObject.Find("LightManager").GetComponent<LightManager>();
    }
    void Start()
    { 
        //if(transform.position.y <  LevelController.CUBES_J / 2 - 1) 
        {
          //  lightManager.InsertLight(transform.position, Color.white, 1);
            //Material mat = GetComponent<Renderer>().material;
            //mat.SetColor("_EmissionColor", Color.white);
        }

    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale -= new Vector3(0.05f * Time.deltaTime, 0.005f * Time.deltaTime, 0.05f * Time.deltaTime);
 
        if(transform.localScale.x < 0.01f)
        {
            if(transform.position.y <  LevelController.CUBES_J / 2 - 1) 
            {
              //  lightManager.DestroyLight(transform.position);
               // lightManager.InsertLight(transform.position, Color.white, 5);
            }

            NetworkServer.Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMarker : MonoBehaviour
{
    
    GameObject cameraGO = null;
   // Light lightGO = null;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.name.IndexOf("CameraMap") >= 0)
            {
                cameraGO = child.gameObject;
                break;
            }

          //  if (child.name.IndexOf("Light") >= 0)
          //  {
          //      lightGO = child.GetComponent<Light>();
           //     break;
           // }
            
        }        

        Active(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Active(bool active)
    {
        cameraGO.SetActive(active);
        //lightGO.enabled = active;
    }
}

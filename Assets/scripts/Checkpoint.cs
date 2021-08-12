using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int isPickup = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       // transform.Rotate(new Vector3(0,Time.deltaTime,0));
    }

    public void SetPickup()
    {
        isPickup = 1;
        int i = 0;
        foreach(Transform child in transform) {
          bool active = i == isPickup;
          child.gameObject.SetActive(active);
          i ++;
        }
    }

    public void UnsetPickup()
    {
        isPickup = 0;
        int i = 0;
        foreach(Transform child in transform) {
          bool active = i == isPickup;
          child.gameObject.SetActive(active);
          i ++;
        }
    }
    
}

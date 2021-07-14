using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleGameObject : MonoBehaviour
{
    Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = false;
    }
 
}

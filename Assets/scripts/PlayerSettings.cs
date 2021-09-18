using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSettings 
{
    public string playerName;
    public float mouseSensivity;

    public static PlayerSettings CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<PlayerSettings>(jsonString);
    }

    // Given JSON input:
    // {"playerName":"Dr Charles","mouseSensivity":1.8}
    // this example will return a PlayerInfo object with
    //  playerName == "Dr Charles" and mouseSensivity == 1.8f.
}

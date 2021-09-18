using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

 
public class Settings : MonoBehaviour
{
    public static string playerName = "no name";
    public static float mouseSensivity = 1.0f;
  

    public static void Init()
    {
        
        string settingsPath = Application.persistentDataPath + "/settings.json";
        if(File.Exists(settingsPath))
        { 
            string settingsData = File.ReadAllText(settingsPath); 
            PlayerSettings ps = PlayerSettings.CreateFromJSON(settingsData); 
            if(ps != null) 
            {
                playerName = ps.playerName;
                mouseSensivity = ps.mouseSensivity;
            }
            
        }
        else 
        {
            string defaultSettingsData = "{\"playerName\":\"unknown\",\"mouseSensivity\":4.0}";
            File.WriteAllText(settingsPath, defaultSettingsData);
        }
    }
  
    void Awake()
    {  
        Init(); 
    }
 
}

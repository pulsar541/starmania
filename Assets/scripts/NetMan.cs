using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMan : NetworkManager
{


        [Tooltip("Assign Main Panel so it can be turned on from Player:OnStartClient")]
        public RectTransform mainPanel;

        [Tooltip("Assign Players Panel for instantiating PlayerUI as child")]
        public RectTransform playersPanel;

        
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position 
            GameObject player = Instantiate(playerPrefab,  LevelController.mapCenter, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player);
            Player.ResetPlayerNumbers();
               
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {  
            
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
            Player.ResetPlayerNumbers();
            
        }
     
 
 
        public override void OnStartServer()
        {
           base.OnStartServer();
           SceneController.Resume(); 
           MapGenerator.Reset();
        
        }
 
        public override void OnStartClient()
        {
            base.OnStartClient();
            SceneController.Resume();
            MapGenerator.Reset();
        }
 

        public override void OnStopServer()
        {
           base.OnStopServer();
           SceneController.Pause(); 
           MapGenerator.Reset();
           
           if(GameObject.Find("Camera") && GameObject.Find("Camera").transform.parent)
                GameObject.Find("Camera").transform.parent = null; 
           
          // GameObject.Find("MapMarker").transform.parent = null;
        }
}
 
public struct PosMessage : NetworkMessage //наследуемся от интерфейса NetworkMessage, чтобы система поняла какие данные упаковывать
{
    public Vector3 vector; //нельзя использовать Property
}

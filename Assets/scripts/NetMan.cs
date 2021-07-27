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

  //  bool playerSpawned; 
  //  NetworkConnection connection;
  //  bool playerConnected;
 
  //  [SerializeField] private GameObject levelControllerPrefab;

    // public void OnCreateCharacter(NetworkConnection conn, PosMessage message)
    // {
    //     GameObject go = Instantiate(playerPrefab, message.vector, Quaternion.identity); //локально на сервере создаем gameObject
    //     NetworkServer.AddPlayerForConnection(conn, go); //присоеднияем gameObject к пулу сетевых объектов и отправляем информацию об этом остальным игрокам
 
    // }
    // public override void OnStartServer()
    // {
    //     base.OnStartServer();
    //     playerSpawned = false;   
    //     playerConnected = false;
    //     NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); //указываем, какой struct должен прийти на сервер, чтобы выполнился свапн    
    // }
 

    // public void ActivatePlayerSpawn( Vector3 pos)
    // {
    //     //Vector3 pos = Input.mousePosition;
    //    // pos.z = 10f;
    //    // pos = Camera.main.ScreenToWorldPoint(pos);

    //     PosMessage m = new PosMessage() { vector = pos }; //создаем struct определенного типа, чтобы сервер понял к чему эти данные относятся
    //     connection.Send(m); //отправка сообщения на сервер с координатами спавна
    //     playerSpawned = true;
    // }

 

    // public override void OnClientConnect(NetworkConnection conn)
    // {
    //     base.OnClientConnect(conn);
    //     connection = conn;
    //     playerConnected = true; 
    // }

    // public override void OnClientDisconnect(NetworkConnection conn)
    // {
    //     base.OnClientDisconnect(conn);
    //     connection = conn;
    //     playerConnected = false; 
    //     playerSpawned = false;
    // }
    

    // private void Update()
    // {
    //     if (/*Input.GetKeyDown(KeyCode.Mouse0) &&*/ !playerSpawned && playerConnected)
    //     {
    //         ActivatePlayerSpawn(LevelController.mapCenter);
    //     }
    // }


    // public override void OnStopServer() {
    //    levelControllerPrefab.GetComponent<LevelController>().Clear();  

    //    playerConnected = false; 
    //    playerSpawned = false;
    // }




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
        }
 
        public override void OnStartClient()
        {
            base.OnStartClient();
            SceneController.Resume();
        }
 

}
 
public struct PosMessage : NetworkMessage //наследуемся от интерфейса NetworkMessage, чтобы система поняла какие данные упаковывать
{
    public Vector3 vector; //нельзя использовать Property
}

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMan : NetworkManager
{
    bool playerSpawned; 
    NetworkConnection connection;
    bool playerConnected;
 
    [SerializeField] private GameObject levelControllerPrefab;

    public void OnCreateCharacter(NetworkConnection conn, PosMessage message)
    {
        GameObject go = Instantiate(playerPrefab, message.vector, Quaternion.identity); //локально на сервере создаем gameObject
        NetworkServer.AddPlayerForConnection(conn, go); //присоеднияем gameObject к пулу сетевых объектов и отправляем информацию об этом остальным игрокам
 
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PosMessage>(OnCreateCharacter); //указываем, какой struct должен прийти на сервер, чтобы выполнился свапн
 
    //    levelControllerPrefab.GetComponent<LevelController>().GenerateLevel();
   //     levelControllerPrefab.GetComponent<LevelController>().Build();        
    }
 

    public void ActivatePlayerSpawn( Vector3 pos)
    {
        //Vector3 pos = Input.mousePosition;
       // pos.z = 10f;
       // pos = Camera.main.ScreenToWorldPoint(pos);

        PosMessage m = new PosMessage() { vector = pos }; //создаем struct определенного типа, чтобы сервер понял к чему эти данные относятся
        connection.Send(m); //отправка сообщения на сервер с координатами спавна
        playerSpawned = true;
    }


    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        connection = conn;
        playerConnected = true;

        //levelControllerPrefab.GetComponent<LevelController>().InitLevel();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !playerSpawned && playerConnected)
        {
            ActivatePlayerSpawn(new Vector3(Global.CUBES_I/2,Global.CUBES_J/2,Global.CUBES_K/2));
        }
    }
}
 
public struct PosMessage : NetworkMessage //наследуемся от интерфейса NetworkMessage, чтобы система поняла какие данные упаковывать
{
    public Vector3 vector; //нельзя использовать Property
}

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;


//Message class for sending transform data to server
public class TDMessage : MessageBase
{
    public Vector3 _pos;
    public Vector3 _euler;

    public TDMessage()
    {
        _pos = new Vector3();
        _euler = new Vector3();
    }

    public TDMessage(Vector3 pos, Vector3 euler)
    {
        _pos = pos;
        _euler = euler;
    }
}

//Probably a useless class, all data exists in the message class
public class TDConnection
{
    public Vector3 _pos;
    public Vector3 _euler;
    public int _connectionID;
}

public class TDServer : MonoBehaviour
{
    const short MESSAGE_DATA = 880;
    const short MESSAGE_INFO = 881;
    private Dictionary<int, TDConnection> _connectionList;
    public int clients;

    // Use this for initialization
    void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            //transform.forward = new Vector3(0, -1, 0);
            //transform.right = new Vector3(-1, 0, 0);
            //transform.up = new Vector3(0, 0, 1);

            SetupServer();

            _connectionList = new Dictionary<int, TDConnection>();
        }
        else
        {
            //gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            clients = _connectionList.Count;
            foreach (TDConnection TDC in _connectionList.Values)
            {
                TDC._pos = this.transform.localPosition;
                TDC._euler = this.transform.eulerAngles;
            }
        }
    }

    // Create a server and listen on a port
    public void SetupServer()
    {
        NetworkServer.Listen(5000); //Listen for connections on the port
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected); //Register a handler for connection
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected); //Register a handler for disconnect
        NetworkServer.RegisterHandler(MESSAGE_DATA, UpdateServer); //Register a handler for when the client updates the server with transform information
    }

    public void UpdateServer(NetworkMessage TDMsg)
    {
        TDMessage transformData = TDMsg.ReadMessage<TDMessage>();
        this.transform.localPosition = transformData._pos;
        Camera.main.transform.eulerAngles = transformData._euler;
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        int connectionID = netMsg.conn.connectionId;
        NetworkServer.SetClientReady(netMsg.conn);
        TDConnection newTDConnection = new TDConnection();
        newTDConnection._connectionID = connectionID;
        _connectionList.Add(connectionID, newTDConnection);
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        int connectionID = netMsg.conn.connectionId;
        _connectionList.Remove(connectionID);
    }
}

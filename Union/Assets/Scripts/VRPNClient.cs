using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class VRPNClient : MonoBehaviour {
	const short MESSAGE_DATA = 880;
	const short MESSAGE_INFO = 881;
	const string SERVER_ADDRESS = "129.59.70.59";
	const string TRACKER_ADDRESS = "192.168.1.100";
	const int SERVER_PORT = 5000;
	NetworkClient myClient;

	// Use this for initialization
	void Start () {
		SetupClient ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	void OnGUI() {
		GUI.Label (new Rect (2, 10 + 200, 150, 100), transform.position.x.ToString ());
	}

	public void SetupClient()
	{
		myClient = new NetworkClient();
		myClient.RegisterHandler (MESSAGE_DATA, DataReceptionHandler);
		myClient.RegisterHandler(MsgType.Connect, OnConnected);     
		myClient.Connect(SERVER_ADDRESS, SERVER_PORT);
	}

	public void DataReceptionHandler(NetworkMessage vrpnMsg)
	{
		VRPNMessage vrpnData = vrpnMsg.ReadMessage<VRPNMessage>();

		transform.position = vrpnData._pos;
		transform.eulerAngles = vrpnData._quat.eulerAngles;
		//message = transform.position.ToString();
	}

	public void OnConnected(NetworkMessage netMsg)
	{
		myClient.SendUnreliable(MESSAGE_INFO, new VRPNInfo("GearVR", TRACKER_ADDRESS));
	}
}

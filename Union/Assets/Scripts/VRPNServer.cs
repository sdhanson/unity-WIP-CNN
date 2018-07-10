using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;


public class VRPNMessage : MessageBase
{
	public Vector3 _pos;
	public Quaternion _quat;

	public VRPNMessage()
	{
		_pos = new Vector3 ();
		_quat = new Quaternion ();
	}

	public VRPNMessage(Vector3 pos, Quaternion quat)
	{
		_pos = pos;
		_quat = quat;
	}
}

public class VRPNInfo : MessageBase
{
	public string _VRPNObject;
	public string _IPAdress;

	public VRPNInfo()
	{
		_VRPNObject = "";
		_IPAdress = "192.168.1.100";
	}

	public VRPNInfo(string VRPNObject)
	{
		_VRPNObject = VRPNObject;
		_IPAdress = "192.168.1.100";
	}

	public VRPNInfo(string VRPNObject, string IPAdress)
	{
		_VRPNObject = VRPNObject;
		_IPAdress = IPAdress;
	}
}
public class VRPNConnection
{
	public Vector3 _pos;
	public Quaternion _quat;
	public string _VRPNObject;
	public int _connectionID;
}

public class VRPNServer : MonoBehaviour {
	const short MESSAGE_DATA = 880;
	const short MESSAGE_INFO = 881;
	private Dictionary<int, VRPNConnection> _connectionList;

	// Use this for initialization
	void Start () {
		if (Application.platform != RuntimePlatform.Android) {
			transform.forward = new Vector3 (0, -1, 0);
			transform.right = new Vector3 (-1, 0, 0);
			transform.up = new Vector3 (0, 0, 1);

			SetupServer ();

			_connectionList = new Dictionary<int, VRPNConnection> ();
		} else {
			gameObject.SetActive (false);
		}
	}

	void Update () 
	{
		foreach (VRPNConnection vrpnC in _connectionList.Values) {
			if (vrpnC._VRPNObject != null) {
				Vector3 tmpPos = VRPN.vrpnTrackerPos (vrpnC._VRPNObject, 0);
				Quaternion tmpQuat = VRPN.vrpnTrackerQuat (vrpnC._VRPNObject, 0);
				Vector3 pos = new Vector3 (-tmpPos.x, tmpPos.z, -tmpPos.y);
				Quaternion quat = new Quaternion (tmpQuat.x, -tmpQuat.z, tmpQuat.y, tmpQuat.w);
				vrpnC._pos = pos;
				vrpnC._quat = quat;
				//NetworkServer.SendToAll (MESSAGE_DATA, new VRPNMessage (pos, quat));
				NetworkServer.SendToClient (vrpnC._connectionID, MESSAGE_DATA, new VRPNMessage (pos, quat));
			}
		}
	}

	void OnGUI()
	{
		int count = 0;
		foreach (VRPNConnection vrpnC in _connectionList.Values) {
			if (vrpnC._VRPNObject != null) {
				string info = vrpnC._VRPNObject + " Pos: " + vrpnC._pos.ToString ("F3") + " Orientation: " + vrpnC._quat.eulerAngles.ToString("F3");
				GUI.Label (new Rect (2, 10 + 20 * count, 1000, 100), info);
			}
		}
	} 

	// Create a server and listen on a port
	public void SetupServer()
	{
		NetworkServer.Listen(5000);
		NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
		NetworkServer.RegisterHandler (MESSAGE_INFO, SetupClient);
	}

	// Create a client and connect to the server port
	public void SetupClient(NetworkMessage vrpnMsg)
	{
		VRPNInfo vrpnData = vrpnMsg.ReadMessage<VRPNInfo> ();
		VRPNConnection vrpnC = _connectionList [vrpnMsg.conn.connectionId];
		vrpnC._VRPNObject = vrpnData._VRPNObject + "@" + vrpnData._IPAdress;
	}

	public void OnConnected(NetworkMessage netMsg)
	{
		int connectionID = netMsg.conn.connectionId;
		NetworkServer.SetClientReady(netMsg.conn);
		VRPNConnection newVRPNConnection = new VRPNConnection ();
		newVRPNConnection._connectionID = connectionID;
		_connectionList.Add (connectionID, newVRPNConnection);
	}

	public void OnDisconnected(NetworkMessage netMsg)
	{
		int connectionID = netMsg.conn.connectionId;
		_connectionList.Remove (connectionID);
	}
}

/*public void DataReceptionHandler(NetworkMessage vrpnMsg)
{
	VRPNMessage vrpnData = vrpnMsg.ReadMessage<VRPNMessage>();

	//_pos = vrpnData._pos;
	//transform.eulerAngles = vrpnData._quat.eulerAngles;
	//message = transform.position.ToString();
}*/

/*public class MyNetworkServer : MonoBehaviour {


	// Use this for initialization
	void Start () {
		
	}
	
	private bool isAtStartup = true;
	private bool isAndroid = false;
	public string message = "";
	public Text messageText;

	private int _connectionID;
	public static Vector3 _pos = new Vector3 ();
	public static Quaternion _quat = new Quaternion ();

	NetworkClient myClient;

	void Update () 
	{
		if (isAtStartup) {
			transform.forward = new Vector3 (0, -1, 0);
			transform.right = new Vector3 (-1, 0, 0);
			transform.up = new Vector3 (0, 0, 1);
			SetupServer ();

			
		} else {
			Vector3 tmpPos = VRPN.vrpnTrackerPos ("GearVR@192.168.1.100", 0);
			Quaternion tmpQuat = VRPN.vrpnTrackerQuat ("GearVR@192.168.1.100", 0);
			Vector3 pos = new Vector3 (-tmpPos.x, tmpPos.z, -tmpPos.y);
			Quaternion quat = new Quaternion (tmpQuat.x, -tmpQuat.z, tmpQuat.y, tmpQuat.w);
			if (_connectionID > 0) {
				NetworkServer.SendToAll((short)880, new VRPNMessage (pos, quat));
			}
			transform.position = pos;
			transform.rotation = quat;
		}
	}

	void OnGUI()
	{
		if (isAtStartup) {
			GUI.Label (new Rect (2, 10, 150, 100), "Press S for server");      
			GUI.Label (new Rect (2, 50, 150, 100), "Press C for client");
		} else if (isAndroid) {
			messageText.text = message;
		} else {
			
		}
	} 

	// Create a server and listen on a port
	public void SetupServer()
	{
		NetworkServer.Listen(5000);
		NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
		NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
		isAtStartup = false;
	}

	// Create a client and connect to the server port
	public void SetupClient()
	{
		myClient = new NetworkClient();
		myClient.RegisterHandler ((short)880, DataReceptionHandler);
		myClient.RegisterHandler(MsgType.Connect, OnConnected);     
		myClient.Connect("129.59.70.59", 5000);
		isAtStartup = false;
	}

	// client function
	public void OnConnected(NetworkMessage netMsg)
	{
		_connectionID = netMsg.conn.connectionId;
		message = "Connected";
	}

	public void OnDisconnected(NetworkMessage netMsg)
	{
		_connectionID = -1;
		message = "Disconnected";
	}

	public void DataReceptionHandler(NetworkMessage _vrpnData)
	{
		VRPNMessage vrpnData = _vrpnData.ReadMessage<VRPNMessage>();
		_pos = vrpnData._pos;
		//transform.eulerAngles = vrpnData._quat.eulerAngles;
		//message = transform.position.ToString();
	}
		

	public Vector3 intendedCenter = new Vector3 (-18f, 0, -9f);
	public float prevXAngle = 0f;
	public Vector3 prevPos = new Vector3();
	public bool resetNeeded = false;
	public bool hasNotReturnedToBounds = false;
	public float virtualAngleTurned = 0f; //each reset
	public float cumulativeAngleTurned = 0f; //total

	public void resettingFSM()
	{
		//Gather pertinent data
		Vector3 deltaTranslationByFrame = _pos - prevPos;
		float realWorldRotation = Camera.main.transform.localEulerAngles.y;
		float deltaRotationByFrame = realWorldRotation - prevXAngle;
		//if crossed threshold from + to - (1 to 359)
		if (deltaRotationByFrame > 90) {
			deltaRotationByFrame = deltaRotationByFrame - 360;
		}
		//if crossed threshold from - to + (359 to 1)
		else if (deltaRotationByFrame < -90) {
			deltaRotationByFrame = deltaRotationByFrame + 360;
		}

		//check to see if a reset is needed (only check if no reset has
		//	been triggered yet, and the subject has returned to inner bounds
		if (!resetNeeded && !hasNotReturnedToBounds && OutOfBounds ()) {
			resetNeeded = true;
			hasNotReturnedToBounds = true;
			virtualAngleTurned = 0f;
		}
		//perform reset by manipulating gain (to do this we will rotate the object in the opposite direction)
		else if (resetNeeded) {
			//Calculate the total rotation neccesary
			float calc1 = Mathf.Rad2Deg * Mathf.Atan2 (intendedCenter.x - _pos.x, intendedCenter.z - _pos.z);
			float rotationRemainingToCenter = calc1 - realWorldRotation;
			//fix rotation variables
			if (rotationRemainingToCenter < -360) {
				rotationRemainingToCenter += 360;
			}
			if (rotationRemainingToCenter < -180) {
				rotationRemainingToCenter = 360 + rotationRemainingToCenter;
			}
			float rotationRemaningToCenterP = 0;
			float rotationRemaningToCenterN = 0;
			//determine left and right angles to rotate
			if (rotationRemainingToCenter < 0) {
				rotationRemaningToCenterN = rotationRemainingToCenter;
				rotationRemaningToCenterP = 360 + rotationRemainingToCenter;
			} else {
				rotationRemaningToCenterP = rotationRemainingToCenter;
				rotationRemaningToCenterN = rotationRemainingToCenter - 360;
			}

			//determine gain based on direction subject has rotated already
			//tuned so that at 360 virtual angle turned the person is pointing back to the center
			float gain = 0;
			if (virtualAngleTurned > 0) {
				gain = (360f - virtualAngleTurned) / rotationRemaningToCenterP - 1;
			} else {
				gain = -(360f + virtualAngleTurned) / rotationRemaningToCenterN - 1;
			}
			//inject rotation
			float injectedRotation = (deltaRotationByFrame) * gain;
			virtualAngleTurned += deltaRotationByFrame; //baseline turn
			virtualAngleTurned += injectedRotation; //amount we make them turn as well
			cumulativeAngleTurned -= injectedRotation; //to keep the person moving in the correct direction

			//add the injected rotation to the parent object
			Vector3 tmp = transform.eulerAngles;
			tmp.y += injectedRotation;
			transform.eulerAngles = tmp;
			//if a full turn has occured then stop resetting
			if (Mathf.Abs (virtualAngleTurned) > 359f) {
				resetNeeded = false;
			}
			message = "Please Turn Around";
		} 
		//Subject needs to walk forward two steps to prevent further triggers
		else if (hasNotReturnedToBounds) {
			if (ReturnedToBounds ()) {
				hasNotReturnedToBounds = false;
			}
			message = "Please Walk Forward";
		}
		//General Operating
		else {
			message = "Please Do Whatever";
		}
		//update position incrementally using sin and cos
		float delX = Mathf.Cos(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.x + Mathf.Sin(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.z;
		float delZ = Mathf.Cos(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.z + Mathf.Sin(cumulativeAngleTurned * Mathf.Deg2Rad) * deltaTranslationByFrame.x;
		transform.Translate(deltaTranslationByFrame);
		//store data for use next frame
		prevPos = _pos;
		prevXAngle = Camera.main.transform.localEulerAngles.y;

		message = message + "\n rotation: " + cumulativeAngleTurned.ToString ();
	}

	public bool OutOfBounds() {
		if (_pos.x > 42f)
			return true;
		if (_pos.x < -78f)
			return true;
		if (_pos.z > 36f)
			return true;
		if (_pos.z < -45f)
			return true;
		return false;
	}

	public bool ReturnedToBounds() {
		if (_pos.x > 32f)
			return false;
		if (_pos.x < -68f)
			return false;
		if (_pos.z > 26f)
			return false;
		if (_pos.z < -35f)
			return false;
		return true;
	}
}
*/
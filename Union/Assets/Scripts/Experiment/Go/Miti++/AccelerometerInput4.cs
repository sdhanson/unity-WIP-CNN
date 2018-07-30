using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccelerometerInput4 : MonoBehaviour
{
    // used to determine direction to walk
    private float yaw;
	private float rad;
	private float xVal;
	private float zVal;

    // determine if person is picking up speed or slowing down
    public static float velocity = 0f;
	public static float method1StartTimeGrow = 0f;
	public static float method1StartTimeDecay = 0f;
	public static bool wasOne = false;
	//phase one when above (+/-) 0.10 threshold
	public static bool wasTwo = true;
	//phase two when b/w -0.10 and 0.10 thresholds

	//Initial X and Y angles (used to determine if user is looking around)
	private float eulerX;
	private float eulerZ;

	private float decayRate = 0.2f;
	bool looking = false;

    // initialize display to get accelerometer from Oculus GO
    OVRDisplay display;

    // queue for determining if walking
	Queue<float> sumQ;
	float sum = 0;
	int countQ = 0;
	int window = 27;

	void Start ()
	{
		//Enable the gyroscope on the phone
		Input.gyro.enabled = true;
		//If we are on the phone, then setup a client device to read transform data from
		if (Application.platform == RuntimePlatform.Android)
			SetupClient ();

		//User must be looking ahead at the start
		eulerX = InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x;
		eulerZ = InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z;

        // initialize the oculus go display
        display = new OVRDisplay ();

        // initialize walking queue
		sumQ = new Queue<float> ();
	}

	void FixedUpdate () //was previously FixedUpdate()
	{

		//Send the current transform data to the server (should probably be wrapped in an if isAndroid but I haven't tested)
		string path = Application.persistentDataPath + "/WIP_looking.txt";

        // debug output
		string appendText = "\n" + String.Format ("{0,20} {1,7} {2, 15} {3, 15} {4, 15} {5, 15} {6, 15} {7, 8} {8, 15}", 
			                    DateTime.Now.ToString (), Time.time,

			                    "ACCELERATION_XYZ: ",

			                    display.acceleration.x, 
			                    display.acceleration.y, 
			                    display.acceleration.z, 

			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x,
			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.y,
			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z);

		File.AppendAllText (path, appendText);

		//Do the movement algorithm, more details inside
		move ();

		if (myClient != null)
			myClient.Send (MESSAGE_DATA, new TDMessage (this.transform.localPosition, Camera.main.transform.eulerAngles));
	}

    // check if average walking speed is high enough to be considered walking - I DON'T THINK THIS IS BEING USED ANYWAY
	float average ()
	{
		float curr = display.acceleration.y;

		if (countQ == window) {
			float temp = sumQ.Peek ();
			sum -= temp;
			sumQ.Dequeue ();
			countQ--;
		}

		sumQ.Enqueue (Mathf.Abs (curr));
		sum += Mathf.Abs (curr);
		countQ++;

		curr = sum / countQ;
		return curr;
	}

	//Algorithm to determine if the user is looking around. Looking and walking generate similar gyro.accelerations, so we
	//want to ignore movements that could be spawned from looking around. Makes sure user's head orientation is in certain window
	bool look (double start, double curr, double diff)
	{
		//Determines if the user's current angle (curr) is within the window (start +/- diff)
		//Deals with wrap around values (eulerAngles is in range 0 to 360)
		if ((start + diff) > 360f) {
			if (((curr >= 0f) && (curr <= (start + diff - 360f))) || ((((start - diff) <= curr) && (curr <= 360f)))) {
				return false;
			}
		} else if ((start - diff) < 0f) {
			if (((0f <= curr) && (curr <= (start + diff))) || (((start - diff + 360f) <= curr) && (curr <= 360f))) {
				return false;
			}
		} else if (((start + diff) <= curr) && (curr <= (start + diff))) {
			return false;
		}
		return true;
	}
	
	//If the user is walking, moves them in correct direction with varying velocities
	//Also sets velocity to 0 if it is determined that the user is no longer walking
	void move ()
	{
		//Get the yaw of the subject to allow for movement in the look direction
		yaw = InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.y;
		//convert that value into radians because math uses radians
		rad = yaw * Mathf.Deg2Rad;
		//map that value onto the unit circle to faciliate movement in the look direction
		zVal = 0.55f * Mathf.Cos (rad);
		xVal = 0.55f * Mathf.Sin (rad);

        // check if person is looking around in X or Z directions
        bool looking = (look (eulerX, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x, 20f) || look (eulerZ, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z, 20f));

		if (!looking) {
			if ((display.acceleration.y >= 0.75f || display.acceleration.y <= -0.75f))
            {
				if (wasTwo)
                { //we are transitioning from phase 2 to 1
					method1StartTimeGrow = Time.time;
					wasTwo = false;
					wasOne = true;
				}
			}
            else
            {
				if (wasOne)
                {
					method1StartTimeDecay = Time.time;
					wasOne = false;
					wasTwo = true;
				}
			}
			if ((display.acceleration.y >= 0.75f || display.acceleration.y <= -0.75f))
            {
				velocity = 3.0f - (3.0f - velocity) * Mathf.Exp ((method1StartTimeGrow - Time.time) / 0.5f); //grow
			}
            else
            {
				velocity = 0.0f - (0.0f - velocity) * Mathf.Exp ((method1StartTimeDecay - Time.time) / decayRate); //decay
			}
		}
        else
        {
			velocity = 0f;
		}

		//Multiply intended speed (called velocity) by delta time to get a distance, then multiply that distamce
		//    by the unit vector in the look direction to get displacement.
		transform.Translate (xVal * velocity * Time.fixedDeltaTime, 0, zVal * velocity * Time.fixedDeltaTime);
	}

	#region NetworkingCode

	//Declare a client node
	NetworkClient myClient;
	//Define two types of data, one for setup (unused) and one for actual data
	const short MESSAGE_DATA = 880;
	const short MESSAGE_INFO = 881;
	//Server address is Flynn, tracker address is Baines, port is for broadcasting
	const string SERVER_ADDRESS = "192.168.1.2";
	const string TRACKER_ADDRESS = "192.168.1.100";
	const int SERVER_PORT = 5000;

	//Message and message text are now depreciated, were used for debugging
	public string message = "";
	public Text messageText;

	//Connection ID for the client server interaction
	public int _connectionID;
	//transform data that is being read from the clien
	public static Vector3 _pos = new Vector3 ();
	public static Vector3 _euler = new Vector3 ();

	// Create a client and connect to the server port
	public void SetupClient ()
	{
		myClient = new NetworkClient (); //Instantiate the client
		myClient.RegisterHandler (MESSAGE_DATA, DataReceptionHandler); //Register a handler to handle incoming message data
		myClient.RegisterHandler (MsgType.Connect, OnConnected); //Register a handler to handle a connection to the server (will setup important info
		myClient.Connect (SERVER_ADDRESS, SERVER_PORT); //Attempt to connect, this will send a connect request which is good if the OnConnected fires
	}

	// client function to recognized a connection
	public void OnConnected (NetworkMessage netMsg)
	{
		_connectionID = netMsg.conn.connectionId; //Keep connection id, not really neccesary I don't think
	}

	// Clinet function that fires when a disconnect occurs (probably unnecessary
	public void OnDisconnected (NetworkMessage netMsg)
	{
		_connectionID = -1;
	}

	//I actually don't know for sure if this is useful. I believe that this is erroneously put here and was duplicated in TDServer code.
	public void DataReceptionHandler (NetworkMessage _transformData)
	{
		TDMessage transformData = _transformData.ReadMessage<TDMessage> ();
		_pos = transformData._pos;
		_euler = transformData._euler;
	}

	#endregion

}
using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccelerometerInput5 : MonoBehaviour
{
	private float yaw;
	private float rad;
	private float xVal;
	private float zVal;

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

	private bool walking = false;
	private float iteration = 0f;

	//Queue to keep track of past X userAcceleration.Y values
	private Queue<float> accelY;
	private float sumY = 0f;
	private float thresholdAccelY = 0.008f;

	//Queue to keep track of diff between X pairs of current and previous
	//userAcceleration.Y values
	private Queue<float> changeY;
	private float sumChangeY = 0f;
	private float thresholdChangeY = 0.008f;
	private float prev = 0f;

	private float decayRate = 0.2f;


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

		//Initialize the queues
		accelY = new Queue<float> ();
		changeY = new Queue<float> ();
	}

	void FixedUpdate () //was previously FixedUpdate()
	{
		string path = Application.persistentDataPath + "/CW4Test_Data.txt";

		// This text is always added, making the file longer over time if it is not deleted
		string appendText = "\n" + String.Format ("{0,20} {1,7} {2, 8} {3, 15} {4, 15} {5, 15} {6, 15} {7, 8} {8, 15} {9, 10} {10, 10} {11, 10} {12, 10} {13, 10} {14, 10} {15, 10} {16, 10}", 
			                    DateTime.Now.ToString (), Time.time, 

			                    Input.GetMouseButtonDown (0),

			                    Input.gyro.userAcceleration.x, 
			                    Input.gyro.userAcceleration.y, 
			                    Input.gyro.userAcceleration.z, 

			                    gameObject.transform.position.x,
			                    gameObject.transform.position.y,
			                    gameObject.transform.position.z,

			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x,
			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.y,
			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z,
			                    sumY.ToString (), walking.ToString (), wasTwo.ToString (), velocity, decayRate);

		File.AppendAllText (path, appendText);

		//Determine if the user is walking, more details inside
		manageWalking ();
		//Do the movement algorithm, more details inside
		move ();
		//Send the current transform data to the server (should probably be wrapped in an if isAndroid but I haven't tested)
		if (myClient != null)
			myClient.Send (MESSAGE_DATA, new TDMessage (this.transform.localPosition, Camera.main.transform.eulerAngles));
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

	//Determines if user has met conditions for STARTING to walk (enough acceleration in Y and Z and not looking around)
	//More lenient than usual conditions because we want the user to start moving IMMEDIATELY when they start walking
	bool inWindow ()
	{
		bool moving = false;
		double xDif = 20f;
		double zDif = 15f;

		//	Checks if the user is moving enough to be considered walking. Thresholds determined by analyzing typical walking averages.
		if ((Input.gyro.userAcceleration.y >= 0.045f || Input.gyro.userAcceleration.y <= -0.045f)
		    && ((Input.gyro.userAcceleration.z < 0.08f) && (Input.gyro.userAcceleration.z > -0.08f))) {
			moving = true;
		}
		//Checks that the user is not looking around
		if (moving && !look (eulerX, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x, xDif)
		    && !look (eulerZ, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z, zDif)) {
			return true;
		}

		return false;
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

		if (!walking) {
			velocity = 0f;
		} else {
//		Idea is that if the user's head movement is below 0.12 (but above threshold to be walking) then they
//		are slowing down. If user continues to walk, head movement will quickly reach > |0.12| again, but
//		if they stop, then we are already decelerating and will not glide.
			if ((Input.gyro.userAcceleration.y >= 0.075f || Input.gyro.userAcceleration.y <= -0.075f)) {
				if (wasTwo) { //we are transitioning from phase 2 to 1
					method1StartTimeGrow = Time.time;
					wasTwo = false;
					wasOne = true;
				}
			} else {
				if (wasOne) {
					method1StartTimeDecay = Time.time;
					wasOne = false;
					wasTwo = true;
				}
			}
	
			//Movement is done exponentially. We want the user to quickly accelerate and quickly decelerate as to minimize
			//starting and stopping latency.
			if ((Input.gyro.userAcceleration.y >= 0.06f || Input.gyro.userAcceleration.y <= -0.06f)) {
				velocity = 3.0f - (3.0f - velocity) * Mathf.Exp ((method1StartTimeGrow - Time.time) / 0.5f); //grow
			} else {
				if (velocity > 2.5f) {
					decayRate = 0.05f;
				} else if (velocity > 2.0f) {
					decayRate = 0.05f;
				} else if (velocity > 1.0f) {
					decayRate = 0.1f;
				} else if (velocity < 0.5f) {
					decayRate = 0.2f;
				} else if (velocity < 0.1f) {
					decayRate = 0.08f;
				}
				velocity = 0.0f - (0.0f - velocity) * Mathf.Exp ((method1StartTimeDecay - Time.time) / decayRate); //decay
				if (velocity < 0.01f) {
					velocity = 0;
					walking = false;
					accelY.Clear ();
					changeY.Clear ();
					iteration = 0f;
				}
			}
		}

		//Multiply intended speed (called velocity) by delta time to get a distance, then multiply that distamce
		//    by the unit vector in the look direction to get displacement.
		transform.Translate (xVal * velocity * Time.fixedDeltaTime, 0, zVal * velocity * Time.fixedDeltaTime);
	}

	//Determines if the user is walking by looking at the average value of userAcceleration.Y and the average change in
	//userAcceleration.Y over the past ~10 time steps
	void manageWalking ()
	{
		//iteration is 0 when the user is stopped. iteration is 10 when the queue is full.
		if (iteration < 9f) {
			//We only start keeping track of values when the user is determined to be walking (inWindow)
			//and then we fill the queue with data for the next 9 time steps
			if (inWindow () || (iteration > 0f)) {
				//Filling the changeY queue (can't add on first round bc don't have prev value)
				if (iteration != 0f) {
					float change = prev - Math.Abs (Input.gyro.userAcceleration.y);
					changeY.Enqueue (Math.Abs (change));
					sumChangeY += Math.Abs (change);
				}
				//Filling the accelY queue
				walking = true;
				accelY.Enqueue (Math.Abs (Input.gyro.userAcceleration.y));
				iteration++;
				sumY += Math.Abs (Input.gyro.userAcceleration.y);
			}
			//Setting prev
			prev = Math.Abs (Input.gyro.userAcceleration.y);
		} else {
			//Adding current value to changeY queue
			float change = prev - Math.Abs (Input.gyro.userAcceleration.y);
			changeY.Enqueue (Math.Abs (change));
			sumChangeY += Math.Abs (change);

			//Adding current value to accelY queue
			accelY.Enqueue (Math.Abs (Input.gyro.userAcceleration.y));
			sumY += Math.Abs (Input.gyro.userAcceleration.y);

			//If the average over the past ten values for accelY or changeY is below the threshold or the user
			//is looking around, the user is not walking anymore. Reset everything and walking=false (so velocity is set to 0)
			//If we are walking, need to keep queue at 10 values, so we remove the oldest value. || ((sumChangeY / 9) < thresholdChangeY) || ((sumY / 10) > 0.8)
			if (((sumY / 10) < thresholdAccelY) || ((sumChangeY / 9) < thresholdChangeY) || look (eulerX, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x, 15f) || look (eulerZ, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z, 10f)) {
				walking = false;
				accelY.Clear ();
				changeY.Clear ();
				iteration = 0f;
			} else {
				//Removing from changeY
				float temp2 = changeY.Peek ();
				changeY.Dequeue ();
				sumChangeY -= temp2;
				//Removing from accelY
				float temp = accelY.Peek ();
				accelY.Dequeue ();
				sumY -= temp;
			}
		}

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

using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Threading;

using TensorFlow;


public class AccelerometerInputCNN : MonoBehaviour
{
    Thread run;
    Thread collect;

    private float yaw;
    private float rad;
    private float xVal;
    private float zVal;

    public static float velocity = 0f;
    public static float method1StartTimeGrow = 0f;
    public static float method1StartTimeDecay = 0f;
    //phase one when above (+/-) 0.10 threshold
    public static bool wasOne = false;
    //phase two when b/w -0.10 and 0.10 thresholds
    public static bool wasTwo = true;
    private float decayRate = 0.4f;

    // initial X and Y angles - used to determine if user is looking around
    private float eulerX;
    private float eulerZ;

    // indicates if person is looking around - not implemented yet
    bool looking = false;

    // set per person
    private float height = 1.75f;

    // set by trained CNN model
    public int inputWidth = 90;

    // third value corresponds to inputWidth
    public TextAsset graphModel;
    private float[,,,] inputTensor = new float[1, 1, 90, 1];

    // queue for keeping track of values for tensor
    //	private Queue<float> accelQ;
    private List<float> accelL;
    //	private int countQ = 0;

    // determine if person is walking from cnn returned value
    private bool walking = false;
    private int standIndex = 1;

    // how many options of activities we have - standing, walking, jogging
    private int activityIndexChoices = 2;

    // FOR DEBUGGING PUTTING AT GLOBAL SCOPE
    float confidence = 0;
    float sum = 0f;
    float test = 0f;
    int activity = 0;
    bool here = false;
    bool longTime = false;
    float line = 0f;


    int index = 1;
    int countCNN = 0;
    float total = 0;
    float test1 = 0f;
    float test2 = 0f;
    float test3 = 0f;

    bool one = true;

    int diff = 20;

    float ctime = 0f;
    float ptime = 0f;

    OVRDisplay display;

    void Start()
    {
#if UNITY_ANDROID
		TensorFlowSharp.Android.NativeBinding.Init ();
#endif
        //List<double> accelLD = new List<double> { 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1139805, 0.1072912, 0.1072912, 0.1962212, 0.1962212, 0.3138215, 0.3138215, 0.3138215, 0.1719452, 0.1719452, 0.05325568, 0.05325568, 0.1709562, 0.1709562, 0.3227122, 0.3227122, 0.3896469, 0.3896469, 0.1290453, 0.1290453, 0.1729235, 0.1729235, 0.06428681, 0.06428681, 0.3944435, 0.3944435, 0.5930174, 0.5930174, 0.4377055, 0.4377055, 0.2751099, 0.2751099, 0.1984278, 0.1984278, 0.5268957, 0.5268957, 0.5268957, 0.6580063, 0.4031855, 0.4031855, 0.2062997, 0.2062997, 0.1068822, 0.1068822, 0.1068822, 0.3127724, 0.3127724, 0.4308511, 0.4308511, 0.3025039, 0.3025039, 0.2085527, 0.222002, 0.222002, 0.222002, 0.5509907, 0.5509907, 0.4942238, 0.4942238 };
        //accelL = new List<float>();
        //for (int k=0; k <accelLD.Count; k++)
        //{
        //    accelL.Add((float)accelLD[k]);
        //}
        // enable the gyroscope on the phone
        Input.gyro.enabled = true;
        // if we are on the right VR, then setup a client device to read transform data from
        if (Application.platform == RuntimePlatform.Android)
            SetupClient();

        // user must be looking ahead at the start
        eulerX = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.x;
        eulerZ = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.z;

        // initialize the oculus go display
        display = new OVRDisplay();

        // initialize the cnn queue
        //		accelQ = new Queue<float> ();
        accelL = new List<float>();

        //start collection
        collect = new Thread(manageCollection);
        collect.Start();

        run = new Thread(manageCNN);
        run.Start();
        //int i;
        //for (i = 0; i < accelL.Count; i++)
        //{
        //    inputTensor[0, 0, i, 0] = accelL[i];
        //    test = inputTensor[0, 0, i, 0];
        //}
        //if (i != 90)
        //{
        //    inputTensor[0, 0, 89, 0] = 0;
        //}
        //float[,] recurrentTensor;

        //     create tensorflow model
        //    using (var graph = new TFGraph())
        //    {
        //        graph.Import(graphModel.bytes);
        //        var session = new TFSession(graph);
        //        var runner = session.GetRunner();

        //         do input tensor list to array and make it one dimensional
        //        var trainingInput = graph.Placeholder(TFDataType.Float, new TFShape(1, 1, 90, 1));
        //        TFTensor input = inputTensor;


        //         set up input tensor and input
        //        runner.AddInput(graph["input_placeholder_x"][0], input);

        //         set up output tensor
        //        runner.Fetch(graph["output_node"][0]);

        //         run model
        //        recurrentTensor = runner.Run()[0].GetValue() as float[,];
        //        here = true;
        //        session.Dispose();
        //        graph.Dispose();

        //    }

        //     find the most confident answer
        //    float highVal = 0;
        //    int highInd = -1;
        //    sum = 0f;

        //    for (int j = 0; j<activityIndexChoices; j++)
        //    {

        //        confidence = recurrentTensor[0, j];
        //        if (highInd > -1)
        //        {
        //            if (recurrentTensor[0, j] > highVal)
        //            {
        //                highVal = confidence;
        //                highInd = j;
        //            }
        //        }
        //        else
        //        {
        //            highVal = confidence;
        //            highInd = j;
        //        }

        //         debugging - sum should = 1 at the end
        //        sum += confidence;
        //    }
        //    Debug.Log(highInd + " " + recurrentTensor[0,0] + " " + recurrentTensor[0, 1]);
    }

    void FixedUpdate() //was previously FixedUpdate()
    {
        // send the current transform data to the server (should probably be wrapped in an if isAndroid but I haven't tested)

        string path = Application.persistentDataPath + "/WIP_looking.txt";


        string appendText = "\r\n" + String.Format("{0,20} {1,7} {2, 15} {3, 15} {4, 15} {5, 15} {6, 15} {7, 8} {8, 10} {9, 10} {10, 10} {11, 10} {12,10} {13,10} {14,10} {15,10} {16,10} {17,10} {18,10} {19,10} {20,10}",
                                DateTime.Now.ToString(), Time.time,

                                display.acceleration.x,
                                display.acceleration.y,
                                display.acceleration.z,

                                InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.x,
                                InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.y,
                                InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.z,

                                confidence, sum, test, index, here, accelL.Count, countCNN, total, diff,

                                line, test1, test2, ctime);

        File.AppendAllText(path, appendText);

        // do the movement algorithm, more details inside
        move();

        if (myClient != null)
            myClient.Send(MESSAGE_DATA, new TDMessage(this.transform.localPosition, Camera.main.transform.eulerAngles));
    }

    void OnApplicationQuit()
    {
        collect.Abort();
        run.Abort();
    }

    void manageCollection()
    {
        Thread.Sleep(1000);
        while (true)
        {
            Thread.Sleep(5);
            collectValues();
        }
    }

    void collectValues()
    {
        float curr = Mathf.Sqrt(Mathf.Pow(display.acceleration.x, 2) + Mathf.Pow(display.acceleration.y, 2) + Mathf.Pow(display.acceleration.z, 2));
        test = curr;

        if (accelL.Count < inputWidth)
        {
            if(accelL.Count == 0)
            {
                ptime = Time.time;
            }
            accelL.Add(curr);
        }
        if (accelL.Count == inputWidth)
        {
            ctime = Time.time - ptime;
            if (one)
            {
                outL();
                one = false;
            }
            accelL.RemoveAt(0);
            accelL.Add(curr);
        }
        line = curr;
    }

    void outL() {
        

        string path = Application.persistentDataPath + "/one.txt";
        string append = "[";

        for (int i = 0; i < accelL.Count; i++)
        {
            append += accelL[i];
            append += ",";

        }
        append += "]";

        File.AppendAllText(path, append);

    }

    void manageCNN()
    {

        while(true)
        {
            Thread.Sleep(100);
            float prev = Time.time;
            evaluate();
            float len = Time.time - prev;
            diff = (int)((0.5 - len)*1000);
            if(diff < 0)
            {
                diff *= -1;
            }
        }
    }

    void evaluate ()
	{
        // convert from list to array 
        if (accelL.Count == 90)
        {
            int i;
            for (i = 0; i < accelL.Count; i++)
            {
                inputTensor[0, 0, i, 0] = accelL[i];
                test = inputTensor[0, 0, i, 0];
            }
            if (i != 90)
            {
                inputTensor[0, 0, 89, 0] = 0;
            }

            float[,] recurrentTensor;

            // create tensorflow model
            using (var graph = new TFGraph())
            {
                graph.Import(graphModel.bytes);
                var session = new TFSession(graph);
                var runner = session.GetRunner();

                // do input tensor list to array and make it one dimensional
                //var trainingInput = graph.Placeholder(TFDataType.Float, new TFShape(1, 1, 90, 1));
                TFTensor input = inputTensor;


                // set up input tensor and input
                runner.AddInput(graph["input_placeholder_x"][0], input);

                // set up output tensor
                runner.Fetch(graph["output_node"][0]);

                // run model
                recurrentTensor = runner.Run()[0].GetValue() as float[,];
                here = true;
                session.Dispose();
                graph.Dispose();

            }

            // find the most confident answer
            float highVal = 0;
            int highInd = -1;
            sum = 0f;

            for (int j = 0; j < activityIndexChoices; j++)
            {

                confidence = recurrentTensor[0, j];
                if (highInd > -1)
                {
                    if (recurrentTensor[0, j] > highVal)
                    {
                        highVal = confidence;
                        highInd = j;
                    }
                }
                else
                {
                    highVal = confidence;
                    highInd = j;
                }

                // debugging - sum should = 1 at the end
                sum += confidence;
            }
            test1 = recurrentTensor[0, 0];
            test2 = recurrentTensor[0, 1];
            index = highInd;
            countCNN++;
        }
       
    }

	// algorithm to determine if the user is looking around. Looking and walking generate similar gyro.accelerations, so we
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

	// if the user is walking, moves them in correct direction with varying velocities
	// also sets velocity to 0 if it is determined that the user is no longer walking
	void move ()
	{
		// get the yaw of the subject to allow for movement in the look direction
		yaw = InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.y;
		// convert that value into radians because math uses radians
		rad = yaw * Mathf.Deg2Rad;
		// map that value onto the unit circle to faciliate movement in the look direction
		zVal = Mathf.Cos (rad);
		xVal = Mathf.Sin (rad);

		bool looking = (look (eulerX, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x, 20f) || look (eulerZ, InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z, 20f));

		if (index != standIndex) {
			walking = true;
            velocity = 1.65f;
		} else
        {
            velocity = 0f;
        }
		// if the user isn't looking and is walking then set the velocity based on increasing or decreasing speed
	//	if (!looking && walking) {
	//		if ((display.acceleration.y >= 0.75f || display.acceleration.y <= -0.75f)) {
	//			if (wasTwo) { //we are transitioning from phase 2 to 1
	//				method1StartTimeGrow = Time.time;
	//				wasTwo = false;
	//				wasOne = true;
	//			}
	//		} else {
	//			if (wasOne) {
	//				method1StartTimeDecay = Time.time;
	//				wasOne = false;
	//				wasTwo = true;
	//			}
	//		}
	//		if ((display.acceleration.y >= 0.75f || display.acceleration.y <= -0.75f)) {
	//			velocity = 1.65f - (1.65f - velocity) * Mathf.Exp ((method1StartTimeGrow - Time.time) / 0.2f); //grow
	//		} else {
	//			// if the acceleration values are low, indicates the user is walking slowly, and exponentially decrease the velocity to 0
	//			velocity = 0.0f - (0.0f - velocity) * Mathf.Exp ((method1StartTimeDecay - Time.time) / decayRate); //decay
	//		}
	//	} else {
	//		velocity = 0f;
	//	}

		// multiply intended speed (called velocity) by delta time to get a distance, then multiply that distamce
		// by the unit vector in the look direction to get displacement.
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
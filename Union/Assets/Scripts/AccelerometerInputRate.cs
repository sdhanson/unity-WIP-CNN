using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccelerometerInputRate : MonoBehaviour {

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

    private float velocityMax = 0.0f;
    // set per person
    private float height = 1.75f;

    // variables for determining the step frequency
    // time of the last step
    private float prevTime = 0f;
    // time between recently detected step and last step
    private float stepTime = 0f;
    // y value when the peak of the current step occurred
    private float maxy = -100f;
    // time the peak of the current step occurred + 0.25 to create the time window
    private float maxt = -1f;
    // alert indicates we MIGHT BE currently stepping and should be on the look out for second peak
    private bool alert = false;
    // low = true means we hit the lower peak threshold
    private bool low = false;
    // high = true means we hit the lower peak threshold
    private bool high = false;
    // unset = true means the velocityMax has not yet been set for the detected step
    private bool unset = true;
    // if user hasn't stepped in a while, firstStep is set to true so there is no lag in the starting step
    private bool firstStep = true;
    // set per person
    private float ht = 2.8f;
    // set per person
    private float lt = -2.2f;

    // variable for debugging to see if we are counting the right number of steps
    private float stepCount = 0f;

    // for the interpolation
    float a;
    float b;
    float c;

    OVRDisplay display;

    void Start()
    {
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

    }

    void FixedUpdate() //was previously FixedUpdate()
    {
        // send the current transform data to the server (should probably be wrapped in an if isAndroid but I haven't tested)

        string path = Application.persistentDataPath + "/WIP_looking.txt";


        string appendText = "\n" + String.Format("{0,20} {1,7} {2, 15} {3, 15} {4, 15} {5, 15} {6, 15} {7, 8} {8, 15} {9, 10} {10, 10} {11, 10} {12, 10} {13, 10}",
                                DateTime.Now.ToString(), Time.time,

                                "ACCELERATION_XYZ: ",

                                display.acceleration.x,
                                display.acceleration.y,
                                display.acceleration.z,

                                InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.x,
                                InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.y,
                                InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.z,

                                velocityMax.ToString(),
                                velocity.ToString(),
                                stepTime.ToString(),
                                stepCount.ToString(),
                                maxt.ToString());

        File.AppendAllText(path, appendText);

        // do the movement algorithm, more details inside
        move();


        if (myClient != null)
            myClient.Send(MESSAGE_DATA, new TDMessage(this.transform.localPosition, Camera.main.transform.eulerAngles));
    }

    // sets the velocity max given the step frequency from frequency()
    void setMax()
    {
        // if this is the first step, prevTime can't be used or very low velocityMax, so set to 1.0f
        // if not the first step, use equation to set velocityMax
        if (!firstStep)
        {
            stepTime = Time.time - prevTime;
            float freq = 1.0f / stepTime;
            velocityMax = a * Mathf.Pow(freq, 2.0f) + b * freq + c;
        }
        else
        {
            velocityMax = 0.75f;
            firstStep = false;
        }
        // set time of last step to current time
        prevTime = Time.time;
    }

    // checks to see if we are currently stepping and then calls setMax() to calculate velocity from discovered step frequency
    void frequency()
    {
        // if we aren't on alert (aka if we are currently just in noise territory and haven't hit some peak recently) AND
        // if we aren't in the shadow of a previous step (aka if we aren't a secondary peak) then check to see if we have hit a high or low peak
        // indicating we might be stepping 
        if (!alert && (Time.time > maxt))
        {
            // checking to see if the signal is beyond the allowed window - INDIVIDUALIZED BOUNDARIES
            if ((display.acceleration.y < lt) || (display.acceleration.y > ht))
            {
                alert = true;
                // distingiush if the signal was high or low
                if (display.acceleration.y < lt)
                {
                    low = true;
                    high = false;
                }
                else
                {
                    low = false;
                    high = true;
                }
                // set the time window to be the initial value - time window is checked so residual peaks from the step don't count as 
                // a second distinct step
                maxy = display.acceleration.y;
                maxt = Time.time + 0.25f;
            }
        }
        else if (alert && (Time.time < maxt))
        {
            // if we are in the alert zone and hit the outside of the other threshold,
            // then this is a valid peak, call the set max function to determine new max velocity
            if (unset && ((high && (display.acceleration.y < lt)) || (low && (display.acceleration.y > ht))))
            {
                stepCount++;
                unset = false;
                maxt = Time.time + 0.25f;
                setMax();
            }
        }
        else if (alert && (Time.time >= maxt))
        {
            // if we have left the max time zone, then reset necessary variables
            maxy = -100;
            alert = false;
            low = false;
            high = false;
            unset = true;
        }
    }

    // NOT IMPLEMENTED FOR THE GO YET
    // algorithm to determine if the user is looking around. Looking and walking generate similar gyro.accelerations, so we
    //want to ignore movements that could be spawned from looking around. Makes sure user's head orientation is in certain window
    bool look(double start, double curr, double diff)
    {
        //Determines if the user's current angle (curr) is within the window (start +/- diff)
        //Deals with wrap around values (eulerAngles is in range 0 to 360)
        if ((start + diff) > 360f)
        {
            if (((curr >= 0f) && (curr <= (start + diff - 360f))) || ((((start - diff) <= curr) && (curr <= 360f))))
            {
                return false;
            }
        }
        else if ((start - diff) < 0f)
        {
            if (((0f <= curr) && (curr <= (start + diff))) || (((start - diff + 360f) <= curr) && (curr <= 360f)))
            {
                return false;
            }
        }
        else if (((start + diff) <= curr) && (curr <= (start + diff)))
        {
            return false;
        }
        return true;
    }

    // if the user is walking, moves them in correct direction with varying velocities
    // also sets velocity to 0 if it is determined that the user is no longer walking
    void move()
    {
        // get the yaw of the subject to allow for movement in the look direction
        yaw = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.y;
        // convert that value into radians because math uses radians
        rad = yaw * Mathf.Deg2Rad;
        // map that value onto the unit circle to faciliate movement in the look direction
        zVal = Mathf.Cos(rad);
        xVal = Mathf.Sin(rad);

        bool looking = (look(eulerX, InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.x, 20f) || look(eulerZ, InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.z, 20f));
        frequency();

        // if the user isn't looking then manage their walking - LOOKING ISNT IMPLEMENTED YET SO ALWAYS FALSE ATM
        if (!looking)
        {
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
                velocity = velocityMax - (velocityMax - velocity) * Mathf.Exp((method1StartTimeGrow - Time.time) / 0.2f); //grow
            }
            else
            {
                // if the acceleration values are low, indicates the user is walking slowly, and exponentially decrease the velocity to 0
                velocity = 0.0f - (0.0f - velocity) * Mathf.Exp((method1StartTimeDecay - Time.time) / decayRate); //decay
            }
        }
        else
        {
            velocity = 0f;
        }

        // low velocity means we haven't stepped in a while and reset firstStep to true
        //		if (velocity < 0.2f) {
        //			firstStep = true;
        //		}

        // multiply intended speed (called velocity) by delta time to get a distance, then multiply that distamce
        // by the unit vector in the look direction to get displacement.
        transform.Translate(xVal * velocity * Time.fixedDeltaTime, 0, zVal * velocity * Time.fixedDeltaTime);
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
    public static Vector3 _pos = new Vector3();
    public static Vector3 _euler = new Vector3();

    // Create a client and connect to the server port
    public void SetupClient()
    {
        myClient = new NetworkClient(); //Instantiate the client
        myClient.RegisterHandler(MESSAGE_DATA, DataReceptionHandler); //Register a handler to handle incoming message data
        myClient.RegisterHandler(MsgType.Connect, OnConnected); //Register a handler to handle a connection to the server (will setup important info
        myClient.Connect(SERVER_ADDRESS, SERVER_PORT); //Attempt to connect, this will send a connect request which is good if the OnConnected fires
    }

    // client function to recognized a connection
    public void OnConnected(NetworkMessage netMsg)
    {
        _connectionID = netMsg.conn.connectionId; //Keep connection id, not really neccesary I don't think
    }

    // Clinet function that fires when a disconnect occurs (probably unnecessary
    public void OnDisconnected(NetworkMessage netMsg)
    {
        _connectionID = -1;
    }

    //I actually don't know for sure if this is useful. I believe that this is erroneously put here and was duplicated in TDServer code.
    public void DataReceptionHandler(NetworkMessage _transformData)
    {
        TDMessage transformData = _transformData.ReadMessage<TDMessage>();
        _pos = transformData._pos;
        _euler = transformData._euler;
    }

    #endregion
}

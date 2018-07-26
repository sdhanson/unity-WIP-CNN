using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.XR;
using System.IO;


public class TrainingCNN : MonoBehaviour
{

	Thread t;
	OVRDisplay display;
    int count = 0;
    float prevX = 0;
    float prevY = 0;
    float prevZ = 0;

	// Use this for initialization
	void Start ()
	{

		t = new Thread (output);
		t.Start ();

	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnApplicationQuit ()
	{
		t.Abort ();
	}

	void output ()
	{
		// initialize the oculus go display
		display = new OVRDisplay ();
		string path = Application.persistentDataPath + "/training_CNN.txt";
		float prev = Time.time;

		while (true) {
            Thread.Sleep(10);

            float t = Time.time;
			float now = (float)System.Math.Round(Time.time, 2);
            // This text is always added, making the file longer over time if it is not deleted

            // GET CHANGE IN ANGLES 
            float changeX = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.x - prevX;
            if(changeX > 180 || changeX < -180)
            {
                changeX = 360 - changeX;
            }
            float changeY = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.y - prevY;
            if (changeY > 180 || changeY < -180)
            {
                changeY = 360 - changeY;
            }
            float changeZ = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.z - prevZ;
            if (changeZ > 180 || changeZ < -180)
            {
                changeZ = 360 - changeZ;
            }


            string appendText = "Sara," + "Looking," + Time.time + "," + display.acceleration.x + "," +
			                    display.acceleration.y + "," +
			                    display.acceleration.z + 
                                "," + changeX + "," +
                                changeY + "," +
                                changeZ +
                                "\n";


            File.AppendAllText (path, appendText);

			prev = t;
            count++;
		}
	}
}

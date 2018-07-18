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
//<<<<<<< HEAD
//			string appendText = "New Line: " +
//			                    now + " " +

//			                    display.acceleration.x + " " +
//			                    display.acceleration.y + " " +
//			                    display.acceleration.z + " " +

//			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x + " " +
//			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.y + " " +
//			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z + "\r\n";

//=======
			string appendText = "Priya," + "Standing," + Time.time + "," + display.acceleration.x + "," +
			                    display.acceleration.y + "," +
			                    display.acceleration.z + "\n";
			// ONLY 0 FOR STANDING NEED TO CHANGE TO ANYTHING ELSE
			// DON'T NEED TO CLEAR OUT SCRIPT BETWEEN THE STANDING / SITTING JUST SWITCH THE 0 TO 1
//>>>>>>> 3d5b9fed67cc1ad6dcb4d8e60688cd757d6acf0b
            File.AppendAllText (path, appendText);

			prev = t;
            count++;
		}
	}
}

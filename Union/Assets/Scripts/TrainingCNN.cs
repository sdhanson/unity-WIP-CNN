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
			while (Time.time - prev < 0.019) {
				
			}

            float t = Time.time;
			float now = (float)System.Math.Round(Time.time, 2);
			// This text is always added, making the file longer over time if it is not deleted
			string appendText = "New Line: " +
			                    now + " " +

			                    display.acceleration.x + " " +
			                    display.acceleration.y + " " +
			                    display.acceleration.z + " " +

			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x + " " +
			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.y + " " +
			                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z + "\r\n";

			File.AppendAllText (path, appendText);

			prev = t;
		}
	}
}

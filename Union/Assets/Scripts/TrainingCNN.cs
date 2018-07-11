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
			while (Time.time - prev < 0.01) {
				
			}

			float now = Time.time;
			// This text is always added, making the file longer over time if it is not deleted
			string appendText = "Sara," + "Sitting," + Time.time "," + display.acceleration.x + "," +
			                    display.acceleration.y + "," +
			                    display.acceleration.z + "\n";
			// ONLY 0 FOR STANDING NEED TO CHANGE TO ANYTHING ELSE
			// DON'T NEED TO CLEAR OUT SCRIPT BETWEEN THE STANDING / SITTING JUST SWITCH THE 0 TO 1

			File.AppendAllText (path, appendText);

			prev = now;
		}
	}
}

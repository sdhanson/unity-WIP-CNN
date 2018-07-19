using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.XR;
using System.IO;


public class TrainingStepsCNN : MonoBehaviour
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
        OVRInput.Update();
        string path2 = Application.persistentDataPath + "/training_steps_CNN_controller.txt";


        if (OVRInput.GetDown(OVRInput.Button.One) == true)
        {
            string controllerText = Time.time.ToString() + "\n";
            File.AppendAllText(path2, controllerText);
        }

    }

	void OnApplicationQuit ()
	{
		t.Abort ();
	}

	void output ()
	{
		// initialize the oculus go display
		display = new OVRDisplay ();
		string path = Application.persistentDataPath + "/training_steps_CNN.txt";
		float prev = Time.time;

		while (true) {

            Thread.Sleep(10);

            float t = Time.time;
			float now = (float)System.Math.Round(Time.time, 2);

			string appendText = "Priya," + "Standing," + Time.time + "," + display.acceleration.x + "," +
			                    display.acceleration.y + "," +
			                    display.acceleration.z + "\n";
            File.AppendAllText (path, appendText);
      
			prev = t;
            count++;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.XR;
using System.IO;


public class TrainingCNNGear : MonoBehaviour
{

    Thread t;
    int count = 0;
    float prevX = 0;
    float prevY = 0;
    float prevZ = 0;

    // Use this for initialization
    void Start()
    {
        Input.gyro.enabled = true;
        t = new Thread(output);
        t.Start();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnApplicationQuit()
    {
        t.Abort();
    }

    void output()
    {
        // initialize the oculus go display
        string path = Application.persistentDataPath + "/training_CNN_Gear.txt";
        float prev = Time.time;

        while (true)
        {
            Thread.Sleep(13);

            float t = Time.time;
            float now = (float)System.Math.Round(Time.time, 2);
            // This text is always added, making the file longer over time if it is not deleted

            // GET CHANGE IN ANGLES 
            float changeX = InputTracking.GetLocalRotation(XRNode.Head).eulerAngles.x - prevX;
            if (changeX > 180 || changeX < -180)
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


            string appendText = "Peter," + "Walking," + Time.time + "," + Input.gyro.userAcceleration.x + "," +
                                Input.gyro.userAcceleration.y + "," +
                                Input.gyro.userAcceleration.z +
                                "," + changeX + "," +
                                changeY + "," +
                                changeZ +
                                "\n";


            File.AppendAllText(path, appendText);

            prev = t;
            prevX = Input.gyro.userAcceleration.x;
            prevY = Input.gyro.userAcceleration.y;
            prevZ = Input.gyro.userAcceleration.z;
            count++;
        }
    }
}

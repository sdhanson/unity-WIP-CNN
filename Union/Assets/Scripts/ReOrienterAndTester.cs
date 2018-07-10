using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine.XR;
using System.IO;
using System;
using UnityEngine.UI;

public class ReOrienterAndTester : MonoBehaviour
{

	//this -- used to move the person to the start of a trial
	private GameObject _humanMover;
	//this, used to reset the subject and allow human movement
	private GameObject _maze;
	//this, used to turn the features of the maze on and off
	private GameObject _voronoi;
	//this, used to turn the features of the vornoi ground plane on and off
	private GameObject _textMessage;

	private TrialManager myTrials;
	private NetworkControlCenter networkControlCenter;
	private int _trial = 0;

	public Text _stringMessage;

	void Awake ()
	{
		lastButtonPress = Time.fixedTime + 10; //ensure no button can be pressed for 10 seconds
	}

	// Use this for initialization
	void Start ()
	{

		_humanMover = GameObject.Find ("TestObject");
		_maze = GameObject.Find ("Maze");
		_voronoi = GameObject.Find ("Voronoi");
		_textMessage = GameObject.Find ("Canvas");
		_voronoi.SetActive (true);
		_textMessage.SetActive (false);

		myTrials = new TrialManager ();


		//setup portion
		_maze.SetActive (false);

		//Networking for controls
		networkControlCenter = new NetworkControlCenter ();
		networkControlCenter.Start (this.gameObject, myTrials, true);
	}

	//public const int TRAINING = 0;
	public const int PRETRIAL = 1;
	public const int INTRIAL = 2;
	public const int POSTTRIAL = 3;
	public const int PRETEST = 0;
	private int state = PRETEST;
	private float lastButtonPress = 0;

	// Update is called once per frame
	void Update ()
	{
		networkControlCenter.Update (myTrials.GetOrderIndex ());

		if (state == PRETEST) {
			if (Input.GetMouseButton (0) && Time.fixedTime > lastButtonPress + 1) {
				lastButtonPress = Time.fixedTime;
				ResetPerson ();
				state = PRETRIAL;
			}
		} else if (state == PRETRIAL) {
			if (Input.GetMouseButton (0) && Time.fixedTime > lastButtonPress + 1) {
				if (Mathf.Abs (_humanMover.transform.position.x - myTrials.GetTrial ().startObject.transform.position.x) < 1.2) {
					if (Mathf.Abs (_humanMover.transform.position.z - myTrials.GetTrial ().startObject.transform.position.z) < 1.2) {
						lastButtonPress = Time.fixedTime;
						state = INTRIAL;
						StartTrial ();
					}
				}
			}
		} else if (state == INTRIAL) {
			if (Input.GetMouseButton (0) && Time.fixedTime > lastButtonPress + 1) {
				lastButtonPress = Time.fixedTime;
				state = POSTTRIAL;
				UserEndTrial ();
				_trial++;
			}
			if (Time.fixedTime > lastButtonPress + 3) {
				_textMessage.SetActive (false);
			}
		} else if (state == POSTTRIAL) {
			if (Input.GetMouseButton (0) && Time.fixedTime > lastButtonPress + 1) {
				lastButtonPress = Time.fixedTime;
				state = PRETRIAL;

				myTrials.MoveToNextTrial ();
				ResetPerson ();
				//networkControlCenter.SendStateUpdate ();
				networkControlCenter.SendStateUpdateUDP ();
			}
		}
	}

	void OnApplicationQuit ()
	{
		networkControlCenter.receiveThread.Abort ();
		if (networkControlCenter.clientR != null)
			networkControlCenter.clientR.Close ();
		if (networkControlCenter.clientS != null)
			networkControlCenter.clientS.Close ();
	}

	public void UpdateTrial (string message)
	{
		state = POSTTRIAL;
		//Alert user to resuming testing
		_textMessage.SetActive (true);
		_voronoi.SetActive (true);
		_maze.SetActive (false);
		_stringMessage.text = "Resuming Testing" + message;
		_trial = myTrials.GetOrderIndex () + 1;
	}



	void ResetPerson ()
	{
		_stringMessage.text = "Please go to " + myTrials.GetTrial ().endObject.name;
		//Place this at corresponding waypoint
		transform.position = myTrials.GetTrial ().startPosition;
		//Place test object at (0, eyeHeight, 0)
		float eyeHeight = _humanMover.transform.localPosition.y;
		_humanMover.transform.localPosition = new Vector3 (0f, eyeHeight, 0f);
		//Turn maze on
		_maze.SetActive (true);
		//Turn voronoi off
		_voronoi.SetActive (false);
		_textMessage.SetActive (false);
		//Record time, time, start, end, position, orientation
		LogData ("PreTrialOrientation");
	}

	void StartTrial ()
	{
		//Turn maze off
		_maze.SetActive (false);
		//Turn voronoi on
		_voronoi.SetActive (true);
		_textMessage.SetActive (true);
		//Give user instruction

		//Record time, time, start, end, position, orientation
		LogData ("BeginTrial");
	}

	void UserEndTrial ()
	{
		//Record time, time, start, end, position, orientation
		_textMessage.SetActive (true);
		_stringMessage.text = "Good Job";

		// If we just completed the last test, end
		if (myTrials.GetOrderIndex () >= myTrials.GetMaxOrderIndex ()) {
			_stringMessage.text = "Testing Complete";
		}

		LogData ("EndTrial");
	}

	void LogData (string action)
	{
		string path = Application.persistentDataPath + "/Summary_Data_Test_Phase.txt";

		string appendText = "\n" + DateTime.Now.ToString () + "\t" +
		                    Time.time + "\t" +

		                    _trial + "\t" + action + "\t" +

		                    myTrials.GetTrial ().startObject.name + "\t" + myTrials.GetTrial ().endObject.name + "\t" +

		                    _humanMover.transform.position.x + "\t" +
		                    _humanMover.transform.position.y + "\t" +
		                    _humanMover.transform.position.z + "\t" +

		                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.x + "\t" +
		                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.y + "\t" +
		                    InputTracking.GetLocalRotation (XRNode.Head).eulerAngles.z;


		File.AppendAllText (path, appendText);
		Debug.Log (appendText);
	}
}

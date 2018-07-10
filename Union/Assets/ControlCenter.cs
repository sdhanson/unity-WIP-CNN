using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlCenter : MonoBehaviour {

	private TrialManager myTrials;
	NetworkControlCenter networkControlCenter;
	public GameObject inputField;
	public int currentTrial = 0;

	// Use this for initialization
	void Start () {
		networkControlCenter = new NetworkControlCenter ();
		myTrials = new TrialManager ();
		networkControlCenter.Start (this.gameObject, myTrials, false);
	}
	
	// Update is called once per frame
	void Update () {
		networkControlCenter.Update (currentTrial);
		if (currentTrial != myTrials.GetOrderIndex())
		{
			currentTrial = myTrials.GetOrderIndex ();
			InputField trial = inputField.GetComponent<InputField> ();
			trial.text = myTrials.GetOrderIndex ().ToString ();
		}
	}

	void OnApplicationQuit()
	{
		networkControlCenter.receiveThread.Abort();
		if (networkControlCenter.clientR != null)
			networkControlCenter.clientR.Close ();
		if (networkControlCenter.clientS != null)
			networkControlCenter.clientS.Close ();
	}

	void OnGui() {
		//InputField trial = inputField.GetComponent<InputField> ();
		//trial.text = myTrials.GetOrderIndex ().ToString ();
	}

	public void UpdateTrial()
	{
		InputField trial = inputField.GetComponent<InputField> ();
		myTrials.SetOrderIndex (int.Parse(trial.text));
		//networkControlCenter.SendClientUpdate ();
		networkControlCenter.SendStateUpdateUDP();
	}
		
}

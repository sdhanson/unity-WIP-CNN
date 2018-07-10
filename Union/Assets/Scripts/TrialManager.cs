using UnityEngine;
using System.Collections;
using System;

public class Trial {
	
	public GameObject startObject;
	public Vector3 startPosition;

	public GameObject endObject;

	public Trial(GameObject startObject, Vector3 startPosition, GameObject endObject) {
		
		this.startObject = startObject;
		this.startPosition = startPosition;

		//Debug.Log (startObject);

		this.endObject = endObject;
	}
}
	

public class TrialManager {

	// The 8 types of trials
	private Trial[] trialTypes;

	// Generated with OrderGenerator.txt
	private readonly Int32[] trialOrder = {
		5,4,3,6,0,5,2,6,4,2,7,5,3,2,7,1,0,3,4,0,2,5,6,1,7,6,0,7,5,2,1,4,3,1,0,7,3,1,4,6
	};

	// Index of current path
	private int orderIndex;
	private int maxOrderIndex;

	public TrialManager() {

		trialTypes = new Trial[] {
			new Trial (GameObject.Find ("Guitar"), new Vector3 (3.918f, 0f, -.673f), GameObject.Find ("Snowman")),
			new Trial (GameObject.Find ("Snowman"), new Vector3 (-4.11f, 0f, .09f), GameObject.Find ("Car")),
			new Trial (GameObject.Find ("Car"), new Vector3 (-3.981f, 0f, -1.406f), GameObject.Find ("Well")),
			new Trial (GameObject.Find ("Well"), new Vector3 (3.373f, 0f, 1.817f), GameObject.Find ("Guitar")),

			new Trial (GameObject.Find ("Treasure Chest"), new Vector3 (3.516f, 0f, -4.344f), GameObject.Find ("Chair and Table")),
			new Trial (GameObject.Find ("Chair and Table"), new Vector3 (-.85f, 0f, -4.117f), GameObject.Find ("Phonebooth")),
			new Trial (GameObject.Find ("Phonebooth"), new Vector3 (-4.079f, 0f, 4.14f), GameObject.Find ("Clock")),
			new Trial (GameObject.Find ("Clock"), new Vector3 (.016f, 0f, 2.724f), GameObject.Find ("Treasure Chest"))

		};

		orderIndex = 0;
		maxOrderIndex = trialOrder.Length - 1;
	}
		

	// Get the current trial according to the trial order
	public Trial GetTrial() {
		Int32 curTrialType = trialOrder [orderIndex];
		return trialTypes [curTrialType];
	}

	public Int32 GetMaxOrderIndex() {
		return maxOrderIndex;
	}

	public Int32 GetOrderIndex() {
		return orderIndex;
	}

	public void SetOrderIndex(Int32 newIndex) {
		orderIndex = newIndex;
	}

	public void MoveToNextTrial() {
		if (orderIndex + 1 > maxOrderIndex) {
			throw new ArgumentOutOfRangeException ();
		} else {
			++orderIndex;
		}
	}
}

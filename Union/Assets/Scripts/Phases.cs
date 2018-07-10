using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Stores each phase (scene) of test
public class Phase {

	private string name;
	private int time;

	public Phase (string name = "", int time = 0)
	{
		this.name = name;
		this.time = time;
	}

	public string Name {
		get {
			return this.name;
		}
		set {
			name = value;
		}
	}

	public int Time {
		get {
			return this.time;
		}
		set {
			time = value;
		}
	}
}
	
// Handles moving between scenes
public class Phases {

	enum PhaseNames {Practice1, Rest1, Practice2, Rest2, Learning, Test};
	public enum PhaseTypes {CW4, Resetting};

	private Phase[] myPhases;

	public Phases()
	{
		// ties size of myPhases to size of enum
		myPhases = new Phase[Enum.GetNames(typeof(PhaseNames)).Length];

		// Hardcode times
		myPhases [(int) PhaseNames.Practice1] = new Phase("", 300);
		myPhases [(int) PhaseNames.Rest1] = new Phase("", 6000); //cause why not
		myPhases [(int) PhaseNames.Practice2] = new Phase("", 300);
		myPhases [(int) PhaseNames.Rest2] = new Phase("", 6000); //cause why not
		myPhases [(int) PhaseNames.Learning] = new Phase("", 600);
		myPhases [(int) PhaseNames.Test] = new Phase("", int.MaxValue);
	}

	public Phase[] getPhases() {
		return this.myPhases;
	}

	// Set learning
	// First practice is same type as learning
	public void SetLearning(PhaseTypes t) {
		myPhases [(int) PhaseNames.Rest1].Name = "Rest Phase";
		if (t == PhaseTypes.CW4) {
			myPhases [(int) PhaseNames.Practice1].Name = "CW4 Practice Phase";
			myPhases [(int) PhaseNames.Learning].Name = "CW4 Learning Phase";
		} else {
			myPhases [(int) PhaseNames.Practice1].Name = "Resetting Practice Phase";
			myPhases [(int) PhaseNames.Learning].Name = "Resetting Learning Phase";
		}
	}

	// Set testing
	// Second practice is same type as testing
	public void SetTesting(PhaseTypes t) {
		myPhases [(int) PhaseNames.Rest2].Name = "Rest Phase";
		if (t == PhaseTypes.CW4) {
			myPhases [(int) PhaseNames.Practice2].Name = "CW4 Practice Phase";
			myPhases [(int) PhaseNames.Test].Name = "CW4 Test Phase";
		} else {
			myPhases [(int) PhaseNames.Practice2].Name = "Resetting Practice Phase";
			myPhases [(int) PhaseNames.Test].Name = "Resetting Test Phase";
		}
	}
}
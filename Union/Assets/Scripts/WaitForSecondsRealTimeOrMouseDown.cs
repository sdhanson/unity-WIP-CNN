using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;

// Ends WaitForSecondsRealtime on mouse press
public class WaitForSecondsRealTimeOrMouseDown : WaitForSecondsRealtime {

	private const int doubleClickThreshold = 3;
	private const float doubleClickTime = 2;

	private int curDoubleClick;
	private float firstDoubleClickTime;

	private bool hasDoubleClick;

	private VRInput myVRInput;

	public override bool keepWaiting {
		get { 
			return base.keepWaiting && !hasDoubleClick;
		}
	}

	public WaitForSecondsRealTimeOrMouseDown(float time, VRInput rhsVRInput) 
		: base(time) {

		curDoubleClick = 0;
		firstDoubleClickTime = 0;

		hasDoubleClick = false;

		this.myVRInput = rhsVRInput;
		this.myVRInput.OnDoubleClick += setDoubleClick;
	}

	private void setDoubleClick() {

		float curTime = Time.fixedTime;

		++curDoubleClick;
		Debug.Log (curDoubleClick);

		// If first click, set time
		if (curDoubleClick <= 1) {
			firstDoubleClickTime = curTime;

		// If clicks exceed threshold, move forward
		} else if (curDoubleClick >= doubleClickThreshold) {

			// If clicks were within timespan
			if (curTime - firstDoubleClickTime < doubleClickTime) {
				hasDoubleClick = true;
				myVRInput.OnDoubleClick -= setDoubleClick;

			// Else, reset the count
			} else {
				curDoubleClick = 0;
			}
		};


	}
}

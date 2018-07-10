using UnityEngine;
using System.Collections;

public class EnablerDisabler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (Application.platform != RuntimePlatform.Android) {
			gameObject.SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

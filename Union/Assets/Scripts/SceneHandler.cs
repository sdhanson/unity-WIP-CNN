using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;

public class SceneHandler : MonoBehaviour {
	
	private Queue<Phase> myScenes;
	private VRInput myVRInput;

	void Awake() {
		myScenes = null;
		myVRInput = this.gameObject.GetComponent<VRInput> ();
	}

	public void PlayNext() {
		if (myScenes != null && myScenes.Count > 0) {
			
			Phase temp = myScenes.Dequeue ();

			// Do not end last scene early
			if (myScenes.Count < 1) {
				Debug.Log ("final scene");
				SceneManager.LoadSceneAsync (temp.Name, 
					LoadSceneMode.Additive);
			} else {
				StartCoroutine(SceneTimer (temp.Name, temp.Time));
			}
		}
	}

	private IEnumerator SceneTimer (string sceneName, float duration)
	{
		SceneManager.LoadSceneAsync (sceneName, LoadSceneMode.Additive);
		yield return new WaitForSecondsRealTimeOrMouseDown (duration, 
			myVRInput);
		SceneManager.UnloadSceneAsync (sceneName);

		PlayNext();
	}

	public void PlayScenes(Phase[] phases) {
		myScenes = new Queue<Phase>(phases);
		PlayNext ();
	}

}

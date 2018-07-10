using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using VRStandardAssets.Utils;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	[SerializeField] private VRInput myVRInput;

    public Canvas menuTemplate;

    private Canvas curMenu;
	private GameObject canvasCamera;
	private Button[] buttons;
	private Text prompt;
	private Phases myPhases;

	private SceneHandler mySceneHandler;

	void Awake() {
		myVRInput = this.gameObject.AddComponent<VRInput>();
	}

	// Use this for initialization
	void Start () {
        Selection();
	}

	// Selection screen
	void Selection()
	{
		curMenu = Instantiate (menuTemplate);
		canvasCamera = GameObject.Find ("Camera");
		buttons = curMenu.GetComponentsInChildren<Button> ();
		prompt = GameObject.Find ("Prompt").GetComponent<Text>();
		myPhases = new Phases();

		myVRInput.OnSwipe += ChooseMode;
	}

    // Interprets swipes into actions
	void ChooseMode(VRInput.SwipeDirection swipe) {
        bool invalidSelection = true;
		string mode = "";

        switch(swipe)
        {
		case VRInput.SwipeDirection.UP:
			invalidSelection = false;
			mode = "Selected up";
			//SceneManager.LoadSceneAsync ("CW4 Learning Phase", LoadSceneMode.Additive);
			myPhases.SetLearning (Phases.PhaseTypes.CW4);
			myPhases.SetTesting (Phases.PhaseTypes.CW4);
			break;
		case VRInput.SwipeDirection.DOWN:
			invalidSelection = false;
			mode =  "Selected down";
			myPhases.SetLearning (Phases.PhaseTypes.Resetting);
			myPhases.SetTesting (Phases.PhaseTypes.Resetting);
			break;
		case VRInput.SwipeDirection.FORWARD:
			invalidSelection = false;
			mode = "Selected forward";
			myPhases.SetLearning (Phases.PhaseTypes.CW4);
			myPhases.SetTesting (Phases.PhaseTypes.Resetting);
			break;
		case VRInput.SwipeDirection.BACKWARD:
			invalidSelection = false;
			mode =  "Selected backward";
			myPhases.SetLearning (Phases.PhaseTypes.Resetting);
			myPhases.SetTesting (Phases.PhaseTypes.CW4);
			break;
		default:
			break;
        }

		if (!invalidSelection) {
			myVRInput.OnSwipe -= ChooseMode;
			StartCoroutine(DisplayMode (mode));
        }
	}

	IEnumerator DisplayMode(string s) {
		prompt.text = s;
		foreach( var b in buttons) {
			Destroy(b.gameObject);
		}
		yield return new WaitForSecondsRealtime (5);
		Experiment();
	}

	void Experiment() {
		//Destroy useless elements just in case
		Destroy (curMenu.gameObject);

		mySceneHandler = this.gameObject.AddComponent<SceneHandler> ();

		mySceneHandler.PlayScenes (myPhases.getPhases());
	}


//	#region Experimental Procedure
//
//    // Experiment itself
//	private Coroutine prac1TaskCoroutine;
//	private Coroutine prac2TaskCoroutine;
//	private Coroutine learnTaskCoroutine;
//	private Coroutine testTaskCoroutine;
//	private int currentTask = -1;
//    void Experiment() {
//        // Destroy useless elements just in case
//		canvasCamera.SetActive(false);
//		//Destroy (curMenu.gameObject);
//        //Destroy(myVRInput);
//
//
//		prac1TaskCoroutine = StartCoroutine(SceneTimer(practice1, 0, 0, practice1Time));
//		prac2TaskCoroutine = StartCoroutine(SceneTimer(practice2, 1, practice1Time, practice2Time));
//		learnTaskCoroutine = StartCoroutine (SceneTimer (learning, 2, practice1Time + practice2Time, learningTime));
//		testTaskCoroutine = StartCoroutine (SceneTimer (test, 3, practice1Time + practice2Time + learningTime, 
//			testTime));
//	}
//
//    // Handles timing of scenes
//	IEnumerator SceneTimer (string sceneName, int order, float startTime, float duration)
//	{
//        yield return new WaitForSecondsRealtime(startTime);
//		currentTask = order;
//		SceneManager.LoadSceneAsync (sceneName, LoadSceneMode.Additive);
//        yield return new WaitForSecondsRealtime(duration);
//        SceneManager.UnloadSceneAsync(sceneName);
//	}
//
//	void SkipCurrentTask()
//	{
//		//canvasCamera.SetActive (true);
//		if (prac1TaskCoroutine != null) StopCoroutine (prac1TaskCoroutine);
//		if (prac2TaskCoroutine != null) StopCoroutine (prac2TaskCoroutine);
//		if (learnTaskCoroutine != null) StopCoroutine (learnTaskCoroutine);
//		if (testTaskCoroutine != null) StopCoroutine (testTaskCoroutine);
//		switch(currentTask)
//		{
//		case 0:
//			SceneManager.UnloadSceneAsync (practice1);
//			prac2TaskCoroutine = StartCoroutine (SceneTimer (practice2, 1, 0, practice2Time));
//			learnTaskCoroutine = StartCoroutine (SceneTimer (learning, 2, practice2Time, learningTime));
//			testTaskCoroutine = StartCoroutine (SceneTimer (test, 3, practice2Time + learningTime, testTime));
//			break;
//		case 1:
//			SceneManager.UnloadSceneAsync(practice2);
//			learnTaskCoroutine = StartCoroutine (SceneTimer (learning, 2, 0, learningTime));
//			testTaskCoroutine = StartCoroutine (SceneTimer (test, 3, learningTime, testTime));
//			break;
//		case 2:
//			SceneManager.UnloadSceneAsync(learning);
//			testTaskCoroutine = StartCoroutine (SceneTimer (test, 3, 0, testTime));
//			break;
//		}
//	}

	//#endregion
}
	

using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GazeGestureManager : MonoBehaviour {
	public static GazeGestureManager Instance { get ; private set;}

	public GameObject FocusedObject{ get; private set;}

	GestureRecognizer recongnizer;

    RecordingWav old_RecordingWav;

    void Awake()
	{
        Instance = this;
    }
	// Use this for initialization
	void Start () {
		
		recongnizer = new GestureRecognizer ();
	
		recongnizer.TappedEvent += (source, tapCount, headRay) => {

			if(FocusedObject != null)
			{
				FocusedObject.SendMessageUpwards("OnSelect");
                if (FocusedObject.GetComponent<RecordingWav>())
                {
                    old_RecordingWav = FocusedObject.GetComponent<RecordingWav>();
                    old_RecordingWav.UIPressed();
                }

            }
		};

		recongnizer.StartCapturingGestures ();

	}
	
	// Update is called once per frame
	void Update () {

		GameObject oldFocusObject = FocusedObject;

		var headPostion = Camera.main.transform.position;
		var gazeDirection = Camera.main.transform.forward;

		RaycastHit hit;
		if (Physics.Raycast (headPostion, gazeDirection, out hit)) {

			FocusedObject = hit.collider.gameObject;

        } else {
			FocusedObject = null;
		
		}

		if (FocusedObject != oldFocusObject) {
		 
			recongnizer.CancelGestures ();
			recongnizer.StartCapturingGestures ();
		
		}
			
		
	}
}

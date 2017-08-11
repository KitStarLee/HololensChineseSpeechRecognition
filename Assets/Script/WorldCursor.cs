using UnityEngine;

public class WorldCursor : MonoBehaviour {

	private SpriteRenderer m_Sprite;

    RecordingWav old_RecordingWav;
    // Use this for initialization
    void Start () {
		
		m_Sprite = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {

		var headPostion = Camera.main.transform.position;
		var gazeDirection = Camera.main.transform.forward;

		RaycastHit hit;
		if (Physics.Raycast (headPostion, gazeDirection, out hit)) {

			m_Sprite.enabled = true;

			this.transform.position = hit.point;
			this.transform.rotation = Quaternion.FromToRotation (Vector3.forward, hit.normal);

            if(hit.collider.GetComponent<RecordingWav>())
            {
                old_RecordingWav = hit.collider.GetComponent<RecordingWav>();
                old_RecordingWav.UIHighlighted();
            }
			
		} else {
			m_Sprite.enabled = false;

            if (old_RecordingWav)
           {
               old_RecordingWav.UINormal();
            }
        }
		
	}
}

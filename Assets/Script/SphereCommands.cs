using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCommands : MonoBehaviour {

    private GazeGestureManager mGazeGestureManager;
    // Use this for initialization
    public GameObject qiu_gameobject;
    List<Vector3> qiu_Pos = new List<Vector3>();

    bool isSelect = false;
    AudioSource mAudioSource;

    void Awake()
    {
        if (GetComponent<Animator>())
        {
            GetComponent<Animator>().enabled = false;
            mAudioSource = GetComponent<AudioSource>();
        }
        foreach (Rigidbody r in qiu_gameobject.GetComponentsInChildren<Rigidbody>())
        {
            r.useGravity = false;
            qiu_Pos.Add(r.transform.localPosition);
        }
       
    }
    void Start () {

		

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnSelect()
	{
        isSelect = !isSelect;
        if(isSelect)
        {
            if (GetComponent<Animator>())
            {

                GetComponent<Animator>().enabled = true;
                GetComponent<Animator>().Play("zhuan");
                GetComponent<MeshRenderer>().material.color = new Color(1f,0.35f,0.45f,0.11f);
                mAudioSource.Play();
                foreach(Rigidbody r in qiu_gameobject.GetComponentsInChildren<Rigidbody>())
                {
                    r.useGravity = true;
                }
            }
        }
        else
        {
            if (GetComponent<Animator>())
            {
                mAudioSource.Pause();
                GetComponent<Animator>().enabled = false;
                GetComponent<MeshRenderer>().material.color = new Color(0.35f, 0.5f, 1f, 0.11f);
                foreach (Rigidbody r in qiu_gameobject.GetComponentsInChildren<Rigidbody>())
                {
                    r.useGravity = false;
                }
            }
        }

        
	}
    public void ResetWorld()
    {
       
        for(int i = 0; i < qiu_Pos.Count; i++)
        {
            qiu_gameobject.GetComponentsInChildren<Rigidbody>()[i].useGravity = false;
            qiu_gameobject.GetComponentsInChildren<Rigidbody>()[i].transform.position = qiu_Pos[i];

        }
    }
}

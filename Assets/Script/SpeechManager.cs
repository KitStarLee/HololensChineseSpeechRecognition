using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour {

    KeywordRecognizer m_keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();


    public GameObject FocusedObject { get; private set; }

    // Use this for initialization
    void Start () {

        keywords.Add("Reset world", () =>
        {
            if (GetComponentInChildren<SphereCommands>())
            {
                GetComponentInChildren<SphereCommands>().ResetWorld();
                m_keywordRecognizer.Start();
            }

        });

        keywords.Add("播放音乐", () =>
        {
            if (GetComponentInChildren<SphereCommands>())
            {
                GetComponentInChildren<SphereCommands>().OnSelect();
                m_keywordRecognizer.Start();
            }
            
        });

        string[] array = new string[2];
        keywords.Keys.CopyTo(array,0);
       
        m_keywordRecognizer = new KeywordRecognizer(array);

        m_keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

        m_keywordRecognizer.Start();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            m_keywordRecognizer.Stop();
            keywordAction.Invoke();
        }
    }
}

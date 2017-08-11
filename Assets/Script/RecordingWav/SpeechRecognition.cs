using UnityEngine;
using System.Collections;
using System.Text;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

#if NETFX_CORE  //UWP下编译  
using Windows.Storage;
using StreamWriter = WinRTLegacy.IO.StreamWriter;
using StreamReader = WinRTLegacy.IO.StreamReader;
#else
using StreamWriter = System.IO.StreamWriter;
using StreamReader = System.IO.StreamReader;
#endif

[Serializable]
public class UploadData
{
    public string format;
    public int rate;
    public int channel;
    public string cuid;
    public string lan;
    public string token;
    public string speech;
    public int len;
   
}

[Serializable]
public class BaiDuTokenData
{
    public string access_token;
    public string session_key;
    public string scope;
    public string refresh_token;
    public string session_secret;
    public int expires_in;

}

[Serializable]
public class AcceptanceIdentification
{
    public string err_no;
    public string err_msg;
    public string sn;
    public string[] result;
    public string corpus_no;

}
public class SpeechRecognition : MonoBehaviour {

    public GameObject text_Group_layout;
    private Text[] textgroup;
    public Text show_text;

    private RecordingWav recordingWav;

    string filePath = null;

    string format = "wav";
    int rate = 8000;
    int channel = 1;
    string cuid = "yxh5274";
    string lan = "zh";
    string token;
    string speech;
    int len;

   
    // Use this for initialization
    void Start () {

        filePath = Path.Combine(Application.persistentDataPath, "Microphone.wav");
        recordingWav = GetComponent<RecordingWav>();

        if (text_Group_layout == null)
            return;

         textgroup = text_Group_layout.gameObject.GetComponentsInChildren<Text>();
        
        //UploadAudio();

    }
    public void UploadAudio()
    {
        StartCoroutine(GetIdentifyWords());
    }

    void MatchTheWords(string needTest)
    {
        if (text_Group_layout == null)
            return;

        Debug.Log(needTest);
        for (int i = 0;i < textgroup.Length;i++)
        {
            if(needTest.Contains(textgroup[i].text))
            {
                textgroup[i].color = Color.green;
            }
            else
            {
                textgroup[i].color = Color.red;
            }
           
        }
    }


    string client_id = "cy57gvVAsEpxHpwtBB9zI9np";
    string client_secret = "T0QiUe8DGXUIS0mIABodTXGhVViTtGYr";
    string url_token = "https://openapi.baidu.com/oauth/2.0/token";


    string url_api = "http://vop.baidu.com/server_api";
   
    string post_string;

    IEnumerator GetIdentifyWords()
    {
       // using (StreamReader audio = new StreamReader(filePath, Encoding.UTF8))
        using (StreamReader audio = new StreamReader(recordingWav.memoryStream))
        {
            // StreamReader audio = new StreamReader(recordingWav.memoryStream);
            BinaryReader br = new BinaryReader(audio.BaseStream);

            
            len = (int)audio.BaseStream.Length;
            
            byte[] buffer = new byte[len];
            br.Read(buffer, 0, buffer.Length);
            speech = Convert.ToBase64String(buffer);
            
            /*

            Char[] buffer_char = new Char[len];
            audio.Read(buffer_char, 0, buffer_char.Length);

            byte[] buffer_byte = Encoding.ASCII.GetBytes(buffer_char);

            speech = Convert.ToBase64String(buffer_byte);
            */

            audio.Dispose();
            recordingWav.memoryStream.Dispose();
        }


        #region GetToken
        WWWForm form1 = new WWWForm();
        form1.AddField("grant_type", "client_credentials");
        form1.AddField("client_id", client_id);
        form1.AddField("client_secret", client_secret);

        WWW w1 = new WWW(url_token, form1);
        yield return w1;

        BaiDuTokenData getToken = new BaiDuTokenData();
        JsonUtility.FromJsonOverwrite(w1.text, getToken);
        token = getToken.access_token;
        #endregion
        
        if (token == null || speech == null)
            show_text.text = "参数信息不够";
        yield return 0;

        var request = new UnityWebRequest(url_api, "POST");
        
        UploadData uploadData = new UploadData();
        uploadData.format = format;
        uploadData.rate = rate;
        uploadData.channel = channel;
        uploadData.cuid = cuid;
        uploadData.token = token;
        uploadData.speech = speech;
        uploadData.len = len;
        uploadData.lan = lan;
        
        string data = JsonUtility.ToJson(uploadData);

        //JsonData data = new JsonData();

        Byte[] post_byte = Encoding.UTF8.GetBytes(data);
        

        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(post_byte);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();

        try
        {
            AcceptanceIdentification acceptData = new AcceptanceIdentification();

            JsonUtility.FromJsonOverwrite(request.downloadHandler.text, acceptData);

            show_text.text = request.downloadHandler.text;

            show_text.text = acceptData.result[0].ToString();

            MatchTheWords(acceptData.result[0]);
        }
        catch(Exception ex)
        {
            show_text.text = "error :" + ex;
        }

       

       
            
       
            
        
    }



}

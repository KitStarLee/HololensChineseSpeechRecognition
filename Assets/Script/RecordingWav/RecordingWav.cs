using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.IO;

#if NETFX_CORE  //UWP下编译  
using Windows.Storage;
using StreamWriter = WinRTLegacy.IO.StreamWriter;
using StreamReader = WinRTLegacy.IO.StreamReader;
#else
using StreamWriter = System.IO.StreamWriter;
using StreamReader = System.IO.StreamReader;
#endif

[RequireComponent(typeof(AudioSource))]
public class RecordingWav : MonoBehaviour
{

    //录音图片变红。停止变蓝
    public Image imageButton;
    //按钮点击的动画。
    public Animator recordingButton;

    string filePath = null;
    int audioLength_time;

    private AudioSource m_audioSource;
    private AudioClip m_audioClip;

    public const int SamplingRate = 8000;
    private const int HEADER_SIZE = 44;

    public SpeechRecognition speechRecognition;


    //判断是否录音
    [HideInInspector]
    public bool isRecording = false;

    //文件的大小
    [HideInInspector]
    public Byte[] speech_Byte;

    //用于缓存处理后的录音文件。待用在 SpeechRecognition类中。
    [HideInInspector]
    public MemoryStream memoryStream;

    public MicrophoneManager microphoneManager;


    // Use this for initialization  


    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();

        filePath = Path.Combine(Application.persistentDataPath, "Microphone.wav");
        
    }

    public void StartRecording( bool isRecording)
    {
        if (isRecording)
        {
            Microphone.End(null);
            
            m_audioClip = Microphone.Start(null, false, 60, SamplingRate);

            imageButton.color = Color.red;
        }
        else
        {
            imageButton.color = Color.white;

            audioLength_time = 0;
            int lastPos = Microphone.GetPosition(null);

            if (Microphone.IsRecording(null))
            {
                audioLength_time = lastPos / SamplingRate;
            }
            else
            {
                audioLength_time = 0;
                Debug.Log("error : 录音时间太短");
            }
            Microphone.End(null);

            if (audioLength_time <= 1.0f)
            {
                return;
            }

            SaveWav(filePath, m_audioClip);

            PlayAudioClip();

        }
    }
    public void PlayAudioClip()
    {

        if (m_audioClip.length > 5 && m_audioClip != null)
        {
            if (m_audioSource.isPlaying)
            {
                m_audioSource.Stop();
            }
            m_audioSource.clip = m_audioClip;
            m_audioSource.Play();
        }
    }

    bool SaveWav(string filename, AudioClip clip)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }

        try
        {  
            FileInfo info = new FileInfo(filePath);
            if (!info.Exists)
            {
                 info.Create();
                 
            }
         
            ConvertAndWrite(clip);

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log("error : " + ex);
            return false;
        }

    }

    //从新计算录音文件的长度大小。录音长度公式为 ： SamplingRate * 实际录音时间
    void ConvertAndWrite(AudioClip clip)
    {
        int actual_Length = (audioLength_time + 1) * SamplingRate * 2;
        //防止数据丢失，多加一秒的时间

        float[] samples = new float[actual_Length];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]  

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of  
        //dataSource array because a float converted in Int16 is 2 bytes.  


        int rescaleFactor = 32767; //to convert float to Int16  

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);

            // bytesData = BitConverter.GetBytes(intData[i]);

            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        speech_Byte = null;
        
        //把处理后的二进制文件。通过内存流先缓存下来。
        memoryStream = new MemoryStream(bytesData,false);
       
        
        StartCoroutine(WriteFileStream());
        
    }

    IEnumerator WriteFileStream()
    {
        yield return new WaitForSeconds(1);
        
        speechRecognition.UploadAudio();
    }
    
    public void UIHighlighted()
    {
        recordingButton.Play("Pressed");
    }

    bool isPressed = false;
    public void UIPressed()
    {
        isPressed = !isPressed;

        recordingButton.Play("Highlighted");
        if(microphoneManager)
        {
            if(isPressed)
            {
                microphoneManager.StartRecording();
                imageButton.color = Color.red;
            }
            else
            {
                microphoneManager.StopRecording();
                imageButton.color = Color.white;
            }
           
        }
        else
        {
            StartRecording(isPressed);
        }
       
    }
    public void UINormal()
    {
        recordingButton.Play("Normal");

        if(isPressed)
        {
          //  StartRecording(false);
        }
       
    }
}
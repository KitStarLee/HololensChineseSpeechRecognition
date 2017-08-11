using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

#if NETFX_CORE  //UWP下编译  
using Windows.Storage;
using System.IO;
using XmlReader = WinRTLegacy.Xml.XmlReader;
using XmlWriter = WinRTLegacy.Xml.XmlWriter;
using StreamWriter = WinRTLegacy.IO.StreamWriter;
using StreamReader = WinRTLegacy.IO.StreamReader;
#else
using XmlReader = System.Xml.XmlReader;
using XmlWriter = System.Xml.XmlWriter;
using StreamWriter = System.IO.StreamWriter;
using StreamReader = System.IO.StreamReader;
#endif

[RequireComponent(typeof(AudioSource))]
public class RecordingWav : MonoBehaviour
{
    public Image imageButton;
    public Animator recordingButton;
    public Text show_text;

    string filePath = null;
    int audioLength_time;

    private AudioSource m_audioSource;
    private AudioClip m_audioClip;

    public const int SamplingRate = 8000;
    private const int HEADER_SIZE = 44;

    public SpeechRecognition speechRecognition;

    [HideInInspector]
    public bool isRecording = false;

    [HideInInspector]
    public Byte[] speech_Byte;
    

    // Use this for initialization  


    void Start()
    {

        m_audioSource = GetComponent<AudioSource>();

        filePath = Application.dataPath + "/StreamingAssets" + "/Microphone.wav";

        show_text.text = filePath;


    }

    public void StartRecording( bool isRecording)
    {
        if (isRecording)
        {
            Microphone.End(null);
            foreach (string d in Microphone.devices)
            {
                Debug.Log("Devid :" + d);
            }

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

            if (audioLength_time <= 1.0f) return;

            SaveWav(filePath, m_audioClip);

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
            Debug.Log("Channel :" + m_audioClip.channels + " ;Samle :" + m_audioClip.samples + " ;frequency :" + m_audioClip.frequency + " ;length :" + m_audioClip.length);
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
            Debug.Log("please save the file and delete is later");
        }

        try
        {
            FileStream fileStream;

            FileInfo info = new FileInfo(filePath);
            if (!info.Exists)
            {
                fileStream = info.Create();
            }
            else
            {
                fileStream = info.OpenWrite();
            }
         
          
         ConvertAndWrite(fileStream, clip);

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log("error : " + ex);
            show_text.text = "error : " + ex;
            return false;
        }

    }

    FileStream CreateEmpty(string filePath)
    {
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++)
        {
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }
    void ConvertAndWrite(FileStream fileStream, AudioClip clip)
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


        Stream stream = new MemoryStream(bytesData, true);

        BinaryReader br = new BinaryReader(stream);

        byte[] buffer = new byte[actual_Length];
        br.Read(buffer, 0, buffer.Length);

        string speech = Convert.ToBase64String(buffer);



        //Debug.Log("speech_Byte str1：" + speech);

        Debug.Log("录音时间 ：" + audioLength_time);
        Debug.Log("Byte大小 ：" + bytesData.Length);
        Debug.Log("samples lenght : " + samples.Length);
        Debug.Log("实际录取长度: " + actual_Length);


        StartCoroutine(WriteFileStream(fileStream, bytesData , actual_Length));

      
        
    }

    IEnumerator WriteFileStream(FileStream fileStream, Byte[] bytesData, int actual_Length)
    {
        fileStream.Write(bytesData, 0, actual_Length);
        show_text.text = " ... ";
        yield return new WaitForSeconds(audioLength_time);

      //  fileStream.Close();
        fileStream.Dispose();

        Debug.Log(" OK ");

        speechRecognition.UploadAudio();
    }
    
    public void UIHighlighted()
    {
        recordingButton.Play("Highlighted");
    }

    bool isPressed = false;
    public void UIPressed()
    {
        isPressed = !isPressed;

        recordingButton.Play("Pressed");
        //StartRecording(isPressed);
    }
    public void UINormal()
    {
        recordingButton.Play("Normal");
       // StartRecording(false);
    }
}
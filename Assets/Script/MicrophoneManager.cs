using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;



[RequireComponent(typeof(AudioSource))]
public class MicrophoneManager : MonoBehaviour
{
    [Tooltip("A text area for the recognizer to display the recognized strings.")]
    public Text DictationDisplay;

    private DictationRecognizer dictationRecognizer;

    // 使用这个字符串缓存当前在文本框中显示的文本。.
    private StringBuilder textSoFar;

    // 使用一个空字符串指定默认的麦克风。. 
    private static string deviceName = string.Empty;
    private int samplingRate = 8000;
    private const int messageLength = 10;

    // 当麦克风在启动后录制完毕后，用这个来重置UI。
    private bool hasRecordingStarted;

    public static MicrophoneManager Instance;

    private AudioSource m_audioSource;
    private AudioClip m_audioClip;


    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.loop = false;

        //  Create a new DictationRecognizer and assign it to dictationRecognizer variable.
        dictationRecognizer = new DictationRecognizer(ConfidenceLevel.High);
        
        // 在用户说话时被触发。当识别器监听时，它提供了到目前为止所听到的文本。
        dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;

        // 在用户暂停之后被触发的，通常是在一个句子的结尾。完整的识别字符串在这里返回。
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;

        // 当识别器停止时，该事件将被触发，无论是停止()被调用，超时发生，还是其他错误。
        dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;

        // 当发生错误时，该事件将被触发。
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;

        // 查询缺省麦克风的最大频率。使用'unused'来忽略最小频率。
        int unused;
        Microphone.GetDeviceCaps(deviceName, out unused, out samplingRate);
        
        textSoFar = new StringBuilder();
        
        hasRecordingStarted = false;
        
    }

    float count = 0;
    void Update()
    {
        // 添加条件来检查是否会识别。运行状态
        if (hasRecordingStarted && !Microphone.IsRecording(deviceName) && dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            hasRecordingStarted = false;

            SendMessage("RecordStop");
        }
    }

    /// <summary>
    /// 打开听写识别器并开始从默认的麦克风录制音频。
    /// </summary>
    /// <returns>The audio clip recorded from the microphone.</returns>
    public AudioClip StartRecording()
    {
        // 关闭PhraseRecognitionSystem。这个控制KeywordRecognizers
        PhraseRecognitionSystem.Shutdown();

        // 开始听写识别器
        dictationRecognizer.Start();

        DictationDisplay.text = "语音识别功能开始....";

        // Set the flag that we've started recording.
        hasRecordingStarted = true;

        // Start recording from the microphone for 10 seconds.
        m_audioClip = Microphone.Start(deviceName, false, messageLength, samplingRate);
        return m_audioClip;
    }

    /// <summary>
    /// 结束录音
    /// </summary>
    public void StopRecording()
    {
        // 检查是否听写识别。状态在运行，则关闭
        if (dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            dictationRecognizer.Stop();
        }

        Microphone.End(deviceName); //录音也结束
    }

    /// <summary>
    /// 这个事件在用户说话时被触发。当识别器监听时，它提供了到目前为止所听到的文本。
    /// </summary>
    /// <param name="text">The currently hypothesized recognition.</param>
    private void DictationRecognizer_DictationHypothesis(string text)
    {
        DictationDisplay.text = textSoFar.ToString() + " " + text + "...";
    }

    /// <summary>
    /// 这个事件是在用户暂停之后触发的，通常是在一个句子的结尾。完整的识别字符串在这里返回。
    /// </summary>
    /// <param name="text">The text that was heard by the recognizer.</param>
    /// <param name="confidence">A representation of how confident (rejected, low, medium, high) the recognizer is of this recognition.</param>
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        textSoFar.Append(text + ". ");

        DictationDisplay.text = textSoFar.ToString();
    }

    /// <summary>
    /// 当识别器停止时，该事件将被触发，无论是停止()被调用，超时发生，还是其他错误。
    /// Typically, this will simply return "Complete". In this case, we check to see if the recognizer timed out.
    /// </summary>
    /// <param name="cause">An enumerated reason for the session completing.</param>
    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        if (cause == DictationCompletionCause.TimeoutExceeded)
        {
            Microphone.End(deviceName);

            DictationDisplay.text = "链接超时....";
            SendMessage("ResetAfterTimeout");
        }
    }

    /// <summary>
    /// 当发生错误时
    /// </summary>
    /// <param name="error">The string representation of the error reason.</param>
    /// <param name="hresult">The int representation of the hresult.</param>
    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        // 3.a: Set DictationDisplay text to be the error string
        DictationDisplay.text = error + "\nHRESULT: " + hresult;
    }

    /*

    private IEnumerator RestartSpeechSystem(KeywordManager keywordToStart)
    {
        while (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            yield return null;
        }

        keywordToStart.StartKeywordRecognizer();
    }
    */
}
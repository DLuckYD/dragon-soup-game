using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoTutorialPageUI : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImage;

    [Header("Text")]
    [SerializeField] private TMP_Text tutorialNumberText;
    [SerializeField] private TMP_Text tutorialNameText;
    [SerializeField] private TMP_Text tutorialDescriptionText;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button replayButton;

    private RenderTexture renderTexture;

    public void Initialize(VideoTutorialPage page)
    {
        tutorialNumberText.text = page.tutorialNumber.ToString();
        tutorialNameText.text = page.tutorialName;
        tutorialDescriptionText.text = page.tutorialDescription;
        
        
        videoPlayer.clip = page.video;
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = page.loopVideo;
        videoPlayer.clip = page.video;

        renderTexture = new RenderTexture(1280, 720, 0);

        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        playButton.onClick.AddListener(Play);
        pauseButton.onClick.AddListener(Pause);
        replayButton.onClick.AddListener(Replay);
        
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.frameReady += OnFrameReady;
        videoPlayer.sendFrameReadyEvents = true;

        videoPlayer.Prepare();
    }

    public void Play()
    {
        if (videoPlayer.clip == null)
            return;

        videoPlayer.Play();
    }

    public void Pause()
    {
        videoPlayer.Pause();
    }

    public void Replay()
    {
        if (videoPlayer.clip == null)
            return;

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    public void Stop()
    {
        videoPlayer.Stop();
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(Play);
        pauseButton.onClick.RemoveListener(Pause);
        replayButton.onClick.RemoveListener(Replay);

        videoPlayer.Stop();
        videoPlayer.targetTexture = null;

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
    
    private bool waitingForPreview;

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (source.clip == null)
            return;

        waitingForPreview = true;

        source.frame = 0;
        source.Play();
    }

    private void OnFrameReady(VideoPlayer source, long frameIdx)
    {
        if (!waitingForPreview)
            return;

        waitingForPreview = false;
        source.Pause();
    }
}
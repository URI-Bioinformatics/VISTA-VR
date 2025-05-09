using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject infoButtonObject;
    [SerializeField] private GameObject videoPlayerObject;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Video Settings")]
    [SerializeField] private VideoClip[] videoClips;
    [SerializeField] private bool autoDisplay = false;

    private Camera mainCam;
    private bool isHidden = false;

    void Start()
    {
        mainCam = Camera.main;

        // if (videoClips.Length > 0)
        // {
        //     videoPlayer.clip = videoClips[0];
        //     if (autoDisplay)
        //     {
        //         ToggleVideo(true);
        //         videoPlayer.Play();
        //         Debug.Log("VideoPlayerManager started with autoDisplay enabled.");
        //     }
        //     else
        //     {
        //         Hide();
        //     }
        // }
        // else
        // {
        //     Debug.LogWarning("No video clips assigned to VideoPlayerManager.");
        //     Hide();
        // }
    }

    public void PickVideo(int index)
    {
        if (index < 0 || index >= videoClips.Length)
        {
            Debug.LogWarning("Invalid video index.");
            return;
        }

        videoPlayer.Stop();
        videoPlayer.clip = videoClips[index];
        videoPlayer.targetTexture?.Release();
        videoPlayer.Play();
    }

    public void SetVideoClip(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Invalid video clip.");
            return;
        }

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.targetTexture?.Release();
        videoPlayer.Play();
    }

    public void PlayVideo(bool play)
    {
        if (play)
        {
            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Pause();
        }
    }

    public void ToggleVideo(bool show)
    {
        videoPlayerObject.SetActive(show);

        if (show)
        {
            videoPlayer.targetTexture?.Release();
            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Stop();
        }
    }

    private void HideVideo()
    {
        isHidden = true;
        ToggleVideo(false);
    }

    private void ShowVideo()
    {
        isHidden = false;
        ToggleVideo(autoDisplay);
    }

    private void Hide()
    {
        infoButtonObject.SetActive(false);
        videoPlayerObject.SetActive(false);
        isHidden = true;
    }

    public VideoPlayer GetVideoPlayer()
    {
        return videoPlayer;
    }
}

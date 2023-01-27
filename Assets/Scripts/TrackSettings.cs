using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackSettings : MonoBehaviour
{

    public bool playTrack = true;
    public bool showOrbs = true;
    GameObject pauseButton;
    GameObject playButton;

    void Awake()
    {
        // find mute and unmute objects in children
        playButton = transform.Find("Buttons/PlayButton").gameObject;
        pauseButton = transform.Find("Buttons/PauseButton").gameObject;

        // set listeners for mute and unmute objects
        playButton.GetComponent<Button>().onClick.AddListener(PlayTrack);
        pauseButton.GetComponent<Button>().onClick.AddListener(PauseTrack);

    }

    public void PlayTrack()
    {
        playTrack = true;
        // pause the other playing tracks
        foreach (TrackSettings trackSettings in FindObjectsOfType<TrackSettings>())
        {
            if (trackSettings != this) trackSettings.PauseTrack();
        }
        playButton.SetActive(false);
        pauseButton.SetActive(true);

    }

    public void PauseTrack()
    {
        playTrack = false;
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

}

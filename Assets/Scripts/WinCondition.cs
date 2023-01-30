using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class WinCondition : MonoBehaviour
{

    public List<GameObject> trackOrbs = new();
    public List<GameObject> targetTrackOrbs = new();

    public List<AudioClip> trackAudioClips = new();
    public List<AudioClip> targetTrackAudioClips = new();
    public bool win = false;
    public bool hasWon = false;

    void Start()
    {
        GameObject volumeObject = GameObject.Find("Main Camera/Global Volume");
        Volume volume = volumeObject.GetComponent<Volume>();
        volume.profile.TryGet(out Bloom bloom);
        bloom.intensity.value = 2f;
    }

    void Update()
    {
        if (hasWon) return;
        // get tracksettings for "Track" and "TargetTrack"
        TrackSettings trackSettings = GameObject.Find("GameObjects/Tracks/Track").GetComponent<TrackSettings>();
        TrackSettings targetTrackSettings = GameObject.Find("GameObjects/Tracks/TargetTrack").GetComponent<TrackSettings>();

        trackOrbs = trackSettings.orbs;
        targetTrackOrbs = targetTrackSettings.orbs;
        trackAudioClips = trackSettings.orbAudioClips;
        targetTrackAudioClips = targetTrackSettings.orbAudioClips;

        for (int i = 0; i < targetTrackAudioClips.Count; i++)
        {
            List<AudioClip> offsetList = new();
            offsetList.AddRange(trackAudioClips.GetRange(i, trackAudioClips.Count - i));
            offsetList.AddRange(trackAudioClips.GetRange(0, i));
            Debug.Log("offsetList: " + offsetList);
            if (offsetList.SequenceEqual(targetTrackAudioClips))
            {
                win = true;
                break;
            }
        }

        if (win)
        {
            Debug.Log("You Win!");
            // get the win text
            GameObject winText = GameObject.Find("Canvas/UI/Text/Level Complete");
            winText.SetActive(true);
            // get global volum
            GameObject volumeObject = GameObject.Find("Main Camera/Global Volume");
            Volume volume = volumeObject.GetComponent<Volume>();
            volume.profile.TryGet(out Bloom bloom);
            bloom.intensity.value = 10f;
            hasWon = true;
            GameObject.Find("Canvas/UI/NextLevel").SetActive(true);
        }
    }
}
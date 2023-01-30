using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackSettings : MonoBehaviour
{

    public bool playTrack = true;
    public bool showOrbs = true;

    public List<GameObject> orbs = new();
    public List<AudioClip> orbAudioClips = new();

    void Start()
    {
        UpdateOrbsList();

    }

    void Update()
    {
        // if game object name is "Track" and playTrack is true then update the list
        if (gameObject.name == "Track" && playTrack)
        {
            // for each orb in the list
            UpdateOrbsList();
        }
    }

    void UpdateOrbsList()
    {
        if (orbs.Count == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                orbs.Add(null);
                orbAudioClips.Add(null);
            }
        }
        // get each orb in the track and add it to the list of orbs at position start index
        foreach (Transform orb in transform)
        {
            int index = orb.gameObject.GetComponent<WaypointsTraveler>().StartIndex;
            orbs[index] = orb.gameObject;
            orbAudioClips[index] = orb.gameObject.GetComponent<WaypointsTraveler>().waypointSound;
        }
    }

    public void ResetTrack()
    {
        // find each WaypointsTraveler child component
        foreach (WaypointsTraveler waypointsTraveler in GetComponentsInChildren<WaypointsTraveler>())
        {
            // reset each WaypointsTraveler
            waypointsTraveler.ResetTraveler();
        }
    }

    public void PlayTrack()
    {
        playTrack = true;
        // pause the other playing tracks
        // foreach (TrackSettings trackSettings in FindObjectsOfType<TrackSettings>())
        // {
        //     if (trackSettings != this) trackSettings.PauseTrack();
        // }
    }

    public void PauseTrack()
    {
        playTrack = false;
    }

}

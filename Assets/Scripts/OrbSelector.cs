using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class OrbSelector : MonoBehaviour
{
    public GameObject orbPrefab;
    public AudioClip orbAudioClip;
    public Material orbMaterial;


    public void OnMouseDown()
    {
        GameObject orbGroup = GameObject.Find("GameObjects/Tracks/Track");
        GameObject targetTrack = GameObject.Find("GameObjects/Tracks/TargetTrack");
        targetTrack.GetComponent<TrackSettings>().playTrack = false;


        foreach (Transform orb in orbGroup.transform)
        {
            if (orb.gameObject.GetComponent<WaypointsTraveler>().positionIndex == 0)
            {
                orb.gameObject.GetComponent<MeshRenderer>().material = orbMaterial;
                // if orbAudioClip is null, use None (AudioClip)
                if (orbAudioClip == null) orbAudioClip = (AudioClip)Resources.Load("None");
                orb.gameObject.GetComponent<WaypointsTraveler>().waypointSound = orbAudioClip;
                // break out for loop
                break;
            }
        }
    }

}

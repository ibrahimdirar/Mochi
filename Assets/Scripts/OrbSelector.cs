using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using WaypointsFree;

public class OrbSelector : MonoBehaviour
{
    public GameObject orbPrefab;
    public AudioClip orbAudioClip;
    public Material orbMaterial;


    public void OnMouseDown()
    {
        Debug.Log("SetActiveOrbParamaters");
        // get orb selector manager
        // play orb audio clip at camera position
        // if level is not active

        OrbSelectorManager orbSelectorManager = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>();

        // get selected track
        GameObject selectedTrack = orbSelectorManager.selectedTrack;
        // get spawner child object
        Spawner spawner = selectedTrack.GetComponentInChildren<Spawner>();

        spawner.CreateOrb(orbPrefab, orbMaterial, orbAudioClip);
        // GameObject orb = Instantiate(orbPrefab, spawner.transform.position, Quaternion.identity);

        // // set orb parent to selected track
        // orb.transform.parent = selectedTrack.transform;

        // // set orb waypoints to selected track waypoints
        // orb.GetComponent<WaypointsTraveler>().Waypoints = selectedTrack.GetComponent<WaypointsGroup>();

        // // set orb start index to spawner start index
        // orb.GetComponent<WaypointsTraveler>().StartIndex = spawner.StartAtIndex;

        // // set orb waypoints to selected track waypoints
        // orb.GetComponent<WaypointsTraveler>().Awake();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

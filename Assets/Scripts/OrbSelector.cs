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



    // Start is called before the first frame update
    void Start()
    {
        // get button
        // Button button = GetComponent<Button>();
        // add listener
        GetComponent<Button>().onClick.AddListener(SetActiveOrbParamaters);
        // set color to transparent
        // this.color = new Color(1, 1, 1, 0);
        // set shape of Graphic mesh to a circle

    }


    public void SetActiveOrbParamaters()
    {
        Debug.Log("SetActiveOrbParamaters");
        // get orb selector manager
        // play orb audio clip at camera position
        AudioSource.PlayClipAtPoint(orbAudioClip, Camera.main.transform.position);

        OrbSelectorManager orbSelectorManager = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>();
        // set active orb paramater
        orbSelectorManager.activeOrbParamaters.orbPrefab = orbPrefab;
        orbSelectorManager.activeOrbParamaters.orbAudioClip = orbAudioClip;
        orbSelectorManager.activeOrbParamaters.orbMaterial = orbMaterial;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbManager : MonoBehaviour
{
    public GameObject orbPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // get beat manager
        // get track
        GameObject track = GameObject.Find("GameObjects/Tracks/Track");
        // get waypoints from track
        WaypointsGroup waypoints = track.GetComponent<WaypointsGroup>();

        // for every beat in beats per sound
        for (int i = 0; i < BeatManager.Instance.beatsPerSound; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity) as GameObject;
            orb.transform.parent = track.transform;
            WaypointsTraveler wt = orb.GetComponent<WaypointsTraveler>();
            wt.SetWaypointsGroup(waypoints);
            wt.StartAtIndex(i, true);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}

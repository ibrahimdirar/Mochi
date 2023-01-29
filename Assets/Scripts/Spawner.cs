using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{

    public int StartAtIndex = 0;
    GameObject track;
    public List<GameObject> orbs = new();

    void Awake()
    {
        // set orb count to beat manager beats per sound
        track = transform.parent.gameObject;
        transform.position = track.GetComponent<WaypointsGroup>().waypoints[0].GetPosition();

        // initialise orbs with None (GameObject) for each beat
        for (int i = 0; i < BeatManager.Instance.beatsPerSound; i++)
        {
            orbs.Add(null);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject track = transform.parent.gameObject;
        transform.position = track.GetComponent<WaypointsGroup>().waypoints[0].GetPosition();

    }

    public void CreateOrb(GameObject orbPrefab, Material orbMaterial, AudioClip orbAudioClip)
    {
        Debug.Log("CreateOrb");
        // spawn orb
        GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity);
        Destroy(orbs[BeatManager.Instance.currentBeat]);
        orbs[BeatManager.Instance.currentBeat] = orb;
        orb.GetComponent<WaypointsTraveler>().waypointSound = orbAudioClip;
        orb.GetComponent<MeshRenderer>().material = orbMaterial;
        // find orb group child object from parent
        Transform orbGroupTransform = transform.parent.Find("Orbs");
        GameObject orbGroup;

        // if (orbGroupTransform == null) orbGroup = new GameObject("Orbs");
        // else orbGroup = transform.parent.Find("Orbs").gameObject;
        if (orbGroupTransform == null)
        {
            orbGroup = new GameObject("Orbs");
            orbGroup.transform.SetParent(transform.parent);
        }
        else orbGroup = orbGroupTransform.gameObject;

        // set orb parent to orb group
        orb.transform.SetParent(orbGroup.transform);
        orb.transform.localScale = new Vector3(75, 75, 75);
        orb.GetComponent<WaypointsTraveler>().Waypoints = transform.parent.GetComponent<WaypointsGroup>();
        orb.GetComponent<WaypointsTraveler>().StartIndex = StartAtIndex;
    }

    // Update is called once per frame
    void Update()
    {
        // toggle mesh renderer based on track settings
        // if (trackSettings.showOrbs) GetComponent<MeshRenderer>().enabled = true;
        // else GetComponent<MeshRenderer>().enabled = false;

        GameObject track = transform.parent.gameObject;
        transform.position = track.GetComponent<WaypointsGroup>().waypoints[0].GetPosition();


    }

}
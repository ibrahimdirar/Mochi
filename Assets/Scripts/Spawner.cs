using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WaypointsFree;

public class Spawner : MonoBehaviour
{

    public int orbCount = 1;
    public int StartAtIndex = 0;


    // Start is called before the first frame update
    void Start()
    {

    }

    public void CreateOrb(GameObject orbPrefab, Material orbMaterial, AudioClip orbAudioClip)
    {
        Debug.Log("CreateOrb");
         if (orbCount > 0)
        {
            // spawn orb
            GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity);
            orb.GetComponent<WaypointsTraveler>().waypointSound = orbAudioClip;
            orb.GetComponent<MeshRenderer>().material = orbMaterial;
            // set orb parent to parent of spawner
            orb.transform.parent = transform.parent;
            orb.GetComponent<WaypointsTraveler>().Waypoints = transform.parent.GetComponent<WaypointsGroup>();
            orb.GetComponent<WaypointsTraveler>().StartIndex = StartAtIndex;
            orb.GetComponent<WaypointsTraveler>().Awake();
            orbCount--;
        }
    }

    // Update is called once per frame
    void Update()
    {
    //    set material color to orb selector manager active color
       GetComponent<MeshRenderer>().material = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>().activeOrbParamaters.orbMaterial;

    }

}
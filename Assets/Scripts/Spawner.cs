using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointsFree;

public class Spawner : MonoBehaviour
{

    public GameObject orbPrefab;
    public GameObject orbParent;
    public int orbCount = 1;
    public int StartAtIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnMouseDown()
    {
         if (orbCount > 0)
        {
            // if user clicks on this object
            if(Input.GetMouseButtonDown(0)) 
            {
                // spawn orb
                GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity);
                orb.transform.parent = orbParent.transform;
                orb.GetComponent<WaypointsTraveler>().Waypoints = orbParent.GetComponent<WaypointsGroup>();
                orb.GetComponent<WaypointsTraveler>().StartIndex = StartAtIndex;
                orb.GetComponent<WaypointsTraveler>().Awake();
                orbCount--;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if orbcount greater than 0
       
    }

}
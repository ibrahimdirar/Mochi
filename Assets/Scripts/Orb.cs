using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public TrackSettings trackSettings;

    void Start()
    {
        trackSettings = transform.parent.GetComponent<TrackSettings>();
    }

    void Update()
    {
        // toggle mesh renderer based on track settings
        if (trackSettings.showOrbs) GetComponent<MeshRenderer>().enabled = true;
        else GetComponent<MeshRenderer>().enabled = false;
    }

}

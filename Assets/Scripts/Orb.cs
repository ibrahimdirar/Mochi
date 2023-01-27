using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public TrackSettings trackSettings;

    void Awake(){
    }

    // Start is called before the first frame update
    void Start()
    {
        trackSettings = transform.parent.parent.GetComponent<TrackSettings>();
        
    }

    // Update is called once per frame
    void Update()
    {
        // toggle mesh renderer based on track settings
        if (trackSettings.showOrbs) GetComponent<MeshRenderer>().enabled = true;
        else GetComponent<MeshRenderer>().enabled = false;
    }

    void OnMouseDown() {
        // add one orbcount back to active track spawner
        OrbSelectorManager orbSelectorManager = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>();
        orbSelectorManager.selectedTrack.GetComponentInChildren<Spawner>().orbCount += 1;
        Destroy(gameObject);
    }
}

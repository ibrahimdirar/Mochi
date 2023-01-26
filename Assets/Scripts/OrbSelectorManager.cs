using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSelectorManager : MonoBehaviour
{

    // create a list of orb prefab and audio clip pairs
    [System.Serializable]
    public class OrbParamaters
    {
        public GameObject orbPrefab;
        public AudioClip orbAudioClip;
        public Material orbMaterial;
    }

    // create a list of orb prefab and audio clip pairs
    public List<OrbParamaters> orbParamaters;

    // active OrbParamater
    public OrbParamaters activeOrbParamaters;

    void Start()
    {
        // set active orb paramater
        activeOrbParamaters = orbParamaters[0];
    }


}

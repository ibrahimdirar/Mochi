using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperSelector;

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

    public Material defaultMaterial;
    public GameObject selectedTrack;

    // create a list of orb prefab and audio clip pairs
    public List<OrbParamaters> orbParamaters;


}

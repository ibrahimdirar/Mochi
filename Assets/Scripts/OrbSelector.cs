using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class OrbSelector : MonoBehaviour
{
    public GameObject orbPrefab;
    public AudioClip orbAudioClip;
    public Material orbMaterial;


    public void OnMouseDown()
    {
        OrbSelectorManager orbSelectorManager = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>();

        Spawner spawner = orbSelectorManager.selectedTrack.GetComponentInChildren<Spawner>();
        spawner.CreateOrb(orbPrefab, orbMaterial, orbAudioClip);
    }
}

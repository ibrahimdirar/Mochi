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



    // Start is called before the first frame update
    void Start()
    {
        // get button
        // Button button = GetComponent<Button>();
        // add listener
        this.GetComponent<Button>().onClick.AddListener(SetActiveOrbParamaters);
        // set color to transparent
        // this.color = new Color(1, 1, 1, 0);
        // set shape of Graphic mesh to a circle

    }


    public void SetActiveOrbParamaters()
    {
        Debug.Log("SetActiveOrbParamaters");
        // get orb selector manager
        OrbSelectorManager orbSelectorManager = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>();
        // set active orb paramater
        orbSelectorManager.activeOrbParamaters.orbPrefab = orbPrefab;
        orbSelectorManager.activeOrbParamaters.orbAudioClip = orbAudioClip;
        orbSelectorManager.activeOrbParamaters.orbMaterial = orbMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

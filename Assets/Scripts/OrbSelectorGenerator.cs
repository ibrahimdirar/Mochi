using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class OrbSelectorGenerator : MonoBehaviour
{

    public GameObject orbSelectorPrefab;

    [InspectorButton("GenerateSelectors")]
    public bool generateSelectors;
    
    void GenerateSelectors()
    {
        // get orb selector manager
        OrbSelectorManager orbSelectorManager = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>();
        
        // for each orb paramater in the list create an orb selector
        foreach (OrbSelectorManager.OrbParamaters orbParamater in orbSelectorManager.orbParamaters)
        {
            // create orb selector
            GameObject orbSelector = Instantiate(orbSelectorPrefab);
            // // set orb selector parent with worldPositionStays = false
            orbSelector.transform.SetParent(transform, false);
            // // set orb selector position
            orbSelector.transform.position = transform.position;
            // // set orb selector scale
            orbSelector.transform.localScale = transform.localScale;
            // set orb selector name
            // orbSelector.name = orbParamater.orbPrefab.name;
            // set orb selector material
            orbSelector.GetComponent<MeshRenderer>().material = orbParamater.orbMaterial;
            orbSelector.GetComponent<OrbSelector>().orbPrefab = orbParamater.orbPrefab;
            orbSelector.GetComponent<OrbSelector>().orbAudioClip = orbParamater.orbAudioClip;
            orbSelector.GetComponent<OrbSelector>().orbMaterial = orbParamater.orbMaterial;
        }

    }


}

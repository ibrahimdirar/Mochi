using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtons : MonoBehaviour
{

    GameObject targetTrack;
    GameObject track;

    public int currentSceneIndex;
    public int nextSceneIndex;
    public string nextSceneName;
    void Start()
    {
        targetTrack = GameObject.Find("GameObjects/Tracks/TargetTrack");
        track = GameObject.Find("GameObjects/Tracks/Track");


        // get the current scene index
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // get the next scene index
        nextSceneIndex = currentSceneIndex + 1;
        // get the next scene name
        nextSceneName = SceneManager.GetSceneByBuildIndex(nextSceneIndex).name;

    }

    public void Exit()
    {
        Debug.Log("Level Select Button Pressed");
        // load the level select scene
        SceneManager.LoadScene("LevelSelect");
    }

    public void Reset()
    {
        Debug.Log("Level Select Button Pressed");
        // load this level
        // for every orb in the track, set the material and audio clip to the default
        // get teh default material from orb selector manager
        Material defaultMaterial = GameObject.Find("OrbSelectorManager").GetComponent<OrbSelectorManager>().defaultMaterial;
        foreach (Transform orb in track.transform)
        {
            orb.gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
            orb.gameObject.GetComponent<WaypointsTraveler>().waypointSound = (AudioClip)Resources.Load("None");
        }
        targetTrack.GetComponent<TrackSettings>().playTrack = true;

    }


    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
    }
}

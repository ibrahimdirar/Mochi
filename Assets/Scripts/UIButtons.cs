using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtons : MonoBehaviour
{


    public void Exit()
    {
        Debug.Log("Level Select Button Pressed");
        // load the level select scene
        SceneManager.LoadScene("LevelSelect");
    }

    public void Reveal()
    {
        Debug.Log("reveal Button Pressed");
        // load this level
        GameObject.Find("TargetTrack").GetComponent<TrackSettings>().showOrbs = true;

    }

    public void Reset()
    {
        Debug.Log("Level Select Button Pressed");
        // load this level
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

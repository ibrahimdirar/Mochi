using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{

    public GameObject playButton;
    public GameObject resetButton;
    public GameObject levelSelectButton;

    public bool levelActive = false;


    private static ButtonManager _Instance;
    public static ButtonManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<ButtonManager>();
            }

            return _Instance;
        }
    }

    public void OnPlayPress(){
        // get the game manager
        Debug.Log("Play Button Pressed");
        levelActive = true;
        playButton.SetActive(false);
        resetButton.SetActive(true);
   }

   public void OnResetPress(){
        // GameManager.Instance.ResetLevel(); 
        Debug.Log("Reset Button Pressed");
        // reset the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        levelActive = false;
        resetButton.SetActive(false);   
        playButton.SetActive(true);
   }


   public void OnLevelSelectPress(){
        Debug.Log("Level Select Button Pressed");
        // load the level select scene
        SceneManager.LoadScene("LevelSelect");
   }

  
}



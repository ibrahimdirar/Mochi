using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class Saver : MonoBehaviour {
	
	public int score;//score holder
	public string LevelManagerName = "LevelManager";

	public int curLevel;//loaded from GameManager
	public int curWorld;//loaded from GameManager

	private int NextWorld;
	private int NextLevel;

	//at start of the level read the loaded level
	void Awake()
	{
		curLevel = GameManager.Instance.loadedLevel;
		curWorld = GameManager.Instance.loadedWorld;
	}

	public void SetScore(int scoreAmount)//call this function once the level is completed and pass in this score
	{
		score = scoreAmount;//stores the score
		SaveMyGame();
	}

	void SaveMyGame()
	{
		NextWorld = curWorld + 1;
		NextLevel = curLevel + 1;
		//unlock next world level 1 when current world is already won, and another world exists
		if (curLevel == GameManager.Instance.worldsAndLevels[curWorld-1] && GameManager.Instance.worldsAndLevels.Count > curWorld)
		{
			//Debug.Log("Triggered next world unlock");
			PlayerPrefs.SetInt("Level" + NextWorld + "_1", 1);//unlock next World with level 1
			//save current score if it is higher than the saved score
			if (PlayerPrefs.GetInt ("Level" + curWorld + "_" + curLevel.ToString () + "_score") < score)
			{
				//if so, save
				PlayerPrefs.SetInt ("Level" + curWorld + "_" + curLevel.ToString () + "_score", score);
			}
		}
		//just unlock the next level
		else if (curLevel < GameManager.Instance.worldsAndLevels[curWorld-1])
		{
			//Debug.Log("Triggered next level unlock");
			PlayerPrefs.SetInt("Level" + curWorld + "_" + NextLevel, 1);
			//save current score if it is higher than the saved score
			if (PlayerPrefs.GetInt ("Level" + curWorld + "_" + curLevel.ToString () + "_score") < score)
			{
				//if so, save
				PlayerPrefs.SetInt ("Level" + curWorld + "_" + curLevel.ToString () + "_score", score);
			}
		}
		//if thats the last level
		else
		{
			//Debug.Log("Just Save");
			//save current score if it is higher than the saved score
			if (PlayerPrefs.GetInt ("Level" + curWorld + "_" + curLevel.ToString () + "_score") < score)
			{
				PlayerPrefs.SetInt ("Level" + curWorld + "_" + curLevel.ToString () + "_score", score);
			}
		}
		BackToLevelSelect ();//call next function
	}

	void BackToLevelSelect()
	{
		//In here we just go back to the Level Select Menu
		//Application.LoadLevel (LevelManagerName);//for older versions of unity
		SceneManager.LoadScene(LevelManagerName);//unity 5.3+
	}
}

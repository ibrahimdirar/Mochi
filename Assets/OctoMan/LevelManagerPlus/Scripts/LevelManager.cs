using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	//Drag this on a Empty GameObject
	//Popoluate it with the Spacer GameObject and the Button
	//set The amount of Levels
	//set the 1st one to be unlocked and interactable
	//give all levels a name of the number of the level(1, 2, 3, 4 ... and so on)
	public bool deleteSaveGames=false;
	[System.Serializable]
	public class Level
	{
		public string LevelName;
		private string LevelText;
		public int UnLocked;
		public bool IsInteractable;
		public Sprite LevelSprite;
	}
	[System.Serializable]
	public class World
	{
		public string worldName;
		//private string worldNumber;
		public List<Level> LevelList;
		public Transform Spacer;//create a gameobject in the canvas and drag it into
	}


	private int curLevel = 1;//autocounting levels
	private int curWorld = 0;//autocounting worlds
	public List<World> WorldList;

	public GameObject levelButton;//the button prefab needs to be dragged in
	public GameObject worldButton;
	//public Transform Spacer;//create a gameobject in the canvas and drag it into
	public Transform worldSpacer;

//	public GameObject levelButton;//the button prefab needs to be dragged in
//	public Transform Spacer;//create a gameobject in the canvas and drag it into
//	public List<Level> LevelList;
	//Score - all level use the same score, so don't give to much score in higher levels
	//Can also be changed in the levelManager GameObject
	public int Star1Points = 5000;//the score the player needs to unlock the first star
	public int Star2Points = 10000;//the score the player needs to unlock the second star
	public int Star3Points = 20000;//the score the player needs to unlock the third star

	public Sprite LockedSprite;
	void Start () 
	{
		if (deleteSaveGames)
		{
			DeleteAll();
		}
		else
		{
			UpdatedContentCheck();
			FillList();
			PushData();
		}
	}

	void FillList()
	{
		foreach (var world in WorldList)
		{
			if(WorldList.Count>1)//if you only have one world it's pointless to create a world button for it.
			{
				curWorld++;
				curLevel = 1;//reset levels for each world
				GameObject newWorldButton = Instantiate(worldButton) as GameObject;//create a new world button for every world
				WorldButton Wbutton = newWorldButton.GetComponent<WorldButton>();
				Wbutton.WorldText.text = "World " + curWorld.ToString();//world.worldNumber;

				//make the world buttons work:
				//Cache the current button
				Button b = Wbutton.GetComponent<Button>();
				//cache the corresponding spacer object
				GameObject value = world.Spacer.gameObject;
				//deactivate all spacer gameobjects
				value.SetActive(false);
				//call the addlistener functions
				WorldSwitcher(b, value);

				//add the world button to the world spacer
				newWorldButton.transform.SetParent(worldSpacer, false);
			}
			else
			{
				curWorld++;
			}
				
			foreach(var level in world.LevelList)
			{
				GameObject newbutton = Instantiate(levelButton) as GameObject;//create the button depend on the given prefab
				LevelButton button = newbutton.GetComponent<LevelButton>();//get the levebutton component of the created button
				//button.LevelText.text = level.LevelText;//set the leveltext set in the levelmanager onto the button
				button.LevelText.text = curLevel.ToString();//set the leveltext on the button
				//if the current looped button has a saved value of 1 (is unlocked), then set it to be unlocked and interactable

				if (PlayerPrefs.GetInt("Level" + curWorld.ToString() + "_" + button.LevelText.text) == 1)//Level1_1 == unlocked?
				{
					level.UnLocked = 1;
					level.IsInteractable = true;
				}

				//set unlocked state
				button.unlocked = level.UnLocked;
				//Cache the Button
				Button b = button.GetComponent<Button>();
				//set interactable state
				b.interactable = level.IsInteractable;
				if (level.UnLocked == 1 && level.LevelSprite != null)
				{
					button.LevelImage.sprite = level.LevelSprite;
				}
				else
				{
					button.LevelImage.sprite = LockedSprite;
				}
				//add a listener with a function on it to load the right level when the button is clicked - outsourced because of foreach loop

				string value = ("Level" + curWorld.ToString() + "_" + button.LevelText.text);
				AddListener(b, value);

				//check stars depending on score
				if(PlayerPrefs.GetInt("Level"+ curWorld.ToString() +"_"+ button.LevelText.text + "_score") >= Star1Points)
				{
					button.Star1.SetActive(true);
				}

				if(PlayerPrefs.GetInt("Level"+ curWorld.ToString() +"_"+ button.LevelText.text + "_score") >= Star2Points)
				{
					button.Star2.SetActive(true);
				}

				if(PlayerPrefs.GetInt("Level"+ curWorld.ToString() +"_"+ button.LevelText.text + "_score") >= Star3Points)
				{
					button.Star3.SetActive(true);
				}

				button.name = "Level" + curWorld.ToString() + "_" + button.LevelText.text;
				//push world and level data in the button
				button.myLevel = curLevel;
				button.myWorld = curWorld;
				//set the parent to be the spacer which needs to be in the canvas
				newbutton.transform.SetParent(world.Spacer,false);
				//increase levelnumber automaticaly for the buttons
				curLevel++;
			}
		}

		SaveAll ();//perform a save only the first time the game has been started
		//show the right/last used Spacer
		if (GameManager.Instance.loadedWorld > 0)
		{
			WorldList[GameManager.Instance.loadedWorld - 1].Spacer.gameObject.SetActive(true);
		}
		else
		{
			WorldList[0].Spacer.gameObject.SetActive(true);
		}
	}
//just the first save, save all levels to whatever their states are.
	void SaveAll()
	{
		int worldcounter = 0;//just counting up worlds
		int levelCounter = 0;//just counting up levels in worlds
		if(PlayerPrefs.HasKey("Level1_1"))//if it has been saved already
		{
			return;// don't do anything
		}
		else//if not saved already
		{
			foreach (World world in WorldList)
			{
				levelCounter = 0;//reset levelcounter per world
				worldcounter++;//increment worldcounter per loop
				foreach (Level level in world.LevelList)
				{
					levelCounter++;//increment levelcounter per loop
					//do the actual save so the system knows which are locked and unlocked.
					PlayerPrefs.SetInt("Level" + worldcounter + "_" + levelCounter, level.UnLocked);
				}
			}
		}
	}
//The following function checks, if there is a new world or even new level in the game and updates everything accordingly
	void UpdatedContentCheck()
	{
		int worldcounter = 0;//just counting up worlds
		int levelCounter = 0;//just counting up levels in worlds
		string lastTestedSave = "Level1_1";//just a string holding the key for the last tested savefile

		foreach (World world in WorldList)
		{
			levelCounter = 0;//reset levelcounter per world
			worldcounter++;//increment worldcounter per loop
			foreach (Level level in world.LevelList)
			{
				levelCounter++;//increment levelcounter per loop
				//check for existing keys in savefiles, if it is not there but the last tested one has a score already
				if (!PlayerPrefs.HasKey("Level" + worldcounter.ToString() + "_" + levelCounter.ToString()) && PlayerPrefs.HasKey(lastTestedSave+"_score"))
				{
					//unlock the new & tested level
					PlayerPrefs.SetInt("Level" + worldcounter + "_" + levelCounter,1);
				}
				lastTestedSave = "Level" + worldcounter.ToString() + "_" + levelCounter.ToString();
			}
		}
	}


//if you want to delete all saved values use this function
	public void DeleteAll()
	{
		PlayerPrefs.DeleteAll ();
		Debug.Log("All Savefiles cleared");
	}
//for the level buttons
	void AddListener(Button b,string value)
	{
		b.onClick.AddListener(() => loadLevels(value));
	}
	//thats the function to load the right level once clicked on a level button
	void loadLevels(string value)
	{
		//Application.LoadLevel (value);
		SceneManager.LoadScene(value);
	}

//show the right spacer for the corresponding world
	void WorldSwitcher(Button b, GameObject value)
	{
		b.onClick.AddListener(() => SwitchWorlds(value));
	}

	void SwitchWorlds(GameObject value)
	{
		foreach (var world in WorldList)
		{
			world.Spacer.gameObject.SetActive(false);
		}
		value.SetActive(true);
	}
//Data for the GameManager
	void PushData()
	{
		GameManager.Instance.worldsAndLevels.Clear();
		foreach (var world in WorldList)
		{
			GameManager.Instance.worldsAndLevels.Add(world.LevelList.Count);
		}
	}
}

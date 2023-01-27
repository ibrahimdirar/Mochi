using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SuperSelector;

public class SkillGridEvents : MonoBehaviour
{
	private GridSelector selector;

	public GameObject target;
	public GameObject textBox;

	// Use this for initialization
	private void Start ()
	{
		selector = GetComponent<GridSelector> ();

		// make grid layout calculations - for initial target positioning
		GridLayoutGroup grid = GetComponent<GridLayoutGroup> ();
		grid.CalculateLayoutInputHorizontal ();
		grid.CalculateLayoutInputVertical ();
		grid.SetLayoutHorizontal ();
		grid.SetLayoutVertical ();

		SelectItem ();
	}

	// Update is called once per frame.
	private void Update ()
	{
		// perform item action on space
		if (Input.GetKeyDown (KeyCode.Space))
		{
			ItemAction ();
		}
	}

	// onSelect callback
	public void SelectItem ()
	{
		// set target position to selected item
		GameObject selectedItem = GetComponent<GridSelector> ().GetSelectedItem ();
		target.transform.localPosition = selectedItem.transform.localPosition;

		// play sound effect
		GetComponent<AudioSource> ().Play ();
	}

	// action to perform on selected item when space key is pressed
	public void ItemAction ()
	{
		GameObject selectedItem = selector.GetSelectedItem ();

		// get selected item name
		string itemName = "No Item";
		if (selectedItem.GetComponent<Image> ().sprite != null)
		{	
			itemName = selectedItem.name;
		}

		//display item name in textBox
		textBox.GetComponent<Text> ().text = itemName + " selected.";
	}
}

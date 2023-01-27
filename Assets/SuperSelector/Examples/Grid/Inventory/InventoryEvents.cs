using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SuperSelector;

public class InventoryEvents : MonoBehaviour
{
	private GridSelector selector;

	public GameObject textBox;

	// Use this for initialization
	private void Start ()
	{
		selector = GetComponent<GridSelector> ();
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
		// set selected item (slot) color to green
		GameObject selectedItem = GetComponent<GridSelector> ().GetSelectedItem ();
		selectedItem.GetComponent<Image> ().color = Color.green;
	}

	// onDeselect callback
	public void DeselectItem ()
	{
		// set selected item (slot) color to default
		GameObject selectedItem = GetComponent<GridSelector> ().GetSelectedItem ();
		selectedItem.GetComponent<Image> ().color = Color.white;
	}

	// action to perform on selected item when space key is pressed
	public void ItemAction ()
	{
		GameObject selectedItem = selector.GetSelectedItem ();

		// get inventory item name
		string itemName = "No Item";
		if (selectedItem.transform.childCount != 0)
		{	
			GameObject inventoryItem = selectedItem.transform.GetChild (0).gameObject;
			itemName = inventoryItem.name;
		}

		//display item name in textBox
		textBox.GetComponent<Text> ().text = itemName + " selected.";
	}
}

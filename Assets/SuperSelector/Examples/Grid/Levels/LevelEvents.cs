using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SuperSelector;

public class LevelEvents : MonoBehaviour
{
	private GridSelector selector;

	private Sprite defaultSprite;
	public Sprite sprite;

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
		// get selected item's image
		GameObject selectedItem = GetComponent<GridSelector> ().GetSelectedItem ();
		Image itemImage = selectedItem.GetComponent<Image> ();

		// store selected item's sprite
		defaultSprite = itemImage.sprite;
		// set selected sprite
		itemImage.sprite = sprite;
	}

	// onDeselect callback
	public void DeselectItem ()
	{
		// reset selected item's sprite
		GameObject selectedItem = GetComponent<GridSelector> ().GetSelectedItem ();
		selectedItem.GetComponent<Image> ().sprite = defaultSprite;
	}

	// action to perform on selected item when space key is pressed
	public void ItemAction ()
	{
		// get selected item's animator
		GameObject selectedItem = selector.GetSelectedItem ();
		Animator animator = selectedItem.GetComponent<Animator> ();

		// play selection animation
		animator.SetTrigger ("Click");
	}
}

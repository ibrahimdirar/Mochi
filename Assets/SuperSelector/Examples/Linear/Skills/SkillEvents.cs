using UnityEngine;
using System.Collections;
using SuperSelector;

public class SkillEvents : MonoBehaviour
{
	private LinearSelector selector;

	// Use this for initialization
	private void Start () 
	{
		selector = GetComponent<LinearSelector> ();
	}

	public void SelectSkill ()
	{
		// Get selected item
		GameObject selectedItem = GetComponent<LinearSelector> ().GetSelectedItem ();
		SpriteRenderer selectedRenderer = selectedItem.GetComponent<SpriteRenderer> ();

		// set selected item's alpha to 100%
		Color selectedColor = selectedRenderer.color;
		selectedColor.a = 1f;
		selectedRenderer.color = selectedColor;
	}

	public void DeselectSkill ()
	{
		// Get selected item
		GameObject selectedItem = selector.GetSelectedItem ();
		SpriteRenderer selectedRenderer = selectedItem.GetComponent<SpriteRenderer> ();

		// set selected item's alpha to 50%
		Color selectedColor = selectedRenderer.color;
		selectedColor.a = 0.5f;
		selectedRenderer.color = selectedColor;
	}
}

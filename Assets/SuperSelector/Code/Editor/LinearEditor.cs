using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using System.Collections;
using SuperSelector;

[CustomEditor(typeof(LinearSelector))]
public class LinearEditor : Editor 
{
	// Input Options
	SerializedProperty mouse;
	SerializedProperty touch;
	SerializedProperty controller;
	
	// Selector Options
	SerializedProperty is2D;
	SerializedProperty selectionAxis;
	SerializedProperty flippedControls;
	SerializedProperty spacing;
	SerializedProperty selectorSpeed;
	SerializedProperty loopItems;
	SerializedProperty slideItems;
	SerializedProperty scrollOnHold;
	SerializedProperty visibility;
	SerializedProperty visibilityAnchor;
	SerializedProperty selectedItemIndex;
	SerializedProperty rotateHorizontally;
	SerializedProperty rotateVertically;
	
	// Selected Item Attributes
	SerializedProperty maxScale;
	SerializedProperty minScale;
	
	// Unity Events
	SerializedProperty onSelect;
	SerializedProperty onDeselect;
	
	private void OnEnable ()
	{
		// create canvas parent if not already created
		LinearSelector linearSelector = (LinearSelector)target;
		if (linearSelector.Is2D () && linearSelector.transform.parent == null && linearSelector.gameObject.activeInHierarchy)
		{
			// create canvas
			GameObject canvas = new GameObject ("Canvas", typeof(RectTransform));
			canvas.AddComponent<Canvas> ();
			canvas.AddComponent<CanvasScaler> ();
			canvas.AddComponent<GraphicRaycaster> ();
			canvas.GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.layer = 5; // UI Layer
			linearSelector.transform.SetParent (canvas.transform);
			
			// create event system
			if (GameObject.FindObjectOfType<EventSystem> () == null)
			{
				GameObject eventSystem = new GameObject ("EventSystem", typeof (EventSystem));
				eventSystem.AddComponent<StandaloneInputModule> ();
			}
		}

		// input options
		mouse = serializedObject.FindProperty ("mouse");
		touch = serializedObject.FindProperty ("touch");
		controller = serializedObject.FindProperty ("controller");
		
		// selector options
		is2D = serializedObject.FindProperty ("is2D");
		selectionAxis = serializedObject.FindProperty ("selectionAxis");
		flippedControls = serializedObject.FindProperty ("flippedControls");
		spacing = serializedObject.FindProperty ("spacing");
		selectorSpeed = serializedObject.FindProperty ("selectorSpeed");
		loopItems = serializedObject.FindProperty ("loopItems");
		slideItems = serializedObject.FindProperty ("slideItems");
		scrollOnHold = serializedObject.FindProperty ("scrollOnHold");
		visibility = serializedObject.FindProperty ("visibility");
		visibilityAnchor = serializedObject.FindProperty ("visibilityAnchor");
		selectedItemIndex = serializedObject.FindProperty ("selectedItemIndex");
		rotateHorizontally = serializedObject.FindProperty ("rotateHorizontally");
		rotateVertically = serializedObject.FindProperty ("rotateVertically");
		
		// selected item attributes
		maxScale = serializedObject.FindProperty ("maxScale");
		minScale = serializedObject.FindProperty ("minScale");
		
		// unity events
		onSelect = serializedObject.FindProperty ("onSelect");
		onDeselect = serializedObject.FindProperty ("onDeselect");
	}
	
	public override void OnInspectorGUI ()
	{
		serializedObject.UpdateIfRequiredOrScript ();

		DrawCustomInspector ();

		serializedObject.ApplyModifiedProperties ();
	}
	
	private void DrawCustomInspector ()
	{
		EditorGUILayout.Separator ();
		
		// input options section
		EditorGUILayout.LabelField ("Input Options:", EditorStyles.boldLabel);
		mouse.boolValue = EditorGUILayout.Toggle ("Mouse", mouse.boolValue);
		touch.boolValue = EditorGUILayout.Toggle ("Touch", touch.boolValue);
		controller.boolValue = EditorGUILayout.Toggle ("Controller/Keyboard", controller.boolValue);
		
		EditorGUILayout.Separator ();
		
		// selector options section
		EditorGUILayout.LabelField ("Selector Options:", EditorStyles.boldLabel);
		is2D.boolValue = EditorGUILayout.Toggle ("2D", is2D.boolValue);
		selectionAxis.enumValueIndex = EditorGUILayout.Popup ("Selection Axis", selectionAxis.enumValueIndex, selectionAxis.enumDisplayNames);
		flippedControls.boolValue = EditorGUILayout.Toggle ("Flipped Controls", flippedControls.boolValue);
		spacing.floatValue = EditorGUILayout.FloatField ("Spacing", spacing.floatValue);
		int speedMax = !is2D.boolValue ? 10 : 100;
		selectorSpeed.floatValue = Mathf.Round (EditorGUILayout.Slider ("Selector Speed", Mathf.Round(selectorSpeed.floatValue), 1, speedMax));
		loopItems.boolValue = EditorGUILayout.Toggle ("Loop Items", loopItems.boolValue);
		slideItems.boolValue = EditorGUILayout.Toggle ("Slide Items", slideItems.boolValue);
		scrollOnHold.boolValue = EditorGUILayout.Toggle ("Scroll On Hold", scrollOnHold.boolValue);

		int itemCount = GetItemCount ();
		visibility.intValue = EditorGUILayout.IntSlider ("Visibility", visibility.intValue, 0, itemCount - 1);
		visibilityAnchor.enumValueIndex = EditorGUILayout.Popup ("Visibility Anchor", visibilityAnchor.enumValueIndex, 
			visibilityAnchor.enumDisplayNames);

		rotateHorizontally.floatValue = EditorGUILayout.FloatField ("Rotate Horizontally", rotateHorizontally.floatValue);
		rotateVertically.floatValue = EditorGUILayout.FloatField ("Rotate Vertically", rotateVertically.floatValue);
		
		EditorGUILayout.Separator ();
		
		// selected item attributes
		EditorGUILayout.LabelField ("Selected Item Attributes:", EditorStyles.boldLabel);
		selectedItemIndex.intValue = EditorGUILayout.IntField ("Selected Item Index", Mathf.Clamp (selectedItemIndex.intValue, 0, itemCount - 1));
		maxScale.floatValue = EditorGUILayout.FloatField ("Max Scale", maxScale.floatValue);
		minScale.floatValue = EditorGUILayout.FloatField ("Min Scale", minScale.floatValue);
		
		EditorGUILayout.Separator ();
		
		// events
		EditorGUILayout.LabelField ("Events:", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField (onSelect);
		EditorGUILayout.PropertyField (onDeselect);
	}

	private int GetItemCount ()
	{
		LinearSelector selector = (LinearSelector)target;
		Transform itemsTransform = selector.transform.Find ("Items");
		int itemCount = 0;
		if (itemsTransform == null)
		{
			Debug.LogError ("The selector must have a child named `Items`.");
		}
		else
		{
			itemCount = itemsTransform.childCount;
		}
		return itemCount;
	}
}
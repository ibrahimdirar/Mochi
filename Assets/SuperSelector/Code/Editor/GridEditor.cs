using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using System.Collections;
using SuperSelector;

[CustomEditor(typeof(GridSelector))]
public class GridEditor : Editor 
{
	GridSelector gridSelector;

	// Input Options
	SerializedProperty mouse;
	SerializedProperty touch;
	SerializedProperty controller;

	// Selector Options
	SerializedProperty flippedControls;
	SerializedProperty spacing;
	SerializedProperty selectorSpeed;
	SerializedProperty loopItems;
	SerializedProperty wrapRows;
	SerializedProperty wrapColumns;
	SerializedProperty slideItems;
	SerializedProperty scrollOnHold;
	SerializedProperty visibilityAnchorX;
	SerializedProperty visibilityX;
	SerializedProperty visibilityAnchorY;
	SerializedProperty visibilityY;
	SerializedProperty selectedItemIndex;

	// Selected Item Attributes
	SerializedProperty maxScale;
	SerializedProperty minScale;

	// Unity Events
	SerializedProperty onSelect;
	SerializedProperty onDeselect;

	// columns
	SerializedProperty columns;

	// foldout variables
	bool displayLooping;
	bool displayVisibility;

	private void OnEnable ()
	{
		// create canvas parent if not already created
		gridSelector = (GridSelector)target;
		if (gridSelector.transform.parent == null && gridSelector.gameObject.activeInHierarchy)
		{
			// create canvas
			GameObject canvas = new GameObject ("Canvas", typeof(RectTransform));
			canvas.AddComponent<Canvas> ();
			canvas.AddComponent<CanvasScaler> ();
			canvas.AddComponent<GraphicRaycaster> ();
			canvas.GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.layer = 5; // UI Layer
			gridSelector.transform.SetParent (canvas.transform);

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
		flippedControls = serializedObject.FindProperty ("flippedControls");
		spacing = serializedObject.FindProperty ("spacing");
		selectorSpeed = serializedObject.FindProperty ("selectorSpeed");
		loopItems = serializedObject.FindProperty ("loopItems");
		wrapRows = serializedObject.FindProperty ("wrapRows");
		wrapColumns = serializedObject.FindProperty ("wrapColumns");
		slideItems = serializedObject.FindProperty ("slideItems");
		scrollOnHold = serializedObject.FindProperty ("scrollOnHold");
		visibilityAnchorX = serializedObject.FindProperty ("visibilityAnchorX");
		visibilityX = serializedObject.FindProperty ("visibilityX");
		visibilityAnchorY = serializedObject.FindProperty ("visibilityAnchorY");
		visibilityY = serializedObject.FindProperty ("visibilityY");
		selectedItemIndex = serializedObject.FindProperty ("selectedItemIndex");

		// selected item attributes
		maxScale = serializedObject.FindProperty ("maxScale");
		minScale = serializedObject.FindProperty ("minScale");

		// unity events
		onSelect = serializedObject.FindProperty ("onSelect");
		onDeselect = serializedObject.FindProperty ("onDeselect");

		// columns
		columns = serializedObject.FindProperty ("columns");

		// foldout variables
		displayLooping = false;
		displayVisibility = false;
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
		flippedControls.boolValue = EditorGUILayout.Toggle ("Flipped Controls", flippedControls.boolValue);
		columns.intValue = EditorGUILayout.IntField ("Columns", columns.intValue);
		spacing.floatValue = EditorGUILayout.FloatField ("Spacing", spacing.floatValue);
		selectorSpeed.floatValue = Mathf.Round (EditorGUILayout.Slider ("Selector Speed", Mathf.Round(selectorSpeed.floatValue), 1, 10));

			// looping items foldout
		displayLooping = EditorGUILayout.Foldout (displayLooping, "Looping");
		if (displayLooping)
		{
			if (Selection.activeTransform)
			{
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Space (20);
				EditorGUILayout.BeginVertical ();
				loopItems.boolValue = EditorGUILayout.Toggle ("Loop Items", loopItems.boolValue);
				wrapRows.boolValue = EditorGUILayout.Toggle ("Loop to Next Row", wrapRows.boolValue);
				wrapColumns.boolValue = EditorGUILayout.Toggle ("Loop to Next Column", wrapColumns.boolValue);
				// looping to next row or column requires loopItems to be true and slideItems to be false
				if (wrapRows.boolValue || wrapColumns.boolValue) loopItems.boolValue = true;
				EditorGUILayout.EndVertical ();
				EditorGUILayout.EndHorizontal ();
			}
		}
		if (!Selection.activeTransform)
		{
			displayLooping = false;
		}

		slideItems.boolValue = EditorGUILayout.Toggle ("Slide Items", slideItems.boolValue);
		scrollOnHold.boolValue = EditorGUILayout.Toggle ("Scroll On Hold", scrollOnHold.boolValue);

			// visibility foldout
		int itemCount = GetItemCount ();
		displayVisibility = EditorGUILayout.Foldout (displayVisibility, "Visibility");
		if (displayVisibility)
		{
			if (Selection.activeTransform)
			{
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Space (20);
				EditorGUILayout.BeginVertical ();
				visibilityX.intValue = EditorGUILayout.IntSlider ("Horizontal Visibility", visibilityX.intValue,
					0, columns.intValue - 1);
				visibilityAnchorX.enumValueIndex = EditorGUILayout.Popup ("Horizontal Visibility Anchor", 
					visibilityAnchorX.enumValueIndex, visibilityAnchorX.enumDisplayNames);
				visibilityY.intValue = EditorGUILayout.IntSlider ("Vertical Visibility", visibilityY.intValue,
					0, Mathf.CeilToInt (itemCount / (float)columns.intValue) - 1);
				visibilityAnchorY.enumValueIndex = EditorGUILayout.Popup ("Vertical Visibility Anchor",
					visibilityAnchorY.enumValueIndex, visibilityAnchorY.enumDisplayNames);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.EndHorizontal ();
			}
		}
		if (!Selection.activeTransform)
		{
			displayVisibility = false;
		}

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
		GridSelector selector = (GridSelector)target;
		Transform itemsTransform = selector.transform;
		int itemCount = 0;
		if (itemsTransform != null)
		{
			itemCount = itemsTransform.childCount;
		}
		return itemCount;
	}
}

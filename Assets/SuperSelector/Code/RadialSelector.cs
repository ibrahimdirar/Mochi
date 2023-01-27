using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SuperSelector
{
	public class RadialSelector : Selector
	{
		/// <summary>The radius of the circle.</summary>
		[SerializeField]
		private float radius = 0;

		/// <summary>The center position of the circle.</summary>
		private Vector3 circleCenter;

		/// <summary>Direction of the visibility anchor.</summary>
		private enum eVisibilityAnchor {Clockwise, CounterClockwise, None}

		/// <summary>Anchors the visibility in the specified direction.</summary>
		[SerializeField]
		private eVisibilityAnchor visibilityAnchor = eVisibilityAnchor.None;


		/*---------------------------------------------------------*/
		/* ------------------- PRIVATE METHODS ------------------- */
		/*---------------------------------------------------------*/

		private void Awake ()
		{
			// cache the selector angle
			angle = GetSelectorAngle ();

			// initialize items list
			InitItems ();

			// return if no items
			if (items.Count == 0) return;

			// set selected item
			selectedItem = items.ElementAt (selectedItemIndex);

			// set menu position
			InitMenuPosition ();
			
			// set tracker position
			trackerPosition = selectedItem.transform.localPosition;
			
			// set visibility
			if (slideItems)
			{
				UpdateVisibility ();
			}
		}

		private void OnEnable ()
		{
			// add callbacks to events
			onSelect.AddListener (OnSelect);
			onDeselect.AddListener(OnDeselect);
		}
		
		private void OnDisable ()
		{
			// remove callbacks from events
			onSelect.RemoveListener (OnSelect);
			onDeselect.RemoveListener (OnDeselect);
		}

		// Use this for initialization
		private void Start ()
		{
			// trigger select event for first selected item
			onSelect.Invoke ();
		}

		/// <summary>
		/// Calculates the new position for an item in the circle.
		/// </summary>
		/// <returns>The new position.</returns>
		/// <param name="itemPosition">Item's original position.</param>
		/// <param name="distance">Distance to move around the circle (in degrees).</param>
		/// <param name="direction">Direction (-1 = forward; 1 = backward).</param> 
		private Vector3 CalculatePosition (Vector3 itemPosition, float distance, int direction)
		{
			// use rodrigues rotation formula to rotate around the center axis
			Vector3 e = SphericalToCartesian (radius, Mathf.Deg2Rad * rotateHorizontally, Mathf.Deg2Rad * rotateVertically).normalized; // axis vector
			Vector3 v = itemPosition - circleCenter; // rotation vector
			float theta = Mathf.Deg2Rad * (distance * direction);

			Vector3 newPos = Mathf.Cos (theta) * v + 
				Mathf.Sin (theta) * Vector3.Cross (e, v) + 
					(1 - Mathf.Cos (theta)) * Vector3.Dot (e, v) * e;

			return newPos + circleCenter;
		}

		/// <summary>
		/// Converts spherical coordinates to cartesian coordinates.
		/// credit: Morten Nobel-Jørgensen
		/// </summary>
		/// <returns>The cartesian coordinates.</returns>
		/// <param name="r">Radius.</param>
		/// <param name="polar">Polar angle in radians.</param>
		/// <param name="elevation">Elevation angle in radians.</param>
		private static Vector3 SphericalToCartesian(float r, float polar, float elevation)
		{
			Vector3 cartesian;
			cartesian.z = r * Mathf.Sin (polar) * Mathf.Cos (elevation);
			cartesian.x = r * Mathf.Cos (polar) * Mathf.Cos (elevation);
			cartesian.y = r * Mathf.Sin (elevation);
			return cartesian;
		}

		/// <summary>
		/// Finds a vector perpendicular to the parameterized one.
		/// </summary>
		/// <returns>The perpendicular vector.</returns>
		/// <param name="vector">Vector.</param>
		private static Vector3 FindPerpendicular (Vector3 vector)
		{
			Vector3 perpendicular = Vector3.zero;
			float dotProduct = Vector3.Dot (Vector3.up, vector);
			// make sure vector is not parallel to up vector
			Vector3 leftHandVector = (System.Math.Round (dotProduct,3) != 1) ? Vector3.up : Vector3.left;
			perpendicular = Vector3.Cross (leftHandVector, vector);

			return perpendicular;
		}

		/// <summary>
		/// Calculates the angle between two points on a circle.
		/// </summary>
		/// <returns>The angle in radians.</returns>
		/// <param name="p1">Point1.</param>
		/// <param name="p2">Point2.</param>
		/// <param name="r">Radius.</param>
		private static float CalculateAngle (Vector3 p1, Vector3 p2, float r)
		{
			// find the sides of the triangle
			float a, b, c;
			a = c = r;
			b = (p2 - p1).magnitude;
			
			// use law of cosines to find the angle
			float rad = Mathf.Clamp (((b * b) - (a * a) - (c * c)) / (-2 * a * c), -1, 1);
			return Mathf.Acos (rad);
		}

		/// <summary>
		/// Calculates the angle between two points (p1 and p2 with respect to p3).
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="p1">Point1.</param>
		/// <param name="p2">Point2.</param>
		/// <param name="p3">Point3.</param>
		private static float CalculateAngle (Vector3 p1, Vector3 p2, Vector3 p3)
		{
			// find the sides of the triangle
			float a, b, c;
			a = (p1 - p3).magnitude;
			b = (p2 - p1).magnitude;
			c = (p2 - p3).magnitude;
			
			// use law of cosines to find the angle
			float rad = Mathf.Clamp (((b * b) - (a * a) - (c * c)) / (-2 * a * c), -1, 1);
			return Mathf.Acos (rad);
		}

		// Update is called once per frame.
		private void Update ()
		{
			// mouse or touch input
			if (mouse || touch)
			{
				// DRAGGING
				if (Input.GetMouseButtonDown (0))
				{
					// cache mouse position for mouse dragging and item scrolling purposes
					mousePosition = Input.mousePosition;
				}
				
				if (!Input.GetMouseButton (0))
				{
					// enable mouse/touch
					touchDisabled = false;
				}
				else if (!touchDisabled)
				{
					// get drag vector
					Vector3 dragVector = Input.mousePosition - mousePosition;
					if (dragVector.sqrMagnitude > 0)
					{
						// compare drag direction to selected axis
						if (GetDragAxis (dragVector).Equals (selectionAxis))
						{
							// briefly disable mouse/touch selection to prevent unintended autoscrolling
							touchDisabled = true;
							
							// get drag direction
							bool forward = GetDragDirection (dragVector).Equals (eDirection.Forward);
							forward = flippedControls ? !forward : forward;

							// go to next item
							StartCoroutine (NextItem (forward));
							
							// start coroutine for autoscrolling
							if (scrollOnHold)
							{
								StartCoroutine (ScrollOnHold (forward));
							}
						}
						else
						{
							// wrong axis, disable mouse until next mouse up
							touchDisabled = true;
						}
					}
				}

				// SCROLLING
				if (Input.mouseScrollDelta.y == 0)
				{
					// enable mouse
					mouseDisabled = false;
				}
				else if (!mouseDisabled)
				{
					// get scroll vector
					Vector3 scrollVector = Input.mouseScrollDelta;

					if (scrollVector.y != 0)
					{
						// briefly disable mouse selection to prevent unintended autoscrolling
						mouseDisabled = true;

						// get drag direction
						bool forward = GetDragDirection(scrollVector).Equals(eDirection.Forward);
						forward = flippedControls ? !forward : forward;

						// go to next item
						StartCoroutine(NextItem(forward));
					}
				}
			}

			// controller input
			if (controller)
			{
				// if controller is disabled, check if it should be re-enabled
				if (controllerDisabled && itemIsReady)
				{
					// get absolute value of controller axis value
					float absControllerAxis = Mathf.Abs (ControllerAxis ());

					// start coroutine for autoscrolling
					if (scrollOnHold && absControllerAxis == 1)
					{
						// prevent extra calls to nextItem until scrolling stops
						controllerInputThreshold = 1;

						// get controller direction
						bool forward = GetControllerDirection().Equals(eDirection.Forward);
						forward = flippedControls ? !forward : forward;

						StartCoroutine(ScrollOnHold(forward));
					}
					else if (absControllerAxis < 0.5f)
					{
						// prevent extra calls to nextItem until input is released enough
						controllerInputThreshold = 0.5f;

						Input.ResetInputAxes();
					}
				}

				// reset input threshold when input completely released
				if (ControllerAxis() == 0 && controllerDisabled && controllerInputThreshold == 0.5f)
				{
					controllerDisabled = false;
					controllerInputThreshold = 0.1f;
				}

				// if controller is enabled and inputting a direction, select next item
				if (!controllerDisabled && Mathf.Abs (ControllerAxis ()) > controllerInputThreshold && itemIsReady)
				{
					// store controller axis value
					controllerPressValue = ControllerAxis ();

					// briefly disable selection to prevent unintended autoscrolling
					controllerDisabled = true;

					// get controller direction
					bool forward = GetControllerDirection ().Equals (eDirection.Forward);
					forward = flippedControls ? !forward : forward;

					// go to next item
					StartCoroutine (NextItem (forward));
				}

				// store controller axis value
				controllerHoldValue = ControllerAxis ();
			}
		}

		/*-------------------------------------------------------------*/
		/* ------------------- PROTECTED OVERRIDES ------------------- */
		/*-------------------------------------------------------------*/

		// Add the specified item to the selector.
		protected override void Add (GameObject item)
		{
			// parent item to selector
			item.transform.SetParent (itemsTransform);
			
			// check if selected item
			if (items.Count == selectedItemIndex)
			{
				// max scale
				item.transform.localScale = MultiplyVector (item.transform.localScale, Vector3.one * maxScale);
			}
			else
			{
				// min scale
				item.transform.localScale = MultiplyVector (item.transform.localScale, Vector3.one * minScale);
			}
			
			// use rodrigues rotation formula to set the item position based on rotation, spacing, and item count
			Vector3 e = SphericalToCartesian (radius, Mathf.Deg2Rad * rotateHorizontally, Mathf.Deg2Rad * rotateVertically).normalized; // axis vector
			Vector3 v = FindPerpendicular (e) * radius; // rotation vector
			float theta = Mathf.Deg2Rad * (items.Count * spacing);
			Vector3 newPos = Mathf.Cos (theta) * v + 
				Mathf.Sin (theta) * Vector3.Cross (e, v) + 
					(1 - Mathf.Cos (theta)) * Vector3.Dot (e, v) * e;
			item.transform.localPosition = newPos;
			
			// add item to list
			items.Add (item);
			
		}

		// Callback for the select event. (i.e. selectedItem.color = Color.Blue;)
		protected override void OnSelect ()
		{
			// Add your own code here...
		}
		
		// Callback for the deselect event. Useful for resetting components and variables.
		protected override void OnDeselect ()
		{
			// Add your own code here...
		}
		
		/// Initializes the position of the menu relative to the selected item.
		protected override void InitMenuPosition ()
		{
			// get vector between selected item and selector center
			Vector3 selectedPos = items[selectedItemIndex].transform.localPosition;
			Vector3 centerPos = itemsTransform.localPosition;
			Vector3 distanceVec = centerPos - selectedPos;

			// adjust each item based on distance vector
			for (int i = 0 ; i < items.Count ; i++)
			{
				items[i].transform.localPosition += distanceVec;
			}

			// adjust circle center
			circleCenter = centerPos + distanceVec;
		}

		// selects the next item in the menu.
		protected override IEnumerator NextItem (bool forward)
		{
			if (selectedItem == null || !itemIsReady)
			{
				yield break;
			}

			// coroutine is running and should not be called again until complete.
			itemIsReady = false;

			// set selector position to the selected item
			trackerPosition = selectedItem.transform.localPosition;
			
			// for a sliding menu
			if (slideItems)
			{
				// cache last index
				int lastIndex = items.Count - 1;

				// check if selected item is at the beginning or end of the menu
				bool beginning = items.IndexOf (selectedItem).Equals (0) && !forward;
				bool end = items.IndexOf (selectedItem).Equals (lastIndex) && forward;
				if (beginning || end)
				{
					// return if selected item is at beginning or end of a non-looping menu
					if (!loopItems)
					{
						itemIsReady = true;
						yield break;
					}
					else if (beginning)
					{
						// set last item to beginning of menu
						LastToFirst ();
					}
					else if (end)
					{
						// set first item to end of menu
						FirstToLast ();
					}
				}

				// invoke deselect callback
				onDeselect.Invoke ();

				// set selected item's position as reference
				initialItemPosition = selectedItem.transform.localPosition;

				// set selected item's scale as reference
				initialItemScale = selectedItem.transform.localScale;

				// slide items
				while (SlideItems (forward, true))
				{
					yield return new WaitForSeconds (0.01f);
				}
				
				// adjust items if not at beginning or end of a looping menu
				if (loopItems)
				{
					if (forward && !end)
					{
						// set first item to end of menu
						FirstToLast ();
					}
					else if (!forward && !beginning)
					{
						// set last item to beginning of menu
						LastToFirst ();
					}
				}
				
				UpdateVisibility ();
			}
			else // non-sliding menu
			{
				// invoke deselect callback
				onDeselect.Invoke ();

				// set selected item's position as reference
				initialItemPosition = selectedItem.transform.localPosition;

				// set selected item's scale as reference
				initialItemScale = selectedItem.transform.localScale;

				// move the tracker until the next item is scaled to max
				while (ScaleItems (!forward))
				{
					MoveTracker (!forward);
					
					//yield return new WaitForSeconds (0.01f);
				}
			}
			// coroutine is ready to be called again.
			itemIsReady = true;
		}

		// Slides the items either forward or backward.
		protected override bool SlideItems (bool forward, bool slideToNext = false)
		{
			// tracker serves as a stationary reference when items are sliding
			trackerPosition = itemsTransform.localPosition;
			
			int direction = forward ? -1 : 1;
			float speed = Mathf.Clamp (selectorSpeed, 0, spacing);
			
			// slide items
			for (int i = 0 ; i < items.Count ; i++)
			{
				// calculate the item's new position
				Vector3 itemPosition = items[i].transform.localPosition;
				items[i].transform.localPosition = CalculatePosition (itemPosition, speed, direction);
			}
			
			// adjust scale
			bool isScaling = ScaleItems (forward, true);
			if (slideToNext && !isScaling)
			{
				return false;
			}
			
			return true;
		}

		// Scales the items based on trackerPosition.
		protected override bool ScaleItems (bool forward, bool slide = false)
		{
			// true if reached beginning or end of a looping menu
			bool boundaryFlag = false;
			
			// get the next item
			GameObject nextItem;
			if (forward)
			{
				int selectedIndex = items.IndexOf (selectedItem);
				nextItem = selectedIndex < items.Count - 1 ? items[selectedIndex + 1] : null;
				
				// next item is null if at the end of the menu
				if (nextItem == null && loopItems)
				{
					nextItem = items.First ();
					boundaryFlag = true;
				}
			}
			else // backward
			{
				int selectedIndex = items.IndexOf (selectedItem);
				nextItem =  selectedIndex > 0 ? items[selectedIndex - 1] : null;
				
				// next item is null if at the beginning of the menu
				if (nextItem == null && loopItems)
				{
					nextItem = items.Last ();
					boundaryFlag = true;
				}
			}
			
			// item scales
			float nextItemScale;
			float selectedItemScale;
			
			// selector is at the beginning or end of a non-looping menu
			if (nextItem == null)
			{
				return false;
			}
			// selector is at the beginning or end of a looping menu
			else if (boundaryFlag)
			{
				// set the min and max scales directly
				nextItemScale = maxScale;
				selectedItemScale = minScale;
			}
			else
			{
				// set the transitioning scales for the next item and selected item
				float lowScale = slide ? maxScale : minScale;
				float highScale = slide ? minScale : maxScale;
				// determine where the item is between its origin and its destination and scales it up or down based on the item (next or selected)
				float angleDegrees = CalculateAngle (trackerPosition, nextItem.transform.localPosition, circleCenter) * Mathf.Rad2Deg;
				nextItemScale = (float)Math.Round (MapValues (Mathf.Clamp (angleDegrees, 0, spacing), 0, spacing, lowScale, highScale),3);
				selectedItemScale = (float)Math.Round (MapValues (Mathf.Clamp (angleDegrees, 0, spacing), spacing, 0, lowScale, highScale),3);
			}
			// apply scales to the next item and selected item
			Vector3 defaultScale = initialItemScale / maxScale;
			nextItem.transform.localScale = MultiplyVector (defaultScale, Vector3.one * nextItemScale);
			selectedItem.transform.localScale = MultiplyVector (defaultScale, Vector3.one * selectedItemScale);

			// returns true when the angle between the selected item's initial position and current position is greater/equal to spacing
			Vector3 referencePosition = slideItems ? initialItemPosition : trackerPosition;
			float selectedItemAngle = CalculateAngle (referencePosition, selectedItem.transform.localPosition, circleCenter) * Mathf.Rad2Deg;
			float referenceDistance = Mathf.Abs (selectedItemAngle - spacing);
			if (selectedItemAngle >= spacing || referenceDistance <= 0.01f)
			{
				// set prior and currently selected items and scales
				priorSelectedItem = selectedItem;
				priorSelectedItem.transform.localScale = MultiplyVector (defaultScale, Vector3.one * minScale);
				selectedItem = nextItem;
				selectedItem.transform.localScale = MultiplyVector (defaultScale, Vector3.one * maxScale);

				// invoke callbacks
				onSelect.Invoke ();

				// update tracker position
				trackerPosition = selectedItem.transform.localPosition;
				
				return false;
			}
			
			return true;
		}

		// Shifts the first item to the end of the menu.
		protected override void FirstToLast ()
		{
			int lastIndex = items.Count - 1;
			GameObject item = items[0];
			item.transform.localPosition = CalculatePosition (items[lastIndex].transform.localPosition, spacing, 1);
			items.Add (item);
			items.RemoveAt (0);
		}

		// Shifts the last item to the beginning of the menu.
		protected override void LastToFirst ()
		{
			int lastIndex = items.Count - 1;
			GameObject item = items[lastIndex];
			item.transform.localPosition = CalculatePosition (items[0].transform.localPosition, spacing, -1);
			items.Insert (0, item);
			items.RemoveAt (lastIndex + 1);
		}

		// Moves the tracker position between items.
		protected override void MoveTracker (bool forward)
		{
			int direction = forward ? -1 : 1;

			// move the item tracker position
			trackerPosition = CalculatePosition (trackerPosition, 1f, direction);
		}

		/// <summary>
		/// Enables or disables item renderers based on the visibility setting.
		/// </summary>
		protected override void UpdateVisibility ()
		{
			int itemCount = items.Count;
			int selectedIndex = items.IndexOf (selectedItem);

			// enable/disable renderer setting - based on visibility
			for (int i = 0 ; i < itemCount ; i++)
			{
				// set enable/disable flag
				bool enableRenderer = false;

				// check if i is in range of visibility

				// counter clockwise visibility
				if (!visibilityAnchor.Equals (eVisibilityAnchor.Clockwise))
				{
					if ((selectedIndex + visibility) >= itemCount)
					{
						int adjustedIndex = (selectedIndex + visibility) % itemCount;
						if (adjustedIndex >= i)
						{
							enableRenderer = true;
						}
					}
					if (i >= selectedIndex && i <= (selectedIndex + visibility))
					{
						enableRenderer = true;
					}
				}

				// clockwise visibility
				if (!visibilityAnchor.Equals (eVisibilityAnchor.CounterClockwise))
				{
					if ((selectedIndex - visibility) < 0)
					{
						int adjustedIndex = itemCount + (selectedIndex - visibility);
						if (adjustedIndex <= i)
						{
							enableRenderer = true;
						}
					}
					if (i <= selectedIndex && i >= (selectedIndex - visibility))
					{
						enableRenderer = true;
					}
				}

				// find item's renderer
				MeshRenderer meshRenderer;
				SpriteRenderer spriteRenderer;
				BillboardRenderer billboardRenderer;
				UnityEngine.UI.Image image;
				if ((meshRenderer = items[i].GetComponent<MeshRenderer> ()) != null)
				{
					meshRenderer.enabled = enableRenderer;
				}
				else if ((spriteRenderer = items[i].GetComponent<SpriteRenderer> ()) != null)
				{
					spriteRenderer.enabled = enableRenderer;
				}
				else if ((billboardRenderer = items[i].GetComponent<BillboardRenderer> ()) != null)
				{
					billboardRenderer.enabled = enableRenderer;
				}
				else if ((image = items[i].GetComponent<UnityEngine.UI.Image> ()) != null)
				{
					image.enabled = enableRenderer;
				}
				else
				{
					Debug.LogWarning ("Visibility setting failed to find "+items[i].name+"'s renderer");
				}
				
				// show/hide children
				foreach (Transform child in items[i].transform) 
					child.gameObject.SetActive(enableRenderer);
			}
		}
	}
}
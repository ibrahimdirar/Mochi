using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SuperSelector
{
	public class LinearSelector : Selector
	{
		/// <summary>Direction of the visibility anchor.</summary>
		private enum eVisibilityAnchor { Left, Right, None }

		/// <summary>Anchors the visibility in the specified direction.</summary>
		[SerializeField]
		private eVisibilityAnchor visibilityAnchor = eVisibilityAnchor.None;

		[SerializeField]
		private Text descriptionText;

		/*---------------------------------------------------------*/
		/* ------------------- PRIVATE METHODS ------------------- */
		/*---------------------------------------------------------*/

		private void Awake()
		{
			// cache the selector angle
			angle = GetSelectorAngle();

			// initialize items list
			InitItems();

			// return if no items
			if (items.Count == 0) return;

			// set selected item
			selectedItem = items.ElementAt(selectedItemIndex);

			// set menu position
			InitMenuPosition();

			// set tracker position
			trackerPosition = selectedItem.transform.localPosition;

			// set visibility
			if (slideItems)
			{
				UpdateVisibility();
			}
		}

		private void OnEnable()
		{
			// add callbacks to events
			onSelect.AddListener(OnSelect);
			onDeselect.AddListener(OnDeselect);
		}

		private void OnDisable()
		{
			// remove callbacks from events
			onSelect.RemoveListener(OnSelect);
			onDeselect.RemoveListener(OnDeselect);
		}

		// Use this for initialization
		private void Start()
		{
			// trigger select event for first selected item
			onSelect.Invoke();
		}

		// Update is called once per frame.
		private void Update()
		{
			// mouse input
			if (mouse || touch)
			{
				// DRAGGING
				if (Input.GetMouseButtonDown(0))
				{
					// cache mouse position for mouse dragging and item scrolling purposes
					mousePosition = Input.mousePosition;
				}

				if (!Input.GetMouseButton(0))
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
						if (GetDragAxis(dragVector).Equals(selectionAxis))
						{
							// briefly disable mouse/touch selection to prevent unintended autoscrolling
							touchDisabled = true;

							// get drag direction
							bool forward = GetDragDirection(dragVector).Equals(eDirection.Forward);
							forward = flippedControls ? !forward : forward;

							// go to next item
							StartCoroutine(NextItem(forward));

							// start coroutine for autoscrolling
							if (scrollOnHold)
							{
								StartCoroutine(ScrollOnHold(forward));
							}
						}
						else
						{
							// wrong axis, disable touch until next mouse up
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
					float absControllerAxis = Mathf.Abs(ControllerAxis());

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
				if (!controllerDisabled && Mathf.Abs(ControllerAxis()) > controllerInputThreshold && itemIsReady)
				{
					// store controller axis value
					controllerPressValue = ControllerAxis();

					// briefly disable selection to prevent unintended autoscrolling
					controllerDisabled = true;

					// get controller direction
					bool forward = GetControllerDirection().Equals(eDirection.Forward);
					forward = flippedControls ? !forward : forward;

					// go to next item
					StartCoroutine(NextItem(forward));
				}
				
				// store controller axis value
				controllerHoldValue = ControllerAxis();
			}
		}


		/*-------------------------------------------------------------*/
		/* ------------------- PROTECTED OVERRIDES ------------------- */
		/*-------------------------------------------------------------*/

		// Add the specified item to the selector.
		protected override void Add(GameObject item)
		{
			// parent item to selector
			item.transform.SetParent(itemsTransform);

			// check if selected item
			if (items.Count == selectedItemIndex)
			{
				// max scale
				item.transform.localScale = MultiplyVector(item.transform.localScale, Vector3.one * maxScale);
			}
			else
			{
				// min scale
				item.transform.localScale = MultiplyVector(item.transform.localScale, Vector3.one * minScale);
			}

			// position
			Vector3 itemPosition = items.Count > 0 ? items.Last().transform.localPosition : itemsTransform.localPosition;
			itemPosition += angle;
			item.transform.localPosition = itemPosition;

			// add item to list
			items.Add(item);
		}

		// Callback for the select event. (i.e. selectedItem.color = Color.Blue;)
		protected override void OnSelect()
		{
			// Add your own code here...
		}

		// Callback for the deselect event. Useful for resetting components and variables.
		protected override void OnDeselect()
		{
			// Add your own code here...
		}

		// Selects the next item in the menu.
		protected override IEnumerator NextItem(bool forward)
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
				bool beginning = items.IndexOf(selectedItem).Equals(0) && !forward;
				bool end = items.IndexOf(selectedItem).Equals(lastIndex) && forward;
				if (beginning || end)
				{
					// return if selected item is at beginning or end of non-looping menu
					if (!loopItems)
					{
						itemIsReady = true;
						yield break;
					}
					else if (beginning)
					{
						// set last item to beginning of menu
						LastToFirst();
					}
					else if (end)
					{
						// set first item to end of menu
						FirstToLast();
					}
				}

				// invoke deselect callback
				onDeselect.Invoke();

				// set selected item's position as reference
				initialItemPosition = selectedItem.transform.localPosition;

				// set selected item's scale as reference
				initialItemScale = selectedItem.transform.localScale;

				// slide items
				while (SlideItems(forward, true))
				{
					yield return new WaitForSeconds(0.01f);
				}

				// adjust items if not at beginning or end of a looping menu
				if (loopItems)
				{
					if (forward && !end)
					{
						// set first item to end of menu
						FirstToLast();
					}
					else if (!forward && !beginning)
					{
						// set last item to beginning of menu
						LastToFirst();
					}
				}

				UpdateVisibility();
			}
			else // non-sliding menu
			{
				// invoke deselect callback
				onDeselect.Invoke();

				// set selected item's position as reference
				initialItemPosition = selectedItem.transform.localPosition;

				// set selected item's scale as reference
				initialItemScale = selectedItem.transform.localScale;

				// move the tracker forward until the next item is scaled to max
				while (ScaleItems(forward))
				{
					MoveTracker(forward);

					yield return new WaitForSeconds(0.01f);
				}
			}
			// coroutine is ready to be called again.
			itemIsReady = true;
		}

		// Slides the items either forward or backward.
		protected override bool SlideItems(bool forward, bool slideToNext = false)
		{
			// tracker should be stationary when items are sliding
			trackerPosition = itemsTransform.localPosition;

			// direction is -1 if forward, 1 if backward.
			int direction = forward ? -1 : 1;

			// adjust selector speed
			float speed = Mathf.Clamp(selectorSpeed / (float)10, 0, spacing);
			
			// set item positions based on angle, direction, and speed
			for (int i = 0; i < items.Count; i++)
			{
				items[i].transform.localPosition += Vector3.Normalize(angle) * direction * speed;
			}

			// adjust scale
			bool isScaling = ScaleItems(forward, true);
			if (slideToNext && !isScaling)
			{
				// adjust items to proper position
				for (int i = 0; i < items.Count; i++)
				{
					items[i].transform.localPosition -= trackerPosition;
				}
				return false;
			}

			return true;
		}

		// Scales the items based on trackerPosition.
		protected override bool ScaleItems(bool forward, bool slide = false)
		{
			// true if reached beginning or end of a looping menu
			bool boundaryFlag = false;

			// get the next item
			GameObject nextItem;
			if (forward)
			{
				int selectedIndex = items.IndexOf(selectedItem);
				nextItem = selectedIndex < items.Count - 1 ? items[selectedIndex + 1] : null;

				// next item is null if at the end of the menu
				if (nextItem == null && loopItems)
				{
					nextItem = items.First();
					boundaryFlag = true;
				}
			}
			else // backward
			{
				int selectedIndex = items.IndexOf(selectedItem);
				nextItem = selectedIndex > 0 ? items[selectedIndex - 1] : null;

				// next item is null if at the beginning of the menu
				if (nextItem == null && loopItems)
				{
					nextItem = items.Last();
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
				float lowSpacing = slide ? spacing : 0;
				float highSpacing = slide ? 0 : spacing;

				// determines where the item is between its origin and its destination and scales it up or down based on the item (next or selected)
				double distance = Math.Round((double)(Vector3.Distance(trackerPosition, nextItem.transform.localPosition)), 5);
				nextItemScale = (float)Math.Round(MapValues(Mathf.Clamp((float)distance, 0, spacing), highSpacing, lowSpacing, lowScale, highScale), 2);
				selectedItemScale = (float)Math.Round(MapValues(Mathf.Clamp((float)distance, 0, spacing), lowSpacing, highSpacing, lowScale, highScale), 2);
			}

			// apply scales to the next item and selected item
			Vector3 defaultScale = initialItemScale / (float)maxScale;
			nextItem.transform.localScale = MultiplyVector(defaultScale, Vector3.one * nextItemScale);
			selectedItem.transform.localScale = MultiplyVector(defaultScale, Vector3.one * selectedItemScale);

			// returns true when selected item reaches its destination (or within 0.01f)
			float direction = forward ? -1 : 1;
			float trackerDistance = Vector3.Distance(trackerPosition, nextItem.transform.localPosition);
			Vector3 selectedItemDestination = initialItemPosition + Vector3.Normalize(angle) * direction * spacing;
			float itemDistance = Vector3.Distance(slide ? selectedItem.transform.localPosition : trackerPosition, initialItemPosition);
			float destinationDistance = Vector3.Distance(selectedItemDestination, selectedItem.transform.localPosition);
			float referenceDistance = slideItems ? destinationDistance : trackerDistance;
			if (itemDistance >= spacing || Mathf.Abs(referenceDistance) <= 0.01f)
			{
				// set prior and currently selected items
				priorSelectedItem = selectedItem;
				priorSelectedItem.transform.localScale = MultiplyVector(defaultScale, Vector3.one * minScale);
				selectedItem = nextItem;
				selectedItem.transform.localScale = MultiplyVector(defaultScale, Vector3.one * maxScale);

				// invoke select callback
				onSelect.Invoke();

				// update tracker position
				trackerPosition = selectedItem.transform.localPosition;

				return false;
			}

			return true;
		}

		// Shifts the first item to the end of the menu.
		protected override void FirstToLast()
		{
			int lastIndex = items.Count - 1;
			GameObject item = items[0];
			item.transform.localPosition = items[lastIndex].transform.localPosition + angle;
			items.Add(item);
			items.RemoveAt(0);
		}

		// Shifts the last item to the beginning of the menu.
		protected override void LastToFirst()
		{
			int lastIndex = items.Count - 1;
			GameObject item = items[lastIndex];
			item.transform.localPosition = items[0].transform.localPosition - angle;
			items.Insert(0, item);
			items.RemoveAt(lastIndex + 1);
		}

		// Moves the tracker position between items.
		protected override void MoveTracker(bool forward)
		{
			// direction is 1 if forward, -1 if backward.
			int direction = forward ? 1 : -1;

			// adjust selector speed
			float speed = Mathf.Clamp(selectorSpeed / (float)10, 0, spacing);

			// move the item tracker position
			trackerPosition += Vector3.Normalize(angle) * direction * speed;
		}

		/// <summary>
		/// Enables or disables item renderers based on the visibility setting.
		/// </summary>
		protected override void UpdateVisibility()
		{
			int itemCount = items.Count;
			int selectedIndex = items.IndexOf(selectedItem);

			// enable/disable renderer setting - based on visibility
			for (int i = 0; i < itemCount; i++)
			{
				// set enable/disable flag
				bool enableRenderer = false;

				// check if i is in range of visibility

				// anchor visibility to the right
				if (!visibilityAnchor.Equals(eVisibilityAnchor.Left))
				{
					if (i >= selectedIndex && i <= (selectedIndex + visibility))
					{
						enableRenderer = true;
					}
				}

				// anchor visibility to the left
				if (!visibilityAnchor.Equals(eVisibilityAnchor.Right))
				{
					if (i <= selectedIndex && i >= (selectedIndex - visibility))
					{
						enableRenderer = true;
					}
				}

				// find item's renderer
				MeshRenderer meshRenderer;
				SpriteRenderer spriteRenderer;
				BillboardRenderer billboardRenderer;
				LineRenderer lineRenderer;
				UnityEngine.UI.Image image;
				if ((meshRenderer = items[i].GetComponent<MeshRenderer>()) != null)
				{
					meshRenderer.enabled = enableRenderer;
				}
				else if ((spriteRenderer = items[i].GetComponent<SpriteRenderer>()) != null)
				{
					spriteRenderer.enabled = enableRenderer;
				}
				else if ((billboardRenderer = items[i].GetComponent<BillboardRenderer>()) != null)
				{
					billboardRenderer.enabled = enableRenderer;
				}
				else if ((image = items[i].GetComponent<UnityEngine.UI.Image>()) != null)
				{
					image.enabled = enableRenderer;
				}
				else if ((lineRenderer = items[i].GetComponent<LineRenderer>()) != null)
				{
					lineRenderer.enabled = enableRenderer;
				}
				else
				{
					Debug.LogWarning("Visibility setting failed to find " + items[i].name + "'s renderer");
				}

				// show/hide children
				foreach (Transform child in items[i].transform)
					child.gameObject.SetActive(enableRenderer);
			}
		}
	}
}
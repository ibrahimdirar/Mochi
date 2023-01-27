using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SuperSelector
{
	[RequireComponent(typeof(GridLayoutGroup))]
	public class GridSelector : Selector 
	{
		/// <summary>Required component of the selector.</summary>
		private GridLayoutGroup grid;

		/// <summary>The number of columns in the selector.</summary>
		[SerializeField]
		private int columns = 1;

		/// <summary>Direction of the visibility anchor on the X axis.</summary>
		private enum eVisibilityAnchorX {Left, Right, None}

		/// <summary>Anchors the visibility in the specified direction.</summary>
		[SerializeField]
		private eVisibilityAnchorX visibilityAnchorX = eVisibilityAnchorX.None;

		/// <summary>Determines the number of items that should be visible on the horizontal axis.</summary>
		[SerializeField]
		private int visibilityX = 0;

		/// <summary>Direction of the visibility anchor on the Y axis.</summary>
		private enum eVisibilityAnchorY {Up, Down, None}

		/// <summary>Anchors the visibility in the specified direction.</summary>
		[SerializeField]
		private eVisibilityAnchorY visibilityAnchorY = eVisibilityAnchorY.None;

		/// <summary>Determines the number of items that should be visible on the vertical axis.</summary>
		[SerializeField]
		private int visibilityY = 0;

		// redefine eDirection to include up and down for grid
		protected new enum eDirection {None, Forward, Backward, Up, Down}

		/// <summary>The current direction of a selection.</summary>
		protected eDirection direction = eDirection.None;

		/// <summary>
		/// Determines whether to select the first item in the next row when the selector reaches the end of the current row. 
		/// NOTE: Requires loopItems to be TRUE, and slideItems to be FALSE.
		/// </summary>
		[SerializeField]
		private bool wrapRows;

		/// <summary>
		/// Determines whether to select the first item in the next column when the selector reaches the end of the current column. 
		/// NOTE: Requires loopItems to be TRUE, and slideItems to be FALSE.
		/// </summary>
		[SerializeField]
		private bool wrapColumns = false;


		/*---------------------------------------------------------*/
		/* ------------------- PRIVATE METHODS ------------------- */
		/*---------------------------------------------------------*/

		private void Awake ()
		{
			// get grid component
			grid = GetComponent<GridLayoutGroup> ();

			// set fixed column constraint
			grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			grid.constraintCount = columns;

			// set grid spacing to user-defined spacing
			grid.spacing = new Vector2 (spacing, spacing);

			// initialize items list
			InitItems ();

			// return if no items
			if (items.Count == 0) return;

			// set selected item
			selectedItem = items.ElementAt (selectedItemIndex);

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
						// briefly disable mouse/touch selection to prevent unintended autoscrolling
						touchDisabled = true;
						
						// set direction based on drag vector
						direction = GetDragDirection (dragVector);
						bool forward = direction.Equals (eDirection.Forward);
						forward = flippedControls ? !forward : forward;

						// go to next item
						StartCoroutine (NextItem (forward));
						
						// start coroutine for autoscrolling
						if (scrollOnHold)
						{
							StartCoroutine (ScrollOnHold (forward));
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
						direction = GetDragDirection(scrollVector) == eDirection.Down ? eDirection.Forward : eDirection.Backward;
						bool forward = direction.Equals(eDirection.Forward);
						forward = flippedControls ? !forward : forward;

						// go to next item
						StartCoroutine(NextItem(forward));

						// start coroutine for autoscrolling
						if (scrollOnHold)
						{
							StartCoroutine(ScrollOnHold(forward));
						}
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

						// set direction based on controller input
						direction = GetControllerDirection();
						bool forward = direction.Equals(eDirection.Forward);
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

					// set direction based on controller input
					direction = GetControllerDirection ();
					bool forward = direction.Equals (eDirection.Forward);
					forward = flippedControls ? !forward : forward;

					// go to next item
					StartCoroutine (NextItem (forward));
				}

				// store controller axis value
				controllerHoldValue = ControllerAxis ();
			}
		}
		

		/*-------------------------------------------------------------*/
		/* -------------------- PUBLIC OVERRIDES --------------------- */
		/*-------------------------------------------------------------*/

		/// <summary>
		/// Gets the item count.
		/// </summary>
		/// <returns>The item count.</returns>
		public override int GetItemCount ()
		{
			return transform.childCount;
		}


		/*-------------------------------------------------------------*/
		/* --------------- PROTECTED METHODS/OVERRIDES --------------- */
		/*-------------------------------------------------------------*/

		// Initializes the items list by adding all children in the Items gameobject to the list. 
		// Note: The item gameobject is a child of the selector.
		protected override void InitItems ()
		{
			items = new List<GameObject> ();
			
			itemsTransform = transform;
			
			foreach (Transform item in itemsTransform)
			{
				// add each item to the selector
				Add (item.gameObject);
			}
		}

		// Add the specified item to the selector.
		protected override void Add (GameObject item)
		{
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
		
		// Selects the next item in the menu.
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

			// calculate row indices for selected item - used to compare wtih selected item's row index
			int selectedIndex = items.IndexOf (selectedItem);
			int rowNumber = Mathf.CeilToInt ((selectedIndex + 1) / (float)columns);
			int totalRows = Mathf.CeilToInt (items.Count / (float)columns);
			int rowFirstIndex = (columns * (rowNumber - 1));
			int rowLastIndex = rowNumber != totalRows ? rowFirstIndex + columns - 1 : items.Count - 1;

			// check if selected item is at the beginning or end of the row or column
			bool end = (rowLastIndex == selectedIndex && direction.Equals (eDirection.Forward)) ||
				(selectedIndex > (items.Count - columns - 1) && direction.Equals (eDirection.Down));
			bool beginning = (rowFirstIndex == selectedIndex && direction.Equals (eDirection.Backward)) || 
				(selectedIndex < columns && direction.Equals (eDirection.Up));

			// for a sliding menu
			if (slideItems)
			{
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
					if ((direction.Equals (eDirection.Forward) || direction.Equals (eDirection.Down)) && !end)
					{
						// set first item to end of menu
						FirstToLast ();
					}
					else if ((direction.Equals (eDirection.Backward) || direction.Equals (eDirection.Up)) && !beginning)
					{
						// set last item to beginning of menu
						LastToFirst ();
					}
				}

				UpdateVisibility ();
			}
			else // non-sliding menu
			{
				if (DeselectShouldBeInvoked (beginning, end))
				{
					// invoke deselect callback
					onDeselect.Invoke ();
				}

				// set selected item's position as reference
				initialItemPosition = selectedItem.transform.localPosition;

				// set selected item's scale as reference
				initialItemScale = selectedItem.transform.localScale;

				// move the tracker forward until the next item is scaled to max
				while (ScaleItems (forward))
				{
					MoveTracker (forward);
				}
			}
			// coroutine is ready to be called again.
			itemIsReady = true;
		}
		
		// Slides the items either forward or backward.
		protected override bool SlideItems (bool forward, bool slideToNext = false)
		{
			// determines how fast to shift from one item to the next
			float speed = Mathf.Clamp (selectorSpeed, 0, Spacing);
			
			// set item positions based on angle, direction, and speed
			for (int i = 0 ; i < items.Count ; i++)
			{
				items[i].transform.localPosition += GetControllerDirection (true) * speed;
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

			// calculate row indices for selected item - used to get next item
			int selectedIndex = items.IndexOf (selectedItem);
			int rowNumber = Mathf.CeilToInt ((selectedIndex + 1) / (float)columns);
			int totalRows = Mathf.CeilToInt (items.Count / (float)columns);
			int rowFirstIndex = (columns * (rowNumber - 1));
			int rowLastIndex = rowNumber != totalRows ? rowFirstIndex + columns - 1 : items.Count - 1;

			// get the next item
			GameObject nextItem = null;
			switch (direction)
			{
			case eDirection.Up:
				// next item is directly above selected item
				int nextItemIndex = selectedIndex - columns;
				nextItem = nextItemIndex >= 0 ? items[nextItemIndex] : null;

				// next item is null if at the beginning of the column
				if (nextItem == null && loopItems)
				{
					// get the item number (index + 1) of the last item in the column
					int lastItemNumber = (selectedIndex + 1) + (columns * (totalRows - rowNumber));

					// check if column wrapping is enabled
					if (wrapColumns)
					{
						// adjust item number if there is an incomplete column
						lastItemNumber -= lastItemNumber - 1 > items.Count ? columns : 0;

						// make sure item exists in the previous column
						// (lastItemNumber is same as index of the last item in the next column)
						int lastRowFirstIndex = columns * (totalRows -1);
						if (lastItemNumber - 1 != lastRowFirstIndex)
						{
							// next item should be last item in previous column
							nextItem = items[lastItemNumber - 2];
						}
					}
					else
					{
						// adjust item number if there is an incomplete column
						lastItemNumber -= lastItemNumber > items.Count ? columns : 0;

						// next item should be last item in current column
						nextItem = items[lastItemNumber - 1];
					}
					boundaryFlag = true;
				}
				break;
			case eDirection.Down:
				// next item is directly below selected item
				nextItemIndex = selectedIndex + columns;
				nextItem = nextItemIndex < items.Count ? items[nextItemIndex] : null;

				// next item is null if at the end of the column
				if (nextItem == null && loopItems)
				{
					// get the item number (index + 1) of the first item in the column
					int firstItemNumber = (selectedIndex + 1) - (columns * totalRows);
					while (firstItemNumber < 1)
					{
						firstItemNumber += columns;
					}

					// check if column wrapping is enabled
					if (wrapColumns)
					{
						// make sure item exists in the next column 
						// (firstItemNumber is same as index of the first item in the next column)
						if (firstItemNumber != columns)
						{
							// next item should be first item in next column
							nextItem = items[firstItemNumber];
						}
					}
					else
					{
						// next item should be first item in current column
						nextItem = items[firstItemNumber - 1];
					}
					boundaryFlag = true;
				}
				break;
			case eDirection.Forward:
				// next item is in front of selected item - check if selected item is at the end of the row
				nextItem = rowLastIndex != selectedIndex ? items[selectedIndex + 1] : null;
					
				// next item is null if at the end of the row
				if (nextItem == null && loopItems)
				{
					// check if row wrapping is enabled
					if (wrapRows)
					{
						// make sure item exists in the next row
						if (rowLastIndex + 1 < items.Count - 1)
						{
							// next item should be first item in next row
							nextItem = items[rowLastIndex + 1];
						}
					}
					else
					{
						// next item should be first item in current row
						nextItem = items[rowFirstIndex];
					}
					boundaryFlag = true;
				}
				break;
			case eDirection.Backward:
				// next item is directly behind selected item - check if selected item is at the beginning of the row
				nextItem = rowFirstIndex != selectedIndex ? items[selectedIndex - 1] : null;

				// next item is null if at the beginning of the row
				if (nextItem == null && loopItems)
				{
					// check if row wrapping is enabled
					if (wrapRows)
					{
						// make sure item exists in the previous row
						if (rowFirstIndex - 1 >= 0)
						{
							// next item should be last item in previous row
							nextItem = items[rowFirstIndex - 1];
						}
					}
					else
					{
						// next item should be last item in current row
						nextItem = items[rowLastIndex];
					}
					boundaryFlag = true;
				}
				break;
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
				float lowScale = slide? maxScale : minScale;
				float highScale = slide ? minScale : maxScale;
				float lowSpacing = slide ? Spacing : 0;
				float highSpacing = slide ? 0 : Spacing;

				// determines where the item is between its origin and its destination and scales it up or down based on the item (next or selected)
				double distance = Math.Round ((double)(Vector3.Distance (trackerPosition, nextItem.transform.localPosition)), 5);
				nextItemScale = (float)Math.Round (MapValues (Mathf.Clamp ((float)distance, 0, Spacing), highSpacing, lowSpacing, lowScale, highScale),2);
				selectedItemScale = (float)Math.Round (MapValues (Mathf.Clamp ((float)distance, 0, Spacing), lowSpacing, highSpacing, lowScale, highScale),2);
			}

			// apply scales to the next item and selected item
			Vector3 defaultScale = initialItemScale / maxScale;
			nextItem.transform.localScale = MultiplyVector (defaultScale, Vector3.one * nextItemScale);
			selectedItem.transform.localScale = MultiplyVector (defaultScale, Vector3.one * selectedItemScale);
			//Debug.Log ("n: " + nextItemScale + ", s: " + selectedItemScale);

			// returns true when selected item reaches its destination (or within 0.01f)
			float trackerDistance = Vector3.Distance (trackerPosition, nextItem.transform.localPosition);
			Vector3 selectedItemDestination = initialItemPosition + GetControllerDirection (true) * Spacing;
			float itemDistance = Vector3.Distance (slide ? selectedItem.transform.localPosition : trackerPosition, initialItemPosition);
			float destinationDistance = Vector3.Distance (selectedItemDestination, selectedItem.transform.localPosition);
			float referenceDistance = slideItems ? destinationDistance  : trackerDistance;
			if (itemDistance >= Spacing || Mathf.Abs(referenceDistance) <= 0.01f)
			{
				// set prior and currently selected items
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
		
		// Shifts the first item In Each Row/Column to the end of the menu.
		protected override void FirstToLast ()
		{
			// if forward selection, shift items in each row
			if (direction.Equals (eDirection.Forward))
			{
				// loop through every first row item
				for (int i = 0 ; i < items.Count ; i += columns)
				{
					GameObject item = items[i];

					// move item position to last position in row
					int rowNumber = Mathf.CeilToInt ((i + 1) / (float)columns);
					int totalRows = Mathf.CeilToInt (items.Count / (float)columns);
					int rowLastIndex = rowNumber != totalRows ? (columns * rowNumber) - 1 : items.Count - 1;
					item.transform.localPosition = items[rowLastIndex].transform.localPosition + GetControllerDirection (false) * Spacing;

					// move item in list to end of the row
					items.Insert (rowLastIndex + 1, item);
					items.RemoveAt (i);
				}
			}
			// if down selection, shift items in each column
			else if (direction.Equals (eDirection.Down))
			{
				// loop through the first item in each column (AKA all items in the first row)
				for (int i = 0 ; i < columns ; i++)
				{
					GameObject item = items[0];
					
					// move item position to last position in column
					int totalRows = Mathf.CeilToInt (items.Count / (float)columns);
					int columnLastIndex = columns * (totalRows - 1);
					int adjustedIndex = i + columnLastIndex; // adjusts index to be in same column as item
					bool needsAdjustment = adjustedIndex > items.Count - 1;
					columnLastIndex -= needsAdjustment ? columns : 0;

					item.transform.localPosition = items[columnLastIndex].transform.localPosition + GetControllerDirection (false) * Spacing;
					
					// move item in list to end of the column
					items.Insert ((needsAdjustment ? columnLastIndex + columns : items.Count), item);
					items.RemoveAt (0);
				}
			}
		}
		
		// Shifts the last item In Each Row to the beginning of the menu.
		protected override void LastToFirst ()
		{
			// if backward selection, shift items in each row
			if (direction.Equals (eDirection.Backward))
			{
				// loop through every last row item
				for (int i = columns - 1 ; i < items.Count + columns - 1 ; i += columns)
				{
					// get current row's last item
					int index = i >= items.Count ? items.Count - 1 : i; 
					GameObject item = items[index];
					
					// move item position to first position in row
					int rowNumber = Mathf.CeilToInt ((index + 1) / (float)columns);
					int rowFirstIndex = (columns * (rowNumber - 1));
					item.transform.localPosition = items[rowFirstIndex].transform.localPosition + GetControllerDirection (false) * Spacing;
					
					// move item in list to first row index
					items.Insert (rowFirstIndex, item);
					items.RemoveAt (index + 1);
				}
			}
			// if up selection, shift items in each column
			else if (direction.Equals (eDirection.Up))
			{
				// loop through the last item in each column (AKA all items in the last row)
				for (int i = 0 ; i < columns ; i++)
				{
					// get current column's last item
					int totalRows = Mathf.CeilToInt (items.Count / (float)columns);
					int columnLastIndex = columns * (totalRows - 1);
					int adjustedIndex = i + columnLastIndex; // adjusts index to be in same column as item
					int columnFirstIndex = i * 2;
					if (adjustedIndex > items.Count - 1)
					{
						adjustedIndex = items.Count - (columns - i);
					}
					GameObject item = items[adjustedIndex];

					// move item position to first position in column
					item.transform.localPosition = items[columnFirstIndex].transform.localPosition + GetControllerDirection (false) * Spacing;
					
					// move item in list to beginning of the column
					items.Insert (i, item);
					items.RemoveAt (adjustedIndex + 1);
				}
			}
		}

		// Moves the tracker position between items (argument is ignored).
		protected override void MoveTracker (bool forward)
		{
			// set the tracker direction
			Vector3 trackerDirection = GetControllerDirection (false);

			// move the item tracker position
			trackerPosition += trackerDirection;
		}

		// Returns value of the controller's selected axis.
		protected override float ControllerAxis ()
		{
			switch (GetControllerDirection ())
			{
				case eDirection.Up:
					return Input.GetAxis ("Vertical");
				case eDirection.Down:
					return Input.GetAxis ("Vertical");
				case eDirection.Forward:
					return Input.GetAxis ("Horizontal");
				case eDirection.Backward:
					return Input.GetAxis ("Horizontal");
				default:
					return 0;
			}
		}

		// Determines whether a controller, keyboard, or joystick is selecting on an axis.
		protected override bool ControllerIsSelecting ()
		{
			bool isSelectingX = Input.GetAxis ("Horizontal") != 0;
			bool isSelectingY = Input.GetAxis ("Vertical") != 0;
			return isSelectingX || isSelectingY;
		}

		// Determines whether a controller, keyboard, or joystick is not holding on an axis.
		// Differs from ControllerIsSelecting in that ControllerIsSelecting will return false 
		// even if the player slightly taps in a direction
		protected override bool ControllerIsNotHolding ()
		{
			bool isNotSelectingX = Math.Abs(Input.GetAxis ("Horizontal")) != 1;
			bool isNotSelectingY = Math.Abs(Input.GetAxis ("Vertical")) != 1;
			return isNotSelectingX && isNotSelectingY;
		}

		/// <summary>
		/// Gets the direction of the controller as an enum eDirection.
		/// Note: called by direction's get accessor.
		/// </summary>
		protected new eDirection GetControllerDirection ()
		{
			// find primary direction (largest value of both axes)
			string primaryAxis = Math.Abs (Input.GetAxis ("Horizontal")) > Math.Abs (Input.GetAxis ("Vertical")) ? "Horizontal" : "Vertical"; 
			eDirection primaryDirection = eDirection.None;

			// determine direction on primary axis
			if (primaryAxis.Equals ("Horizontal"))
			{
				primaryDirection = Input.GetAxis (primaryAxis) < 0 ? eDirection.Backward : eDirection.Forward;
			}
			else
			{
				primaryDirection = Input.GetAxis (primaryAxis) < 0 ? eDirection.Down : eDirection.Up;
			}

			return primaryDirection;
		}

		/// <summary>
		/// Gets the direction of the controller as a vector.
		/// </summary>
		/// <returns>The controller direction.</returns>
		/// <param name="flipped">If set to <c>true</c>, the directions are flipped (i.e. up will be down).</param>
		protected Vector3 GetControllerDirection (bool flipped)
		{
			switch (direction)
			{
			case eDirection.Up:
				return itemsTransform.up * (flipped ? -1 : 1);
			case eDirection.Down:
				return -itemsTransform.up  * (flipped ? -1 : 1);
			case eDirection.Forward:
				return itemsTransform.right  * (flipped ? -1 : 1);
			case eDirection.Backward:
				return -itemsTransform.right  * (flipped ? -1 : 1);
			default:
				return Vector3.one;
			}
		}

		// Gets the drag direction of a mouse or touch.
		protected new eDirection GetDragDirection (Vector3 dragVector)
		{
			// find primary direction
			string primaryAxis = Enum.GetName (typeof(eSelectionAxis), GetDragAxis (dragVector));
			eDirection primaryDirection = eDirection.None;

			// determine direction on primary axis
			if (primaryAxis.Equals ("Horizontal"))
			{
				primaryDirection = Vector3.Dot (dragVector, itemsTransform.right) < 0 ? eDirection.Backward : eDirection.Forward;
			}
			else
			{
				primaryDirection = Vector3.Dot (dragVector, itemsTransform.up) < 0 ? eDirection.Down : eDirection.Up;
			}

			return primaryDirection;
		}
		
		/// <summary>
		/// Gets spacing based on direction.
		/// </summary> 
		protected float Spacing
		{
			get
			{
				return direction.Equals (eDirection.Forward) || direction.Equals (eDirection.Backward) ? 
					grid.spacing.x + grid.cellSize.x : grid.spacing.y + grid.cellSize.y;
			}
		}

		/// <summary>
		/// Checks if the onDeselect event should be invoked. Used when slideItems is false.
		/// </summary>
		/// <param name="beginning">true if selected item is at the beginning of row or column.</param>
		/// <param name="end">true if selected item is at the end of row or column.</param>
		/// <returns><c>true</c>, if the event should be invoked, <c>false</c> otherwise.</returns>
		protected bool DeselectShouldBeInvoked (bool beginning, bool end)
		{
			int selectedIndex = items.IndexOf (selectedItem);

			// always deselect if not at the beginning or end of the selector
			if (!(beginning || end))
			{
				return true;
			}
			else if (beginning)
			{
				// selecting backwards at the beginning of a looping menu
				if (direction.Equals (eDirection.Backward) && loopItems)
				{
					// don't deselect if the selected item is at the first index
					if ((wrapRows && selectedIndex != 0) || !wrapRows)
						return true;
				}
				// selecting upwards at the beginning of a looping menu
				else if (direction.Equals (eDirection.Up) && loopItems)
				{
					// don't deselect if the selected item is at the first index
					if ((wrapColumns && selectedIndex != 0) || !wrapColumns)
						return true;
				}
			}
			else if (end)
			{
				// selecting forwards at the beginning of a looping menu
				if (direction.Equals (eDirection.Forward) && loopItems)
				{
					// don't deselect if the selected item is at the last index
					if ((wrapRows && selectedIndex != items.Count - 1) || !wrapRows)
						return true;
				}
				// selecting downwards at the beginning of a looping menu
				else if (direction.Equals (eDirection.Down) && loopItems)
				{
					// don't deselect if the selected item is at the last index
					if ((wrapColumns && selectedIndex != items.Count - 1) || !wrapColumns)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// IEnumerator that scrolls to the specified index in the selector.
		/// (Note: will not work if slideItems is enabled)
		/// </summary>
		protected override IEnumerator ScrollTo (int index)
		{
			// slideItems must be disabled
			if (slideItems)
			{
				yield break;
			}

			// requires wrapRows to be enabled
			bool defaultWrapRows = wrapRows;
			wrapRows = true;

			if (items.Count <= index)
			{
				yield break;
			}
			
			// true if index is in front of the selected item
			bool forward = selectedItemIndex < index;

			// set direction to forward or backward
			direction = forward ? eDirection.Forward : eDirection.Backward;

			// loop until reaching index
			while (!selectedItem.Equals(items[index]))
			{
				// select next item
				if (itemIsReady) StartCoroutine (NextItem (forward));
				// pause between item selections
				yield return new WaitForSeconds (0.1f);
			}

			// wait until coroutine is complete
			while (!itemIsReady) {}

			// reset variables
			direction = eDirection.None;
			wrapRows = defaultWrapRows;
		}

		// IEnumerator that scrolls the items automatically when the player holds down a directional button or touch.
		protected override IEnumerator ScrollOnHold (bool forward)
		{
			// wait before for starting autoscroll
			yield return new WaitForSeconds (1.0f);
			
			// calculate beginning and end of menu
			int selectedIndex = items.IndexOf (selectedItem);
			int rowNumber = Mathf.CeilToInt ((selectedIndex + 1) / (float)columns);
			int rowFirstIndex = (columns * (rowNumber - 1));
			int rowLastIndex = (columns * rowNumber) - 1;
			bool end = (rowLastIndex == selectedIndex && direction.Equals (eDirection.Forward)) ||
				(selectedIndex >= (items.Count - columns - 1) && direction.Equals (eDirection.Down));
			bool beginning = (rowFirstIndex == selectedIndex && direction.Equals (eDirection.Backward)) || 
				(selectedIndex < columns && direction.Equals (eDirection.Up));

			// loop if the selector is not at the beginning or end of a non-looping menu, or loop indefinitely if a looping menu 
			while (!(beginning || end) || loopItems)
			{
				if (mouse || touch)
				{
					// break if input removed
					if (!Input.GetMouseButton (0))
					{
						break;
					}
				}

				if (controller)
				{
					// break if input removed
					if (ControllerIsNotHolding ())
					{
						break;
					}
				}

				// select next item
				if (itemIsReady) StartCoroutine (NextItem (forward));
				// pause between item selections
				yield return new WaitForSeconds (0.1f);
			}
		}

		/// <summary>
		/// Enables or disables item renderers based on the visibility setting.
		/// </summary>
		protected override void UpdateVisibility ()
		{
			// get column and row indices for selected item
			int selectedIndex = items.IndexOf (selectedItem);
			int selectedColumn = selectedIndex % columns;
			int selectedRow = Mathf.CeilToInt ((selectedIndex + 1) / (float)columns) - 1;

			// enable/disable renderer setting - based on visibility
			for (int i = 0 ; i < items.Count ; i++)
			{
				// check if i is in range of visibility for both axes
				bool enableRendererX = false;
				bool enableRendererY = false;

				// get column and row indices for item
				int column = i % columns;
				int row = Mathf.CeilToInt ((i + 1) / (float)columns) - 1;

				// anchor visibility to the right
				if (!visibilityAnchorX.Equals (eVisibilityAnchorX.Left))
				{
					if (column >= selectedColumn && column <= (selectedColumn + visibilityX))
					{
						enableRendererX = true;
					}
				}

				// anchor visibility to the left
				if (!visibilityAnchorX.Equals (eVisibilityAnchorX.Right))
				{
					if (column <= selectedColumn && column >= (selectedColumn - visibilityX))
					{
						enableRendererX = true;
					}
				}

				// anchor visibility down
				if (!visibilityAnchorY.Equals (eVisibilityAnchorY.Up))
				{
					if (row >= selectedRow && row <= (selectedRow + visibilityY))
					{
						enableRendererY = true;
					}
				}

				// anchor visibility up
				if (!visibilityAnchorY.Equals (eVisibilityAnchorY.Down))
				{
					if (row <= selectedRow && row >= (selectedRow - visibilityY))
					{
						enableRendererY = true;
					}
				}

				// only display item if enableRenderer is true for both axes
				bool enableRenderer = (!enableRendererX || !enableRendererY) ? false : true;

				Renderer renderer;
				UnityEngine.UI.Image image;
				if ((renderer = items[i].GetComponent<Renderer> ()) != null)
				{
					renderer.receiveShadows = enableRenderer;
				}
				else if ((image = items[i].GetComponent<UnityEngine.UI.Image> ()) != null)
				{
					Color c = image.color;
					c.a = enableRenderer ? 255 : 0;
					image.color = c;
				}
				else
				{
					Debug.LogWarning ("Visibility setting failed to find " + items[i].name + "'s renderer");
				}

				// show/hide children
				foreach (Transform child in items[i].transform)
					child.gameObject.SetActive(enableRenderer);
			}
		}
	}
}
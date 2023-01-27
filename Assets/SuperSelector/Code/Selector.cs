using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SuperSelector
{
	abstract public class Selector : MonoBehaviour
	{
		[Header("Input Options:")]

		/// <summary>True if selector should accept input from the mouse.</summary>
		[SerializeField]
		protected bool mouse = true;

		/// <summary>True if selector should accept touch input from mobile devices.</summary>
		[SerializeField]
		protected bool touch = true;

		/// <summary>True if selector should accept input from game controllers, keyboard, and joystick.</summary>
		[SerializeField]
		protected bool controller = true;

		/// <summary>Direction of a selection.</summary>
		public enum eDirection { None, Forward, Backward }

		/// <summary>The axis on which selection occurs.</summary>
		public enum eSelectionAxis { Horizontal, Vertical }


		[Header("Selector Options:")]

		/// <summary>2D (Canvas) or 3D</summary>
		[SerializeField]
		protected bool is2D = false;

		/// <summary>Determines the axis on which the selector should select items.</summary>
		[SerializeField]
		protected eSelectionAxis selectionAxis = eSelectionAxis.Horizontal;

		/// <summary>Determines whether directional controls are flipped.</summary>
		[SerializeField]
		protected bool flippedControls;

		/// <summary>The spacing between items (Note: in degrees for radial selector).</summary>
		[SerializeField]
		protected float spacing;

		/// <summary>
		/// Determines how fast the selector will slide to the next item.
		/// </summary>
		[SerializeField]
		protected float selectorSpeed = 1.0f;

		/// <summary>Determines whether the items can be selected in a loop.</summary>
		[SerializeField]
		protected bool loopItems;

		/// <summary>Determines whether items should slide when selected or remain static.</summary>
		[SerializeField]
		protected bool slideItems;

		/// <summary>Determines whether items should scroll when user holds down key/mouse/finger.</summary>
		[SerializeField]
		protected bool scrollOnHold;

		/// <summary>Determines the number of items that should be visible on the screen.</summary>
		[SerializeField]
		protected int visibility;

		/// <summary>The selected item's index on the selector (NOT the item's index in the list).</summary>
		[SerializeField]
		protected int selectedItemIndex = 0;

		/// <summary>
		/// The pitch angle of the selector in degrees.
		/// 0 degrees for a horizontal selector; -90 degrees for a vertical selector.
		/// </summary>
		[SerializeField]
		protected float rotateVertically;

		/// <summary>The yaw angle of the selector in degrees.</summary>
		[SerializeField]
		protected float rotateHorizontally;

		/// <summary>Cached selector angle.</summary>
		protected Vector3 angle;


		[Header("Selected Item Attributes:")]

		/// <summary>Maximum scale for selected items</summary>
		[SerializeField]
		protected float maxScale = 1.4f;

		/// <summary>Minimum scale for unselected items</summary>
		[SerializeField]
		protected float minScale = 0.7f;

		/// <summary>Determines whether the selected item should have an outline (2D only).</summary>
		//[SerializeField]
		//protected bool outline2D;

		/// <summary>The color of the outline (2D only).</summary>
		//[SerializeField]
		//protected Color outline2DColor;

		/// <summary>The selected item.</summary>
		protected GameObject selectedItem;

		/// <summary>The item selected before the current item.</summary>
		protected GameObject priorSelectedItem;

		/// <summary>List of items in the selector.</summary>
		public List<GameObject> items;

		/// <summary>Transform of the Items gameobject, a child of the Selector gameobject.</summary>
		protected Transform itemsTransform;

		/// <summary>The mouse position.</summary>
		protected Vector3 mousePosition;

		/// <summary>Tracker that moves between items for scaling and selection purposes.</summary>
		protected Vector3 trackerPosition;

		/// <summary>The initial position for the selected item before the selector slides the items.</summary>
		protected Vector3 initialItemPosition;

		/// <summary>The initial scale for the selected item before the selector slides the items.</summary>
		protected Vector3 initialItemScale;

		/// <summary>The input axis value stored when a controller press is made in selecting the next item.</summary>
		protected float controllerPressValue = 0;

		/// <summary>The input axis value stored every frame to check when the controller is being held down.</summary>
		protected float controllerHoldValue = 0;

		/// <summary>Determines whether touch should be ignored or not. Prevents autoscrolling.</summary>
		protected bool touchDisabled = false;

		/// <summary>Determines whether mouse should be ignored or not. Prevents autoscrolling.</summary>
		protected bool mouseDisabled = false;

		/// <summary>Determines whether controller selections should be ignored or not. Prevents autoscrolling.</summary>
		protected bool controllerDisabled = false;

		/// <summary>The input axis value that the controller input must exceed to select the next item.</summary>
		protected float controllerInputThreshold = 0.1f;

		/// <summary>
		/// Event called whenever a new item is selected. Useful for playing animations or setting component variables.
		/// </summary>
		public UnityEvent onSelect;
		/// <summary>
		/// Event called whenever an item is deselected. Useful for resetting components and variables
		/// </summary>
		public UnityEvent onDeselect;


		/*--------------------------------------------------------*/
		/* ------------------- PUBLIC METHODS ------------------- */
		/*--------------------------------------------------------*/

		/// <summary>
		/// Moves the selector to the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public void MoveTo(int index)
		{
			StartCoroutine(ScrollTo(index));
		}

		/// <summary>
		/// Moves the selector to the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public void MoveTo(GameObject item)
		{
			if (!items.Contains(item))
			{
				return;
			}

			MoveTo(items.IndexOf(item));
		}

		/// <summary>
		/// Moves the selector to the first item.
		/// </summary>
		public void MoveToBeginning()
		{
			MoveTo(0);
		}

		/// <summary>
		/// Moves the selector to the last item.
		/// </summary>
		public void MoveToEnd()
		{
			MoveTo(items.Count - 1);
		}

		/// <summary>
		/// Gets the selected item.
		/// </summary>
		/// <returns>The selected item.</returns>
		public GameObject GetSelectedItem()
		{
			return selectedItem;
		}

		/// <summary>
		/// Gets the previously selected item.
		/// </summary>
		/// <returns>The previously selected item.</returns>
		public GameObject GetPriorSelectedItem()
		{
			return priorSelectedItem;
		}

		/// <summary>
		/// Gets the item count.
		/// </summary>
		/// <returns>The item count.</returns>
		public virtual int GetItemCount()
		{
			return transform.Find("Items").childCount;
		}

		/// <summary>
		/// Gets the tracker position. (readonly)
		/// </summary>
		/// <value>The tracker position.</value>
		public Vector3 TrackerPosition
		{
			get
			{
				return trackerPosition;
			}
		}

		/// <summary>
		/// Returns true if selector is 2D
		/// </summary>
		public bool Is2D()
		{
			return is2D;
		}


		/*-----------------------------------------------------------*/
		/* ------------------- PROTECTED METHODS ------------------- */
		/*-----------------------------------------------------------*/

		/* ----- Abstracts ----- */

		/// <summary>
		/// Add the specified item to the selector.
		/// </summary>
		/// <param name="item">Item.</param>
		abstract protected void Add(GameObject item);

		/// <summary>
		/// Raises the select event.
		/// </summary>
		abstract protected void OnSelect();

		/// <summary>
		/// Raises the deselect event.
		/// </summary>
		abstract protected void OnDeselect();

		/// <summary>
		/// Selects the next item in the menu.
		/// </summary>
		/// <param name="forward">If set to <c>true</c> the next item is forward, otherwise its backward.</param>
		abstract protected IEnumerator NextItem(bool forward);

		/// <summary>
		/// Slides the items either forward or backward.
		/// </summary>
		/// <returns><c>true</c>, if the items are still sliding, <c>false</c> otherwise.</returns>
		/// <param name="forward">If set to <c>true</c>, slide the items forward, otherwise slide them backward.</param>
		/// <param name="slideToNext">If set to <c>true</c>, slide the items until the next item is reached, otherwise slide indefinitely.</param>
		abstract protected bool SlideItems(bool forward, bool slideToNext = false);

		/// <summary>
		/// Scales the items based on trackerPosition.
		/// </summary>
		/// <returns><c>true</c>, if the selected item and next item are still being scaled, <c>false</c> otherwise.</returns>
		/// <param name="forward">If set to <c>true</c>, the next item is forward, otherwise backward.</param>
		abstract protected bool ScaleItems(bool forward, bool slide = false);

		/// <summary>
		/// Shifts the first item to the end of the menu.
		/// </summary>
		abstract protected void FirstToLast();

		/// <summary>
		/// Shifts the last item to the beginning of the menu.
		/// </summary>
		abstract protected void LastToFirst();

		/// <summary>
		/// Moves the tracker position between items.
		/// </summary>
		/// <param name="forward">If set to <c>true</c>, moves the tracker forward, otherwise backward.</param>
		abstract protected void MoveTracker(bool forward);

		/// <summary>
		/// Enables or disables item renderers based on the visibility setting.
		/// </summary>
		abstract protected void UpdateVisibility();

		/* ----- Implemented ----- */

		/// <summary>
		/// Initializes the items list by adding all children in the Items gameobject to the list. 
		/// Note: The item gameobject is a child of the selector.
		/// </summary>
		protected virtual void InitItems()
		{
			items = new List<GameObject>();

			itemsTransform = transform.Find("Items");
			if (itemsTransform == null)
			{
				Debug.LogError("The selector must have a child named `Items`.");
			}

			foreach (Transform item in itemsTransform)
			{
				// add each item to the selector
				Add(item.gameObject);
			}
		}

		/// <summary>
		/// Initializes the position of the menu relative to the selected item.
		/// </summary>
		protected virtual void InitMenuPosition()
		{
			// get vector between selected item and selector center
			Vector3 selectedPos = items[selectedItemIndex].transform.localPosition;
			Vector3 centerPos = itemsTransform.localPosition;
			Vector3 distanceVec = centerPos - selectedPos;

			// adjust each item based on distance vector
			for (int i = 0; i < items.Count; i++)
			{
				items[i].transform.localPosition += distanceVec;
			}
		}

		/// <summary>
		/// Returns value of the controller's axis.
		/// </summary>
		/// <returns>The axis value.</returns>
		protected virtual float ControllerAxis()
		{
			return Input.GetAxis(Enum.GetName(typeof(eSelectionAxis), selectionAxis));
		}

		/// <summary>
		/// Determines whether a controller, keyboard, or joystick is selecting on the selected axis.
		/// </summary>
		/// <returns><c>true</c>, if controller is selecting one of the axis' directions, <c>false</c> otherwise.</returns>
		protected virtual bool ControllerIsSelecting()
		{
			return ControllerAxis() != 0;
		}

		/// <summary>
		/// Determines whether a controller, keyboard, or joystick is not holding on the selected axis.
		/// Differs from ControllerIsSelecting in that ControllerIsSelecting will return false even if the player slightly taps in a direction
		/// </summary>
		/// <returns><c>true</c>, if controller is not holding down one of the axis' directions, <c>false</c> otherwise.</returns>
		protected virtual bool ControllerIsNotHolding()
		{
			float axis = ControllerAxis();
			return (axis != 1) && (axis != -1);
		}

		/// <summary>
		/// Gets the direction of the controller on the selected axis.
		/// </summary>
		/// <returns>Either Backward or Forward.</returns>
		protected virtual eDirection GetControllerDirection()
		{
			return ControllerAxis() < 0 ? eDirection.Backward : eDirection.Forward;
		}

		/// <summary>
		/// Gets the drag direction of a mouse or touch.
		/// </summary>
		/// <returns>The drag direction.</returns>
		/// <param name="dragVector">Drag vector.</param>
		protected virtual eDirection GetDragDirection(Vector3 dragVector)
		{
			Vector3 dotVector = (GetDragAxis(dragVector).Equals(eSelectionAxis.Horizontal)) ? Vector3.left : Vector3.up;
			return Vector3.Dot(Vector3.Normalize(dragVector), dotVector) < 0 ? eDirection.Backward : eDirection.Forward;
		}

		/// <summary>
		/// Gets the axis on which the mouse or touch is dragging.
		/// </summary>
		/// <returns>The drag axis.</returns>
		/// <param name="dragVector">Drag vector.</param>
		protected eSelectionAxis GetDragAxis(Vector3 dragVector)
		{
			float dragDot = Vector3.Dot(Vector3.Normalize(dragVector), itemsTransform.right);
			return (dragDot <= 0.5f && dragDot >= -0.5f) ? eSelectionAxis.Vertical : eSelectionAxis.Horizontal;
		}

		/// <summary>
		/// Gets the selector angle.
		/// </summary>
		/// <returns>A vector containing the angle of the selector across all planes.</returns>
		protected Vector3 GetSelectorAngle()
		{
			// calculate angle vector components
			float pitch = rotateVertically;
			float yaw = rotateHorizontally;
			float x = Mathf.Cos(Mathf.Deg2Rad * pitch) * Mathf.Cos(Mathf.Deg2Rad * yaw);
			float y = Mathf.Sin(Mathf.Deg2Rad * pitch) * Mathf.Cos(Mathf.Deg2Rad * yaw);
			float z = Mathf.Sin(Mathf.Deg2Rad * yaw);

			// return selector angle vector
			Vector3 selectorAngle = new Vector3(x, y, z);
			selectorAngle.Normalize();
			return selectorAngle * spacing;
		}

		/// <summary>
		/// Maps one range of float values onto another.
		/// </summary>
		protected static float MapValues(float value1, float low1, float high1, float low2, float high2)
		{
			return low2 + (high2 - low2) * (value1 - low1) / (high1 - low1);
		}

		/// <summary>
		/// Multiplies two vectors with each other.
		/// </summary>
		/// <returns>The resulting vector.</returns>
		/// <param name="lh">Lefthand.</param>
		/// <param name="rh">Righthand.</param>
		protected static Vector3 MultiplyVector(Vector3 lh, Vector3 rh)
		{
			Vector3 v = Vector3.zero;
			v.x = lh.x * rh.x;
			v.y = lh.y * rh.y;
			v.z = lh.z * rh.z;
			return v;
		}

		/// <summary>
		/// IEnumerator that scrolls to the specified index in the selector.
		/// </summary>
		/// <returns>IEnumerator.</returns>
		/// <param name="index">Index of the item.</param>
		protected virtual IEnumerator ScrollTo(int index)
		{
			if (items.Count <= index)
			{
				yield break;
			}

			// true if index is in front of the selected item
			bool forward = items.IndexOf(GetSelectedItem()) < index;

			// item to scroll to
			GameObject scrollToItem = items[index];

			// loop until reaching index
			while (!selectedItem.Equals(scrollToItem))
			{
				// select next item
				if (itemIsReady) StartCoroutine(NextItem(forward));
				// pause between item selections
				yield return new WaitForSeconds(0.1f);
			}
		}
		/// <summary>True if NextItem coroutine is ready to be called again.</summary>
		protected bool itemIsReady = true;

		/// <summary>
		/// IEnumerator that scrolls the items automatically when the player holds down a directional button or touch.
		/// </summary>
		/// <param name="forward">If set to <c>true</c>, the next item is forward, otherwise backward.</param>
		protected virtual IEnumerator ScrollOnHold(bool forward)
		{
			bool lastMouseInput = Input.GetMouseButton(0);
			bool lastControllerInput = !ControllerIsNotHolding();

			// wait before for starting autoscroll
			yield return new WaitForSeconds(0.1f);
			
			// recheck direction
			eDirection direction = forward ? eDirection.Forward : eDirection.Backward;
			
			
			if ((mouse || touch) && lastMouseInput)
			{
				// update direction if necessary
				Vector3 dragVector = Input.mousePosition - mousePosition;
				bool sameDirection = GetDragDirection(dragVector).Equals(direction);
				if ((!flippedControls && !sameDirection) || (flippedControls && sameDirection))
				{
					forward = !forward;
				}
			}

			if (controller && lastControllerInput)
			{
				// update direction if necessary
				bool sameDirection = GetControllerDirection().Equals(direction);
				if ((!flippedControls && !sameDirection) || (flippedControls && sameDirection))
				{
					forward = !forward;
				}
			}

			// loop if the selector is not at the beginning or end of a non-looping menu, or loop indefinitely if a looping menu
			while ((forward ? !items.IndexOf(selectedItem).Equals(items.Count - 1) : !items.IndexOf(selectedItem).Equals(0)) || loopItems)
			{
				if (mouse || touch)
				{
					// break if input removed
					if (Input.GetMouseButton(0) != lastMouseInput)
					{
						break;
					}
				}

				if (controller)
				{
					// break if input removed
					if (ControllerIsNotHolding() == lastControllerInput)
					{
						break;
					}
				}

				// select next item
				if (itemIsReady) StartCoroutine(NextItem(forward));
				// pause between item selections
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}
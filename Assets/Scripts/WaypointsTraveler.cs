using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsTraveler : MonoBehaviour
{

    [InspectorButton("SnapToWaypoint")]
    public bool snapToWaypoint;
    public WaypointsGroup Waypoints = null;
    public int StartIndex = 0;
    public bool AutoPositionAtStart = true;
    public TravelDirection StartTravelDirection = TravelDirection.FORWARD;
    public AudioClip waypointSound = null;

    public int positionIndex = 0; // Index of the next waypoint to move toward
    public List<Waypoint> waypointsList; //Reference to the list of waypoints located in Waypoints 

    Vector3 nextPosition;
    Vector3 startPosition;
    Vector3 destinationPosition;

    int travelIndexCounter = 1;
    public float beatProgress = 0;
    public Vector3 beatPosition = Vector3.zero;
    public int beatIndex = 0;

    bool isMoving = false; // Movement on/off

    Vector3 positionOriginal;
    Quaternion rotationOriginal;


    void Start()
    {
        waypointsList = Waypoints.waypoints;

        positionOriginal = transform.position;
        rotationOriginal = transform.rotation;

        ResetTraveler();
    }

    void Update()
    {
        isMoving = transform.parent.GetComponent<TrackSettings>().playTrack;

        if (isMoving == true && BeatLerp())
        {
            if (waypointSound != null && positionIndex == 0)
            {
                AudioSource.PlayClipAtPoint(waypointSound, Camera.main.transform.position);
            }
            SetNextPosition();
        }
    }

    public void SnapToWaypoint()
    {
        transform.position = waypointsList[StartIndex].GetPosition();
    }

    public void ResetTraveler()
    {
        transform.SetPositionAndRotation(positionOriginal, rotationOriginal);

        StartAtIndex(StartIndex, AutoPositionAtStart);
        travelIndexCounter = StartTravelDirection == TravelDirection.REVERSE ? -1 : 1;

    }

    public void SetWaypointsGroup(WaypointsGroup newGroup)
    {
        Waypoints = newGroup;
        waypointsList = null;
        if (newGroup != null)
        {
            waypointsList = newGroup.waypoints;
        }

    }

    public void StartAtIndex(int ndx, bool autoUpdatePosition = true)
    {
        StartIndex = ndx;
        if (StartTravelDirection == TravelDirection.REVERSE)
            ndx = waypointsList.Count - ndx - 1;

        ndx = Mathf.Clamp(ndx, 0, waypointsList.Count - 1);
        positionIndex = ndx - 1;
        if (autoUpdatePosition)
        {
            transform.position = waypointsList[ndx].GetPosition();
        }
        SetNextPosition();
    }

    void SetNextPosition()
    {
        int posCount = waypointsList.Count;
        if (posCount > 0)
        {
            positionIndex += travelIndexCounter;

            if (positionIndex >= posCount)
                positionIndex = 0;
            else if (positionIndex < 0)
                positionIndex = posCount - 1;


            nextPosition = waypointsList[positionIndex].GetPosition();
            nextPosition.z = transform.position.z;

            startPosition = transform.position;
            destinationPosition = nextPosition;
        }
    }


    bool BeatLerp()
    {

        // beat progress since last frame
        float frac = beatProgress - Mathf.Floor(beatProgress);

        beatProgress = BeatManager.Instance.beatProgress;
        beatPosition = Vector3.Lerp(startPosition, destinationPosition, frac);
        transform.position = beatPosition;

        // split beatProgress into int and fraction
        if ((int)beatProgress != beatIndex)
        {
            beatIndex = (int)beatProgress;
            return true;
        }
        return false;
    }

}

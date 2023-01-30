using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{

    void Start()
    {
        // get Track component
        GameObject track = GameObject.Find("GameObjects/Tracks/Track");
        // get line renderer from track
        LineRenderer trackLineRenderer = track.GetComponent<LineRenderer>();
        // get first two points from line renderer
        Vector3 point1 = trackLineRenderer.GetPosition(0);
        // get last point from line renderer
        Vector3 point2 = trackLineRenderer.GetPosition(trackLineRenderer.positionCount - 1);

        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        // lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, point1);
        lineRenderer.SetPosition(1, point2);
    }


}
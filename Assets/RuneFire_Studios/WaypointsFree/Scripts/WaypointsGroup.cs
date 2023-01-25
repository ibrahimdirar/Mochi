using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaypointsFree
{
    public class WaypointsGroup : MonoBehaviour
    {
        public PositionConstraint XYZConstraint = PositionConstraint.XYZ;
        [HideInInspector]
        public List<Waypoint> waypoints;   // The waypoint components controlled by this WaypointsGroupl IMMEDIATE children only
        private LineRenderer lineRenderer;

        public int vertices = 4;
        public int radius = 1;

        [InspectorButton("GenerateVertices")]
        public bool generateVertices;

        private void Awake()
        {
            if(waypoints != null)
            {
                foreach (Waypoint wp in waypoints)
                    wp.SetWaypointGroup(this);
            }
            lineRenderer = GetComponent<LineRenderer>();
            // set linerender size to number of vertices
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            Draw();
        }

        // Update is called once per frame
        void Update()
        {
            // Draw();
        }

        void GenerateVertices(){
            waypoints = new List<Waypoint>();
            for (int i = 0; i < vertices; i++)
            {
                Waypoint wp = new Waypoint();
                Vector3 vertex = new Vector3(Mathf.Cos(i * 2 * Mathf.PI / vertices), Mathf.Sin(i * 2 * Mathf.PI / vertices), 0);
                vertex *= radius;
                vertex += transform.position;
                wp.UpdatePosition(vertex, PositionConstraint.XY);
                AddWaypoint(wp, i);
            }
            Draw();
        }

        void Draw(){
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = waypoints.Count;
            for (int i = 0; i < waypoints.Count; i++)
            {
                lineRenderer.SetPosition(i, waypoints[i].XY);
            }
        }

        /// <summary>
        /// Returns a list of  Waypoints; resets the parent transform if reparent == true
        /// </summary>
        /// <returns></returns>
        public List<Waypoint> GetWaypointChildren(bool reparent = true)
        {
            if (waypoints == null)
                waypoints = new List<Waypoint>();

            if(reparent == true)
            { 
                foreach (Waypoint wp in waypoints)
                    wp.SetWaypointGroup(this);
             }


            return waypoints;
        }


        public void AddWaypoint(Waypoint wp, int ndx = -1)
        {
            if (waypoints == null) waypoints = new List<Waypoint>();
            if (ndx == -1)
                waypoints.Add(wp);
            else
                waypoints.Insert(ndx, wp);
            wp.SetWaypointGroup(this);
        }

    }
}
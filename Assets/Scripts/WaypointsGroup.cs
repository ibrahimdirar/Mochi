using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
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
        if (waypoints != null)
        {
            foreach (Waypoint wp in waypoints)
                wp.SetWaypointGroup(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // if name of this object is "Track", then set gradient to blue
        // if name of this object is "TargetTrack" the entire track is white
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;

        GenerateVertices();
    }

    // Update is called once per frame
    void Update()
    {
        GenerateVertices();
    }

    void GenerateVertices()
    {
        vertices = BeatManager.Instance.beatsPerSound;
        waypoints = new List<Waypoint>();
        for (int i = 0; i < vertices; i++)
        {
            Waypoint wp = new();
            Vector3 vertex = new(Mathf.Cos(i * 2 * Mathf.PI / vertices), Mathf.Sin(i * 2 * Mathf.PI / vertices), 0);
            vertex *= radius;
            // vertex += transform.position;
            wp.UpdatePosition(vertex, PositionConstraint.XY);
            AddWaypoint(wp, i);
        }
        Draw();
    }

    void Draw()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = waypoints.Count;
        for (int i = 0; i < waypoints.Count; i++)
        {
            lineRenderer.SetPosition(i, waypoints[i].XY + transform.position);
        }
    }

    /// <summary>
    /// Returns a list of  Waypoints; resets the parent transform if reparent == true
    /// </summary>
    /// <returns></returns>
    public List<Waypoint> GetWaypointChildren(bool reparent = true)
    {
        waypoints ??= new List<Waypoint>();

        if (reparent == true)
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

    // number vertices by index on gizmos
    void OnDrawGizmos()
    {
        if (waypoints == null) return;

        // set label style of black font and white background with size 10 font
        GUIStyle style = new();
        style.normal.textColor = Color.black;
        style.fontSize = 10;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.background = Texture2D.whiteTexture;

        for (int i = 0; i < waypoints.Count; i++)
        {
            UnityEditor.Handles.Label(waypoints[i].XY + transform.position, i.ToString(), style);
        }

    }

    // draw string on gizmos
    void DrawString(string text, Vector3 worldPos, Color? colour = null)
    {
        _ = GUI.color;
        if (colour.HasValue) GUI.color = colour.Value;
        var view = SceneView.currentDrawingSceneView;
        if (view != null)
        {
            var screenPos = view.camera.WorldToScreenPoint(worldPos);
            Handles.BeginGUI();
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + (size.y / 2), size.x, size.y), text);
            Handles.EndGUI();
        }
    }

}
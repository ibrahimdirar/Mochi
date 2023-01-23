using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    // number of vertices, 1 = circle, 2 = line, 3 = triangle, 4 = square, etc.
    public int vertices = 3;

    // radius of the shape
    public float radius = 1f;

    // speed of the object on this path
    public float speed = 5f;

    // list of orbs attached to this path
    public List<Orb> orbs = new List<Orb>();

    // list of positions on the path
    public List<Vector3> positions = new List<Vector3>();

    // snap the orbs to the path
    public bool snap = true;

    private LineRenderer lineRenderer = null;

    public bool isClockwise = true;


    // make path editable in the editor
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // set linerender size to number of vertices
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.positionCount = vertices;
        lineRenderer.loop = true;

        // create the path
        CreatePath();
        DrawPath();

        // add all children orbs to the orbs list
        Orb[] childrenOrbs = GetComponentsInChildren<Orb>();
        foreach (Orb orb in childrenOrbs)
        {
            orbs.Add(orb);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
    }


    void CreatePath()
    {
        // clear the positions list
        positions.Clear();

        // calculate the angle between each vertex
        float angle = 360f / vertices * (isClockwise ? 1 : -1);

        // loop through the vertices
        for (int i = 0; i < vertices; i++)
        {
            // calculate the vertex position
            Vector3 position = Quaternion.AngleAxis(angle * i, Vector3.forward) * Vector3.right * radius;

            // rotate the position around the transform's position
            position = Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.forward) * position;
            // shift the postion by the transform's position
            position += transform.position;

            // set position z to -1
            position.z = Constants.OrbZPosition;

            // add the position to the list
            positions.Add(position);

        }
    
    }

    void DrawPath()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // loop through the positions
        for (int i = 0; i < positions.Count; i++)
        {
            // get the position
            Vector3 position = positions[i];

            // get the next position
            Vector3 nextPosition = positions[(i + 1) % positions.Count];

            // draw a line between the positions
            lineRenderer.SetPosition(i, position);
            Debug.DrawLine(position, nextPosition, Color.white, 1000f);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionManager : MonoBehaviour
{
    public List<GameObject> tracks;
    public int screenWidth;
    public int screenHeight;

    void Position()
    {
        GameObject track = GameObject.Find("GameObjects/Tracks/Track");
        GameObject targetTrack = GameObject.Find("GameObjects/Tracks/TargetTrack");

        Vector3 pos = track.transform.localPosition;

        // get width and height of track from line renderer
        // and set the position of the track to the center of the screen
        // minus half the width and height of the track
        // pos.x -= track.GetComponent<LineRenderer>().bounds.size.x / 2 * 75;
        // pos.y -= targetTrack.GetComponent<LineRenderer>().bounds.size.y / 2 * 50;

        // set local position of track
        // tracks[i].transform.localPosition = pos;
        pos.x = Screen.width / 4;
        targetTrack.transform.localPosition = -pos;
        track.transform.localPosition = pos;

    }

    void Awake()
    {
        Position();
    }

    // Start is called before the first frame update
    void Start()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;
    }

    // Update is called once per frame
    void Update()
    {
        Position();
    }
}

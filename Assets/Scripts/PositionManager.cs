using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointsFree;

public class PositionManager : MonoBehaviour
{
    public bool isLandscape = true;
    public List<GameObject> tracks;
    public GameObject trackCollection;
    public int screenWidth;
    public int screenHeight;

    void Position(){
        trackCollection = GameObject.Find("GameObjects/Tracks");
        tracks = new List<GameObject>();
        for (int i = 0; i < trackCollection.transform.childCount; i++){
            tracks.Add(trackCollection.transform.GetChild(i).gameObject);
        }


        // divide the screen into equal parts for each track
        // and place the track in the center of that part
        for (int i = 0; i < tracks.Count; i++){
            Vector3 pos = new Vector3((Screen.width / 2) * (i + 0.5f), (Screen.height / tracks.Count), 0);

        // get width and height of track from line renderer
        // and set the position of the track to the center of the screen
        // minus half the width and height of the track
        pos.x -= (tracks[i].GetComponent<LineRenderer>().bounds.size.x / 2) * 75;
        pos.y -= (tracks[i].GetComponent<LineRenderer>().bounds.size.y / 2) * 50;

        // set local position of track
        tracks[i].transform.localPosition = pos;

        }
    }

    void Awake(){
        Position();
    }

    // Start is called before the first frame update
    void Start()
    {
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }

    // Update is called once per frame
    void Update()
    {
        Position();
    }
}

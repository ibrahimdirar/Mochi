using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour 
{

    public Vector3 nextPosition;
    public int nextPositionIndex;
    public Track track;
    public float distanceToNextPosition;
    
    void Awake(){
        // get track component which is the parent of the orb
        UpdateNextPosition();

    }

    void Start(){}

    void Update(){
        if (GameManager.Instance.levelActive){
            // get the current position as Vector2 with x and y
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
            // get the next position as Vector2 with x and y
            Vector2 nextPosition2D = new Vector2(nextPosition.x, nextPosition.y);

            distanceToNextPosition = Vector2.Distance(currentPosition, nextPosition2D);

            // when orb hits the next position, move to the next position
            if (distanceToNextPosition < 0.001f){
                // get orb sound from Audio Source component
                AudioSource orbSound = GetComponent<AudioSource>();
                // play orb sound
                orbSound.Play();
                // update next position
                UpdateNextPosition();
                // move the orb to the next position
            }
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, track.speed * Time.deltaTime);
        }

    }

    void OnCollisionEnter(Collision collision){
        // if the orb hits another orb, end the level
        if (collision.gameObject.tag == "Orb"){
            // end the level
            GameManager.Instance.EndLevel();
        }
    }

    public void SnapToPath(){
        // snap the orb to the path if snap is set
        if (track.snap){
            // get the closest point on the line
            Vector3 closestPoint = ClosestPointOnLine();
            // move the orb to the closest point
            transform.position = closestPoint;
        }
    }

    void UpdateNextPosition(){
        // update the next position index
        nextPositionIndex++;
        track = GetComponentInParent<Track>() as Track;
        // if the next position index is greater than the number of positions in the track
        if (nextPositionIndex >= track.positions.Count){
            // set the next position index to 0
            nextPositionIndex = 0;
        }
        // set the next position
        nextPosition = track.positions[nextPositionIndex];
        // set the z position to -2
        nextPosition.z = Constants.OrbZPosition;
    }

    Vector3 ClosestPointOnLine(){
        List<Vector3> closestPositions = GetTwoClosestPositions();
        Vector3 closestPosition = closestPositions[0];
        Vector3 secondClosestPosition = closestPositions[1];

        Vector3 vVector1 = transform.position - closestPosition;
        Vector3 vVector2 = (secondClosestPosition - closestPosition).normalized;

        float d = Vector3.Distance(closestPosition, secondClosestPosition);
        float t = Vector3.Dot(vVector2, vVector1);

        if (t <= 0)
            return closestPosition;

        if (t >= d)
            return secondClosestPosition;

        Vector3 vVector3 = vVector2 * t;

        Vector3 vClosestPoint = closestPosition + vVector3;

        // set z position to -2
        vClosestPoint.z = -2;

        return vClosestPoint;
    }

    List<Vector3> GetTwoClosestPositions(){
        // create a list of the two closest positions
        List<Vector3> closestPositions = new List<Vector3>();

        // smallest distance
        float smallestDistance = Mathf.Infinity;
        // second smallest distance
        float secondSmallestDistance = Mathf.Infinity;

        // closest position
        Vector3 closestPosition = Vector3.zero;
        // second closest position
        Vector3 secondClosestPosition = Vector3.zero;

        track = GetComponentInParent<Track>() as Track;

        // loop through the positions
        for (int i = 0; i < track.positions.Count; i++){
            // get the position
            Vector3 position = track.positions[i];
            // get the distance between the orb and the position
            float distance = Vector3.Distance(transform.position, position);
            // if the distance is smaller than the smallest distance
            if (distance < smallestDistance){
                // set the second smallest distance to the smallest distance
                secondSmallestDistance = smallestDistance;
                // set the second closest position to the closest position
                secondClosestPosition = closestPosition;
                // set the smallest distance to the distance
                smallestDistance = distance;
                // set the closest position to the position
                closestPosition = position;
            }
            // if the distance is smaller than the second smallest distance
            else if (distance < secondSmallestDistance){
                // set the second smallest distance to the distance
                secondSmallestDistance = distance;
                // set the second closest position to the position
                secondClosestPosition = position;
            }
        }

        // add the closest position to the list
        closestPositions.Add(closestPosition);
        // add the second closest position to the list
        closestPositions.Add(secondClosestPosition);

        // return the list
        return closestPositions;

    }

    // snap the orb to the path when the orb is moved on the scene editor
    void OnDrawGizmosSelected(){
        SnapToPath();
    }
}
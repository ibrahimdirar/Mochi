using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{

    public float speed = 0.1f;

    // Update is called once per frame
    void Update()
    {
        // move background to the left at a constant speed
        transform.Translate(speed * Time.deltaTime * Vector3.left);
    }
}

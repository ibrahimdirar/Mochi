using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{

    public float beatsPerSecond = 5;
    public float beatProgress = 0;
    public int beatsPerSound = 4;
    public int currentBeat = 0;

    private static BeatManager _Instance;
    public static BeatManager Instance
     {
         get
         {
             if (_Instance == null)
             {
                 _Instance = GameObject.FindObjectOfType<BeatManager>();
             }

             return _Instance;
         }
     }

    void Update()
    {
        beatProgress += Time.deltaTime * beatsPerSecond;
        currentBeat = Mathf.FloorToInt(beatProgress) % beatsPerSound;
    }
}

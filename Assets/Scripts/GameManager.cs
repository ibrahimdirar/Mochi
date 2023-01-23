using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int level = 1;
    public bool levelActive = false;

    private static GameManager _Instance;
    public static GameManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<GameManager>();
            }

            return _Instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // once the level has finished loading set levelActive to true
        levelActive = true;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EndLevel(){
        if (levelActive){
            levelActive = false;
            // end the level
            Debug.Log("Level Ended");

            // get all orbs in the scene
            Orb[] orbs = FindObjectsOfType<Orb>();
            // cause all orbs to fall off the screen down y axis
            foreach (Orb orb in orbs){
                // give the orb a random force
                orb.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * 5, ForceMode.Impulse);
                // turn on gravity for orbs
                orb.GetComponent<Rigidbody>().useGravity = true;

            }

        }
    }
}

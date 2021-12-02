using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beeping : MonoBehaviour
{
    
    [SerializeField] AudioSource beeping;
    [SerializeField] PlayerController movementScript;
    float timeSinceLastMove;

    bool hasPlayed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!(movementScript.currentX > 0.1f || movementScript.currentX < -0.1f || movementScript.currentZ > 0.1f || movementScript.currentZ < -0.1f))
        {
            timeSinceLastMove += Time.deltaTime;
            if(timeSinceLastMove > 10f)
            {
                if(hasPlayed == false)
                {
                    if(beeping.isPlaying == false)
                    {
                        beeping.Play();
                        hasPlayed = true;
                    }
                    
                }
            }
        }
        else
        {
            timeSinceLastMove = 0f;
            hasPlayed = false;
        }
    }
}

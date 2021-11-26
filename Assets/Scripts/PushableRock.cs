using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableRock : MonoBehaviour
{
    [SerializeField] PhysicMaterial pushing;
    [SerializeField] PhysicMaterial still;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Collider>().material = still;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter (Collision other) 
    {
        if(other.gameObject.tag == "player")
        {
            GetComponent<Collider>().material = pushing;
        }
    }
 
    void OnCollisionExit (Collision other) 
    {
        if(other.gameObject.tag == "player")
        {
            GetComponent<Collider>().material = still;
        }
    }
}

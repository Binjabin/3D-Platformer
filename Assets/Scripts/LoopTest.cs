using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OneToHundred();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OneToHundred()
    {
        for (int i = 0; i > -1000; i--)
        {
            Debug.Log(i);
        }
    }
}

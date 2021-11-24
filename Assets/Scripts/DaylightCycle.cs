using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaylightCycle : MonoBehaviour
{
    [SerializeField] GameObject planet;
    [SerializeField] GameObject sun;

    [SerializeField] float rotationSpeed;


    // Update is called once per frame
    void Update()
    {
        float newX = rotationSpeed * Time.deltaTime;
        transform.Rotate(newX, 0, 0);
    }

    void Start()
    {
        sun.transform.eulerAngles = new Vector3(90, 0, 0);
    }
}

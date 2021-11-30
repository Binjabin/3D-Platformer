using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    public float gravity = -9.8f;
    Vector3 gravityUp;


    public void Attract(Transform body)
    {
        gravityUp = (body.position - transform.position).normalized;
        Vector3 localUp = body.up;

        // Apply downwards gravity to body
        body.GetComponent<Rigidbody>().AddForce(gravityUp * gravity);
        // Allign bodies up axis with the centre of planet
        body.rotation = Quaternion.FromToRotation(localUp, gravityUp) * body.rotation;
        body.transform.eulerAngles = body.rotation.eulerAngles;
    }
}

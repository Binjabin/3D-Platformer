using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private vector2 Sensitivity;
    private Vector2 rotation;

    private Vector2 GetInput()
    {
        Vector2 input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        return input;
    }

    private void Update()
    {
        Vector2 wantedVelocity = GetInput() * Sensitivity;
        rotation += wantedVelocity * Time.deltaTime;
        transform.localEulerAngles = new Vector3(rotation.y, rotation.x, 0); 
    }
    
}

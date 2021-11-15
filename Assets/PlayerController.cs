using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float xInput;
    float zInput;

    Vector3 inputDirection;
    Vector3 moveAmount;
    Vector3 finalVelocity;
    Vector3 desiredVelocity;

    float minGroundDotProduct;
    float maxGroundAngle;
    Vector3 contactNormal;

    Vector3 currentVelocity;
    bool desiredJump;
    [SerializeField] float maxAcceleration, maxAirAcceleration = 1f;
    float maxSpeedChange;

    [SerializeField] float moveSpeed;

    Rigidbody rigidbody;

    [SerializeField] float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    int jumpPhase;

    [SerializeField] int groundContactCount;
    bool OnGround => groundContactCount > 0;

    Vector3 gravityUp;
    [SerializeField] GameObject planetObject;

    // Start is called before the first frame update

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle);
    }

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        OnValidate();
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");
        gravityUp = (transform.position - planetObject.transform.position).normalized;

        inputDirection = new Vector3(xInput, 0, zInput).normalized;
        desiredVelocity = inputDirection * moveSpeed;

        desiredJump |= Input.GetButtonDown("Jump");

    }

    void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }



        finalVelocity = transform.TransformDirection(currentVelocity);

        rigidbody.velocity = finalVelocity;
        ClearState();
    }

    void Jump()
    {
        if (OnGround || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(currentVelocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            currentVelocity += contactNormal * jumpSpeed;
        }
    }




    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (Vector3.Dot(gravityUp, normal) >= minGroundDotProduct)
            {
                contactNormal += normal;
                groundContactCount += 1;
            }


        }
    }

    void UpdateState()
    {
        currentVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        if (OnGround)
        {
            jumpPhase = 0;
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        Vector3 flattenedNormal = transform.InverseTransformDirection(contactNormal);
        return vector - flattenedNormal * Vector3.Dot(vector, flattenedNormal);
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(currentVelocity, xAxis);
        float currentZ = Vector3.Dot(currentVelocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        currentVelocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }
}
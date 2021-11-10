using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject planetObject;

    float xInput;
    float zInput;

    Vector3 inputDirection;
    Vector3 moveAmount;
    Vector3 finalVelocity;
    Vector3 desiredVelocity;

    Vector3 gravityUp;

    Vector3 contactNormal;


    Vector3 currentVelocity;
    bool desiredJump;
    [SerializeField] float maxAcceleration, maxAirAcceleration = 1f;
    float maxSpeedChange;

    [SerializeField] float moveSpeed;

    float minGroundDotProduct;

    

    Rigidbody rigidbody;

    [SerializeField] float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    int jumpPhase;

    bool onGround;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        gravityUp = (rigidbody.position - planetObject.transform.position).normalized;
        

        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(xInput, 0, zInput).normalized;
        desiredVelocity = inputDirection * moveSpeed;

        desiredJump |= Input.GetButtonDown("Jump");



    }

    void FixedUpdate()
    {
        UpdateState();
        float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        maxSpeedChange = acceleration * Time.fixedDeltaTime;
        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, desiredVelocity.x, maxSpeedChange);
        currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, desiredVelocity.z, maxSpeedChange);

        if(desiredJump)
        {
            desiredJump = false;
            Jump();
        }



        finalVelocity = transform.TransformDirection(currentVelocity);

        rigidbody.velocity = finalVelocity;
        onGround = false;
    }

    void Jump()
    {
        if(onGround || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            if (currentVelocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - currentVelocity.y, 0f);
            }
            currentVelocity += jumpSpeed * transform.InverseTransformDirection(contactNormal);
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
            if (minGroundDotProduct <= Vector3.Dot(gravityUp, normal))
            {
                onGround = true;
                contactNormal = normal;
            }
            Debug.Log(Vector3.Dot(gravityUp, normal));

        }
    }

    void UpdateState()
    {
        currentVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        if (onGround)
        {
            jumpPhase = 0;
        }
        else
        {
            contactNormal = gravityUp;
        }
    }
    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        OnValidate();
    }
}

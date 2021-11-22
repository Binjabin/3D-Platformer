using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject playerVisuals;
    Animator playerAnimator;
    
    float xInput;
    float zInput;

    float currentX;
    float currentZ;

    Vector3 inputDirection;
    Vector3 moveAmount;
    Vector3 finalVelocity;
    Vector3 desiredVelocity;

    float minGroundDotProduct, minStairsDotProduct;
    [SerializeField] float maxGroundAngle, maxStairsAngle = 50f;

    Vector3 contactNormal, steepNormal;
    [SerializeField] int groundContactCount, steepContactCount;
    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;

    Vector3 currentVelocity;
    bool desiredJump;
    [SerializeField] float maxAcceleration, maxAirAcceleration = 1f;
    float maxSpeedChange;

    [SerializeField] float moveSpeed;

    Rigidbody rigidbody;

    [SerializeField] float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] int maxAirJumps = 0;
    int jumpPhase;

    Vector3 xAxis;
    Vector3 zAxis;

    Vector3 gravityUp, gravityForward, gravityRight;
    [SerializeField] GameObject planetObject;
    [SerializeField, Range(0f, 100f)] float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)] float probeDistance = 1f;
    [SerializeField] LayerMask probeMask = -1, stairsMask = -1;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    [SerializeField]Transform playerInputSpace = default;

    // Start is called before the first frame update

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerAnimator = playerVisuals.GetComponent<Animator>();
        OnValidate();
    }

    void DoVisuals()
    {
        inputDirection = new Vector3(xInput, 0, zInput).normalized;
        if(inputDirection != Vector3.zero)
        {
            Quaternion visualsTargetRotation = Quaternion.LookRotation(currentX * xAxis + currentZ * zAxis);
            Quaternion newRotation = Quaternion.Lerp(playerVisuals.transform.localRotation, visualsTargetRotation, Time.deltaTime * 5f);
            playerVisuals.transform.localEulerAngles = new Vector3(0, newRotation.eulerAngles.y, 0);
        }

        if(currentX > 0.1f || currentX < -0.1f || currentZ > 0.1f || currentZ < -0.1f)
        {
            playerAnimator.SetBool("isWalking", true);
            float currentWalkSpeed = (new Vector3(currentX, 0, currentZ)).magnitude;
            playerAnimator.SetFloat("walkSpeed", currentWalkSpeed / 5);
        }
        else
        {
            playerAnimator.SetBool("isWalking", false);
            playerAnimator.SetFloat("walkSpeed", 0);
        }
        if(groundContactCount > 0)
        {
            playerAnimator.SetBool("isGrounded", true);
        }
        else
        {
            playerAnimator.SetBool("isGrounded", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");
        gravityUp = (transform.position - planetObject.transform.position).normalized;


        if (playerInputSpace) 
        {
			gravityRight = ProjectDirectionOnPlane(playerInputSpace.right, gravityUp);
			gravityForward = ProjectDirectionOnPlane(playerInputSpace.forward, gravityUp);
		}
		else 
        {
			gravityRight = ProjectDirectionOnPlane(Vector3.right, gravityUp);
			gravityForward = ProjectDirectionOnPlane(Vector3.forward, gravityUp);
		}
		desiredVelocity = new Vector3(inputDirection.x, 0f, inputDirection.z) * moveSpeed;

        desiredJump |= Input.GetButtonDown("Jump");

    }

    void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        DoVisuals();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }



        rigidbody.velocity = currentVelocity;
        ClearState();
    }



    void Jump()
    {
        playerAnimator.SetTrigger("Jump");
        Vector3 jumpDirection;
        if(OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if(OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if(maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            
            return;
        }

        jumpPhase += 1;
        if(stepsSinceLastJump > 1)
        {
            jumpPhase = 0;
        }
        stepsSinceLastJump = 0;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        jumpDirection = (jumpDirection + gravityUp).normalized;
        float alignedSpeed = Vector3.Dot(currentVelocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        currentVelocity += jumpDirection * jumpSpeed;
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
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (Vector3.Dot(gravityUp, normal) >= minDot)
            {
                contactNormal += normal;
                groundContactCount += 1;
            }
            else if (Vector3.Dot(gravityUp, normal) > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }

        }
    }
    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        currentVelocity = rigidbody.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
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

    Vector3 ProjectDirectionOnPlane (Vector3 direction, Vector3 normal) 
    {
		return (direction - normal * Vector3.Dot(direction, normal)).normalized;
	}

    void AdjustVelocity()
    {
        xAxis = ProjectDirectionOnPlane(gravityRight, contactNormal);
		zAxis = ProjectDirectionOnPlane(gravityForward, contactNormal);


        currentX = Vector3.Dot(currentVelocity, xAxis);
        currentZ = Vector3.Dot(currentVelocity, zAxis);



        
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        
        currentVelocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    void ClearState()
    {
        groundContactCount = 0;
        steepContactCount = 0;
        contactNormal = Vector3.zero;
        steepNormal = Vector3.zero;
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = currentVelocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(rigidbody.position, -gravityUp, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }
        groundContactCount = 1;
        contactNormal = hit.normal;
        
        float dot = Vector3.Dot(currentVelocity, hit.normal);
        if (dot > 0f)
        {
            currentVelocity = (currentVelocity - hit.normal * dot).normalized * speed;
        }
        return true;


    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }
}
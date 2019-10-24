using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class CustomPhysic : MonoBehaviour
{
    [Tooltip("How much of the gravity should be applied?\n(Same as gravityScale in the rigidbody")]
    public float gravityModifier = 1;

    [Tooltip("Greater than this value will detected as a ground\nThis value " +
        "can be gain by cos(x) where x is the ground slope degree"), Range(0, 1)]
    public float minGrounded_GroundNormal = .65f;

    protected Rigidbody2D rigid;
    protected Vector2 velocity;
    protected bool isGrounded;

    /// <summary>
    /// ground normal is a normalized vector that is prependecular to the 
    /// ground game object that player is standing on.
    /// </summary>
    protected Vector2 groundNormal;

    /// <summary>
    /// used to filter colliders to interact with and get the collider matrix in the
    /// Physics2D collision matrix...
    /// </summary>
    protected ContactFilter2D m_ContactFilter;

    /// <summary>
    /// used to store rigidbody cast results colliders that shows 
    /// what collider gonna collide with game object
    /// </summary>
    protected List<RaycastHit2D> castBufferList = new List<RaycastHit2D>(20);

    /// <summary>
    /// I don't understand why we need this
    /// </summary>
    protected Vector2 targetVelocity;


    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

#if UNITY_EDITOR
        if (!rigid.isKinematic)
        {
            Debug.LogError("Rigidbody of {0} is not kinematic please fix that and run again");
            EditorApplication.isPlaying = false;
        }
#endif
    }

    protected virtual void Start()
    {
        m_ContactFilter.useTriggers = false;
        m_ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        m_ContactFilter.useLayerMask = true;
    }

    protected virtual void FixedUpdate()
    {
        velocity.x = targetVelocity.x;
        velocity += gravityModifier * Physics2D.gravity * Time.fixedDeltaTime;

        Vector2 deltaPosition = velocity * Time.fixedDeltaTime; //How much should we move in this frame

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        if (!isGrounded)
            moveAlongGround = Vector2.right;

        Vector2 move = moveAlongGround * deltaPosition.x;

        isGrounded = false;

        Moving(move, false); //move in the x axis

        move = Vector2.up * deltaPosition.y;

        Moving(move, true); //move in the y axis
    }
    protected virtual void Moving(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;
        Vector2 direction = move.normalized;

        RaycastHit2D[] castResult = new RaycastHit2D[20]; //used to get the collision results in the rigidbody cast
        int count = rigid.Cast(direction, m_ContactFilter, castResult, distance);

        castBufferList.Clear();
        for (int i = 0; i < count; i++)
            castBufferList.Add(castResult[i]);

        foreach (RaycastHit2D hit in castBufferList)
        {
            Vector2 currentNormal = hit.normal;

            if (currentNormal.y > minGrounded_GroundNormal)
            {
                isGrounded = true;

                if (yMovement)
                {
                    groundNormal = currentNormal;
                    currentNormal.x = 0;
                }
            }

            //reduce velocity when colliding with something so for example
            //if we standing on the ground our velocity doesn't increase with gravity
            float projection = Vector2.Dot(velocity, currentNormal);
            if (projection < 0)
            {
                velocity -= projection * currentNormal;
            }

            float dotProduct = Vector2.Dot(direction, currentNormal);
            if (dotProduct < 0)
            {
                float colliderDistance = hit.distance;
                distance = colliderDistance < distance ? colliderDistance : distance;
            }
        }

        rigid.position += distance * direction;
    }
}

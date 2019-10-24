using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerControllerRB : CustomPhysic
{
    [Tooltip("Max speed"), Space(10)]
    public float moveSpeed = 10;
    [Tooltip("How fast should player accelerate when on the ground?")]
    public float walkAcceleration = 5;
    [Tooltip("How fast should player deccelerate when on the ground?")]
    public float walkDecceleration = 7;

    [Space(10)]
    [Tooltip("Should moving in the air be different than on the ground? " +
        "this is max speed in the air")]
    public float airSpeed = 0;
    [Tooltip("Same as walkAcceleration but in the air\nNot that setting to zero will use walkAcceleration")]
    public float airAcceleration = 0;
    [Tooltip("Same as walkDecceleration but in the air\nNot that setting to zero will use walkDecceleration")]
    public float airDecceleration = 0;

    [Tooltip("formula: v = SquarRoot(2 * gravity * jumpHeight)"), Space(10)]
    public float jumpHeight = 4;

    private float preInput;

    protected override void Start()
    {
        base.Start();

        AirCondition();
    }
    private void AirCondition()
    {
        if (airSpeed == 0)
            airSpeed = moveSpeed;
        if (airAcceleration == 0)
            airAcceleration = walkAcceleration;
        if (airDecceleration == 0)
            airDecceleration = walkDecceleration;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (velocity.y > 0)
                velocity.y *= .25f;
        }

        float xInput = Input.GetAxisRaw("Horizontal");

        float acceleration = isGrounded ? walkAcceleration : airAcceleration;
        float decceleration = isGrounded ? walkDecceleration : airDecceleration;
        float speed = isGrounded ? moveSpeed : airSpeed;

        if (xInput != 0)
        {
            if (xInput != preInput)
            {
                preInput = xInput;
                targetVelocity.x = 0; //can be alot better!!!
            }
            else
                targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, xInput * speed, acceleration * Time.deltaTime);
        }
        else
            targetVelocity.x = Mathf.MoveTowards(targetVelocity.x, 0, decceleration * Time.deltaTime);
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mario : MonoBehaviour
{
    public enum eDirection {idle, left, right }

    [Header("Horizontal Movement")]
    public float currentSpeed;
    public float maxWalkSpeed;
    public float maxRunSpeed;
    public float currentMaxSpeed;
    public float damping;
    public float acceleration;
    public float walkAcceleration;
    public float runAcceleration;
    public eDirection direction;
    public bool running;
    public bool blockedRight;
    public bool blockedLeft;
    public float wallDetectBuffer;

    [Header("Jumping")]
    private float jumpMultiplier;
    public float jumpSpeed;
    public float unJumpSpeed;
    public bool grounded;
    public float groundDetectBuffer;
    public float minJumpMulti;
    public float maxJumpMulti;

    [Header("Other Stuff")]
    public bool crouching;
    private Vector2 ogOffset;
    private Vector2 ogSize;
    private Vector2 crouchOffset = new Vector2(0f, -0.5f);
    private Vector2 crouchedSize = new Vector2(1, 1);

    [Header("Animation")]
    public float skidThreshold;

    [Header("Components")]
   
    public Animator animator;
    public BoxCollider2D myCollider;
    public Rigidbody2D body;
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody2D>();
        myCollider = gameObject.GetComponent<BoxCollider2D>();

        ogOffset = myCollider.offset;
        ogSize = myCollider.size;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics2D.BoxCast(myCollider.bounds.center, myCollider.bounds.size, 0, Vector2.down, groundDetectBuffer, 1 << 8);
        blockedLeft = Physics2D.BoxCast(myCollider.bounds.center, myCollider.bounds.size, 0, Vector2.left, wallDetectBuffer, 1 << 8);
        blockedRight = Physics2D.BoxCast(myCollider.bounds.center, myCollider.bounds.size, 0, Vector2.right, wallDetectBuffer, 1 << 8);

        if (blockedLeft)
        {
            currentSpeed = Mathf.Clamp(currentSpeed, 0, currentMaxSpeed);
        }
        if (blockedRight)
        {
            currentSpeed = Mathf.Clamp(currentSpeed, -currentMaxSpeed, 0);
        }
        if (crouching)
        {
            myCollider.offset = crouchOffset;
            myCollider.size = crouchedSize;
        }
        else
        {
            myCollider.offset = ogOffset;
            myCollider.size = ogSize;
        }

        InputManager();
        if (running)
        {
            currentMaxSpeed = maxRunSpeed;
            acceleration = runAcceleration;
        }
        else
        {
            currentMaxSpeed = maxWalkSpeed;
            acceleration = walkAcceleration;
        }
        if (direction == eDirection.right)
        {

                currentSpeed += acceleration * Time.deltaTime;

        }
        if (direction == eDirection.left) 
        { 

            currentSpeed -= acceleration * Time.deltaTime;

        }
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0, damping * Time.deltaTime);

        currentSpeed = Mathf.Clamp(currentSpeed, -currentMaxSpeed, currentMaxSpeed);

        Vector2 movement = new Vector2(currentSpeed, 0);
        transform.Translate(movement * Time.deltaTime);

        if (currentSpeed >= maxRunSpeed - 1)
        {
            jumpMultiplier = 1.25f;
        }
        else
        {
            jumpMultiplier = 1;
        }

        //This is all for animation.

        animator.SetFloat("Speed", Mathf.Abs(currentSpeed));
        if (Mathf.Abs(currentSpeed) == maxWalkSpeed)
        {
            animator.SetFloat("walkingState", 1);
        }
        if (Mathf.Abs(currentSpeed) == maxRunSpeed)
        {
            animator.SetFloat("walkingState", 2);
        }
        if (Mathf.Abs(currentSpeed) < maxWalkSpeed)
        {
            animator.SetFloat("walkingState", 0.5f);
        }
        if (body.velocity.y < 0)
        
        {
            animator.SetFloat("walkingState", 0);
        }

        if (grounded)
        {
            animator.SetBool("isJumping", false);
        }
        if (body.velocity.y > 0)
        {
            animator.SetBool("isJumping", true);
        }
        
        
    }

    private void FixedUpdate()
    {
     
    }

    void InputManager()
    {
        direction = eDirection.idle;
        if (Input.GetKey(KeyCode.A)&&!blockedLeft)
        {
            if (!crouching)
            {
                direction = eDirection.left;
                spriteRenderer.flipX = true;


                if (currentSpeed > skidThreshold)
                {
                    animator.SetBool("isSkidding", true);
                }
                else { animator.SetBool("isSkidding", false); }

            }
            else if (crouching && !grounded)
            {
                direction = eDirection.left;

                if (currentSpeed > skidThreshold)
                {
                    animator.SetBool("isSkidding", true);
                }
                else { animator.SetBool("isSkidding", false); }
            }
        }
    
        if (Input.GetKey(KeyCode.D)&&!blockedRight)
        {
            if (!crouching)
            {
                direction = eDirection.right;
                spriteRenderer.flipX = false;
                
                if (currentSpeed < -skidThreshold)
                {
                    animator.SetBool("isSkidding", true);
                }
                else { animator.SetBool("isSkidding", false); }
                
            }
            else if (crouching && !grounded)
            {
                direction = eDirection.right;
                spriteRenderer.flipX = false;


                if (currentSpeed < -skidThreshold)
                {
                    animator.SetBool("isSkidding", true);
                }
                else { animator.SetBool("isSkidding", false); }
            }
        }

        if (grounded)
        {
            running = Input.GetKey(KeyCode.LeftShift);
        }
       


        if (Input.GetKey(KeyCode.S)&& grounded)
        {
            
            animator.SetBool("isCrouching", true);
            crouching = true;
        }

        if (!Input.GetKey(KeyCode.S) && grounded)
        {
            animator.SetBool("isCrouching", false);
            crouching = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (grounded == true)
            {
                Jump();
                print("jump");
            }
            
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            UnJump();
        }
    }

    void Jump()
    {
       // jumpMultiplier = Mathf.Lerp(minJumpMulti, maxJumpMulti, currentSpeed/maxRunSpeed);
        body.AddForce(transform.up * jumpSpeed * jumpMultiplier, ForceMode2D.Impulse);
        animator.SetBool("isJumping", true);
    }
    void UnJump()
    {
        if (body.velocity.y > unJumpSpeed)
        {
            Vector2 newVelocity = body.velocity;
            newVelocity.y = unJumpSpeed;
            body.velocity = newVelocity;
        }
    }

}

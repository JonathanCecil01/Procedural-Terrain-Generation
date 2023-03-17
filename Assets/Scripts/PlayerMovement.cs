using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed =12f;

    public float gravity = -9.81f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;

    public LayerMask groundMask;

    Vector3 velocity;

    bool isGround;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask );
        
        if(isGround & velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Jump");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right*x + transform.forward*z+ transform.up*y;
        controller.Move(move*speed*Time.deltaTime);

        velocity.y += gravity*Time.deltaTime;

        controller.Move(velocity*Time.deltaTime*Time.deltaTime);

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitSimulator : MonoBehaviour
{
    [SerializeField]
    public GameObject rabbit;
    public float speed = 12f;
    public float gravity = -9.8f;
    Vector3 velocity;
    public CharacterController controller;
    
    void Start(){
        
    }
    void Update(){

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Jump");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right*x + transform.forward*z + transform.up*y;
        
        controller.Move(move*speed*Time.deltaTime);

        velocity.y += gravity*Time.deltaTime;

        controller.Move(velocity*Time.deltaTime*Time.deltaTime);

    }
    
}

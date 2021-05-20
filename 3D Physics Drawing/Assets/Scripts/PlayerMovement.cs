using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerMovement : MonoBehaviour
{
    //public InputActionAsset controls;
    
    //[Space(10)]

    public float speed = 12.0f;
    
    public CharacterController controller;

    Vector3 velocity;

    public float gravity = -9.8f;

    public Transform groundCheck;
    public float groundDistance;

    public LayerMask groundMask;
    bool isGrounded;

    public float jumpHeight;

    float x;
    float z;

    bool jump;

    // Start is called before the first frame update
    void Awake()
    {
        //controls.Player.Movement.performed += dir => Move(dir.ReadValue<Vector2>());
    }

    public void Move(InputAction.CallbackContext context) 
    {
        //if (context.performed)
        //{
            //Debug.Log("Moving");
            var dir = context.ReadValue<Vector2>();

            x = dir.x;
            z = dir.y;
        //}
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            Debug.Log("Jumping");
            jump = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2.0f;
        }

        Vector3 move = (transform.right * x) + (transform.forward * z);
        controller.Move(move * speed * Time.deltaTime);

        if (jump && isGrounded)
        {
            jump = false;
            velocity.y += Mathf.Sqrt(jumpHeight * (gravity/3) * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

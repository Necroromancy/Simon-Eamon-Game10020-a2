using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Stuff")]
    [Range(0, 20)]
    public float moveSpeed = 6f;

    public float sprintScale = 1.5f;
    private float sprintSpeed;

    [Range(0,10)]
    public float mouseSensitivity = 5;
    
    private float increment = 0.25f;    //Sliders move in 0.25 incremenets.

    [Header("Jump Stuff")]
    [Range(1, 10)]
    public float jumpHeight = 1.5f;
    [Tooltip("This uses the default Unity gravity of -9.8, but change ONLY THE Y VALUE here if you want to adjust gravity")]
    public Vector3 gravity = Physics.gravity;   //Default will be Unity's gravity (-9.8) but can be changed here.

    private float coyoteTime;
    public float coyoteTimeMax = 0.1f; // 100ms coyote time

    [Tooltip("Percentage of jump height for a tap vs hold of the button (i.e., 0.5f = a tap of the jump button is 50% jump height)")]
    public float lowJumpFraction = 0.5f;
    [Tooltip("Speed up the player on descent. These numbers can be adjusted.")]
    public float fastFallMultiplier = 1.75f;

    [Header("DRAG THE CAMERA IN HERE OR THIS DOESN'T WORK AND YOU WILL GET VERY DIZZY!")]
    public Transform CameraRef;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0;

    void OnValidate()
    {
        // Snap values to increments of 0.5
        moveSpeed = Mathf.Round(moveSpeed / increment) * increment;
        mouseSensitivity = Mathf.Round(mouseSensitivity / increment) * increment;
        jumpHeight = Mathf.Round(jumpHeight / increment) * increment;
    }   //This is only for the slider snapping. Don't worry about it ;)

    void Start()
    {
        sprintSpeed = moveSpeed * sprintScale;
        //Probably unnecessary, but just to make gravity gets assigned properly.
        gravity = Physics.gravity;

        //This sucker uses the CC instead of the RB.
        controller = GetComponent<CharacterController>();

        //Hide the cursor. Hit ESC to bring it back.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; //These are not scaled by deltaTime. If things get jittery, add * Time.deltaTime and increase sensivity by 100x
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);  //Camera can't move past 85 degrees or -85 degrees. TECHNICALLY radians, but you know what I mean.

        CameraRef.localRotation = Quaternion.Euler(xRotation, 0f, 0f);  //Oy-ler. Only the camera can move up and down.
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        //if the player is on the ground, reset coyoteTime. If they are not on the ground, count down.
        if (controller.isGrounded)
            coyoteTime = coyoteTimeMax;
        else
            coyoteTime -= Time.deltaTime;

        //This makes sure the player stays on the ground. Sometimes physics are weird and you can get air when moving quickly.
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        //Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //Calculate target movement direction
        Vector3 move = transform.right * x + transform.forward * z;

        // Determine current speed
        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            currentSpeed = sprintSpeed;
        }

        //Go there!
        controller.Move(move * currentSpeed * Time.deltaTime);

        //If the player hits spacebar and the coyoteTime is reset, you can jump
        if (Input.GetButtonDown("Jump") && coyoteTime > 0f)
        {
            //Ooo...kinematics! v^2 = u^2 + 2(as) - AKA the third suvat equation
            //i.e., final velocity calculation is initial velocity squared + 2 * (acceleration * vector displacement [how far, and in what direction an object has moved
            //from its initial point to its end point])
            //0 = u^2 + 2 * a * s
            //u^2 = -2 * a * s
            //Therefore, u = √-2 * a * s

            //Neat, eh?

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            coyoteTime = 0f; // reset coyote time
        }

        //Jump height: tap vs hold
        if (velocity.y > 0f && !Input.GetButton("Jump"))
        {
            //Apply extra gravity while rising to shorten jump. You have to do this a bit differently than with a RB.
            //lowJumpFraction = fraction of gravity applied for short jump (0 < value < 1)
            velocity.y += Physics.gravity.y * (1f / Mathf.Clamp(lowJumpFraction, 0.01f, 1f) - 1f) * Time.deltaTime;
        }

        //Apply extra gravity when falling
        if (velocity.y < 0f)
        {
            velocity.y += Physics.gravity.y * (fastFallMultiplier - 1f) * Time.deltaTime;
        }

        //Apply normal gravity by default.
        velocity.y += Physics.gravity.y * Time.deltaTime;

        //No matter what the jumping scenario (?) is, apply that to the character controller. If no jump is being pressed, then we are scaling by 0 so nothing happens.
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }


}

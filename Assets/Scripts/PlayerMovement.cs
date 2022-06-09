// Some stupid rigidbody based movement by Dani

using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    //Assingables
    public Transform playerCam;
    public Transform orientation;
    private Transform player;
    public GrapplingGun grapplingGun;
    
    //Other
    private Rigidbody rb;

    //Rotation and look
    private float xRotation;
    public float sensitivity = 50f;
    private float sensMultiplier = 1f;
    
    //Movement
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;
    
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 10f;

    //Dashing
    public float dashForce = 2;
    private Vector3 initialVel;
    private Vector3 dashDirection;
    private float dashTime;
    private float startDashTime = 0.15f;

    public bool isColliding;
    
    //Input
    float x, y;
    bool jumping, sprinting;
    
    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        player = grapplingGun.player;
    }
    
    void Start() {
        playerScale =  transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    private void FixedUpdate() {
        // Movement();
    }

    private void Update() {
        // MyInput();
        Look();
        if (player.transform.position.y < -25) ResetPlayer();
    }

    private void ResetPlayer()
    {
        player.transform.position = new Vector3(0, 2, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }

    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");


        // Dashing
        if (dashDirection != new Vector3(0, 0, 0))
        {
            if (dashTime <= 0) {
                rb.velocity = initialVel + dashDirection*1.5f;
                dashDirection = new Vector3(0, 0, 0); 
                dashTime = startDashTime;

                return;
            }
            else { dashTime -= Time.deltaTime; }

            rb.velocity = dashDirection * dashForce /10;

            return;
        }

        initialVel = rb.velocity;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            int d1=-1;
            int d2=-1;
            if (Input.GetKey(KeyCode.W)) { d1 = 0; }
            if (Input.GetKey(KeyCode.A)) { if (d1 == -1) { d1 = 1; } else { d2 = 1; } }
            if (Input.GetKey(KeyCode.D)) { if (d1 == -1) { d1 = 2; } else { d2 = 2; } }
            if (Input.GetKey(KeyCode.S)) { if (d1 == -1) { d1 = 3; } else { d2 = 3; } }
            Dash(d1, d2);
        }
    }

    private void Movement() {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        
        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Find player average velocity
        Vector3 vel = rb.velocity;
        double averageVel = Math.Sqrt(vel[0]*vel[0] + vel[2]*vel[2]);

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) { Jump(); }

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;
        
        // Movement in air
        if (!grounded) {
            multiplier = 0.6f;
            multiplierV = 0.6f;
        }
        
        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Dash(int direction, int dir2 = -1)
    {
        switch (direction)
        { 
            case 0:
                dashDirection = new Vector3(playerCam.forward.x,0,playerCam.forward.z);
                break;
            case 1:
                dashDirection = -playerCam.right;
                break;
            case 2:
                dashDirection = playerCam.right;
                break;
            case 3:
                dashDirection = -playerCam.up;
                break;
            case 4:
                dashDirection = playerCam.up;
                break;

            default:
                break;
        }
        switch (dir2)
        {
            case 0:
                dashDirection += new Vector3(playerCam.forward.x, 0, playerCam.forward.z);
                break;
            case 1:
                dashDirection += -playerCam.right;
                break;
            case 2:
                dashDirection += playerCam.right;
                break;
            case 3:
                dashDirection += -playerCam.up;
                break;
            case 4:
                dashDirection += playerCam.up;
                break;
        }
        Debug.Log(dashDirection);
        //rb.AddForce(dashDirection * dashForce * 2f);
    }

    private void Jump() 
    {
        if (!grounded) { return; }
        if (!readyToJump) { return; }

        if (grapplingGun.Stuck)
        {
            grapplingGun.UnstickRope();
            rb.AddForce(playerCam.forward * jumpForce * 2f);
        }


        grounded = false;
        readyToJump = false;

        //Add jump forces
        rb.AddForce(Vector2.up * jumpForce * 1.5f);
        rb.AddForce(normalVector * jumpForce * 0.5f);

        //Reset velocities

        //If jumping while falling, reset y velocity.
        Vector3 vel = rb.velocity;
        if (rb.velocity.y < 0.5f)
            rb.velocity = new Vector3(vel.x, 0, vel.z);
        else if (rb.velocity.y > 0) 
            rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
        
        Invoke(nameof(ResetJump), jumpCooldown);
    
    }

    public void ResetJump() 
    {
        readyToJump = true;
    }
    public void Ground() 
    {
        grounded = true;
    }


    private float desiredX;
    private void Look() 
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        
        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag) 
    {
        if (!grounded || jumping) return;

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    public Vector2 FindVelRelativeToLook() 
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v) 
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    private void OnCollisionStay(Collision other) 
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal)) {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded) {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded() { grounded = false; }
    //private void ResetSpeed()
    //{
    //    float xvel = rb.velocity[0];
    //    float yvel = rb.velocity[2];
    //    float newXVel = (float)((xvel * maxSpeed) / (Math.Sqrt(xvel * xvel + yvel * yvel)));
    //    float newYVel = (float)((yvel * maxSpeed) / (Math.Sqrt(xvel * xvel + yvel * yvel)));
    //    rb.velocity = new Vector3(newXVel, rb.velocity[1], newYVel);
    //    x = 0;
    //    y = 0;
    //}

}

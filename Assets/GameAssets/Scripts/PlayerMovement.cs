using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

struct PlayerInput {
    public uint id;
    public float xinput;
    public bool jump;
}

public class PlayerMovement : NetworkBehaviour
{

    // Maximum horizontal speed attainable by walking.
    // If the player is moving faster than this speed and attempts to move
    // in that direction, they cannot gain speed. However, they can slow down
    // by moving in the opposite direction.
    public float maxHorizontalWalkSpeed;
    public float walkForce;
    public float decelForce;
    public float jumpForce;

    private Rigidbody rb;
    private float playerHeight;

    [SyncVar]
    private PlayerInput input;

    private uint inputId = 0;


    // Start is called before the first frame update
    void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        this.playerHeight = GetComponent<BoxCollider>().size.y / 2;

        if(isLocalPlayer) {
            Camera.main.GetComponent<FollowCamera>().target = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer) {
            float xinput = Input.GetAxisRaw("Horizontal");
            bool jump = Input.GetButton("Jump");
            CmdMove(xinput, jump);
        }
    }

    void FixedUpdate()
    {
        ProcessInput(input);
    }

    [Command]
    void CmdMove(float xinput, bool jump) {
        this.input = new PlayerInput {
            id = inputId++,
            xinput = xinput,
            jump = jump
        };
    }

    void ProcessInput(PlayerInput input) {
        Vector3 velocity = rb.velocity;
        bool isGrounded = IsGrounded();

        if (input.xinput == 0 && isGrounded) {
            // decelerate if we are on the ground
            rb.AddForce(new Vector3(-velocity.x * decelForce, 0));
        }

        if (CanAddForce(input.xinput, velocity)) {
            rb.AddForce(new Vector3(walkForce * input.xinput, 0));
        }

        // vertical movement
        if (input.jump && isGrounded) {
            rb.AddForce(new Vector3(0, jumpForce), ForceMode.Impulse);
        }
    }

    private bool IsGrounded() {
        return rb.velocity.y > -0.01 && Physics.Raycast(transform.position, -Vector3.up, playerHeight + 0.05f);
    }

    private bool CanAddForce(float xinput, Vector3 velocity) {

        if(Mathf.Sign(xinput) == Mathf.Sign(velocity.x)) {
            if(Mathf.Abs(velocity.x) > maxHorizontalWalkSpeed) {
                return false;
            }
        }

        return true;
    }
}

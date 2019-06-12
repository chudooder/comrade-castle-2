﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

struct PlayerInput {
    public uint id;
    public float xinput;
    public bool jump;
    public float aimAngle;
}

struct PlayerState {
    public Vector3 position;
    public Vector3 velocity;
}

public class PlayerMovement : NetworkBehaviour
{

    // Maximum horizontal speed attainable by walking.
    // If the player is moving faster than this speed and attempts to move
    // in that direction, they cannot gain speed. However, they can slow down
    // by moving in the opposite direction.
    public float maxHorizontalWalkSpeed;
    public float walkAccel;
    public float walkDecel;
    public float jumpVelocity;
    public float gravity;

    private CharacterController cc;
    private Transform weapon;
    private float playerHeight;

    [SyncVar]
    private PlayerInput input;

    /* velocity is updated as follows:
        1. After this.cc.Move is called, velocity is set to the resulting position delta
        2. When the state SyncVar updates, velocity is updated to match the server
    */
    public Vector3 velocity = new Vector3();

    private uint inputId = 0;


    // Start is called before the first frame update
    void Start()
    {
        this.cc = GetComponent<CharacterController>();
        this.weapon = transform.Find("WeaponPivot");
        this.playerHeight = this.cc.height;

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
            Vector3 mousePos = ProjectMouseToPlane();
            float aimAngle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * 180 / Mathf.PI;
            CmdMove(xinput, jump, aimAngle);
        }

        ProcessInput(input);        
    }

    [Command]
    void CmdMove(float xinput, bool jump, float aimAngle) {
        this.input = new PlayerInput {
            id = inputId++,
            xinput = xinput,
            jump = jump,
            aimAngle = aimAngle
        };
    }

    void ProcessInput(PlayerInput input) {
        bool isGrounded = IsGrounded();
        float deltaTime = Time.deltaTime;

        if (input.xinput == 0 && isGrounded) {
            // decelerate if we are on the ground
            velocity += new Vector3(deltaTime * -velocity.x * walkDecel, 0, 0);
        }

        if (CanAddForce(input.xinput, velocity)) {
            velocity += new Vector3(deltaTime * walkAccel * input.xinput, 0, 0);
        }

        // vertical movement
        if (input.jump && isGrounded) {
            velocity.y += jumpVelocity;
        }

        if(!isGrounded) {
            velocity.y -= deltaTime * gravity;
        }

        this.cc.Move(deltaTime * velocity);
        this.velocity = this.cc.velocity;

        if(input.aimAngle > 90) {
            weapon.rotation = Quaternion.AngleAxis(180, Vector3.up) * Quaternion.AngleAxis(180 - input.aimAngle, Vector3.forward);
        } else if(input.aimAngle < -90) {
            weapon.rotation = Quaternion.AngleAxis(180, Vector3.up) * Quaternion.AngleAxis(-180 - input.aimAngle, Vector3.forward);
        } else {
            weapon.rotation = Quaternion.AngleAxis(input.aimAngle, Vector3.forward);
        }
    }    

    private bool IsGrounded() {
        return velocity.y > -0.01 && Physics.Raycast(transform.position, -Vector3.up, playerHeight/2 + 0.05f);
    }

    private bool CanAddForce(float xinput, Vector3 velocity) {

        if(Mathf.Sign(xinput) == Mathf.Sign(velocity.x)) {
            if(Mathf.Abs(velocity.x) > maxHorizontalWalkSpeed) {
                return false;
            }
        }

        return true;
    }

    private Vector3 ProjectMouseToPlane() {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.back, new Vector3(0, 0, 0));
        float distance;
        if (plane.Raycast(cameraRay, out distance)) {
            Vector3 point = cameraRay.GetPoint(distance);
            return point;
        }
        return Vector3.right;
    }
}

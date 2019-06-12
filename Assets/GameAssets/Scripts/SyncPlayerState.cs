using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncPlayerState : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnStateUpdate))]
    private PlayerState state;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        this.playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // only servers should update the player state
        if(isServer) {
            UpdatePlayerState();
        }
    }

    void UpdatePlayerState() {
        this.state = new PlayerState {
            position = transform.position,
            velocity = playerMovement.velocity
        };
    }

    // hooks only run on client
    void OnStateUpdate(PlayerState newState) {
        this.transform.position = newState.position;
        playerMovement.velocity = newState.velocity;
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    // INPUT
    Vector2 playerInput;

    // REFERENCES
    PlayerManager player;
    Rigidbody playerBody;

    // VALUES
    public float playerSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerManager.instance;
        playerBody = GetComponent<Rigidbody>();
    }

    bool IsInit()
    {
        if (!playerBody ||
            !player)
        {
            return false;
        }

        return true;
    }

    void Update()
    {
        if (!IsInit()) return;

        HandleMovement();
    }

    private void FixedUpdate()
    {
        MovementInput();
    }

    void MovementInput()
    {
        playerInput = new Vector2(Input.GetAxis("Horizontal"),
                                  Input.GetAxis("Vertical"));
    }

    void HandleMovement()
    {
        Vector2 playerMove = Vector2.zero;
        float playerSpeed = 0f;

        switch (player.playerShape)
        {
            case PlayerManager.PlayerShape.DEFAULT:
                break;
            case PlayerManager.PlayerShape.GRAB:
                playerMove = new Vector2(playerInput.x, 0);
                playerSpeed = player.defaultSpeed;
                break;
            case PlayerManager.PlayerShape.CAT:
                playerMove = new Vector2(playerInput.x, 0);
                playerSpeed = player.catSpeed;
                break;
            case PlayerManager.PlayerShape.FLY:
                playerMove = new Vector2(0, playerInput.y);
                playerSpeed = player.defaultSpeed / 2;
                break;
            default:
                break;
        }

        playerBody.AddForce(playerMove * playerSpeed, ForceMode.VelocityChange);
    }

    void HandleGravity()
    {
        switch (PlayerManager.instance.playerShape)
        {
            case PlayerManager.PlayerShape.DEFAULT:
                break;
            case PlayerManager.PlayerShape.GRAB:
                break;
            case PlayerManager.PlayerShape.CAT:
                break;
            case PlayerManager.PlayerShape.FLY:
                break;
            default:
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    // VECTORS
    Vector2 playerInput;
    Vector2 playerMove;

    // REFERENCES
    PlayerManager player;
    Rigidbody2D playerBody;

    // VALUES
    public float playerCurrentSpeed { private set; get; } = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerManager.instance;
        playerBody = GetComponent<Rigidbody2D>();
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
        if (!IsInit()) return;

        MovementInput();
        HandlePlayerShape();
    }

    void MovementInput()
    {
        playerInput = new Vector2(Input.GetAxis("Horizontal"),
                                  Input.GetAxis("Vertical"));
    }

    void HandleMovement()
    {
        playerMove = new Vector2(playerInput.x, 0);

        playerBody.velocity = playerMove * playerCurrentSpeed;

        player.playerSprite.flipX = playerInput.x > 0.1 && playerBody.velocity.magnitude != 0 ? false : true;
    }

    void HandlePlayerShape()
    {

        switch (player.playerShape)
        {
            case PlayerManager.PlayerShape.DEFAULT:
                playerCurrentSpeed = player.defaultSpeed;
                break;
            case PlayerManager.PlayerShape.GRAB:
                playerCurrentSpeed = player.defaultSpeed;
                break;
            case PlayerManager.PlayerShape.CAT:
                playerCurrentSpeed = player.catSpeed;
                break;
            case PlayerManager.PlayerShape.FLY:
                playerCurrentSpeed = player.defaultSpeed / 2;
                break;
            default:
                break;
        }

    }
}

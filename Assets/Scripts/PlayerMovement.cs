using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    // VECTORS
    private Vector2 playerInput;
    private Vector2 playerVelocity;
    [SerializeField] private float groundCheckDist = 1.0f;
    [SerializeField] private LayerMask groundLayerMask;

    // REFERENCES
    private PlayerManager player;
    private Rigidbody2D playerBody;

    // VALUES
    public float playerCurrentSpeed { private set; get; } = 1.0f;
    public float jumpStrength = 2f;

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
        HandleJump();
    }

    private void FixedUpdate()
    {
        if (!IsInit()) return;

        MovementInput();
        HandlePlayerShape();
        player.PassPlayerPosition(transform.position);

        playerVelocity = playerBody.velocity;
    }

    void MovementInput()
    {
        playerInput = new Vector2(Input.GetAxis("Horizontal"),
                                  Input.GetAxis("Vertical"));
    }

    void HandleMovement()
    {
        //playerMove = new Vector2(playerInput.x, playerBody.velocity.y);

        //playerBody.velocity = playerMove * playerCurrentSpeed;
        playerBody.velocity = new Vector2(playerInput.x * playerCurrentSpeed, playerBody.velocity.y);

        if (playerBody.velocity.x > 0) player.playerSprite.flipX = false;
        if (playerBody.velocity.x < 0) player.playerSprite.flipX = true;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            Debug.Log("Jump");
            playerBody.AddForce(playerBody.transform.up * jumpStrength, ForceMode2D.Impulse);
        }
    }

    bool IsGrounded()
    {
        Vector2 playerOrigin = playerBody.transform.position;
        Vector2 groundDirection = -playerBody.transform.up;

        RaycastHit2D hit = Physics2D.Raycast(playerOrigin, groundDirection, groundCheckDist, groundLayerMask);
        Color groundRayColor = hit.collider != null ? Color.green : Color.red;

        Debug.DrawRay(playerOrigin, groundDirection * groundCheckDist, groundRayColor, 1);

        if (hit.collider != null) return true;
        else return false;
        
    }

    void HandlePlayerShape()
    {

        switch (player.playerShape)
        {
            case PlayerManager.PlayerShape.DEFAULT:
                playerCurrentSpeed = player.defaultSpeed;
                break;
            case PlayerManager.PlayerShape.GRAB:
                playerCurrentSpeed = player.grabSpeed;
                break;
            case PlayerManager.PlayerShape.CAT:
                playerCurrentSpeed = player.catSpeed;
                break;
            case PlayerManager.PlayerShape.FLY:
                playerCurrentSpeed = player.flySpeed;
                break;
            default:
                break;
        }

    }
}

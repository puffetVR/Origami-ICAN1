using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // VECTORS
    private Vector2 playerInput;
    private Vector2 playerVelocity;
    private float groundCheckDist = .1f;
    
    [SerializeField] private LayerMask worldLayerMask;

    // REFERENCES
    private PlayerManager player;
    private Rigidbody2D playerBody;

    [Header("Jump")]
    [SerializeField] private float wallCheckDist = .3f;
    private float wallJumpDirection;
    public bool hasWallJumped { private set; get; } = false;
    private float coyoteTimeCounter = 0f;
    private float jumpBufferCounter = 0f;
    private float wallJumpCounter = 0f;
    private float flyDragShiftTime;
    private float flyDragShiftCounter = 0f;

    public float playerCurrentJumpPower { private set; get; } = 1.0f;
    public float playerCurrentSpeed { private set; get; } = 1f;
    private float accelerationRate;
    public float playerCurrentDrag { private set; get; } = 0f;

    // FLAGS
    public bool isGrounded { private set; get; }

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

        HandleJump();

        if (isGrounded) coyoteTimeCounter = player.playerData.coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!IsInit()) return;

        HandleMovement();
        HandleDrag();
        MovementInput();
        HandlePlayerShape();
        player.PassPlayerPosition(transform.position);

        playerVelocity = playerBody.velocity;
        isGrounded = IsGrounded();
    }

    void MovementInput()
    {
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"),
                                  Input.GetAxisRaw("Vertical"));
    }

    void HandleDrag()
    {
        playerBody.drag = playerCurrentDrag;

        if (player.playerShape == PlayerManager.PlayerShape.FLY)
        {
            flyDragShiftTime = player.playerData.wallJumpDuration * 1.1f;
            flyDragShiftCounter = flyDragShiftCounter < flyDragShiftTime && flyDragShiftCounter >= 0 ? flyDragShiftCounter += Time.deltaTime : -1;

            //playerCurrentSpeed = playerInput.y > 0 ? player.flyMaxSpeed : player.flyMinSpeed;
        }
        else flyDragShiftCounter = 0;
    }

    void HandleMovement()
    {
        accelerationRate = hasWallJumped && player.playerShape == PlayerManager.PlayerShape.CAT ? player.playerData.airAcceleration : player.playerData.acceleration;

        float moveInput = playerInput.x * playerCurrentSpeed;
        float speed = moveInput - playerBody.velocity.x;
        float playerMovement = speed * accelerationRate;

        playerBody.AddForce(playerMovement * Vector2.right);

        if (/*playerBody.velocity.x > 0 || */playerInput.x > 0) player.playerSprite.flipX = false;
        if (/*playerBody.velocity.x < 0 || */playerInput.x < 0) player.playerSprite.flipX = true;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = player.playerData.jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (player.playerShape == PlayerManager.PlayerShape.CAT)
        {
            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
            {
                playerBody.velocity = new Vector2(playerBody.velocity.x, player.playerData.jumpStrength);
                Debug.Log("Jump");
                jumpBufferCounter = 0f;
            }

            if (jumpBufferCounter > 0f && CanWallJump())
            {
                Vector2 jumpDir = new Vector2(wallJumpDirection * player.playerData.wallJumpStrength.x, player.playerData.wallJumpStrength.y);
                Debug.Log("WallJump " + jumpDir);
                playerBody.velocity = jumpDir;
                hasWallJumped = true;
            }
        }

        wallJumpCounter = hasWallJumped ? wallJumpCounter += Time.deltaTime : 0f;
        hasWallJumped = wallJumpCounter > player.playerData.wallJumpDuration || isGrounded ? false : hasWallJumped;

        // Shorter jump if not holding the jump button all the way
        // Stronger gravity when falling
        if (Input.GetButtonUp("Jump") && playerBody.velocity.y > 0f)
        {
            //if (!hasWallJumped) playerBody.velocity = new Vector2(playerBody.velocity.x, playerBody.velocity.y * .5f);
            playerBody.velocity = new Vector2(playerBody.velocity.x, playerBody.velocity.y * .5f);
            playerBody.gravityScale = player.playerData.fallGravityScale;
            coyoteTimeCounter = 0f;
        }

        // Faster and clamp fall speed
        if (playerBody.velocity.y < 0f)
        {
            playerBody.gravityScale = player.playerData.fallGravityScale;

            playerBody.velocity =
                new Vector2(playerBody.velocity.x,
                Mathf.Clamp(playerBody.velocity.y, Mathf.Abs(player.playerData.maxFallSpeed) * -1, float.MaxValue));
        }

    }

    bool IsGrounded()
    {
        Vector2 playerOrigin = playerBody.transform.position;
        Vector2 groundDirection = -playerBody.transform.up;

        RaycastHit2D hit = Physics2D.Raycast(playerOrigin, groundDirection, groundCheckDist, worldLayerMask);
        Color rayColor = hit.collider != null ? Color.green : Color.red;

        Debug.DrawRay(playerOrigin, groundDirection * groundCheckDist, rayColor, 1);

        if (hit.collider != null)
        {
            playerBody.gravityScale = player.playerData.defaultGravityScale;
            //hasWallJumped = false;
            return true;
        }
        else return false;        
    }

    bool CanWallJump()
    {
        if (player.playerShape != PlayerManager.PlayerShape.CAT || isGrounded) return false;

        Vector2 playerOrigin = playerBody.transform.position;
        Vector2 leftDirection = -playerBody.transform.right;
        Vector2 rightDirection = playerBody.transform.right;

        RaycastHit2D leftHit = Physics2D.Raycast(playerOrigin, leftDirection, wallCheckDist, worldLayerMask);
        RaycastHit2D rightHit = Physics2D.Raycast(playerOrigin, rightDirection, wallCheckDist, worldLayerMask);

        Color leftRayColor = leftHit.collider != null ? Color.green : Color.red;
        Color rightRayColor = rightHit.collider != null ? Color.green : Color.red;
        Debug.DrawRay(playerOrigin, leftDirection * wallCheckDist, leftRayColor, 1);
        Debug.DrawRay(playerOrigin, rightDirection * wallCheckDist, rightRayColor, 1);
        
        //wallJumpDirection = Vector2.zero;
        wallJumpDirection = 0f;

        if (rightHit.collider != null)
        {
            //wallJumpDirection = leftDirection;
            wallJumpDirection = -1f;
            return true;
        }

        if (leftHit.collider != null)
        {
            //wallJumpDirection = rightDirection;
            wallJumpDirection = 1f;
            return true;
        }

        return false;

    }

    void HandlePlayerShape()
    {
        switch (player.playerShape)
        {
            case PlayerManager.PlayerShape.DEFAULT:
                playerCurrentSpeed = player.playerData.defaultSpeed;
                playerCurrentDrag = player.playerData.defaultDrag;
                break;
            case PlayerManager.PlayerShape.CAT:
                playerCurrentSpeed = player.playerData.catSpeed;
                playerCurrentDrag = player.playerData.catDrag;
                break;
            case PlayerManager.PlayerShape.FLY:
                playerCurrentSpeed = player.playerData.flySpeed;
                if (flyDragShiftCounter >= 0) playerCurrentDrag = player.playerData.flyMinDrag;
                else playerCurrentDrag = playerInput.y < 0 ? player.playerData.flyMinDrag : player.playerData.flyMaxDrag;
                break;
            default:
                break;
        }

    }

}

using UnityEngine;
using static PlayerManager;

public class PlayerMovement : MonoBehaviour
{
    // VECTORS
    private Vector2 playerVelocity;
    private int playerDirection = 1;
    private Vector2 lastGroundedPosition;
    
    [SerializeField] private LayerMask worldLayerMask;

    // REFERENCES
    private PlayerManager player;
    private Rigidbody2D playerBody;
    private BoxCollider2D playerCollider;
    [SerializeField] private BoxCollider2D spriteBounds;

    public bool hasWallJumped { private set; get; } = false;
    private float coyoteTimeCounter = 0f;
    private float jumpBufferCounter = 0f;
    private float wallJumpCounter = 0f;
    private float flyDragShiftCounter = 0f;

    public float playerCurrentJumpPower { private set; get; } = 1.0f;
    public float playerCurrentSpeed { private set; get; } = 1f;
    private float accelerationRate;
    public float playerCurrentDrag { private set; get; } = 0f;

    private const float positionCacheTick = 2f;
    private float timeSinceLastPosTick = positionCacheTick;

    // FLAGS
    public bool isGrounded { private set; get; }
    private bool preventWallSlide = false;
    private bool hasWallJumpedToFly = false;
    public bool keepPlayerInBounds = true;

    void Start()
    {
        player = instance;
        playerBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    bool IsInit()
    {
        if (!playerBody ||
            !player ||
            !playerCollider)
        {
            Debug.LogError("Oh oh stinky!");
            return false;
        }

        return true;
    }

    void Update()
    {
        if (!IsInit()) return;

        HandleJump();

        if (isGrounded)
        {
            accelerationRate = player.data.acceleration;
            coyoteTimeCounter = player.data.coyoteTime;
            if (hasWallJumpedToFly) hasWallJumpedToFly = false;
        }
        else coyoteTimeCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!IsInit()) return;

        HandleMovement();
        HandleBounds();
        HandleDirection();
        HandleDrag();
        //MovementInput();
        HandlePlayerShape();
        player.PassPlayerPosition(transform.position);

        playerVelocity = playerBody.velocity;
        isGrounded = IsGrounded();
    }

    #region Movement
    void HandleDrag()
    {
        playerBody.drag = playerCurrentDrag;

        if (player.playerShape == PlayerShape.FLY && hasWallJumpedToFly)
        {
            if (flyDragShiftCounter < (player.data.wallJumpDuration * 1.1f))
            {
                flyDragShiftCounter += Time.deltaTime;
            }
            else
            {
                hasWallJumpedToFly = false;
            }

            //playerCurrentSpeed = playerInput.y > 0 ? player.flyMaxSpeed : player.flyMinSpeed;
        }
        else flyDragShiftCounter = 0;
        
        if (player.playerShape == PlayerShape.CAT)
        {
            if (!isGrounded && !preventWallSlide && IsAgainstWall() && GameManager.instance.Input.playerInput.x != 0) playerCurrentDrag = player.data.maxDrag;
            else playerCurrentDrag = player.data.minDrag;
        }
    }

    void HandleMovement()
    {
        accelerationRate = hasWallJumped ? player.data.airAcceleration : player.data.acceleration;

        float moveInput = GameManager.instance.Input.playerInput.x * playerCurrentSpeed;
        float speed = moveInput - playerBody.velocity.x;
        float playerMovement = speed * accelerationRate;

        playerBody.AddForce(playerMovement * Vector2.right);
    }

    void HandleDirection()
    {
        player.playerSprite.flipX = playerDirection > 0 ? false : true;

        if (player.playerShape == PlayerShape.CAT || player.playerShape == PlayerShape.FLY)
            playerCollider.offset = new Vector2(player.data.shapeColXPos * playerDirection, playerCollider.offset.y);
        else playerCollider.offset = new Vector2(0, playerCollider.offset.y);

        if (!hasWallJumped && GameManager.instance.Input.playerInput.x > 0) playerDirection = 1;
        if (!hasWallJumped && GameManager.instance.Input.playerInput.x < 0) playerDirection = -1;
    }
    #endregion

    #region Jump
    void HandleJump()
    {
        if (GameManager.instance.Input.interact) jumpBufferCounter = player.data.jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        //Reset wall slide prevention
        if (!isGrounded && preventWallSlide && !IsAgainstWall()) preventWallSlide = false;

        if (player.playerShape == PlayerShape.CAT)
        {
            // Ground Jump Logic
            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
            {
                if (IsAgainstWall()) preventWallSlide = true;
                playerBody.velocity = new Vector2(playerBody.velocity.x, player.data.jumpStrength);
                Debug.Log("Jump");
                jumpBufferCounter = 0f;
            }

            // Wall Jump Logic
            if (jumpBufferCounter > 0f && GameManager.instance.Input.interact && CanWallJump())
            {
                playerCurrentDrag = player.data.minDrag;
                //Vector2 jumpDir = new Vector2(wallJumpDirection * player.data.wallJumpStrength.x, player.data.wallJumpStrength.y);
                Vector2 jumpDir = new Vector2(playerDirection * player.data.wallJumpStrength.x, player.data.wallJumpStrength.y);
                Debug.Log("WallJump " + jumpDir);
                playerBody.velocity = jumpDir;
                hasWallJumped = true;
            }
        }

        wallJumpCounter = hasWallJumped ? wallJumpCounter += Time.deltaTime : 0f;
        hasWallJumped = wallJumpCounter > player.data.wallJumpDuration || isGrounded ? false : hasWallJumped;

        // Shorter jump if not holding the jump button all the way
        // Stronger gravity when falling
        if (GameManager.instance.Input.interactUp && playerBody.velocity.y > 0f)
        {
            //if (!hasWallJumped) playerBody.velocity = new Vector2(playerBody.velocity.x, playerBody.velocity.y * .5f);
            playerBody.velocity = new Vector2(playerBody.velocity.x, playerBody.velocity.y * .5f);
            playerBody.gravityScale = player.data.fallGravityScale;
            coyoteTimeCounter = 0f;
        }

        // Faster and clamp fall speed
        if (playerBody.velocity.y < 0f)
        {
            playerBody.gravityScale = player.data.fallGravityScale;

            playerBody.velocity =
                new Vector2(playerBody.velocity.x,
                Mathf.Clamp(playerBody.velocity.y, Mathf.Abs(player.data.maxFallSpeed) * -1, float.MaxValue));
        }

    }

    bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;
        //Vector3 cast = new Vector2((player.playerData.shapeColXPos / 2) * -playerDirection, -(playerCollider.offset.y / 2));
        Vector2 castL = new Vector2(bounds.center.x + bounds.extents.x,
            bounds.center.y - bounds.extents.y);
        Vector2 castR = new Vector2(bounds.center.x - bounds.extents.x,
            bounds.center.y - bounds.extents.y);
        //Vector2 playerOrigin = playerBody.transform.position;
        Vector2 groundDirection = -playerBody.transform.up;

        RaycastHit2D hitL = Physics2D.Raycast(castL, groundDirection, player.data.groundCheckDist, worldLayerMask);
        RaycastHit2D hitR = Physics2D.Raycast(castR, groundDirection, player.data.groundCheckDist, worldLayerMask);
        Color rayColorL = hitL.collider != null ? Color.green : Color.red;
        Color rayColorR = hitR.collider != null ? Color.green : Color.red;

        Debug.DrawRay(castL, groundDirection * player.data.groundCheckDist, rayColorL, 1);
        Debug.DrawRay(castR, groundDirection * player.data.groundCheckDist, rayColorR, 1);

        if (hitL.collider != null || hitR.collider != null)
        {
            playerBody.gravityScale = player.data.defaultGravityScale;
            //hasWallJumped = false;
            return true;
        }
        else return false;        
    }

    bool IsAgainstWall()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 cast = new Vector2(playerDirection > 0 ? bounds.center.x + bounds.extents.x : bounds.center.x - bounds.extents.x,
            bounds.center.y - bounds.extents.y);
        Vector2 direction = playerBody.transform.right * playerDirection;

        RaycastHit2D hit = Physics2D.Raycast(cast, direction, player.data.groundCheckDist, worldLayerMask);
        Color rayColor = hit.collider != null ? Color.green : Color.red;

        Debug.DrawRay(cast, direction * player.data.groundCheckDist, rayColor, 1);

        if (hit.collider != null) return true;
        else return false;
    }

    bool CanWallJump()
    {
        if (player.playerShape != PlayerShape.CAT || isGrounded) return false;

        Vector2 playerOrigin = playerBody.transform.position;
        Vector2 leftDirection = -playerBody.transform.right;
        Vector2 rightDirection = playerBody.transform.right;

        RaycastHit2D leftHit = Physics2D.Raycast(playerOrigin, leftDirection, player.data.wallCheckDist, worldLayerMask);
        RaycastHit2D rightHit = Physics2D.Raycast(playerOrigin, rightDirection, player.data.wallCheckDist, worldLayerMask);

        Color leftRayColor = leftHit.collider != null ? Color.green : Color.red;
        Color rightRayColor = rightHit.collider != null ? Color.green : Color.red;
        Debug.DrawRay(playerOrigin, leftDirection * player.data.wallCheckDist, leftRayColor, 1);
        Debug.DrawRay(playerOrigin, rightDirection * player.data.wallCheckDist, rightRayColor, 1);
        
        if (rightHit.collider != null)
        {
            playerDirection = -1;
            return true;
        }

        if (leftHit.collider != null)
        {
            playerDirection = 1;
            return true;
        }

        return false;

    }
    #endregion

    void HandlePlayerShape()
    {
        switch (player.playerShape)
        {
            case PlayerShape.DEFAULT:
                playerCurrentSpeed = player.data.defaultSpeed;
                playerCurrentDrag = player.data.minDrag;
                playerCollider.size = new Vector2(player.data.defaultWidth, 1);
                spriteBounds.offset = player.data.defaultBoundsOffset;
                spriteBounds.size = player.data.defaultBoundsSize;
                break;
            case PlayerShape.CAT:
                playerCurrentSpeed = player.data.catSpeed;
                playerCollider.size = new Vector2(player.data.catWidth, 1);
                spriteBounds.offset = player.data.catBoundsOffset;
                spriteBounds.size = player.data.catBoundsSize;
                break;
            case PlayerShape.FLY:
                playerCurrentSpeed = player.data.flySpeed;
                if (hasWallJumped) hasWallJumpedToFly = true;
                if (flyDragShiftCounter > 0) playerCurrentDrag = player.data.minDrag;
                else playerCurrentDrag = GameManager.instance.Input.playerInput.y < 0 ? player.data.minDrag : player.data.maxDrag;
                playerCollider.size = new Vector2(player.data.catWidth, 1);
                spriteBounds.offset = player.data.flyBoundsOffset;
                spriteBounds.size = player.data.flyBoundsSize;
                break;
            default:
                break;
        }

    }

    private void LateUpdate()
    {
        HandlePositionCaching();
        ConfinePlayerToBounds();
    }

    #region Bounds stuff
    void HandleBounds()
    {
        if (!keepPlayerInBounds) return;

        playerBody.position = new Vector2(Mathf.Clamp(playerBody.position.x, GameManager.instance.levelBoundsMin.x, GameManager.instance.levelBoundsMax.x),
                                          Mathf.Clamp(playerBody.position.y, float.MinValue, GameManager.instance.levelBoundsMax.y));

    }

    bool IsPlayerInCameraBounds()
    {
        if (player.playerSprite.isVisible) return true;
        else return false;
    }

    void ConfinePlayerToBounds()
    {
        if (!keepPlayerInBounds) return;

        if (!IsPlayerInCameraBounds())
        {
            playerBody.velocity = Vector2.zero;
            playerBody.position = lastGroundedPosition;
        }
    }

    void HandlePositionCaching()
    {
        /*
        bool justLanded = !isGrounded && IsGrounded();
        if (justLanded == true)
        {
            timeSinceLastPosTick = 0;
            Debug.Log("Just landed.");
        }
        */

        timeSinceLastPosTick = isGrounded ? timeSinceLastPosTick + Time.deltaTime : 0;

        if (timeSinceLastPosTick > positionCacheTick)
        {
            timeSinceLastPosTick = 0;
            lastGroundedPosition = transform.position;
        }
    }
    #endregion

}

using Unity.VisualScripting;
using UnityEngine;
using static PlayerManager;

public class PlayerMovement : MonoBehaviour
{
    // VECTORS
    private Vector2 playerVelocity;
    private int playerDirection = 1;
    private Vector2 lastGroundedPosition;
    
    [SerializeField] private LayerMask worldLayerMask;
    [SerializeField] private LayerMask wallJumpLayerMask;

    // REFERENCES
    private PlayerManager player;
    private Rigidbody2D playerBody;
    private BoxCollider2D playerCollider;

    public bool hasWallJumped { private set; get; } = false;
    public bool isBeingPushedUpwards { private set; get; } = false;
    private float coyoteTimeCounter = 0f;
    private float jumpBufferCounter = 0f;
    private float wallJumpCounter = 0f;
    //private float flyDragShiftCounter = 0f;

    public float playerCurrentJumpPower { private set; get; } = 1.0f;
    public float playerCurrentSpeed { private set; get; } = 1f;
    private float accelerationRate;
    public float playerCurrentDrag { private set; get; } = 0f;
    public float playerCurrentGravity { private set; get; }
    private float dragModifier = 1f;
    private float gravityModifier = -1f;

    private const float positionCacheTick = 2f;
    private float timeSinceLastPosTick = positionCacheTick;

    // FLAGS
    public bool isGrounded { private set; get; }
    private bool preventWallSlide = false;
    //private bool hasWallJumpedToFly = false;
    public bool keepPlayerInBounds = true;
    private bool isHoldingInteractAfterShapeshift = false;
    private bool isDiving = false;

    public bool isInAirZone
    {
        get { return _isInAirZone; }
        private set
        {
            if (_isInAirZone == value) return;
            _isInAirZone = value;
            OnValueChange(_isInAirZone);
        }
    }
    private bool _isInAirZone;
    void OnValueChange(bool state)
    {
        GameManager.instance.UI.RefreshShapeshiftPrompt();
    }

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

    public void ForceVelocity(Vector2 vel)
    {
        playerBody.velocity = vel;
    }

    #region Update
    void Update()
    {
        if (!IsInit()) return;

        HandleJump();

        // Player has to let go of jumping button after changing to bird to prevent misuse of its ability
        isHoldingInteractAfterShapeshift = GameManager.instance.Input.interact && GameManager.instance.Input.shapeshift ? true : isHoldingInteractAfterShapeshift;
        isHoldingInteractAfterShapeshift = GameManager.instance.Input.interactUp && isHoldingInteractAfterShapeshift ? false : isHoldingInteractAfterShapeshift;

        if (isGrounded)
        {
            coyoteTimeCounter = player.data.coyoteTime;
            //if (hasWallJumpedToFly) hasWallJumpedToFly = false;
        }
        else coyoteTimeCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!IsInit()) return;

        HandleMovement();
        HandleBounds();
        HandleDirection();
        HandleDragAndGravity();
        HandlePlayerShape();

        player.PassPlayerPosition(transform.position);

        playerVelocity = playerBody.velocity;
        isGrounded = IsGrounded();
    }
    private void LateUpdate()
    {
        HandlePositionCaching();
        ConfinePlayerToBounds();
    }
    #endregion

    void HandlePlayerShape()
    {
        switch (player.playerShape)
        {
            case PlayerShape.DEFAULT:
                playerCurrentSpeed = player.data.defaultSpeed;
                playerCurrentDrag = player.data.drag;

                playerCollider.size = new Vector2(player.data.defaultWidth, 1);
                break;
            case PlayerShape.CAT:
                playerCurrentSpeed = player.data.catSpeed;
                // Drag is handled in walljump logic

                playerCollider.size = new Vector2(player.data.catWidth, 1);
                break;
            case PlayerShape.FLY:

                //if (hasWallJumped) hasWallJumpedToFly = true;

                playerCollider.size = new Vector2(player.data.catWidth, 1);
                break;
            default:
                break;
        }
    }

    #region Movement
    void HandleDragAndGravity()
    {
        gravityModifier = isBeingPushedUpwards ? -2 : 1f;
        //dragModifier = isBeingPushedUpwards ? 0f : 1f;

        playerBody.gravityScale = playerCurrentGravity * gravityModifier;
        playerBody.drag = playerCurrentDrag * dragModifier;

        switch (player.playerShape)
        {
            case PlayerShape.DEFAULT:
            case PlayerShape.CAT:
                //flyDragShiftCounter = 0;

                // def gravity when grounded
                if (playerBody.gravityScale >= 0) playerCurrentGravity = player.data.defaultGravityScale;
                
                // Faster fall speed
                if (playerBody.velocity.y < 0f && gravityModifier > 0) playerCurrentGravity = player.data.fallGravityScale;

                // Cat Wall Slide
                playerCurrentDrag = !isGrounded && !preventWallSlide && IsAgainstWall()
                    && GameManager.instance.Input.playerInput.x != 0 && player.playerShape == PlayerShape.CAT ? 
                    player.data.catWallSlideDrag : player.data.drag;

                break;

            case PlayerShape.FLY:
                isDiving = GameManager.instance.Input.interact && !isBeingPushedUpwards && !isHoldingInteractAfterShapeshift ? true : false;

                // Diving : Gliding
                playerCurrentGravity = isDiving ? player.data.diveGravityScale : player.data.defaultGravityScale * 2;
                //playerCurrentDrag = isDiving ? player.data.flyDropDrag : player.data.flyGlideDrag;
                playerCurrentDrag = player.data.flyGlideDrag;
                playerCurrentSpeed = isDiving ? player.data.flyDropSpeed : player.data.flySpeed;
                break;

            /*
            if (flyDragShiftCounter > 0)
            {
                playerCurrentSpeed = player.data.flySpeed;
            }
            else
            {
                playerCurrentSpeed = GameManager.instance.Input.interact && !isBeingPushedUpwards ? player.data.flyDropSpeed : player.data.flySpeed;
            }
                          
            if (hasWallJumpedToFly)
            {
                if (flyDragShiftCounter < (player.data.wallJumpDuration * 1.1f))
                {
                    flyDragShiftCounter += Time.deltaTime;
                }
                else
                {
                    hasWallJumpedToFly = false;
                }
            }
            */

        }

        // Clamp Vertical Velocity
        playerBody.velocity = new Vector2(playerBody.velocity.x,
                              Mathf.Clamp(playerBody.velocity.y, Mathf.Abs(player.data.maxFallSpeed) * -1, Mathf.Abs(player.data.maxFallSpeed)));
    }

    void HandleMovement()
    {
        // Reduce acceleration if player has recently wall jumped. Reset acceleration when ground is touched
        accelerationRate = hasWallJumped && !isGrounded
            || player.playerShape == PlayerShape.FLY ? player.data.airAcceleration : player.data.acceleration;

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
        // Jump Buffer
        if (GameManager.instance.Input.interactDown && player.playerShape == PlayerShape.CAT) jumpBufferCounter = player.data.jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // Reset Wall Slide Prevention
        if (!isGrounded && preventWallSlide && !IsAgainstWall()) preventWallSlide = false;

        // Jump Logic
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
            if (jumpBufferCounter > 0f && GameManager.instance.Input.interactDown && CanWallJump())
            {
                playerCurrentDrag = player.data.drag;
                Vector2 jumpDir = new Vector2(playerDirection * player.data.wallJumpStrength.x, player.data.wallJumpStrength.y);
                Debug.Log("WallJump " + jumpDir);
                playerBody.velocity = jumpDir;
                hasWallJumped = true;
                wallJumpCounter = 0f;
            }
        }

        // Shortens jump if not holding the jump button all the way    
        if (GameManager.instance.Input.interactUp && playerBody.velocity.y > 0f)
        {
            playerBody.velocity = new Vector2(playerBody.velocity.x, playerBody.velocity.y * .5f);
            if (playerBody.gravityScale >= 0) playerCurrentGravity = player.data.fallGravityScale;
            coyoteTimeCounter = 0f;
        }

        // Count time since last wall jump, used to prevent player from climbing with wall jumps
        wallJumpCounter = hasWallJumped ? wallJumpCounter += Time.deltaTime : 0f;
        hasWallJumped = wallJumpCounter > player.data.wallJumpDuration || isGrounded ? false : hasWallJumped;
    }

    bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;

        Vector2 castL = new Vector2(bounds.center.x + bounds.extents.x,
            bounds.center.y - bounds.extents.y);
        Vector2 castR = new Vector2(bounds.center.x - bounds.extents.x,
            bounds.center.y - bounds.extents.y);

        Vector2 groundDirection = -playerBody.transform.up;

        RaycastHit2D hitL = Physics2D.Raycast(castL, groundDirection, player.data.groundCheckDist, worldLayerMask);
        RaycastHit2D hitR = Physics2D.Raycast(castR, groundDirection, player.data.groundCheckDist, worldLayerMask);
        Color rayColorL = hitL.collider != null ? Color.green : Color.red;
        Color rayColorR = hitR.collider != null ? Color.green : Color.red;

        Debug.DrawRay(castL, groundDirection * player.data.groundCheckDist, rayColorL, 1);
        Debug.DrawRay(castR, groundDirection * player.data.groundCheckDist, rayColorR, 1);

        if (hitL.collider != null || hitR.collider != null) return true;
        else return false;        
    }

    bool IsAgainstWall()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 cast = new Vector2(playerDirection > 0 ? bounds.center.x + bounds.extents.x : bounds.center.x - bounds.extents.x,
            bounds.center.y - bounds.extents.y);
        Vector2 direction = playerBody.transform.right * playerDirection;

        RaycastHit2D hit = Physics2D.Raycast(cast, direction, player.data.groundCheckDist, wallJumpLayerMask);
        Color rayColor = hit.collider != null ? Color.green : Color.red;

        Debug.DrawRay(cast, direction * player.data.groundCheckDist, rayColor, 1);

        if (hit.collider != null) return true;
        else return false;
    }

    bool CanWallJump()
    {
        if (player.playerShape != PlayerShape.CAT || isGrounded) return false;

        //Vector2 playerOrigin = playerBody.transform.position;
        Bounds bounds = playerCollider.bounds;
        Vector2 castOrigin = new Vector2(playerBody.transform.position.x, bounds.center.y);

        Vector2 leftDirection = -playerBody.transform.right;
        Vector2 rightDirection = playerBody.transform.right;

        RaycastHit2D leftHit = Physics2D.Raycast(castOrigin, leftDirection, player.data.wallCheckDist, wallJumpLayerMask);
        RaycastHit2D rightHit = Physics2D.Raycast(castOrigin, rightDirection, player.data.wallCheckDist, wallJumpLayerMask);

        Color leftRayColor = leftHit.collider != null ? Color.green : Color.red;
        Color rightRayColor = rightHit.collider != null ? Color.green : Color.red;
        Debug.DrawRay(castOrigin, leftDirection * player.data.wallCheckDist, leftRayColor, 1);
        Debug.DrawRay(castOrigin, rightDirection * player.data.wallCheckDist, rightRayColor, 1);
        
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

    #region Air Zone
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Pusher")) return;

        isInAirZone = false;

        Debug.Log("Pushing player downwards.");
        isBeingPushedUpwards = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Pusher")) return;

        isInAirZone = true;

        if (player.playerShape == PlayerShape.FLY)
        {
            Debug.Log("Pushing player upwards.");
            isBeingPushedUpwards = true;
        }
        else if (playerBody.gravityScale < 0) isBeingPushedUpwards = false;

        //if (player.playerShape != PlayerShape.FLY) isInAirZone = true;
        //else isInAirZone = false;

    }

    #endregion

    #region Bounds stuff
    void HandleBounds()
    {
        if (!keepPlayerInBounds) return;

        float minX = GameManager.instance.levelBoundsMin.x + player.playerWidth;
        float maxX = GameManager.instance.levelBoundsMax.x - player.playerWidth;
        float maxY = GameManager.instance.levelBoundsMax.y - player.playerHeight;

        playerBody.position = new Vector2(Mathf.Clamp(playerBody.position.x, minX,maxX),
                                          Mathf.Clamp(playerBody.position.y, float.MinValue, maxY));

        // Prevents player from storing Y velocity when again top level bounds
        if (playerBody.position.y >= maxY && playerVelocity.y > 0) playerBody.velocity = new Vector2(playerBody.velocity.x, 0);

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

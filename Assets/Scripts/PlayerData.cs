using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float acceleration = 5f;
    public float airAcceleration = 2f;
    public float drag = 0f;
    public float catWallSlideDrag = 15f;
    public float flyDropDrag = 2f;
    public float flyGlideDrag = 10f;

    [Header("Collision")]
    //public float collisionWidth = .6f;
    public float groundCheckDist = .1f;
    public float wallCheckDist = .3f;
    public float shapeColXPos = .5f;

    [Header("Default Shape")]
    public float defaultSpeed = 2f;
    public float defaultWidth = .3f;
    public Vector2 defaultBoundsOffset;
    public Vector2 defaultBoundsSize;
    [Header("Cat Shape")]
    public float catSpeed = 3.5f;
    public float catWidth = .7f;
    public Vector2 catBoundsOffset;
    public Vector2 catBoundsSize;
    [Header("Fly Shape")]
    public float flySpeed = 15f;
    public float flyDropSpeed = 5f;
    //public float flyMaxSpeed = 6f;
    public Vector2 flyBoundsOffset;
    public Vector2 flyBoundsSize;

    [Header("Gravity")]
    public float defaultGravityScale = 1.5f;
    public float fallGravityScale = 2.5f;
    public float diveGravityScale = 10f;
    public float slideGravityScale = .2f;
    //public float pushedGravityScale = 1f;
    public float maxFallSpeed = 10f;

    [Header("Jump")]
    public float jumpStrength = 7f;
    public Vector2 wallJumpStrength = new Vector2(8f, 8f);
    public float wallJumpDuration = 0.4f;
    public float coyoteTime = .2f;
    public float jumpBufferTime = .2f;
}

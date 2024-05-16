using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float acceleration = 5f;
    public float airAcceleration = 2f;

    [Header("Default Shape")]
    public float defaultSpeed = 2f;
    public float defaultDrag = 0f;
    [Header("Cat Shape")]
    public float catSpeed = 3.5f;
    public float catDrag = 2f;
    [Header("Fly Shape")]
    public float flyMinSpeed = 1.5f;
    public float flySpeed = 3f;
    public float flyMaxSpeed = 6f;
    public float flyMinDrag = 0f;
    public float flyDrag = 5f;
    public float flyMaxDrag = 10f;

    [Header("Gravity")]
    public float defaultGravityScale = 1f;
    public float fallGravityScale = 1.5f;
    public float maxFallSpeed = 10f;

    [Header("Jump")]
    public float jumpStrength = 7f;
    public Vector2 wallJumpStrength = new Vector2(8f, 8f);
    public float wallJumpDuration = 0.4f;
    public float coyoteTime = .2f;
    public float jumpBufferTime = .2f;
}

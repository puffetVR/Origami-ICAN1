using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;

        if (instance != this) Destroy(this);

        playerMovement = GetComponentInChildren<PlayerMovement>();
    }
    #endregion

    #region Player Shape Enum
    public enum PlayerShape { DEFAULT, CAT, FLY }
    public SpriteRenderer playerSprite;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite catSprite;
    [SerializeField] private Sprite flySprite;

    public PlayerShape playerShape
    {
        get { return _playerShape; }
        private set
        {
            if (_playerShape == value) return;
            _playerShape = value;
            OnPlayerShapeChanged(_playerShape);
        }
    }
    private PlayerShape _playerShape;
    void OnPlayerShapeChanged(PlayerShape shape)
    {
        switch (shape)
        {
            case PlayerShape.DEFAULT:
                playerSprite.sprite = defaultSprite;
                break;
            case PlayerShape.CAT:
                playerSprite.sprite = catSprite;
                break;
            case PlayerShape.FLY:
                playerSprite.sprite = flySprite;
                break;
            default:
                break;
        }

        Debug.Log(shape);
    }
    #endregion

    #region Attributes
    [Header("References")]
    public PlayerData data;
    public PlayerMovement playerMovement { get; private set; }
    public CameraController playerCamera { get; private set; }
    #endregion

    public Vector3 playerPosition { get; private set; }
    public float playerWidth { get; private set; }
    public float playerHeight { get; private set; }

    void Start()
    {
        OnPlayerShapeChanged(playerShape);
    }

    void Update()
    {
        PlayerInput();

        if (playerMovement.isGrounded && playerShape == PlayerShape.FLY) playerShape = PlayerShape.CAT;

        playerWidth = playerSprite.bounds.size.x;
        playerHeight = playerSprite.bounds.size.y;
    }

    public void PassPlayerPosition(Vector3 pos)
    {
        playerPosition = pos;
    }

    void PlayerInput()
    {
        if (Input.GetButtonDown("Shape"))
        {

            Shapeshift();
        }

    }

    void Shapeshift()
    {
        if (playerMovement.isGrounded) playerShape = playerShape == PlayerShape.DEFAULT
                                            || playerShape == PlayerShape.FLY ?
                                               PlayerShape.CAT : PlayerShape.DEFAULT;
        else playerShape = playerShape == PlayerShape.DEFAULT
                        || playerShape == PlayerShape.CAT ?
                           PlayerShape.FLY : PlayerShape.CAT;
    }


}

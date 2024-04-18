using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;

        if (instance != this) Destroy(this);
    }
    #endregion

    #region Player Shape Enum
    public enum PlayerShape { DEFAULT, GRAB, CAT, FLY }
    public SpriteRenderer playerSprite;
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite grabSprite;
    [SerializeField] Sprite catSprite;
    [SerializeField] Sprite flySprite;

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
            case PlayerShape.GRAB:
                playerSprite.sprite = grabSprite;
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
    // VALUES
    public float defaultSpeed { private set; get; } = 2f;
    public float catSpeed { private set; get; } = 3.5f;
    #endregion

    void Start()
    {
        OnPlayerShapeChanged(playerShape);
    }

    void Update()
    {
        PlayerInput();
    }

    void PlayerInput()
    {
        if (Input.GetKey(KeyCode.Alpha1)) playerShape = PlayerShape.GRAB;
        if (Input.GetKey(KeyCode.Alpha2)) playerShape = PlayerShape.CAT;
        if (Input.GetKey(KeyCode.Alpha3)) playerShape = PlayerShape.FLY;
    }

}

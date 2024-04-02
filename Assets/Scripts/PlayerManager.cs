using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;

        if (instance != this) Destroy(this);
    }

    public enum PlayerShape { DEFAULT, GRAB, CAT, FLY }

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
        defaultShape.SetActive(false);
        grabShape.SetActive(false);
        catShape.SetActive(false);
        flyShape.SetActive(false);

        switch (shape)
        {
            case PlayerShape.DEFAULT:
                defaultShape.SetActive(true);
                break;
            case PlayerShape.GRAB:
                grabShape.SetActive(true);
                break;
            case PlayerShape.CAT:
                catShape.SetActive(true);
                break;
            case PlayerShape.FLY:
                flyShape.SetActive(true);
                break;
            default:
                break;
        }

        Debug.Log(shape);
    }

    // SHAPE FX
    public GameObject defaultShape;
    public GameObject grabShape;
    public GameObject catShape;
    public GameObject flyShape;

    // VALUES
    public float defaultSpeed = 0.1f;
    public float catSpeed = 0.3f;

    void Start()
    {
        
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

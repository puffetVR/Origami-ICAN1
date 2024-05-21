using System.Collections;
using System.Collections.Generic;
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

        Debug.Log("<color=magenta>Current shape: " + shape + "</color>");
    }
    #endregion

    #region Attributes
    [Header("References")]
    public PlayerData data;
    public PlayerMovement playerMovement { get; private set; }
    public CameraController playerCamera { get; private set; }
    #endregion
    public float playerWidth { get; private set; }
    public float playerHeight { get; private set; }

    void Start()
    {
        OnPlayerShapeChanged(playerShape);
    }

    void Update()
    {
        PlayerInput();

        if (interactibles != null)
        {
            GetTargetInteractible();

            // UI interaction
            GameManager.instance.UI.InteractionPrompt(targetInteractible);
        }

        if (playerMovement.isGrounded && playerShape == PlayerShape.FLY) playerShape = PlayerShape.CAT;

        playerWidth = playerSprite.bounds.size.x;
        playerHeight = playerSprite.bounds.size.y;
    }

    #region Player Position
    public Vector3 playerPosition { get; private set; }

    public void PassPlayerPosition(Vector3 pos)
    {
        playerPosition = pos;
    }
    #endregion

    void PlayerInput()
    {
        if (GameManager.instance.Input.shapeshift) Shapeshift();

        if (GameManager.instance.Input.interact) Interact();

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

    #region Interaction
    public Interactible targetInteractible { get; private set; }
    public List<Interactible> interactibles { get; private set; } = new List<Interactible>();

    public void AddToInteractibles(Interactible interactible)
    {
        if (interactibles.Contains(interactible))
        {
            Debug.LogWarning(interactible.gameObject.name + " is already listed.");
            return;
        }

        interactibles.Add(interactible);
    }
    public void RemoveFromInteractibles(Interactible interactible)
    {
        if (!interactibles.Contains(interactible))
        {
            Debug.LogWarning(interactible.gameObject.name + " is not listed.");
            return;
        }

        interactibles.Remove(interactible);
    }

    void Interact()
    {
        if (playerShape != PlayerShape.DEFAULT) return;

        Debug.Log("<color=yellow>[INTERACT]</color>");
        if (targetInteractible != null) targetInteractible.Interact();
    }

    void GetTargetInteractible()
    {
        if (interactibles.Count == 0)
        {
            targetInteractible = null;
            return;
        }

        targetInteractible = interactibles[0];

        if (interactibles.Count > 1) targetInteractible = GetClosestInteractible();

    }

    Interactible GetClosestInteractible()
    {
        Interactible currentTarget = targetInteractible;
        float distToNearest = Vector2.Distance(playerPosition, currentTarget.transform.position);

        for (int i = 1; i < interactibles.Count; i++)
        {
            float distToCurrent = Vector2.Distance(playerPosition, interactibles[i].transform.position);

            if (distToCurrent < distToNearest)
            {
                currentTarget = interactibles[i];
                break;
            }
        }

        return currentTarget;
    }
    #endregion
}

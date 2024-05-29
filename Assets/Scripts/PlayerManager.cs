using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;

        if (instance != this) Destroy(this);

        move = GetComponentInChildren<PlayerMovement>();
        cam = GetComponentInChildren<CameraController>();
        GameManager.instance.Player = this;
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

            poof.Play();
            poofSound.Play();

            OnPlayerShapeChanged(_playerShape);
        }
    }
    private PlayerShape _playerShape;
    void OnPlayerShapeChanged(PlayerShape shape)
    {
        GameManager.instance.UI.interactionActive = false;

        anim.SetLayerWeight(1, 0);
        anim.SetLayerWeight(2, 0);
        anim.SetLayerWeight(3, 0);

        glideSound.Stop();

        switch (shape)
        {
            case PlayerShape.DEFAULT:
                GameManager.instance.UI.interactionActive = true;
                playerSprite.sprite = defaultSprite;
                spriteBounds.offset = data.defaultBoundsOffset;
                spriteBounds.size = data.defaultBoundsSize;
                //anim.SetLayerWeight(0, 0);
                break;
            case PlayerShape.CAT:
                playerSprite.sprite = catSprite;
                spriteBounds.offset = data.catBoundsOffset;
                spriteBounds.size = data.catBoundsSize;
                //anim.SetLayerWeight(1, 0);
                break;
            case PlayerShape.FLY:
                glideSound.Play();
                glideInstance = glideSound.EventInstance;

                playerSprite.sprite = flySprite;
                spriteBounds.offset = data.flyBoundsOffset;
                spriteBounds.size = data.flyBoundsSize;
                //anim.SetLayerWeight(2, 0);
                break;
            default:
                break;
        }

        anim.SetLayerWeight((int)shape + 1, 1);

        // Get Player Size
        playerWidth = spriteBounds.bounds.size.x / 1.5f;
        playerHeight = spriteBounds.bounds.size.y / 1.5f;

        GameManager.instance.UI.RefreshUI();

        Debug.Log("<color=magenta>Current shape: " + shape + "</color>");
    }
    #endregion

    #region Attributes
    [Header("References")]
    public PlayerData data;
    public PlayerMovement move { get; private set; }
    public CameraController cam { get; private set; }
    public Animator anim;

    #endregion
    public float playerWidth { get; private set; }
    public float playerHeight { get; private set; }
    [SerializeField] private BoxCollider2D spriteBounds;

    public Transform shapeshiftPromptOrigin;

    public Transform playerPosToBook;

    public ParticleSystem poof;


    [Header("Sounds")]
    [SerializeField] private StudioEventEmitter poofSound;
    public StudioEventEmitter glideSound;
    public FMOD.Studio.EventInstance glideInstance { get; private set; }
    public EventReference jumpSound;
    public EventReference wallJumpSound;
    //float t = 0;

    void Start()
    {
        playerShape = GameManager.instance.defaultShape;
        OnPlayerShapeChanged(playerShape);
    }

    void Update()
    {
        if (GameManager.instance.isPaused) return;

        PlayerInput();

        // Handle Interactions
        if (interactibles != null)
        {
            GetTargetInteractible();

            // UI interaction
            GameManager.instance.UI.InteractionPrompt(targetInteractible);
        }

        // Force shapeshift back to cat when touching ground as bird
        if (move.isGrounded && playerShape == PlayerShape.FLY && !move.isInAirZone)
        {
            Debug.Log("Forced shapeshift");
            playerShape = PlayerShape.CAT;
            move.ForceVelocity(Vector2.zero);
        }

        glideInstance.setParameterByName("Volume", move.playerVelocity.sqrMagnitude / 100);

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

        if (GameManager.instance.Input.interactDown) Interact();

    }

    void Shapeshift()
    {
        if (move.isGrounded && GameManager.instance.unlockCat)
        {

            if (move.isInAirZone && GameManager.instance.unlockBird)
            {
                playerShape = PlayerShape.FLY;
                return;
            }

            playerShape = playerShape == PlayerShape.DEFAULT
                                            || playerShape == PlayerShape.FLY ?
                                               PlayerShape.CAT : PlayerShape.DEFAULT;
        }
        
        else if (GameManager.instance.unlockBird && GameManager.instance.unlockCat) 
            playerShape = playerShape == PlayerShape.DEFAULT
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

        if (targetInteractible.gameObject.activeSelf == false)
        {
            interactibles.Remove(targetInteractible);
            targetInteractible = null;
        }

        if (interactibles.Count > 1 && targetInteractible != null) targetInteractible = GetClosestInteractible();

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

    public void BookCutscene()
    {
        StartCoroutine(BookCutsceneRoutine());
    }

    IEnumerator BookCutsceneRoutine()
    {

        GameManager.instance.lockPlayerControl = true;

        // Move player to pos
        move.forcedXMovement = 1;

        yield return new WaitUntil(() => move.playerVelocity.x == 0);

        move.forcedXMovement = 0;

        // When in pos, trigger animation
        anim.SetBool("climb", true);

        yield return new WaitForSeconds(3);       

        // get book pos
        Vector2 oui = GameObject.Find("BookPos").transform.position;

        // Make player climb book (kinematic rb, move rb position to above book)
        Rigidbody2D rb = move.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        rb.position = oui;

        yield return null;
        anim.SetBool("climb", false);

        yield return null;

        //yield return new WaitUntil(() => Vector2.Distance(rb.position, oui) == 0);

        // Spawn invisible wall so player cant get down from book
        GameObject.Find("BookWall").GetComponent<BoxCollider2D>().enabled = true;
        yield return null;

        rb.isKinematic = false;
        GameManager.instance.lockPlayerControl = false;

        // Lift book up
        cam.ShakeCamera(.15f, .3f);
        GameObject.Find("Book").GetComponent<Animator>().SetBool("lift", true);

        // Level end on shelf




        yield return null;
    }

}

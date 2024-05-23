using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        instance = this;

        if (instance != this) Destroy(this);

        Player = PlayerManager.instance;
    }
    #endregion

    public PlayerManager Player { get; private set; }
    public UIManager UI;
    public InputManager Input;

    public LevelData levelData;
    //[Header("Level Stuff teehee")]
    //public bool hasEntryTransition 

    public BoxCollider2D levelBounds;
    public Vector2 levelBoundsMin { get; private set; }
    public Vector2 levelBoundsMax { get; private set; }

    private void FixedUpdate()
    {
        CalculateLevelBounds();
    }

    void CalculateLevelBounds()
    {
        levelBoundsMin = new Vector2(levelBounds.bounds.min.x, levelBounds.bounds.min.y);
        levelBoundsMax = new Vector2(levelBounds.bounds.max.x, levelBounds.bounds.max.y);
    }
}

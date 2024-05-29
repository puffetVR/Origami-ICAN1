using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        instance = this;

        if (instance != this) Destroy(this);

        Player = PlayerManager.instance;

        if (fadeInLevel) StartCoroutine(UI.FadeOut());
    }
    #endregion

    public PlayerManager Player;
    public UIManager UI;
    public InputManager Input;

    public bool isPaused { get; private set; }
    public bool lockPlayerControl = false;
    public bool keepPlayerInBounds = true;

    [Header("Level Stuff")]
    [SerializeField] private int nextLevelIndex;
    public BoxCollider2D levelBounds;
    public Vector2 levelBoundsMin { get; private set; }
    public Vector2 levelBoundsMax { get; private set; }
    public bool fadeInLevel = true;

    public PlayerManager.PlayerShape defaultShape;
    
    public bool unlockCat = true;
    public bool unlockBird = true;


    public void StripPlayerControl(bool s)
    {
        lockPlayerControl = s;
        keepPlayerInBounds = !s;
    }
    public void UnlockBird()
    {
        unlockBird = true;
    }

    public void UnlockCat()
    {
        unlockCat = true;
    }

    private void Update()
    {
        // Pause Game Input
        if (Input.pause) PauseGame(!isPaused);
    }

    private void FixedUpdate()
    {
        CalculateLevelBounds();
    }

    void CalculateLevelBounds()
    {
        levelBoundsMin = new Vector2(levelBounds.bounds.min.x, levelBounds.bounds.min.y);
        levelBoundsMax = new Vector2(levelBounds.bounds.max.x, levelBounds.bounds.max.y);
    }

    public void PauseGame(bool state)
    {
        Time.timeScale = state == true ? 0 : 1;

        UI.pauseMenu.SetActive(state);

        isPaused = state;
    }

    public IEnumerator LevelEnd()
    {
        StripPlayerControl(true);
        //lockPlayerControl = true;
        //keepPlayerInBounds = false;
        Player.move.forcedXMovement = 1;

        StartCoroutine(LoadNextLevel());

        yield return null;
    }

    public IEnumerator LoadNextLevel()
    {
        StartCoroutine(UI.FadeIn());

        yield return new WaitUntil(() => UI.hasFadedIn);

        Debug.Log("Loading next level.");
        SceneManager.LoadScene(nextLevelIndex);
    }

    public IEnumerator KillPlayer()
    {
        Player.cam.followTarget = false;
        StartCoroutine(UI.FadeIn());

        yield return new WaitUntil(() => UI.hasFadedIn);

        Player.move.ResetPlayerToLastKnownPosition();

        StartCoroutine(UI.FadeOut());
        Player.cam.followTarget = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public Image currentSlide;
    public List<Sprite> sprites = new List<Sprite>();
    int currentSlideIndex = -1;
    //bool isReadyForNextSlide = false;

    public GameObject skipPrompt;

    private void Awake()
    {
        skipPrompt.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(NextSlide());
    }

    IEnumerator NextSlide()
    {
        // Fade
        StartCoroutine(GameManager.instance.UI.FadeOut());

        currentSlideIndex++;
        currentSlide.sprite = sprites[currentSlideIndex];
        
        // Has done fading or skipped?
        yield return null;
        yield return new WaitUntil(() => GameManager.instance.UI.hasFadedOut
        || GameManager.instance.Input.jumpDown && !GameManager.instance.UI.hasFadedOut);

        GameManager.instance.UI.hasFadedOut = true;
        skipPrompt.SetActive(true);

        yield return null;
        yield return new WaitUntil(() => GameManager.instance.Input.jumpDown);

        skipPrompt.SetActive(false);

        // Fade
        StartCoroutine(GameManager.instance.UI.FadeIn());

        if (currentSlideIndex >= sprites.Count - 1)
        {
            StartCoroutine(GameManager.instance.LoadNextLevel());
            yield break;
        }

        // Has done fading or skipped?
        yield return null;
        yield return new WaitUntil(() => GameManager.instance.UI.hasFadedIn
        || GameManager.instance.Input.jumpDown && !GameManager.instance.UI.hasFadedIn);

        GameManager.instance.UI.hasFadedIn = true;

        StartCoroutine(NextSlide());
    }

}

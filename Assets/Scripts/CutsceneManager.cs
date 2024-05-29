using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public Image currentSlide;
    //public List<Sprite> sprites = new List<Sprite>();
    public List<Slide> slides = new List<Slide>();
    int currentSlideIndex = -1;

    public GameObject skipPrompt;

    [System.Serializable]
    public class Slide
    {
        public Sprite image;
        public FMODUnity.EventReference sound;
        public UnityEvent action;
        //public bool fade = true;
    }

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
        currentSlideIndex++;

        // Fade
        StartCoroutine(GameManager.instance.UI.FadeOut());
        //if (!slides[currentSlideIndex].fade) GameManager.instance.UI.hasFadedOut = true;

        //currentSlide.sprite = sprites[currentSlideIndex];
        currentSlide.sprite = slides[currentSlideIndex].image;
        if (!slides[currentSlideIndex].sound.IsNull) FMODUnity.RuntimeManager.CreateInstance(slides[currentSlideIndex].sound).start();
        if (slides[currentSlideIndex].action != null) slides[currentSlideIndex].action.Invoke();

        // Has done fading or skipped?
        yield return null;
        yield return new WaitUntil(() => GameManager.instance.UI.hasFadedOut
        || !GameManager.instance.isPaused && GameManager.instance.Input.jumpDown && !GameManager.instance.UI.hasFadedOut);

        GameManager.instance.UI.hasFadedOut = true;
        skipPrompt.SetActive(true);

        yield return null;
        yield return new WaitUntil(() => !GameManager.instance.isPaused && GameManager.instance.Input.jumpDown);

        skipPrompt.SetActive(false);

        // Fade
        StartCoroutine(GameManager.instance.UI.FadeIn());
        //if (!slides[currentSlideIndex].fade) GameManager.instance.UI.hasFadedIn = true;

        if (currentSlideIndex >= slides.Count - 1)
        {
            StartCoroutine(GameManager.instance.LoadNextLevel());
            yield break;
        }

        // Has done fading or skipped?
        yield return null;
        yield return new WaitUntil(() => GameManager.instance.UI.hasFadedIn
        || !GameManager.instance.isPaused && GameManager.instance.Input.jumpDown && !GameManager.instance.UI.hasFadedIn);

        GameManager.instance.UI.hasFadedIn = true;

        StartCoroutine(NextSlide());
    }

}

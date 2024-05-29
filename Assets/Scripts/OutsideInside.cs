using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class OutsideInside : MonoBehaviour
{
    public SpriteRenderer fader;

    public bool isInside = false;
    bool _isInside;

    bool hasFadedIn;
    bool hasFadedOut;

    private void Start()
    {
        ShowState(isInside);
    }

    void FixedUpdate()
    {
        if (isInside != _isInside)
        {
            Debug.Log("Switching inside state");
            _isInside = isInside;
            ShowState(_isInside);
        } 
    }

    public void ShowState(bool state)
    {
        Debug.Log("Showing inside " + state);

        //fader.gameObject.SetActive(!state);

        if (!isInside) StartCoroutine(FadeIn());
        else StartCoroutine(FadeOut());

    }

    IEnumerator FadeIn()
    {
        hasFadedOut = true;
        hasFadedIn = false;
        Debug.Log("Start fade in");

        Color fadeCol = fader.color;

        for (float alpha = 0f; alpha <= 1; alpha += 2f * Time.deltaTime)
        {
            if (hasFadedIn)
            {
                Debug.Log("Skipped fade in");
                fadeCol.a = 1;
                fader.color = fadeCol;
                yield break;
            }

            fadeCol.a = alpha;
            fader.color = fadeCol;
            yield return null;
        }

        hasFadedIn = true;
        Debug.Log("End fade in");
    }

    IEnumerator FadeOut()
    {
        hasFadedIn = true;
        hasFadedOut = false;
        Debug.Log("Start fade out");

        Color fadeCol = fader.color;

        for (float alpha = 1f; alpha >= 0; alpha -= 3f * Time.deltaTime)
        {
            if (hasFadedOut)
            {
                Debug.Log("Skipped fade out");
                fadeCol.a = 0;
                fader.color = fadeCol;
                yield break;
            }

            fadeCol.a = alpha;
            fader.color = fadeCol;
            yield return null;
        }

        hasFadedOut = true;
        Debug.Log("End fade out");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isInside = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isInside = false;
    }
}

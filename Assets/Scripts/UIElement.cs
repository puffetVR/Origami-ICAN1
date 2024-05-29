using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIElement : MonoBehaviour
{
    public bool isUi = false;

    public enum ElementType { INTERACT, SHAPESHIFT, JUMP, MOVE }
    public ElementType eType;

    UIManager ui;
    public Image image;
    public TMPro.TextMeshProUGUI text;

    bool hasFadedIn;
    bool hasFadedOut;

    void Start()
    {
        ui = GameManager.instance.UI;
        ui.uiElements.Add(this);
        ui.RefreshUI();

        if (!isUi)
        {
            Color fadeCol = image.color;
            fadeCol.a = 0;
            image.color = fadeCol;
            text.color = fadeCol;
        }


    }

    public void SetUISprite(ControllerUI scheme)
    {
        switch (eType)
        {
            case ElementType.INTERACT:
                image.sprite = scheme.interactKey;
                break;
            case ElementType.SHAPESHIFT:
                image.sprite = scheme.shapeshiftKey;
                break;
            case ElementType.JUMP:
                image.sprite = scheme.jumpKey;
                break;
            case ElementType.MOVE:
                image.sprite = scheme.movementKeys;
                break;
            default:
                break;
        }
        
    }

    public void Appear()
    {
        StartCoroutine(FadeIn());
    }

    public void Disappear()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        hasFadedOut = true;
        hasFadedIn = false;
        Debug.Log("Start fade in");

        Color fadeCol = image.color;
        //if (isActive) fadeCol = ui.activeColor;
        //else fadeCol = ui.unactiveColor;

        for (float alpha = 0; alpha <= 1; alpha += 4f * Time.deltaTime)
        {
            if (hasFadedIn)
            {
                Debug.Log("Skipped fade in");
                fadeCol.a = 1;
                image.color = fadeCol;
                text.color = fadeCol;
                yield break;
            }

            fadeCol.a = alpha;
            image.color = fadeCol;
            text.color = fadeCol;
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

        Color fadeCol = image.color;
        float cA = fadeCol.a;

        //if (isActive) fadeCol = ui.activeColor;
        //else fadeCol = ui.unactiveColor;

        for (float alpha = 1; alpha >= 0; alpha -= 4f * Time.deltaTime)
        {
            if (hasFadedOut)
            {
                Debug.Log("Skipped fade out");
                fadeCol.a = 0;
                image.color = fadeCol;
                text.color = fadeCol;
                yield break;
            }

            fadeCol.a = alpha;
            image.color = fadeCol;
            text.color = fadeCol;
            yield return null;
        }

        hasFadedOut = true;
        Debug.Log("End fade out");
    }

}

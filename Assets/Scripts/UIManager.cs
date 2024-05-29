using UnityEngine;
using UnityEngine.UI;
using static InputManager;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private ControllerUI pcData;
    private ControllerUI psData;
    private ControllerUI xbData;

    private ControllerUI currentSchemeData;

    public Image interactionPromptSprite;
    public Image shapeshiftPromptSprite;

    public List<UIElement> uiElements = new List<UIElement>();

    Interactible currentInteractible;
    public GameObject interactionPrompt;
    public bool interactionActive = false;
    public Color activeColor;
    public Color unactiveColor;
    public TextMeshProUGUI interactionText;
    public Image interactionButton;
    public GameObject unactiveCross;

    public GameObject shapeshiftPrompt;

    //public UIElement shapeshiftElement;
    //public UIElement interactionElement;

    public GameObject pauseMenu;
    public Image fader;
    public bool hasFadedIn = false;
    public bool hasFadedOut = false;

    private void Awake()
    {
        InitControlData();
    }

    private void Start()
    {
        RefreshUI();
    }

    void InitControlData()
    {
        pcData = Resources.Load<ControllerUI>("Controllers/Keyboard");
        psData = Resources.Load<ControllerUI>("Controllers/Playstation");
        xbData = Resources.Load<ControllerUI>("Controllers/Xbox");

        currentSchemeData = pcData;
    }

    private void FixedUpdate()
    {
        if (PlayerManager.instance) shapeshiftPrompt.transform.position = PlayerManager.instance.shapeshiftPromptOrigin.position;
        //if (PlayerManager.instance) shapeshiftElement.transform.position = PlayerManager.instance.shapeshiftPromptOrigin.position;
    }

    public void RefreshUI()
    {
        GameManager.instance.PauseGame(GameManager.instance.isPaused);

        RefreshControlsUI();
        RefreshInteractionPrompt(currentInteractible ? true : false);
        RefreshShapeshiftPrompt();
    }

    public void RefreshControlsUI()
    {
        switch (GameManager.instance.Input.controller)
        {
            case ControlScheme.PC:
                currentSchemeData = pcData;
                break;
            case ControlScheme.PS:
                currentSchemeData = psData;
                break;
            case ControlScheme.XB:
                currentSchemeData = xbData;
                break;
        }

        foreach (var item in uiElements)
        {
            item.SetUISprite(currentSchemeData);
        }
    }

    public void RefreshInteractionPrompt(bool state)
    {
        interactionButton.color = interactionActive ? activeColor : unactiveColor;
        interactionText.color = interactionActive ? activeColor : unactiveColor;

        unactiveCross.SetActive(!interactionActive);
        interactionPrompt.SetActive(state);
        //if (state && state == interactionActive) interactionElement.Appear();
        //else
        //{
        //    interactionElement.Disappear();
        //    unactiveCross.SetActive(false);
        //}
        RefreshShapeshiftPrompt();
    }

    public void RefreshShapeshiftPrompt()
    {
        if (!PlayerManager.instance) return;

        bool state = currentInteractible && !interactionActive
            || PlayerManager.instance.move.isInAirZone && PlayerManager.instance.playerShape != PlayerManager.PlayerShape.FLY && GameManager.instance.unlockBird
            ? true : false;

        //if (state) shapeshiftElement.Appear();
        //else shapeshiftElement.Disappear();
        shapeshiftPrompt.SetActive(state);
    }

    public void InteractionPrompt(Interactible interactible)
    {
        if (currentInteractible != interactible) currentInteractible = interactible;
        else return;

        Debug.Log("Using InteractionPrompt()");

        RefreshInteractionPrompt(interactible != null ? true : false);

        if (interactible != null) interactionPrompt.transform.position =
                interactible.interactionTextOrigin != null ? interactible.interactionTextOrigin.position : interactible.transform.position;
    }

    public void GoToMainMenu()
    {
        Debug.Log("Going to Main menu...");
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    public IEnumerator FadeIn()
    {
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

    public IEnumerator FadeOut()
    {
        hasFadedOut = false;
        Debug.Log("Start fade out");

        Color fadeCol = fader.color;

        for (float alpha = 1f; alpha >= 0; alpha -= 1f * Time.deltaTime)
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
}
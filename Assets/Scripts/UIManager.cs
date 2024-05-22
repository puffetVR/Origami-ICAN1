using UnityEngine;
using UnityEngine.UI;
using static InputManager;
using TMPro;

public class UIManager : MonoBehaviour
{

    private ControllerUI pcData;
    private ControllerUI psData;
    private ControllerUI xbData;

    private ControllerUI currentSchemeData;

    public Image interactionPromptSprite;
    public Image shapeshiftPromptSprite;

    Interactible currentInteractible;
    public GameObject interactionPrompt;
    public bool interactionActive = false;
    public Color activeColor;
    public Color unactiveColor;
    public TextMeshProUGUI interactionText;
    public Image interactionButton;
    public GameObject unactiveCross;

    public GameObject shapeshiftPrompt;

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
        shapeshiftPrompt.transform.position = PlayerManager.instance.shapeshiftPromptOrigin.position;
    }
    public void RefreshUI()
    {
        RefreshControlsUI();
        RefreshPrompts(currentInteractible ? true : false);
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

        interactionPromptSprite.sprite = currentSchemeData.interactKey;
        shapeshiftPromptSprite.sprite = currentSchemeData.shapeshiftKey;
    }

    void RefreshPrompts(bool state)
    {
        interactionButton.color = interactionActive ? activeColor : unactiveColor;
        interactionText.color = interactionActive ? activeColor : unactiveColor;

        unactiveCross.SetActive(!interactionActive);
        interactionPrompt.SetActive(state);
        shapeshiftPrompt.SetActive(state && !interactionActive ? true : false);
    }

    public void InteractionPrompt(Interactible interactible)
    {
        if (currentInteractible != interactible) currentInteractible = interactible;
        else return;

        Debug.Log("Using InteractionPrompt()");

        RefreshPrompts(interactible != null ? true : false);

        if (interactible != null) interactionPrompt.transform.position = 
                interactible.interactionTextOrigin != null ? interactible.interactionTextOrigin.position : interactible.transform.position;
    }
}

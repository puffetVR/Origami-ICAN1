using UnityEngine;
using UnityEngine.UI;
using static InputManager;

public class UIManager : MonoBehaviour
{

    private ControllerUI pcData;
    private ControllerUI psData;
    private ControllerUI xbData;

    private ControllerUI currentSchemeData;

    public Image interactionPromptSprite;

    public GameObject interactionPrompt;
    Interactible currentInteractible;

    private void Awake()
    {
        InitControlData();
    }

    void InitControlData()
    {
        pcData = Resources.Load<ControllerUI>("Controllers/Keyboard");
        psData = Resources.Load<ControllerUI>("Controllers/Playstation");
        xbData = Resources.Load<ControllerUI>("Controllers/Xbox");

        currentSchemeData = pcData;
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

        SetSprites();
    }

    void SetSprites()
    {
        interactionPromptSprite.sprite = currentSchemeData.interactKey;
    }

    public void InteractionPrompt(Interactible interactible)
    {
        if (currentInteractible != interactible) currentInteractible = interactible;
        else return;

        interactionPrompt.SetActive(interactible != null ? true : false);

        if (interactible != null) interactionPrompt.transform.position = 
                interactible.interactionTextOrigin != null ? interactible.interactionTextOrigin.position : interactible.transform.position;
    }
}

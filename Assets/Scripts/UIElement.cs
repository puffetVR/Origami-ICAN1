using UnityEngine;
using UnityEngine.UI;

public class UIElement : MonoBehaviour
{
    public enum ElementType { INTERACT, SHAPESHIFT, JUMP, MOVE }
    public ElementType eType;

    UIManager ui;
    public Image image;

    void Start()
    {
        ui = GameManager.instance.UI;
        ui.uiElements.Add(this);
        ui.RefreshUI();
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

}

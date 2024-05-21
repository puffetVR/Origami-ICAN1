using UnityEngine;

[CreateAssetMenu(menuName = "UI/ControlScheme")]
public class ControllerUI : ScriptableObject
{
    [Header("Movement")]
    public Sprite movementKeys;
    public Sprite downKey;

    [Header("Actions")]
    public Sprite interactKey;
    public Sprite shapeshiftKey;
    
}

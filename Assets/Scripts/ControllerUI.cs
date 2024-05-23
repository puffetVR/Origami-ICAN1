using UnityEngine;

[CreateAssetMenu(menuName = "UI/ControlScheme")]
public class ControllerUI : ScriptableObject
{
    [Header("Movement")]
    public Sprite movementKeys;
    public Sprite diveKey;
    public Sprite jumpKey;

    [Header("Actions")]
    public Sprite shapeshiftKey;
    public Sprite interactKey;
    
}

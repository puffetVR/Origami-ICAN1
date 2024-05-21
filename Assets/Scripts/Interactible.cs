using UnityEngine;
using UnityEngine.Events;

public class Interactible : MonoBehaviour
{
    private PlayerManager player;

    public float interactionRadius = 1f;
    public Transform interactionOrigin;
    public Transform interactionTextOrigin;

    [SerializeField] private UnityEvent OnInteract;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(InteractionPosition(), interactionRadius);
    }

    Vector2 InteractionPosition()
    {
        return interactionOrigin ? interactionOrigin.position : transform.position;
    }

    void Start()
    {
        player = PlayerManager.instance;
    }

    void LateUpdate()
    {
        if (!player) return;

        if (IsPlayerInRange() && !IsListed()) player.AddToInteractibles(this);

        if (!IsPlayerInRange() && IsListed()) player.RemoveFromInteractibles(this);
    }

    public bool IsPlayerInRange()
    {
        return Vector2.Distance(player.playerPosition, InteractionPosition()) < interactionRadius ? true : false;
    }

    bool IsListed()
    {
        // Null reference safety check
        //if (player == null || player.interactibles == null) return false;

        if (player.interactibles.Contains(this)) return true;
        else return false;

    }

    public void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
        OnInteract.Invoke();
    }
}

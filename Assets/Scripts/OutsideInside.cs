using UnityEngine;
using UnityEngine.UI;

public class OutsideInside : MonoBehaviour
{
    public SpriteRenderer[] images = new SpriteRenderer[0];

    public bool isInside = false;
    bool _isInside;

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

        foreach (var image in images)
        {
            image.gameObject.SetActive(!state);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameManager Game;
    Camera playerCamera;

    Vector3 cameraPosition;
    public float cameraFollowSpeed = 1f;
    float currentZoom = defaultZoom;
    [Range(minZoom, maxZoom)] public float cameraZoom = defaultZoom;
    public const float minZoom = 1.20f;
    public const float maxZoom = 10f;
    const float defaultZoom = 5;
    public float cameraZoomSpeed = 1f;
    public Transform cameraTarget;

    // Start is called before the first frame update
    void Start()
    {
        Game = GameManager.instance;
        playerCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        CameraZoom();
        PlayerFollow();

    }

    void CameraZoom()
    {
        currentZoom = Mathf.Lerp(currentZoom, cameraZoom, Time.deltaTime * cameraZoomSpeed);
        playerCamera.orthographicSize = currentZoom;

    }

    void PlayerFollow()
    {
        cameraPosition = new Vector3(cameraTarget.position.x, cameraTarget.position.y, -20);
        transform.position = Vector3.Lerp(transform.position, cameraPosition, Time.deltaTime * cameraFollowSpeed);
    }
}

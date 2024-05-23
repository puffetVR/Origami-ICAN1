using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameManager Game;
    Camera mCam;

    public bool followTarget = true;

    Vector3 cameraPosition;
    public float cameraFollowSpeed = 1f;
    float currentZoom = defaultZoom;
    [Range(minZoom, maxZoom)] public float cameraZoom = defaultZoom;
    public const float minZoom = 1.20f;
    public const float maxZoom = 10f;
    const float defaultZoom = 5.45f;
    public float cameraZoomSpeed = 1f;
    public Transform cameraTarget;
    public float xPos;
    public float yPos;

    private Vector2 camCenterToBounds, camBoundsToLevelBounds;
    public Vector2 camBoundsMin { get; private set; }
    public Vector2 camBoundsMax { get; private set; }
    public Vector2 camCenter { get; private set; }

    void Start()
    {
        Game = GameManager.instance;
        mCam = Camera.main;

        CameraInitPosZoom();
    }

    bool IsInit()
    {
        if (!Game ||
            !mCam)
        {
            Debug.LogError("Oh oh stinky!");
            return false;
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInit()) return;

    }

    private void FixedUpdate()
    {
        if (!IsInit()) return;

        CalculateBounds();
        CameraZoom();
        TargetFollow();
    }

    void CameraInitPosZoom()
    {
        CalculateBounds();

        cameraPosition = new Vector3(xPos, yPos, -20);
        transform.position = cameraPosition;

        currentZoom = cameraZoom;
    }

    void CalculateBounds()
    {
        // Get Camera Bounds & Center
        camCenter = transform.position;

        Vector2 camMinBounds = mCam.ScreenToWorldPoint(Vector2.zero);
        Vector2 camMaxBounds = mCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        camBoundsMin = new Vector2(camMinBounds.x, camMinBounds.y);
        camBoundsMax = new Vector2(camMaxBounds.x, camMaxBounds.y);
       
        Bounds levelBounds = Game.levelBounds.bounds;
        Vector2 levelBoundsMin = new Vector2(levelBounds.min.x, levelBounds.min.y);
        Vector2 levelBoundsMax = new Vector2(levelBounds.max.x, levelBounds.max.y);

        // Length between Camera Bounds & Camera Center
        camCenterToBounds = new Vector2(camBoundsMin.x - camCenter.x, camBoundsMin.y - camCenter.y) * -1;

        // good but unpractical & not compatible with zoom 
        //Vector2 h = new Vector2(Game.levelData.horizontalSize.x, Game.levelData.horizontalSize.y);
        //Vector2 v = new Vector2(Game.levelData.verticalSize.x, Game.levelData.verticalSize.y);

        // Camera Destination Pos = levelBounds + camCenterToBounds
        Vector2 h = new Vector2(levelBoundsMin.x + camCenterToBounds.x,
                                levelBoundsMax.x - camCenterToBounds.x);
        Vector2 v = new Vector2(levelBoundsMin.y + camCenterToBounds.y,
                                levelBoundsMax.y - camCenterToBounds.y);

        // We confine the camera within the level bounds if possible
        xPos = Mathf.Clamp(cameraTarget.position.x,
            h.x != 0 ? h.x : float.MinValue,
            h.y != 0 ? h.y : float.MaxValue);
        yPos = Mathf.Clamp(cameraTarget.position.y,
            v.x != 0 ? v.x : float.MinValue,
            v.y != 0 ? v.y : float.MaxValue);
    }

    void CameraZoom()
    {
        currentZoom = Mathf.Lerp(currentZoom, cameraZoom, Time.deltaTime * cameraZoomSpeed);
        mCam.orthographicSize = currentZoom;

    }

    void TargetFollow()
    {
        cameraPosition = new Vector3(xPos, yPos, -20);
        if (followTarget) transform.position = Vector3.Lerp(transform.position, cameraPosition, Time.deltaTime * cameraFollowSpeed);
    }
}

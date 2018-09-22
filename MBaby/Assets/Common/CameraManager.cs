using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraManager : MonoBehaviour
{
    public Transform player;
    public Camera mainCamera;

    // Player movement allowed before the Carmra Start moving
    public Vector2 allowPlayerOffset;

    private Vector3 playerStarting;
    private Vector3 cameraStarting;

    public Vector3 cameraTarget;
    private TilesManager tilesManager;

    public static CameraManager instance;
    private float cameraLimitUp;
    private float cameraLimitDown;
    private float cameraLimitRL;

    void Awake()
    {
        CameraManager.instance = this;
    }

    void Start()
    {
        tilesManager = TilesManager.instance;

        playerStarting = player.position;
        cameraStarting = mainCamera.transform.position;
        cameraTarget = cameraStarting;

        Vector3 upright = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
        Vector3 downleft = mainCamera.ScreenToWorldPoint(Vector3.zero);

        cameraLimitUp = tilesManager.upBound - upright.y;
        cameraLimitDown = tilesManager.lowBound - downleft.y;
        cameraLimitRL = tilesManager.leftRightBound - upright.x;
    }

    void FixedUpdate()
    {
        MoveCameraFocusPlayer();
        LimitCameraArea();

        Vector3 camV3 = mainCamera.transform.position;

        if (((Vector2)camV3 - (Vector2)cameraTarget).sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(player.position.x - camV3.x) > allowPlayerOffset.x)
                camV3.x = Mathf.Lerp(camV3.x, cameraTarget.x, 0.05f);
            if (Mathf.Abs(player.position.y - camV3.y) > allowPlayerOffset.y)
                camV3.y = Mathf.Lerp(camV3.y, cameraTarget.y, 0.05f);
            mainCamera.transform.position = camV3;
        }
    }

    public void MoveCameraFocusPlayer()
    {
        Vector3 offset = player.position - playerStarting;
            cameraTarget.x = cameraStarting.x + offset.x;        
            cameraTarget.y = cameraStarting.y + offset.y;
    }

    public void LimitCameraArea()
    {
        cameraTarget.x = Mathf.Clamp(cameraTarget.x, cameraStarting.x - cameraLimitRL, cameraStarting.x + cameraLimitRL);
        cameraTarget.y = Mathf.Clamp(cameraTarget.y, cameraStarting.y + cameraLimitDown, cameraStarting.y + cameraLimitUp);
    }

}

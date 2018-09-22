using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour {

    public UnityEvent move;
    public UnityEvent shoot;
    public UnityEvent backButton;

    public float touchSensitive = 10f;

    [Header("Show for debug only")]
    public float axisH = 0;
    public float axisV = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.touchCount > 0)

            if (Input.touches[0].rawPosition.x < Screen.width)
            {
                axisH = (Input.touches[0].position.x - Input.touches[0].rawPosition.x) / touchSensitive;
                axisV = (Input.touches[0].position.y - Input.touches[0].rawPosition.y) / touchSensitive;
                move.Invoke();
            }

        if (Input.touchCount > 1)
            shoot.Invoke();

    }

    public void onShoot()
    {
        shoot.Invoke();
    }

}

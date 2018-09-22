using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fps : MonoBehaviour {

    public float minFps = Mathf.Infinity;
    public float fps = 0;
    public float averageFps = 0;
    private int counts = 0;
    private int frameStart = 0;
    private float timeStart = 0;

    public bool reset = false;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Time.time > counts)
        {
            counts++;
            Count();
        }

        if (reset)
        {
            reset = false;
            minFps = Mathf.Infinity;
            averageFps = 0;
            frameStart = Time.frameCount;
            timeStart = Time.time;
        }
    }

    void Count()
    {
        fps = 1f / Time.deltaTime;

        if (timeStart < Time.time)
            averageFps = (float)(Time.frameCount - frameStart) / (Time.time - timeStart);

        if (fps < minFps)
            minFps = fps;

    }
}

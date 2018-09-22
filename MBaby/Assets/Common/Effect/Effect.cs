using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour {

    // Object to Apply setting
    // 0 : Apply effect to This Object Only
    // 1 : Apply effect to others
    public int applySelected = 0;
    public List<SpriteRenderer> srList;
    public List<Renderer> rList;
    public List<Transform> tList;

    private List<Color> _srList = new List<Color>();
    private List<Color> _rList = new List<Color>();
    private List<Vector3> _tList = new List<Vector3>();

    // Size setting
    // 0 : Constant Size
    // 1 : Change Size
    public int sizeSelected = 0;
    // Time setting
    // 0 : Play Once
    // 1 : PingPong
    // 2 : Repeat
    public int timeSelected = 0;
    public AnimationCurve sizeCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public float stayTime = 0f;
    public bool keepStay = false;

    // Colour setting
    // 0 : Disable 
    // 1 : Set End Alpha
    // 2 : Set End Color
    public int colorSelected = 0;
    public AnimationCurve colorCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public Color endColor;
    public float endAlpha;

    // Follow setting
    // 0 : Disable
    // 1 : Fixed offset
    // 2 : Fixed distant
    // 3 : Fixed distnat and Area
    // 4 : Rotate Around
    public int followSelected = 0;
    // 0 : Instant
    // 1 : Fixed speed
    // 2 : Ratio per second
    public int followSpeedSelected = 0;
    public bool followAble = false;
    public Transform followTo;
    public Vector2 followOffset = Vector2.zero;
    public float followDistant = 0f;
    public int followStartArea = 90;
    public int followEndArea = 270;
    public float followSpeed = 10f;
    [Range(0f, 1f)]
    public float followRatio = 1f;

    //Rotation setting
    // 0 : Disable
    // 1 : Look At target
    // 2 : Rotation by Curve
    public int rotationSelected = 0;
    // 0 : Start as Fixed Z
    // 1 : Start at Random
    public int rotationStartSelected = 0;
    public float rotationAtStart = 0;
    public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public Transform rotateTo;
    public float degPerSecond;

    //Spawn Setting
    // 0 : Disable
    // 1 : Spawn when Effect Ended
    // 2 : Spawn when Kill
    public int spawnSelected = 0;
    public Transform spawn;
    public float spawnTime = 0f;
    public bool spawnOnce = false;
    private float spawnNumber = 1;

    //--
    private float bornTime;
    private float timer;    

    private float cycle = 0;
    private bool paused = false;
    private bool hiding = false;
    private bool noRotate = false;
    private bool noFollow = false;
    private bool noSpawn = false;

    private bool killing = false;
    private float killTime = 0;

    void Awake()
    {
   
    }
    
    void Start()
    {
        StartRotation();

        bornTime = Time.time;
        timer = bornTime;
        
        if (stayTime <= 0) stayTime = 0.01f;

        SetReference();
    }

    void Update()
    {
        if (paused == false)
        {
            UpdateColor();
            if (!noSpawn) UpdateSpawn();
            if (!hiding)  UpdateSize();            
        }
        CheckKill();
    }

    void FixedUpdate()
    {
        if (paused == false)
        {
            timer += Time.fixedDeltaTime;
            if (!noFollow) UpdateFollow();
            if (!noRotate) UpdateRotation();
        }
        cycle = (timer - bornTime) / stayTime;
    }

    void SetReference()
    {
        if (applySelected > 0)
        {
            if (srList.Count > 0)
            {
                for (int j = 0; j < srList.Count; j++)
                    _srList.Add(srList[j].color);
            }
            if (rList.Count > 0)
            {
                for (int j = 0; j < rList.Count; j++)
                    _rList.Add(rList[j].material.color);
            }
            if (tList.Count > 0)
            {
                for (int j = 0; j < tList.Count; j++)
                    _tList.Add(tList[j].transform.localScale);
            }
        }
        else
        {
            if (GetComponent<SpriteRenderer>() != null)
            {
                _srList = new List<Color>();
                srList = new List<SpriteRenderer>();
                foreach (SpriteRenderer sr in GetComponents<SpriteRenderer>())
                {
                    _srList.Add(sr.color);
                    srList.Add(sr);
                }
            }
            if (GetComponent<Renderer>() != null)
            {
                _rList = new List<Color>();
                rList = new List<Renderer>();
                foreach (Renderer r in GetComponents<Renderer>())
                {
                    _rList.Add(r.material.color);
                    rList.Add(r);
                }
            }
            _tList = new List<Vector3>
            {
                transform.localScale
            };
            tList = new List<Transform>
            {
                this.transform
            };
        }
    }

    void UpdateSize()
    {
        float ratio = 1;
        if (sizeSelected == 1)
        {
            float t = sizeCurve.keys[sizeCurve.length-1].time;
            ratio = sizeCurve.Evaluate(Step() * t);
        }
        
        for (int i = 0; i < tList.Count; i++)
        {
            if (tList[i] != null) 
            tList[i].localScale = _tList[i] * ratio;
            else
            {
                tList.RemoveAt(i);
                _tList.RemoveAt(i);
            }
        }
    }

    void UpdateColor()
    {
        if(colorSelected != 0)
        {
            float t = colorCurve.keys[colorCurve.length - 1].time;
            float ratio = colorCurve.Evaluate(Step() * t);

            for (int i = 0; i < srList.Count; i++)
            {
                if (srList[i] != null)
                {
                    switch (colorSelected)
                    {
                        case 1:
                            Color c = srList[i].color;
                            c.a = endAlpha;
                            srList[i].color = Color.Lerp(_srList[i], c, ratio);
                            break;
                        case 2:
                            srList[i].color = Color.Lerp(_srList[i], endColor, ratio);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    srList.RemoveAt(i);
                    _srList.RemoveAt(i);
                }
            }

            for (int i = 0; i < rList.Count; i++)
            {
                if (rList[i] != null)
                {
                    switch (colorSelected)
                    {
                        case 1:
                            Color c = rList[i].material.color;
                            c.a = endAlpha;
                            rList[i].material.color = Color.Lerp(_rList[i], c, ratio);
                            break;
                        case 2:
                            rList[i].material.color = Color.Lerp(_rList[i], endColor, ratio);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    rList.RemoveAt(i);
                    _rList.RemoveAt(i);
                }
            }
        }
    }

    void UpdateFollow()
    {
        Vector3 v3 = Vector3.zero;

        switch (followSelected)
        {
            case 1:
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                    {
                        v3 = followTo.position + (Vector3)followOffset;
                        UpdateFollow(i,v3);
                    }
                }
                break;
            case 2:
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                    {
                        v3 = followTo.position - (followTo.position - tList[i].position).normalized * followDistant;
                        UpdateFollow(i, v3);
                    }
                }
                break;
            case 3:
                bool inArea;
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                    {
                        Vector3 dir = followTo.position - tList[i].position;
                        float angle = 270 - Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        angle += followTo.eulerAngles.z;
                        angle = angle % 360;

                        inArea = false;
                        if (((angle > followStartArea) && (angle < followEndArea)) || ((angle < followStartArea) && (angle > followEndArea)))
                            inArea = true;                        

                        if (followStartArea > followEndArea)
                            inArea = !inArea;

                        if (inArea)
                        {
                            v3 = followTo.position - (followTo.position - tList[i].position).normalized * followDistant;
                        }
                        else
                        {
                            float toStart = Mathf.Abs(angle - followStartArea);
                            if (toStart > 180f) toStart = 360f - toStart;

                            float toEnd = Mathf.Abs(angle - followEndArea);
                            if (toEnd > 180f) toEnd = 360f - toEnd;

                            float to = followStartArea - followTo.eulerAngles.z;
                            if (toStart > toEnd) to = followEndArea - followTo.eulerAngles.z;

                            v3 = new Vector3(Mathf.Sin(to * Mathf.Deg2Rad), Mathf.Cos(to * Mathf.Deg2Rad), 0f);                            
                            v3 = v3 * followDistant + followTo.position;

                        }
                        UpdateFollow(i, v3);
                    }
                }
                break;
            case 4:
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                    { 
                        v3 = followTo.position - (followTo.position - tList[i].position).normalized * followDistant;
                        v3 = v3 + Quaternion.Euler(0, 0, 90) * (followTo.position - tList[i].position);
                        UpdateFollow(i, v3);
                    }
                }
                break;
            default:
                break;
        }
    }

    void UpdateFollow(int i, Vector3 target)
    {
        switch (followSpeedSelected)
        {
            case 0:
                tList[i].position = target;
                break;
            case 1:
                if ((target - tList[i].position).sqrMagnitude > 0.01f)
                    tList[i].position = tList[i].position + (target - tList[i].position).normalized * Time.fixedDeltaTime * followSpeed;
                break;
            case 2:
                if ((target - tList[i].position).sqrMagnitude > 0.01f)
                    tList[i].position = Vector3.Lerp(tList[i].position, target, Time.fixedDeltaTime * followRatio);
                break;
            default:
                break;
        }
    }

    void StartRotation()
    {
        float zStart = 0f;
        switch (rotationStartSelected)
        {
            case 0:    // 0 : Start as Fixed Z
                zStart = rotationAtStart;
                break;
            case 1:    // 1 : Start at Random
                zStart = Random.Range(0f, 360f);
                break;
            default:
                break;
        }
        for (int i = 0; i < tList.Count; i++)
        {
            if (tList[i] != null)
            {
                Vector3 v3 = tList[i].eulerAngles;
                v3.z = zStart;
                v3 = tList[i].eulerAngles;
            }
        }
    }
    
    void UpdateRotation()
    {
        float zTarget = 0f;
        float temp = 0f;

        switch (rotationSelected)
        {
            case 0:            // 0 : Disable
                break;
            case 1:            // 1 : Look At target
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                    {
                        temp = tList[i].eulerAngles.z;
                        tList[i].transform.up = rotateTo.position - tList[i].position;
                        zTarget = tList[i].eulerAngles.z;

                        float t = (zTarget - temp);
                        if (t < -180f) t = t + 360f;
                        if (t > 180f) t = t -360f;                    

                        t = Mathf.Clamp(t, -degPerSecond * Time.fixedDeltaTime, degPerSecond * Time.fixedDeltaTime);
                        tList[i].eulerAngles = new Vector3( 0,0,temp + t );
                    }
                }        
                break;
            case 2:            // 2 : Rotation by Curve
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                    {
                        temp = tList[i].eulerAngles.z;
                        float t = 0;
                        t = rotationCurve.Evaluate(Step()) * degPerSecond * Time.fixedDeltaTime;
                        tList[i].eulerAngles = new Vector3(0,0,temp + t);
                    }
                }
                break;
            case 3:            // 3 : Same At target
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                    {
                        temp = tList[i].eulerAngles.z;
                        zTarget = rotateTo.eulerAngles.z;

                        float t = (zTarget - temp);
                        if (t < -180f) t = t + 360f;
                        if (t > 180f) t = t - 360f;

                        t = Mathf.Clamp(t, -degPerSecond * Time.fixedDeltaTime, degPerSecond * Time.fixedDeltaTime);
                        tList[i].eulerAngles = new Vector3(0, 0, temp + t);
                    }
                }
                break;
            default:
                break;
        }
    }

    void UpdateSpawn()
    {
        switch (spawnSelected)
        {
            case 0:            // 0 : Disable
                break;
            case 1:            // 1 : Spawn when Effect Ended
                if (spawnNumber + spawnTime < cycle)
                {
                    spawnOnce = true;
                    spawnNumber++;
                }
                break;
            case 2:            // 2 : Spawn when Kill
                if (killing)
                {
                    if (killTime + spawnTime < Time.time) spawnOnce= true;
                } 
                break;
            default:
                break;
        }

        if ((spawnOnce) && (tList.Count > 0) && (tList[0] != null))
        {
            spawnOnce = false;
            Instantiate(spawn.gameObject, tList[0].transform.position, tList[0].transform.rotation);
        }
    }

    void CheckKill()
    {
        if (!keepStay)
        {
            if (cycle > 1)
            {
                Kill();
            }
        }
    }

    float Step()
    {
        float a = cycle;
        if (a < 0) a = 0;

        switch (timeSelected)
        {
            case 0:
                a = Mathf.Clamp01(a);
                break;
            case 1:
                a = Mathf.PingPong(a, 1f);
                break;
            case 2:
                a = a - Mathf.FloorToInt(a);
                break;
            default:
                break;
        }
        return a;        
    }

    public void Kill()
    {
        killing = true;
        Hide(true);
        NoFollow(true);
        NoRotate(true);
        killTime = Time.time;
        if (spawnSelected == 2) Kill(spawnTime+0.1f);
        else Kill(0);
    }
    public void Kill(float delay)
    {
        foreach (Transform t in tList)
        {
            if (t != null)
            {
                foreach (Transform child in t)
                {
                    Destroy(child.gameObject, delay);
                }
                Destroy(t.gameObject, delay);
            }
        }
    }

    public void Pause() { Pause(!paused);  }
    public void Pause(bool b) { paused = b; }

    public void Hide() { Hide(!hiding); }
    public void Hide(bool b)
    {
        if (b)
        {
            hiding = true;
            if (tList.Count > 0)
                for (int i = 0; i < tList.Count; i++)
                {
                    if (tList[i] != null)
                        tList[i].localScale = Vector3.zero;
                }
        }
        else hiding = false;
    }

    public void NoFollow() { NoFollow(!noFollow); }
    public void NoFollow(bool b) { noFollow = b; }

    public void NoRotate() { NoRotate(!noRotate); }
    public void NoRotate(bool b) { noRotate = b; }

    public void NoSpawn() { NoSpawn(!noSpawn); }
    public void NoSpawn(bool b) { noSpawn = b; }



}

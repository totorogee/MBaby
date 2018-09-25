using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BrushType { square, circle }

public class DynamicTexture : MonoBehaviour {

    private int h = 200;
    private int w = 200;
    private int pixToWould = 100;

    public Transform result;

    public Texture2D inputT;

    public BrushType brushType = BrushType.square;
    //public Texture2D circleBrush; // ?
    private Color[] circleColors;

    public int brushArea = 5;
    public int splashMultipler = 2;
    public float _strength = 0.1f;
    private float strength = 0.1f;
    private Color[] inColors;
    private Vector2 pixPt;

    //private Color[] inColorsP; // only record path
    // public Texture2D inputTP;

    public bool debugClear = false;

    public bool testTouchEffect = true;
    private bool mouseDown = false;
    private float mouseDownTime = 0f;

    public Material dt2d;
    public Vector2 offset;
    private Vector2 offsetDelayed;
    public float tween;
    public Button reset;

    void Start () {
        h = inputT.height;
        w = inputT.width;
        inColors = inputT.GetPixels();
        // inColorsP = inputTP.GetPixels();
        if (splashMultipler <= 0) splashMultipler = 1;

        strength = _strength / (float)splashMultipler;

        if (brushType == BrushType.circle)
        {
            SetCircleBrush();
        }

        tween = dt2d.GetFloat("_Tween");
        offset = dt2d.GetTextureOffset("_WaveTex");
        offsetDelayed = dt2d.GetTextureOffset("_WaveTex");

    }

	void Update () {

        float leftMouse = Input.GetAxisRaw("Fire1");

        if (leftMouse > 0)
        {
            Vector2 v2S = Input.mousePosition;
            Vector2 v2W = Camera.main.ScreenToWorldPoint(v2S);
            pixPt = (v2W - (Vector2)result.transform.position);
            pixPt = new Vector2(pixPt.x * pixToWould + w / 2, pixPt.y * pixToWould + h / 2);
            strength = _strength / (float)splashMultipler;
        }

        if (leftMouse > 0)
        {
            if (pixPt.x >0 && pixPt.x<w && pixPt.y > 0 && pixPt.y <h)
            {
                if (brushType == BrushType.square)
                    Draw(new Vector2Int(Mathf.FloorToInt(pixPt.x), Mathf.FloorToInt(pixPt.y)), inputT, brushArea, splashMultipler, strength);
                else
                    DrawCircle(new Vector2Int(Mathf.FloorToInt(pixPt.x), Mathf.FloorToInt(pixPt.y)), inputT);
            }
        }

        if (testTouchEffect)
        {
            if (leftMouse == 0)
            {
                if (mouseDown && (pixPt.x > 0 && pixPt.x < w && pixPt.y > 0 && pixPt.y < h))
                    TouchEffect(new Vector2Int(Mathf.FloorToInt(pixPt.x), Mathf.FloorToInt(pixPt.y)), Time.time - mouseDownTime);
                mouseDown = false;
            }

            if (leftMouse > 0 && !mouseDown)
            {
                if (pixPt.x > 0 && pixPt.x < w && pixPt.y > 0 && pixPt.y < h)
                {
                    mouseDown = true;
                    mouseDownTime = Time.time;
                    offset = offsetDelayed;
                }
            }
        }

        if (debugClear)
        {
            debugClear = false;
            ClearResult();
        }

        if (tween < 0.001) // hard
        {
            tween = 0;
        }
        else
        {
            tween = Mathf.Lerp(tween, 0, 5f * Time.deltaTime);
        }

        offset = Vector2.Lerp(offset, offsetDelayed, 10f * Time.deltaTime);

        dt2d.SetFloat("_Tween", tween);
        dt2d.SetTextureOffset("_WaveTex", offset);
    }

    private void Draw(Vector2Int pos, Texture2D t2d, int area, int splash, float strength)
    {
        // inColorsP[pos.y * w + pos.x].b -= 1;

        if (splash <= 0) splash = 1;

        for (int s = 1; s <= splash; s++)
        {
            int yMin = Mathf.Max(0, pos.y - area * s);
            int yMax = Mathf.Min(h, pos.y + area * s);
            int xMin = Mathf.Max(0, pos.x - area * s);
            int xMax = Mathf.Min(w, pos.x + area * s);

            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    if (Mathf.Abs(pos.x - x) + Mathf.Abs(pos.y - y) < area * s)
                    {
                        inColors[ y*w + x ].b -= strength;
                    }
                }
            }
        }

        inputT.SetPixels(inColors);
        inputT.Apply();
        //inputTP.SetPixels(inColorsP);
        //inputTP.Apply();
    }

    private void DrawCircle(Vector2Int pos, Texture2D t2d)
    {

        int mid = brushArea * splashMultipler;
        int max = brushArea * splashMultipler *2;

        int yMin = Mathf.Max(0, pos.y - mid);
        int yMax = Mathf.Min(h, pos.y + mid);
        int xMin = Mathf.Max(0, pos.x - mid);
        int xMax = Mathf.Min(w, pos.x + mid);

        for (int y = yMin; y < yMax; y++)
        {
            for (int x = xMin; x < xMax; x++)
            {
                inColors[y* w + x].b -= 1f-circleColors[(y+mid-pos.y) * max + (x+mid-pos.x) ].b;
            }
        }

        TouchEffect(pos, 0);

        inputT.SetPixels(inColors);
        inputT.Apply();
    }

    private void TouchEffect(Vector2Int pos, float time)
    {
        offsetDelayed = new Vector2(0.5f - (float)pos.x / w , 0.5f - (float)pos.y / h );
        if (time == 0)
        {
            tween = 0.25f; // hard
        }
        else  tween = 0.4f; // hard

    }

    public void ClearResult()
    {
        for (int i = 0; i < inColors.Length; i++)
        {
            inColors[i] = Color.white;
        }
        inputT.SetPixels(inColors);
        inputT.Apply();
    }

    void SetCircleBrush()
    {
        circleColors = new Color[brushArea * splashMultipler * brushArea * splashMultipler *4];
        int mid = brushArea * splashMultipler;
        int max = brushArea * splashMultipler *2;

        for (int i = 0; i < circleColors.Length; i++)
        {
            circleColors[i] = Color.white;
        }

        for (int s = 1; s <= splashMultipler; s++)
        {
            int iMin =  mid - brushArea * s;
            int iMax =  mid + brushArea * s;
            int areaSq = brushArea * s * brushArea * s;

            for (int y = iMin; y < iMax; y++)
            {
                for (int x = iMin; x < iMax; x++)
                {
                    if ((mid - x)* (mid - x) + (mid - y)* (mid - y) < areaSq)
                    {
                        circleColors[y * max + x].b -= strength;
                    }
                }
            }
        }
    }



}

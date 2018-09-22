using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum WaveChangeType { On, Off, Switch, Bomb }

public class TilesManager : MonoBehaviour
{
    public static TilesManager instance;

    [Header("Must set at editor")]
    public Transform theTilesPrefabs;
    public AnimationCurve waveCurve;

    [Header("General Setting")]
    public float leftRightBound = 8f;
    public float upBound = 8f;
    public float lowBound = -8f;
    [HideInInspector]
    public int maxRange;
    public float tileSize = 1;
    [Range(0f, 0.5f)]
    public float _tileDistant = 0.12f;
    private float tileDistant = 0f;

    private Vector3 startScale;
    private Color spriteColor;
    private int currentTileId = 0;

    private Tiles[,] tilesList;
    private List<Tiles> tilesListV2 = new List<Tiles>();
    private int currentTileEvent = 1;

    [Header("Transparent Edge")]
    public bool _transparentEdge = true;
    private bool transparentEdge = true;
    public int transparentEdgeWide = 3;

    [Header("Wave frequency")]
    public float waveSpeedX = 1f;

    [Header("Wave amplitude")]
    public float _waveAmp = 1f;
    private float waveAmp = 0.2f;

    [Header("Wave traveling speed")]
    public float waveSpeedY = 30f;

    private float timer;

    [HideInInspector]
    public Transform debugUnit;
    [HideInInspector]
    public bool debugAbleWave = false;
    [HideInInspector]
    public bool debugDisableWave = false;
    [HideInInspector]
    public bool debugBombWave = false;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one TileManagr in scene!");
            return;
        }
        instance = this;
    }

    void Start()
    {
        maxRange = (int)upBound+1;
        tileDistant = _tileDistant + 0.75f;
        transparentEdge = _transparentEdge;
        waveAmp = _waveAmp / 5f;

        timer = Time.time;

        float temp = tileSize / theTilesPrefabs.GetComponent<Renderer>().bounds.size.x;
        theTilesPrefabs.localScale = theTilesPrefabs.localScale * temp;

        tileDistant = tileDistant * tileSize;

        startScale = theTilesPrefabs.localScale;
        spriteColor = theTilesPrefabs.GetComponent<SpriteRenderer>().color;
        tilesList = new Tiles[maxRange * 2 +1, maxRange * 2 +1];

        for (int i = -maxRange; i <= maxRange; i++)
        {
            for (int j = -maxRange; j <= maxRange; j++)
            {
                int _i = i + maxRange;
                int _j = j + maxRange;

                Vector3 offset = Vector3.zero;
                offset.x = (i) * tileDistant;
                offset.y = (j) * tileDistant * 1.155f;
                offset.z = 0.1f;
                Tiles tempTiles;
                if (i % 2 == 0)
                {
                    offset.y += 0.5f * tileDistant * 1.155f;
                }
                offset += transform.position;
                if ((offset.x < leftRightBound) && (offset.x > -leftRightBound) && (offset.y < upBound) && (offset.y > lowBound))
                {
                    Transform _tile = Instantiate(theTilesPrefabs, offset, theTilesPrefabs.transform.rotation);

                    _tile.transform.parent = gameObject.transform;
                    tempTiles = _tile.GetComponent<Tiles>();
                    tempTiles.tileID = currentTileId;
                    tempTiles.myPos.x = _i;
                    tempTiles.myPos.y = _j;
                    currentTileId++;

                    tilesList[_i, _j] = tempTiles;
                    tilesListV2.Add(tempTiles);
                }
            }
        }
        if (debugUnit == null)
        {
            debugUnit = NearestTile(Vector3.zero).transform;
        }
    }

    void FixedUpdate()
    {
        if (Time.time > timer + 0.2f)
        {
            if (transparentEdge == true)
            {
                StartEdgeTrans(transparentEdgeWide, 0.2f);
                transparentEdge = false;
            }
        }
    }

    public void StartWave(int waveArea, Vector3 _position, WaveChangeType _type)
    {
        bool inRange;
        Tiles tile = NearestTile(_position, out inRange);
        if (inRange)
        {
            if (_type == WaveChangeType.Bomb)
                StartCoroutine(BombWave(tile, waveArea, waveArea, currentTileEvent));
            else
                StartCoroutine(AbleWave(tile, waveArea, waveArea, currentTileEvent, _type));
        }

        currentTileEvent++;
    }

    public void StartEdgeTrans(int att, float min)
    {
        for (int i = 0; i < tilesListV2.Count; i++)
        {

            if (tilesListV2[i] != null)
                if (tilesListV2[i].isEdge == true)
                {
                    StartCoroutine(EdgeTrans(tilesListV2[i], att, att, min, currentTileEvent));
                }
        }

        currentTileEvent++;
    }

    public bool InRange(Vector2Int pos)
    {
        Vector2Int temp;
        return InRange(pos, out temp);
    }

    public bool InRange(Vector2Int pos, out Vector2Int nearest)
    {
        bool temp = true;
        nearest = pos;

        if ((pos.x < 0) || (pos.y < 0) || (pos.x >= maxRange * 2 +1) || (pos.y >= maxRange * 2))
        {
            temp = false;
            if (pos.x < 0) { nearest.x = 0; }
            if (pos.y < 0) { nearest.y = 0; }
            if (pos.x >= maxRange * 2) { nearest.x = (maxRange * 2) +1; }
            if (pos.y >= maxRange * 2) { nearest.y = (maxRange * 2) ; }
        }
        return temp;
    }

    public Tiles NearestTile(Vector3 _position)
    {
        bool temp;
        return NearestTile(_position, out temp);
    }

    public Tiles NearestTile(Vector3 _position, out bool inRange)
    {
        Vector2 tempPos = Vector2.zero;
        inRange = true;
        tempPos = _position - instance.transform.position;

        int _x = Mathf.RoundToInt(tempPos.x / tileDistant);
        _x += maxRange;
        float offset = 0;
        if (_x % 2 == 0)
            offset = -0.5f;

        int _y = Mathf.RoundToInt((tempPos.y + offset) / (tileDistant * 1.155f));
        _y += maxRange;

        Vector2Int pos = new Vector2Int
        {
            x = Mathf.Clamp(_x, 0, tilesList.GetLength(0)-1),
            y = Mathf.Clamp(_y, 0, tilesList.GetLength(1)-1)
        };

        inRange = InRange(pos, out pos);        

        return tilesList[pos.x, pos.y];
    }

    public Tiles[] NextSixTiles(Tiles tile)
    {
        bool temp;
        return NextSixTiles(tile, out temp);
    }

    public Tiles[] NextSixTiles(Tiles tile, out bool isEgde)
    {
        isEgde = false;
        Tiles[] temp = new Tiles[6];
        for (int i = 0; i < temp.Length; i++)
            temp[i] = null;

        int offset = 0;
        if ((tile.myPos.x - maxRange) % 2 == 0)
            offset = 1;

        if (InRange(new Vector2Int(tile.myPos.x, tile.myPos.y + 1)))
            temp[0] = tilesList[tile.myPos.x, tile.myPos.y + 1];
        if (InRange(new Vector2Int(tile.myPos.x + 1, tile.myPos.y + offset)))
            temp[1] = tilesList[tile.myPos.x + 1, tile.myPos.y + offset];
        if (InRange(new Vector2Int(tile.myPos.x + 1, tile.myPos.y - 1 + offset)))
            temp[2] = tilesList[tile.myPos.x + 1, tile.myPos.y - 1 + offset];
        if (InRange(new Vector2Int(tile.myPos.x, tile.myPos.y - 1)))
            temp[3] = tilesList[tile.myPos.x, tile.myPos.y - 1];
        if (InRange(new Vector2Int(tile.myPos.x - 1, tile.myPos.y + offset)))
            temp[4] = tilesList[tile.myPos.x - 1, tile.myPos.y + offset];
        if (InRange(new Vector2Int(tile.myPos.x - 1, tile.myPos.y - 1 + offset)))
            temp[5] = tilesList[tile.myPos.x - 1, tile.myPos.y - 1 + offset];

        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i] == null)
                isEgde = true;
        }

        return temp;
    }

    IEnumerator AbleWave(Tiles tile, int waveArea, int currentPosition, int Id, WaveChangeType _type)
    {
        if ((Id != tile.lastActionId) && currentPosition > 0)
        {
            tile.lastActionId = Id;
            if (_type == WaveChangeType.On)
                tile.rdr.enabled = true;
            if (_type == WaveChangeType.Off)
                tile.rdr.enabled = false;
            if (_type == WaveChangeType.Switch)
                tile.rdr.enabled = !tile.rdr.enabled;

            yield return new WaitForSeconds(1 / waveSpeedY);
            for (int i = 0; i < 6; i++)
            {
                if (tile.nextSixTiles[i] != null)
                    if (tile.nextSixTiles[i].lastActionId < Id)
                    {
                        StartCoroutine(AbleWave(tile.nextSixTiles[i], waveArea, currentPosition - 1, Id, _type));
                    }
            }
        }
    }

    IEnumerator EdgeTrans(Tiles tile, int waveArea, int currentPosition, float min, int Id)
    {
        if (tile != null)
        {
            if ((Id != tile.lastActionId) && currentPosition > 0)
            {
                tile.lastActionId = Id;
                float n = min + ((waveArea - currentPosition) / (float)waveArea) * (1 - min);
                tile.srdr.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, spriteColor.a * n);

                yield return new WaitForSeconds(0.5f);
                for (int i = 0; i < 6; i++)
                {
                    if (tile.nextSixTiles[i] != null)
                        if (tile.nextSixTiles[i].lastActionId < Id)
                        {
                            StartCoroutine(EdgeTrans(tile.nextSixTiles[i], waveArea, currentPosition - 1, min, Id));
                        }
                }
            }
        }
    }

    IEnumerator BombWave(Tiles tile, int waveArea, int currentPosition, int Id)
    {
        if (tile != null)
        {
            if ((Id != tile.lastActionId) && currentPosition > 0)
            {
                tile.lastActionId = Id;
                StartCoroutine(ApplyBombWave(tile, waveArea, currentPosition));

                yield return new WaitForSeconds(1 / waveSpeedY);
                for (int i = 0; i < 6; i++)
                {
                    if (tile.nextSixTiles[i] != null)
                        if (tile.nextSixTiles[i].lastActionId < Id)
                        {
                            StartCoroutine(BombWave(tile.nextSixTiles[i], waveArea, currentPosition - 1, Id));
                        }
                }
            }
        }
    }

    IEnumerator ApplyBombWave(Tiles tile, int waveArea, int currentPosition)
    {
        if (tile != null)
        {
            float timer = Time.time;
            float duration = waveSpeedX;

            while (Time.time < timer + duration)
            {
                float i = waveCurve.Evaluate((Time.time - timer) / duration) * waveAmp * currentPosition / waveArea + 1f;

                tile.transform.localScale = startScale * i;
                yield return 0;
            }

            tile.transform.localScale = startScale;
        }
    }
}
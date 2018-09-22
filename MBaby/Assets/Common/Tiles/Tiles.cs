using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    public int tileID = -1;
    public int lastActionId = 0;
    public Tiles[] nextSixTiles = new Tiles[6];
    public bool isEdge = false;
    public Vector2Int myPos = new Vector2Int();

    [HideInInspector]
    public SpriteRenderer srdr;
    [HideInInspector]
    public Renderer rdr;
    private TilesManager tm;

    void Awake()
    {
        rdr = GetComponent<Renderer>();
        srdr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        tm = TilesManager.instance;
        nextSixTiles = tm.NextSixTiles(this, out isEdge); 
    }

    public void OffThis()
    {
        GetComponent<Renderer>().enabled = false;
    }
}

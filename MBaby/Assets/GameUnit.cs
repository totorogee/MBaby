using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shooter;


public class GameUnit : MonoBehaviour
{
    public GamePlay.Body body;
    public GamePlay.Move movement;
    public List<GamePlay.Spawnable> spawns;
    [HideInInspector]
    public Rigidbody2D rb2D;

   // private Renderer myRdr;   

    void Awake()
    {
        if (GetComponent<Rigidbody2D>() != null)
            rb2D = gameObject.GetComponent<Rigidbody2D>();
        else
            rb2D = gameObject.AddComponent<Rigidbody2D>();

      //  if (GetComponent<Renderer>() != null)
      //      myRdr = gameObject.GetComponent<Renderer>();
    }

    // Use this for initialization
    void Start()
    {


        body.SetBody(transform);
        movement.SetMove(transform);

        if (spawns.Count > 0)
            foreach (GamePlay.Spawnable spawn in spawns)
            {

                spawn.StartSpawnable(transform);
            }
    }

    // Update is called once per frame
    void Update()
    {
        if ((spawns.Count > 0) && (movement.itMove))
        {
            Vector3 vectorToTarget = spawns[0].target.position - transform.position; // target = [0] [TEMP]
            float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - 90;

            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            //transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * movement.rotateSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 5f);
        }

        if (spawns.Count > 0)
            foreach (GamePlay.Spawnable spawn in spawns)
            {
                spawn.Shoot(transform);
                spawn.CheckIfTriggerByHp(body.hpCurrent / body.hpMax);
            }

        if (body.deadTime > 0)
            if (Time.time > body.bornTime + body.deadTime)
            {
                body.Killing(UnitDeathBy.TimeOver);
            }
    }

    void FixedUpdate()
    {
        if (movement.itMove)
        {            
            movement.MoveToNextStep(transform);
            movement.MoveStep();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        body.Hit(col);
    }
}

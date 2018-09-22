using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shooter
{
    public enum ObjectType { Wall, Player, Enemy, PlayerBomb, EnemyBomb, PlayerBullet, EnemyBullet, Melee, Other }
    public enum HitLevel { Wall, Player, Enemy, Bomb, Bullet, Melee }
    /* hitLevel : ( How unit interact when collise )
     * Player(1) < Enemy(2) < Bomb(3) < Bullet(4) < Melee(5) ; Wall(0) : Wall have no interaction 
     * When Higher level hit lower one : Higher consumed, Lower one due Damage     */

    public enum UnitDeathBy { Hit, TimeOver, HpZero, Explosion, Catch }
    public enum SpawnType { Burst, BurstTwins, Once, Refill}
    
    public enum SpawnTrigger { Start, HpRemain }

    public enum MoveMode { Simple, SinWave, Accelerated, KeepDistant }
    public enum MoveTrigger { Start, Hit, Call, HpRemain }

    /* moveType : 
     * Active   - ignore spawner and have own moving pattern
     * Passive  - a start speed set by spawner */

    public class GamePlay : MonoBehaviour
    {
        [System.Serializable]
        public class Body
        {
            [System.Serializable]
            public class Explosive
            {
                public LayerMask expOn;
                public int expDamage;
                public int expForce;
                public float expArea;
                public bool expOnHit;
                public bool expOnTimer;
                public bool expOnShotDead;
                public Transform effectOnExp;
                public bool expHaveTileEffect = false;
                public int effectRadis = 5;
            }

            [Header("General Setting")]
            public string unitName;
            public ObjectType type;
            public Color primaryColor;
            public Renderer primaryColorApplyTo;
            public Color secondaryColor;
            public Renderer secondaryColorApplyTo;


            [Header("HP / Damage Setting")]
            public int hpMax;
            public int hpCurrent;
            public int hitDamage;

            public LayerMask hitOn;
            public HitLevel role;
            public Transform effectOnHit;
            public Transform effectOnShotDead;
            public Transform effectOnTimeOver;

            [Header("Explosion setting")]
            public bool canExplose = false;
            public Explosive expSetting = new Explosive();
            private float visibleAfterDead = 0.5f;

            [Header(" - Max time on screen ; 0 for not destroy")]
            public float deadTime;

            [HideInInspector]
            public float bornTime;
            [HideInInspector]
            public Transform phyCollider;
            [HideInInspector]
            public Transform myTransform;
            [HideInInspector]
            public bool dead = false;


            public void Hit(Body other)
            {
                if (((int)role == 0) || ((int)other.role == 0))
                {
                    Debug.Log("Error : Wall object trying to interact");
                }
                else if (role < other.role)
                {
                    Debug.Log("Error : Level setting error");
                }
                // Case 1 - Both Subject and Other count as Hit and consumed
                else if (role == other.role)
                {
                    Killing(UnitDeathBy.Hit);
                    other.Killing(UnitDeathBy.Hit);
                }
                // Case 2 - Subject hit Other : Subject is consumed and Other Due Damage
                else
                {
                    Killing(UnitDeathBy.Hit);
                    other.DueDamage(hitDamage, false);
                }
            }

            public void Exploded()
            {
                Collider2D[] cols = Physics2D.OverlapCircleAll(myTransform.position, expSetting.expArea);

                if (cols.Length > 0)
                {
                    foreach (Collider2D col in cols)
                    {
                        if (col.isTrigger && expSetting.expOn == (expSetting.expOn | 1 << LayerMask.NameToLayer(col.gameObject.tag.ToString())))
                        {
                            if (col.transform.GetComponent<Rigidbody2D>() != null)
                            {
                                Rigidbody2D rb2d = col.GetComponent<Rigidbody2D>();
                                Vector2 force = col.transform.position - myTransform.position;
                                float distant = force.sqrMagnitude;
                                force = force.normalized * (expSetting.expArea - distant / expSetting.expArea);
                                rb2d.AddForce(force * expSetting.expForce);
                                rb2d.AddTorque(Random.Range(0f, 1f) * expSetting.expForce);

                                Body other = null;
                                if (col.gameObject.GetComponent<GameUnit>())
                                {
                                    other = col.gameObject.GetComponent<GameUnit>().body;
                                }
                            //    else if (col.gameObject.GetComponent<Player>())
                             //   {
                             //       other = col.gameObject.GetComponent<Player>().body;
                             //   }
                             //   other.DueDamage(expSetting.expDamage, true);
                            }
                        }
                    }
                }
            }

            public void Killing(UnitDeathBy Reason)
            {
                if ((!dead) && ( role != HitLevel.Player))
                {

                    dead = true;
                    bool willExp = false;
                    Transform effect = effectOnHit;
                    float stay = 0.2f; // hardcode

                    switch (Reason)
                    {
                        case UnitDeathBy.Hit:
                            if (canExplose)
                                willExp = expSetting.expOnHit;
                            effect = effectOnHit;
                            break;

                        case UnitDeathBy.HpZero:
                            if (canExplose)
                                willExp = expSetting.expOnShotDead;
                            effect = effectOnShotDead;
                            break;

                        case UnitDeathBy.TimeOver:
                            if (canExplose)
                                willExp = expSetting.expOnTimer;
                            effect = effectOnTimeOver;
                            break;

                        case UnitDeathBy.Explosion:
                            if (canExplose)
                            {
                                willExp = (expSetting.expOnHit || expSetting.expOnShotDead || expSetting.expOnTimer);
                                stay = visibleAfterDead;
                            }
                            effect = effectOnShotDead;
                            break;

                        case UnitDeathBy.Catch:
                            break;
                    }

                    if (willExp)
                    {
                        Instantiate(expSetting.effectOnExp, myTransform.position, Quaternion.identity);
                        if (expSetting.expHaveTileEffect)
                        TilesManager.instance.StartWave(expSetting.effectRadis, myTransform.position, WaveChangeType.Bomb);
                        Exploded();
                    }
                    Instantiate(effect, myTransform.position, Quaternion.identity);

                    if (myTransform.GetComponent<Collider2D>() != null)
                        myTransform.GetComponent<Collider2D>().enabled = false;

                    foreach (Transform child in myTransform)
                    {
                        Destroy(child.gameObject);
                        if (myTransform.GetComponent<GameUnit>().movement.itMove)
                        Destroy(myTransform.GetComponent<GameUnit>().movement.nextStep.gameObject);
                    }
                    Destroy(myTransform.gameObject, stay);

                    hpCurrent = 0;
                }
            }

            public void Hit(Collider2D col)
            {
                if (col.tag != "Melee")
                {
                    Body body = null;
                    if (col.gameObject.GetComponent<GameUnit>())
                    {
                        body = col.gameObject.GetComponent<GameUnit>().body;
                    }
                 //   else if (col.gameObject.GetComponent<Player>())
                 //   {
                 //       body = col.gameObject.GetComponent<Player>().body;
                 //   }

                    if (hitOn == (hitOn | (1 << body.phyCollider.gameObject.layer)))
                    {
                        Hit(body);
                    }
                }

            }

            public void DueDamage(int damage, bool explosion)
            {
                hpCurrent -= damage;
                if ((hpCurrent <= 0) && (explosion == true))
                    Killing(UnitDeathBy.Explosion);
                if ((hpCurrent <= 0) && (explosion == false))
                    Killing(UnitDeathBy.HpZero);
            }

            private void SetLayerAndTag()
            {
                Transform from = myTransform;
                from.gameObject.layer = LayerMask.NameToLayer("GameLayer");
                from.gameObject.tag = type.ToString();
                bool setPhyColAlready = false;

                Collider2D[] cols;
                cols = from.GetComponents<Collider2D>();
                foreach (Collider2D col in cols)
                {
                    col.isTrigger = true;
                }

                foreach (Transform child in from)
                {
                    if (child.gameObject.name == "PhyCollider")
                    {
                        child.gameObject.layer = LayerMask.NameToLayer(type.ToString());
                        phyCollider = child;
                        setPhyColAlready = true;
                    }
                }

                if ((!setPhyColAlready) && (from.GetComponent<Collider2D>() != null))
                {
                    Debug.Log("Setting Auto PhyCollider for " + from);
                    // Auto set Collider : should avoid as cannot copy value ( collider type is unknown ) //
                    // TO DO : Should disable Auto set Collider after debug
                    GameObject go = new GameObject
                    {
                        layer = LayerMask.NameToLayer(type.ToString())
                    };
                    go.transform.SetParent(from);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                    go.gameObject.name = "PhyCollider";

                    foreach (Collider2D col in cols)
                    {
                        System.Type type = col.GetType();
                        go.AddComponent(type);
                    }
                }
            }

            public void SetBody(Transform from)
            {
                if (hpMax <= 0)
                {
                    hpMax = 1;
                }

                myTransform = from;
                hpCurrent = hpMax;
                SetLayerAndTag();
                bornTime = Time.time;

                if (primaryColorApplyTo == null)
                {
                    if (from.GetComponent<Renderer>() != null)
                        from.GetComponent<Renderer>().material.color = primaryColor;
                }
                else
                {
                    primaryColorApplyTo.material.color = primaryColor;
                }

                if (secondaryColorApplyTo != null)
                    secondaryColorApplyTo.material.color = secondaryColor;

            }
        }

        [System.Serializable]
        public class Spawnable
        {
            [Header("Spawnable Setting")]

            public Transform theObject;
            // [HideInInspector]
            public Transform theObjectClone;
            public SpawnType type;
            public SpawnTrigger trigger;
            [Range(0, 1)]
            public float triggeringHp;
            private bool triggered = false;         // for "hp remain trigger
            private bool ended = false;             // in "once mode" and shoot once

            public float burstCoolTime;
            public int noOfRoundsInABurst;
            public float bulletCoolTime;
            public Vector2 startSpeed;
            public Vector2 offset;

            [HideInInspector]
            public Transform target;

            private float nextShootTime;
            private int currentBurstNo;
            private bool bulletReady = false;       // for bullet cooltime and brust cooltime

            public void Load()
            {
                if (type == SpawnType.Refill)
                {
                    if ((theObject == null || theObject.gameObject.activeSelf == false) && (bulletReady == false))
                    {
                        nextShootTime = Time.time + burstCoolTime;
                        bulletReady = true;
                    }
                }
                else if ((Time.time > nextShootTime) && (bulletReady == false))
                {
                    if (currentBurstNo > 1)
                    {
                        nextShootTime = Time.time + bulletCoolTime;
                        currentBurstNo--;
                        bulletReady = true;
                    }
                    else if (currentBurstNo == 1)
                    {
                        nextShootTime = Time.time + burstCoolTime;
                        currentBurstNo = noOfRoundsInABurst;
                        bulletReady = true;
                    }
                    else
                    {
                        Debug.Log("0 bullet ");
                    }
                }
            }

            public void Shoot(Transform shootFrom, Vector2 offset, Quaternion shootTo)
            {
                Load(); // check if bullet ready amd countdown if its not

                if ((triggered) && (!ended) && (bulletReady))
                {
                    if (type == SpawnType.Refill)
                    {
                        if ((bulletReady == true) && (Time.time > nextShootTime))
                        {
                            bulletReady = false;
                            theObject = Instantiate(theObjectClone, shootFrom.position, Quaternion.identity);
                            theObject.gameObject.SetActive(true);
                        }
                    }
                    else if (theObject != null)
                    {
                        bulletReady = false;

                        if (type == SpawnType.Once)
                        {
                            ended = true;
                        }

                        List<int> x = new List<int> { 1 };
                        if (type == SpawnType.BurstTwins)
                        {
                            x.Add(-1);
                        }

                        foreach (int _x in x)
                        {
                            Vector3 V3 = new Vector3(offset.x * _x, offset.y, 0f);
                            V3 = shootFrom.position + shootTo * V3;
                            GameObject go = Instantiate(theObject.gameObject, V3, shootTo);

                            go.SetActive(true);
                            Vector3 V3b = shootTo * startSpeed;
                            if (go.GetComponent<Rigidbody2D>() != null)
                            {
                                go.GetComponent<Rigidbody2D>().velocity = V3b;
                            }
                        }
                    }
                }
            }

            public void Shoot(Transform shootFrom)
            {
                Quaternion shootTo = shootFrom.transform.rotation;
                Shoot(shootFrom, offset, shootTo);
            }

            public void CheckIfTriggerByHp(float hp)
            {
                if (hp <= triggeringHp)
                {
                    triggered = true;
                }
            }

            public void StartSpawnable(Transform shootFrom)
            {
                nextShootTime = 0;

                if (trigger == SpawnTrigger.Start)
                    triggered = true;

                if (type == SpawnType.Once)
                {
                    currentBurstNo = 1;
                    noOfRoundsInABurst = 1;
                }

                if ((type == SpawnType.Burst) || (type == SpawnType.BurstTwins))
                {
                    currentBurstNo = noOfRoundsInABurst;
                }

                if (type == SpawnType.Refill)
                {
                    currentBurstNo = 1;
                    noOfRoundsInABurst = 1;
                    theObjectClone = Instantiate(theObject, shootFrom.position, Quaternion.identity);
                    theObjectClone.gameObject.SetActive(false);
                }
            }
        }

        [System.Serializable]
        public class Move
        {

            [System.Serializable]
            public class Order
            {
                public bool pulsed;
                public MoveMode moveMode;
                public MoveTrigger moveTrigger;

                // moveTrigger : move/hit

                // moveTrigger : call
                public bool called;

                // moveTrigger : remainHP
                public float triggeringHP;

                // moveMode : sine
                public float speed_y;
                public float speed_x;
                public float amplitude;

                //moveMode : simple
                public Vector2 direction;
                public float speed;

                //moveMode : acc
                public Vector2 _direction;
                public float acc;
                public float maxspeed;

                //moveMode : keepdistace
                public float distance;
                public float myDirection;
                public void ReportDirection(Transform transform, Move move )
                {

                }

                public Transform target;
                public float rotateSpeed; // (recover)
            }

            [HideInInspector]
            public bool moveAble;

            [Header("Movment Setting")]
            public bool itMove = false;


            public Vector2 startingSpeed;

            public List<Order> orders;
            public Transform nextStep; // next position it move to
            public float speed = 1; // general spped
            public float stepSpeed = 1; // speed to last position if hit by force ( recover )
            public Transform target; // 
            public float stunRecovery; // time
            public float pulseTime; // time between each pulse
            public AnimationCurve pulseEffect; // speed penalty of pulse

            private Rigidbody2D rb2d;

            //private bool stuned = false;
            //private bool recovered = true;
            //private int currectOrder = 0;
            //private float pulseTimer = 0;

            public Vector3 debugOffset;

            public float moveStepInterval = 3f;  // time between undate steps
            private float nextStepTime = 0;


            public void SetMove(Transform from)
            {
                moveAble = true;

                if (from.GetComponent<Rigidbody2D>() != null)
                {
                    rb2d = from.gameObject.GetComponent<Rigidbody2D>();
                }
                else
                {
                    Debug.Log("Error : Unit should have Rigidbody");
                    rb2d = from.gameObject.AddComponent<Rigidbody2D>();
                }

                if (itMove)
                {
                    rb2d.velocity = startingSpeed;
                }

                if (rb2d.mass <= 1)     rb2d.mass = 1;
                rb2d.gravityScale = 0f;
                rb2d.angularDrag = 0f;

                if (itMove)
                {
                    nextStep.position = from.position;
                    nextStep.rotation = from.rotation;
                    rb2d.angularDrag = stunRecovery;
                    
                }
            }

            public void MoveToNextStep(Transform from)
            {
                if ((from!= null) && (nextStep !=null))
                {
                    Vector3 tempv3 = nextStep.position - from.position;
                    if (tempv3.sqrMagnitude > 0.1f)
                    {
                        tempv3 = tempv3.normalized * stepSpeed;
                        rb2d.AddForce(tempv3);
                    }
                    else
                    {
                        rb2d.velocity = rb2d.velocity * 0.9f;
                    }

                    tempv3 = rb2d.velocity;
                    if (tempv3.sqrMagnitude > stepSpeed * stepSpeed)
                        tempv3 = tempv3.normalized * stepSpeed;

                    rb2d.velocity = tempv3;
                }
            }

            public void MoveStep()
            {

                if ((nextStepTime < Time.time) && (nextStep != null))
                {
                    Vector3 tempv3 = debugOffset;
                    tempv3 += target.position;
                    tempv3 += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                    nextStep.position = tempv3;
                    nextStepTime = Time.time + moveStepInterval * Random.Range(0.5f ,1.5f);
                }

            }

            public float PulsedSpeed()
            {
                return 1;
            }

            public void CheckOrder()
            {

            }

        }

        [System.Serializable]
        public class Grouping
        {
            [Header("Grouping Setting")]

            public bool groupAble;
            public bool isCenter;
            public bool isMember;
            public Grouping master;
            public List<GameObject> member;
            public List<Spot> spots;

            public class Spot
            {
                public Vector2 offest;
                public enum SpotType { Attact, Protect, Wait }
            }
        }

    }

}

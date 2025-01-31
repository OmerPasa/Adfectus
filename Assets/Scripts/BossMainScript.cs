using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainScript : MonoBehaviour
{
    public GameObject Healthbar;
    public GameObject GameManager;
    public Transform[] waypoints;
    public Transform target;
    [SerializeField] public CapsuleCollider2D BossCollider;
    Rigidbody2D rb2d;
    Vector2 moveDirection;
    public int moveSpeed;
    private int waypointIndex;
    public float BossHealth = 5;
    private float dist;
    public float damageDelay = 5f;
    public bool Chasing;
    public bool Weakened;
    public bool Patroling;
    public bool laserAnim;
    public bool teethAnim;
    public bool fadein;
    public bool fadeout;
    AttackData fade;
    private Animator animator;
    private string currentAnimaton;
    const string BOSS_LASER = "Boss_Laser";
    const string BOSS_TEETH = "Boss_Teeth";
    const string BOSS_WEAK = "Boss_Weak";
    const string BOSS_TAKEDAMAGE = "Boss_TakeDamage";
    const string BOSS_DEATH = "Boss_Death";

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        BossCollider.enabled = false;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        target = GameObject.Find("Player").transform;
        Healthbar.GetComponent<healthbar_control>().SetMaxHealth(BossHealth);
        waypointIndex = 0;
        Patroling = true;
    }

    void Update()
    {
        /*Debug.Log("currrentState patroling " + Patroling);
        Debug.Log("currrentState chase " + Chasing);
        Debug.Log("currrentState weakened " + Weakened);*/
        if (BossHealth <= 0)
        {
            Debug.Log("game won");
            Patroling = false;
            ChangeAnimationState(BOSS_WEAK);
            GameManager.GetComponent<GameManager>().GameWon();
        }
        if (Weakened)
        {
            Patroling = false;
            BossCollider.enabled = true;
            ChangeAnimationState(BOSS_WEAK);
        }
        else
        {
            Patroling = true;
        }
        if (Chasing && !Weakened)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            moveDirection = direction;
        }
        if (!Chasing && !Weakened && Patroling)
        {
            Patrol();
        }
        dist = Vector2.Distance(transform.position, waypoints[waypointIndex].position);
        if (dist < 1f)
        {
            IncreaseIndex();
        }

        if (fadein)
        {
            if (fade.f1 > fade.f2)
            {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, fade.f2 / fade.f1);
                // Debug.Log("fadein: " + (fade.f2 / fade.f1));
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                fadein = false;
            }
            fade.f2 += Time.deltaTime;
        }

        if (fadeout)
        {
            if (fade.f1 > fade.f2)
            {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, (fade.f1 - fade.f2) / fade.f1);
                Debug.Log("fadeout: " + ((fade.f1 - fade.f2) / fade.f1));
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                fadeout = false;
            }
            fade.f2 += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (target && Chasing)
        {
            rb2d.velocity = new Vector2(moveDirection.x, moveDirection.y) * moveSpeed;
        }
    }
    public void attackAnim(AttackData clone)
    {
        if (clone.type == 3)
        {
            Debug.Log("teeth");
            ChangeAnimationState(BOSS_TEETH);
        }
        if (clone.type == 1 || clone.type == 2 || clone.type == 4 || clone.type == 5 || clone.type == 6)
        {
            Debug.Log("laser");
            ChangeAnimationState(BOSS_LASER);
        }
    }
    void Patrol()
    {
        Chasing = false;
        transform.position = Vector2.MoveTowards(transform.position, waypoints[waypointIndex].position, moveSpeed * Time.deltaTime);
        //transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }

    void IncreaseIndex()
    {
        waypointIndex++;
        if (waypointIndex >= waypoints.Length)
        {
            waypointIndex = 0;
        }
    }

    public void BossTakeDamage(float damage)
    {
        Debug.Log("Boss Taken damage");
        ChangeAnimationState(BOSS_TAKEDAMAGE);
        BossHealth -= damage;
        Debug.Log("BossHealth " + BossHealth);
        Healthbar.GetComponent<healthbar_control>().SetHealth(BossHealth);
        Invoke("DamageDelayComplete", damageDelay);
    }
    void DamageDelayComplete()
    {
        Weakened = false;
        BossCollider.enabled = false;
        Patroling = true;
    }
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }

    public void fadeIn(AttackData data)
    {
        fade = data;
        fade.f1 = TimeB.quarterBeatDuration * fade.duration.duration / 1000;
        fade.f2 = 0.0f;
        fadein = true;
    }

    public void fadeOut(AttackData data)
    {
        fade = data;
        fade.f1 = TimeB.quarterBeatDuration * fade.duration.duration / 1000;
        fade.f2 = 0.0f;
        fadeout = true;
    }
}
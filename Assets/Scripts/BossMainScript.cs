using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainScript : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform target;
    [SerializeField] public CapsuleCollider2D BossCollider;
    Rigidbody2D rb2d;
    Vector2 moveDirection;
    public int moveSpeed;
    private int waypointIndex;
    public float BossHealth = 5;
    private float dist;
    public float damageDelay;
    public bool Chasing;
    public bool Weakened;
    private Animator animator;
    private string currentAnimaton;
    const string BOSS_LASER = "Boss_Laser";
    const string BOSS_TEETH= "Boss_Teeth";
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
        // ChangeAnimationState(BOSS_LASER);
        animator = GetComponent<Animator>();
        target = GameObject.Find("Player").transform;
        waypointIndex = 0;
    }

    void Update()
    {
        if (BossHealth <= 0)
        {
            ChangeAnimationState(BOSS_DEATH);
            GetComponent<GameManager>().GameWon();
        }
        if (Weakened)
        {
            BossCollider.enabled = true;
            ChangeAnimationState(BOSS_WEAK);

        }
        if (target && Chasing && !Weakened)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            moveDirection = direction;
        }
        else
        {
            Patrol();
        }
        dist = Vector2.Distance(transform.position, waypoints[waypointIndex].position);
        if (dist < 1f)
        {
            IncreaseIndex();
        }
    }

    private void FixedUpdate()
    {
        if (target && Chasing)
        {
            rb2d.velocity = new Vector2(moveDirection.x, moveDirection.y) * moveSpeed;
        }
    }
    void Patrol()
    {
        Chasing = false;
        ChangeAnimationState(BOSS_LASER);
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
        ChangeAnimationState(BOSS_TAKEDAMAGE);
        BossHealth -= damage;
        GetComponent<healthbar_control>().SetHealth(BossHealth);
        damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
        Invoke("DamageDelayComplete", damageDelay); 
    }
    void DamageDelayComplete()
    {
        Weakened = false;
        BossCollider.enabled = true;
    }
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }
}
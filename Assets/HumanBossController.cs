using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanBossController : MonoBehaviour
{
    #region outside_connections
    public GameObject character;
    public GameObject GM;
    public Transform Character;
    public Transform EyeRay;
    public Transform MidRay;
    public Transform groundCheck;
    public GameObject bullet;
    public Rigidbody2D Rigidbody2D;
    public Transform attackPos;
    public LayerMask whatIsEnemies;
    #endregion

    #region Ranges
    [Range(0f, 10f)]
    public float attackRange;
    [Range(0f, 10f)]
    public float viewRange;
    [Range(0f, 10f)]
    public float minRange;
    [Range(0f, 10f)]
    public float closeAttackRange;
    public float closeAttackTime;
    [Range(0f, 10f)]
    public float bulletRange;
    public float bulletTime;
    public float movementSpeed;
    public float jumpPower;
    public float jumpTime;
    #endregion

    #region variables
    float closeATime2 = 0;
    float bulletTime2 = 0;
    float jumpTime2 = 0;
    float distance = 1;
    private float damageDelay;
    public int health = 4;
    public int damage = 3;
    int Count;
    [SerializeField] private float timeBtwAttack;
    [SerializeField] public float startTimeBtwAttack;
    bool grounded = true;
    bool pathBlocked = false;
    bool pathBlocked_ButCANJump;
    bool StopMoving;
    bool IsFacing_Left;
    #endregion

    #region State_Machine States
    public HumanBossBaseState currentState;
    public HumanBossRunState RunningState = new HumanBossRunState();
    public HumanBossMeleeState MeleeState = new HumanBossMeleeState();
    public HumanBossMediumState MediumState = new HumanBossMediumState();
    public HumanBossLongState LongState = new HumanBossLongState();
    #endregion

    #region Animations
    private bool isAttacking;
    private bool isTakingDamage;
    private bool isDying;
    private bool Is_jumping;
    private Animator animator;
    private string currentAnimaton;

    const string ENEMY_IDLE = "Mole_Idle";
    const string ENEMY_TAKEDAMAGE = "Mole_TakeDamage";
    const string ENEMY_DEATH = "Mole_Explode";
    const string ENEMY_ATTACK = "Mole_Attack";
    const string ENEMY_JUMP = "Mole_Jump";
    const string ENEMY_JUMPATTACK = "Mole_JumpAttack";
    const string ENEMY_MOVEMENT = "Mole_Movement";
    #endregion

    private void Start()
    {
        currentState = RunningState;
        currentState.EnterState(this);
        animator = GetComponent<Animator>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        GM = GameObject.Find("GameManager");
        Character = GameObject.Find("Player").transform;
    }
    void Update()
    {
        currentState.UpdateState(this);

        #region Flipping
        if (Character != null)
        {
            if (transform.position.x < Character.position.x)
            {
                //turn object
                transform.localScale = new Vector3(-1f, 1f, 1f);
                IsFacing_Left = true;
            }
            else if (transform.position.x > Character.position.x)
            {
                //turn object ro other side
                transform.localScale = new Vector3(1f, 1f, 1f);
                IsFacing_Left = false;
            }
        }
        #endregion

        #region Raycast
        if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")))
        {
            grounded = true;
            //Debug.Log("isGROUNDED_EnemyBoss");
        }
        else
        {
            grounded = false;
        }

        var castDist = distance;
        if (IsFacing_Left)
        {
            castDist = -distance;
        }

        Vector2 endPos = MidRay.position + Vector3.left * castDist;
        RaycastHit2D Midray = Physics2D.Linecast(MidRay.position, endPos, 1 << LayerMask.NameToLayer("Ground"));

        if (Midray.collider != null)
        {
            if (Midray.collider.gameObject.CompareTag("Ground"))
            {
                pathBlocked_ButCANJump = true;

            }
        }
        else
        {
            pathBlocked_ButCANJump = false;
        }
        #endregion

        #region DrawingLines
        Debug.DrawLine(MidRay.position, endPos, Color.green, Time.deltaTime * 10);

        Vector2 endPos1 = EyeRay.position + Vector3.left * castDist;
        RaycastHit2D Eyeray = Physics2D.Linecast(EyeRay.position, endPos1, 1 << LayerMask.NameToLayer("Ground"));

        if (Eyeray.collider != null)
        {
            if (Eyeray.collider.gameObject.CompareTag("Ground"))
            {
                pathBlocked = true;
            }
        }
        //drawing line
        Debug.DrawLine(EyeRay.position, endPos1, Color.green, Time.deltaTime * 10);

        #endregion

        #region Moving Scripts
        if (Character != null)
        {
            if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("ENEMY_ATTACK"))
            {
                StopMoving = true;
            }
            else
            {
                StopMoving = false;
            }
            Vector3 karPos = Character.transform.position;
            Vector3 pos = transform.position;
            if (Mathf.Abs(karPos.x - pos.x) < viewRange)
            {
                if (!StopMoving)
                {
                    // Move towards character
                    if (Mathf.Abs(karPos.x - pos.x) > minRange && !(pathBlocked && grounded))
                    {
                        float direction = Mathf.Sign(karPos.x - pos.x);
                        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
                        rb2d.velocity = new Vector2(direction * movementSpeed, rb2d.velocity.y);
                    }

                    // Jump if path is blocked but can jump
                    if (pathBlocked_ButCANJump && grounded && !Is_jumping)
                    {
                        jump();
                        Is_jumping = true;
                    }
                }

            }
            if (Mathf.Abs(karPos.x - pos.x) < closeAttackRange)
            {
                StopMoving = true;
                closeAttack();
            }
            else if (Mathf.Abs(karPos.x - pos.x) < bulletRange)
            {
                // shoot(karPos, pos);
            }
        }
        #endregion

        #region IdleorMove animations
        //if (pos.y != tempY) { grounded = false; } 
        //else { grounded = true; }
        // animation  part for idle or movement
        if (!isAttacking && !isTakingDamage && !isDying)
        {
            if (Rigidbody2D.velocity.x != 0.00f)
            {
                ChangeAnimationState(ENEMY_MOVEMENT);
            }
            else
            {
                ChangeAnimationState(ENEMY_IDLE);
            }
        }
        #endregion

        #region MeleeAttack
        /// DETECTİNG POSSİBLE DAMAGE DEALABLE OBJECTS
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);

        if (timeBtwAttack <= 0)
        {
            if (enemiesInRange.Length >= 1)
            {
                //for giving every one of enemies damage.
                for (int i = 0; i < enemiesInRange.Length; i++)
                {
                    isAttacking = true;
                    ChangeAnimationState(ENEMY_ATTACK);
                    damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
                    Invoke("AttackComplete", damageDelay);
                    //enemiesInRange[i].GetComponent<PlayerController>().PlayerTakeDamage(damage); // player controller  ı görmi
                }
            }
            timeBtwAttack = startTimeBtwAttack;
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
        }
        #endregion
    }




    void AttackComplete()
    {
        isAttacking = false;
        Debug.Log("ATTACKCOMPLETEBOSS");
    }
    // Callback to draw gizmos only if the object is selected.
    void OnDrawGizmos()
    {
        // Draw attack range sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw view range wire cube
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(viewRange * 2, viewRange * 2, 0f));

        // Draw min range wire cube
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(minRange * 2, minRange * 2, 0f));

        // Draw close attack range sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, closeAttackRange);

        // Draw bullet range sphere
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bulletRange);
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            isTakingDamage = true;
            Destroy(collision.gameObject);
            ChangeAnimationState(ENEMY_TAKEDAMAGE);
            health--;
            damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
            Invoke("DamageDelayComplete", damageDelay);
        }
        if (health <= 0)
        {
            isDying = true;
            ChangeAnimationState(ENEMY_DEATH);
            Debug.Log("MOLE_DIED");
            Invoke("Die", 0.9f);
        }
    }
    void DamageDelayComplete()
    {
        isTakingDamage = false;
        Is_jumping = false;
    }
    void Die()
    {
        GM.GetComponent<GameManager>().EndGame();
        Destroy(this);
    }
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }
    void jump()
    {
        Debug.Log("AI_JUMPİNG");
        if (jumpTime - (Time.realtimeSinceStartup - jumpTime2) <= 0)
        {
            jumpTime2 = Time.realtimeSinceStartup;

            gameObject.GetComponent<Rigidbody2D>().velocity += new Vector2(0f, jumpPower);
            Invoke("DamageDelayComplete", 2);
        }
    }

    void closeAttack()
    {
        //gettingDamage
        //animation
        if (closeAttackTime - (Time.realtimeSinceStartup - closeATime2) <= 0)
        {
            Debug.Log("hit");
            closeATime2 = Time.realtimeSinceStartup;
        }
    }

    void shoot(Vector3 karPos, Vector3 pos)
    {

        if (bulletTime - (Time.realtimeSinceStartup - bulletTime2) <= 0)
        {
            bulletTime2 = Time.realtimeSinceStartup;
            Debug.Log("fire");//ate�

            Vector3 a = (karPos - pos);
            GameObject bulletClone = (GameObject)Instantiate(bullet, pos + transform.right * 0.5f + new Vector3(0, 0, -0.21f), transform.rotation);
            bulletClone.transform.up = new Vector3(a.x, a.y, 0);
            bulletClone.GetComponent<BulletScriptt>().StartShooting(a.x < 0);


        }
    }
    void OnTriggerExit2D(Collider2D temas)
    {
        if (temas.tag == "block")
        {
            pathBlocked = false;
        }
    }

    public void SwitchState(HumanBossBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }
}

public abstract class HumanBossBaseState
{
    public abstract void EnterState(HumanBossController Boss);
    public abstract void UpdateState(HumanBossController Boss);
    public abstract void onCollisionEnter(HumanBossController Boss);
}

public class HumanBossRunState : HumanBossBaseState
{
    public override void EnterState(HumanBossController Boss)
    {
        Debug.Log("Running State Human Boss");
    }

    public override void UpdateState(HumanBossController Boss)
    {
        //Boss.SwitchState(Boss.HumanBossMeleeState); // tthis will switch states!
    }
    public override void onCollisionEnter(HumanBossController Boss)
    {

    }
}

public class HumanBossMeleeState : HumanBossBaseState
{
    public override void EnterState(HumanBossController Boss)
    {

    }

    public override void UpdateState(HumanBossController Boss)
    {

    }
    public override void onCollisionEnter(HumanBossController Boss)
    {

    }
}

public class HumanBossMediumState : HumanBossBaseState
{
    public override void EnterState(HumanBossController Boss)
    {

    }

    public override void UpdateState(HumanBossController Boss)
    {

    }
    public override void onCollisionEnter(HumanBossController Boss)
    {

    }
}

public class HumanBossLongState : HumanBossBaseState
{
    public override void EnterState(HumanBossController Boss)
    {

    }

    public override void UpdateState(HumanBossController Boss)
    {

    }
    public override void onCollisionEnter(HumanBossController Boss)
    {

    }
}

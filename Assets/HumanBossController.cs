using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TarodevController;

public class HumanBossController : MonoBehaviour
{
    #region outside_connections
    public GameObject gM;
    public GameObject character;
    public Transform eyeRay;
    public Transform midRay;
    public Transform groundCheck;
    public GameObject bullet;
    public Rigidbody2D rg2d;
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
    [Range(0f, 10f)]
    public float pushForce;
    public float pushDistance = 200f;
    public float maxMovementSpeed;
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
    public float damageDelay;
    public int health = 4;
    public float damage;
    int Count;
    [SerializeField] public float timeBtwAttack;
    [SerializeField] public float startTimeBtwAttack;
    public bool grounded = true;
    public bool pathBlocked = false;
    public bool pathBlocked_ButCANJump;
    public bool stopMoving;
    bool isFacing_Left;
    #endregion

    #region State_Machine States
    public HumanBossBaseState currentState;
    public HumanBossRunState runningState = new HumanBossRunState();
    public HumanBossMeleeState meleeState = new HumanBossMeleeState();
    public HumanBossMediumState mediumState = new HumanBossMediumState();
    public HumanBossLongState longState = new HumanBossLongState();
    #endregion

    #region Animations
    public bool isAttacking;
    private bool isTakingDamage;
    private bool isDying;
    public bool is_jumping;
    public Animator animator;
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
        damage = PlayerPrefs.GetFloat("damageToPlayer");
        // starting state for the state machine
        currentState = runningState;
        // "this" is a reference to the context (this EXACT Monobehavior script)
        currentState.EnterState(this);
        animator = GetComponent<Animator>();
        rg2d = GetComponent<Rigidbody2D>();
        gM = GameObject.Find("GameManager");
        character = GameObject.Find("Player");
    }
    void Update()
    {
        //so it can update every frame too while we are in our states
        currentState.UpdateState(this);

        #region Flipping
        if (character != null)
        {
            if (transform.position.x < character.transform.position.x)
            {
                //turn object
                transform.localScale = new Vector3(-1f, 1f, 1f);
                isFacing_Left = true;
            }
            else if (transform.position.x > character.transform.position.x)
            {
                //turn object ro other side
                transform.localScale = new Vector3(1f, 1f, 1f);
                isFacing_Left = false;
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
        if (isFacing_Left)
        {
            castDist = -distance;
        }

        Vector2 endPos = midRay.position + Vector3.left * castDist;
        RaycastHit2D Midray = Physics2D.Linecast(midRay.position, endPos, 1 << LayerMask.NameToLayer("Ground"));

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
        Debug.DrawLine(midRay.position, endPos, Color.green, Time.deltaTime * 10);

        Vector2 endPos1 = eyeRay.position + Vector3.left * castDist;
        RaycastHit2D Eyeray = Physics2D.Linecast(eyeRay.position, endPos1, 1 << LayerMask.NameToLayer("Ground"));

        if (Eyeray.collider != null)
        {
            if (Eyeray.collider.gameObject.CompareTag("Ground"))
            {
                pathBlocked = true;
            }
        }
        //drawing line
        Debug.DrawLine(eyeRay.position, endPos1, Color.green, Time.deltaTime * 10);

        #endregion


        #region IdleorMove animations
        //if (pos.y != tempY) { grounded = false; } 
        //else { grounded = true; }
        // animation  part for idle or movement
        if (!isAttacking && !isTakingDamage && !isDying)
        {
            if (rg2d.velocity.x != 0.00f)
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
            /*Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);

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
            }*/
            #endregion
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {//so it can do collisions when we change collision states? every frame too while we are in our states
        currentState.OnCollisionEnter(this , collision);
    }

    void AttackComplete()
    {
        isAttacking = false;
        Debug.Log("ATTACKCOMPLETEBOSS2");
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
        is_jumping = false;
    }
    void Die()
    {
        gM.GetComponent<GameManager>().EndGame();
        Destroy(this);
    }
    public void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }
    public void Jump()
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
    public abstract void EnterState(HumanBossController boss);
    public abstract void UpdateState(HumanBossController boss);
    public abstract void OnCollisionEnter(HumanBossController boss ,Collision2D collision);
}

public class HumanBossRunState : HumanBossBaseState
{
    public override void EnterState(HumanBossController boss)
    {
        Debug.Log("Boss2 run state entered");
    }

    public override void UpdateState(HumanBossController boss)
    {
        Debug.Log("Boss2 run state updating");
        if (boss.character != null)
        {
            if (boss.animator.GetCurrentAnimatorStateInfo(0).IsName("ENEMY_ATTACK"))
            {
                boss.stopMoving = true;
            }
            else
            {
                boss.stopMoving = false;
            }
            Vector3 karPos = boss.character.transform.position;
            Vector3 pos = boss.transform.position;
            boss.timeBtwAttack -= Time.deltaTime;
            if (Mathf.Abs(karPos.x - pos.x) < boss.viewRange)
            {
                if (!boss.stopMoving)
                {
                    // Move towards character
                    if (Mathf.Abs(karPos.x - pos.x) > boss.minRange && !(boss.pathBlocked && boss.grounded))
                    {
                        float direction = Mathf.Sign(karPos.x - pos.x);
                        Rigidbody2D rb2d = boss.GetComponent<Rigidbody2D>();
                        rb2d.velocity = new Vector2(direction * boss.movementSpeed, rb2d.velocity.y);
                    }

                    // Jump if path is blocked but can jump
                    if (boss.pathBlocked_ButCANJump && boss.grounded && !boss.is_jumping)
                    {
                        boss.Jump();
                        boss.is_jumping = true;
                    }
                }
            }
            //Boss.SwitchState(Boss.HumanBossMeleeState); // tthis will switch states!
        }
        if (Vector3.Distance(boss.transform.position, boss.character.transform.position) <= boss.attackRange && boss.timeBtwAttack <= 0 )
        {
            boss.SwitchState(boss.meleeState);
        }
    }
    public override void OnCollisionEnter(HumanBossController boss , Collision2D collision)
    {

    }
}

public class HumanBossMeleeState : HumanBossBaseState
{

    public override void EnterState(HumanBossController boss)
    {
        Debug.Log("Boss2 melee state started");
        // Calculate the direction towards the player
        Vector2 playerPosition = boss.character.transform.position;
        Vector2 bossPosition = boss.transform.position;
        float direction = Mathf.Sign(playerPosition.x - bossPosition.x);

        // Set the boss's movement speed to a high value
        boss.movementSpeed = boss.maxMovementSpeed;

        // Move the boss towards the player until they collide
        Rigidbody2D rb2d = boss.GetComponent<Rigidbody2D>();
        rb2d.velocity = new Vector2(direction * boss.movementSpeed, rb2d.velocity.y);

    }

    public override void UpdateState(HumanBossController boss)
    {
            if (Vector3.Distance(boss.transform.position, boss.character.transform.position) >= boss.attackRange)
            {
                boss.SwitchState(boss.runningState);
            }
            Debug.Log("Boss2 melee state updating");
    }
    public override void OnCollisionEnter(HumanBossController boss, Collision2D collision)
    {
        Vector2 karPos = boss.character.transform.position;
        Vector2 pos = boss.transform.position;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (boss.timeBtwAttack <= 0)
            {
                // Apply push force to the player
                // Apply push effect to the player

                var pushDirection = (karPos - pos).normalized;
                // Apply the push force
                boss.rg2d.AddForce(pushDirection * 5f, ForceMode2D.Impulse);

                if (!boss.isAttacking)
                {
                    boss.isAttacking = true;
                    boss.ChangeAnimationState("ENEMY_ATTACK");
                    boss.damageDelay = boss.animator.GetCurrentAnimatorStateInfo(0).length;

                    // Apply damage to the player
                    TarodevController.PlayerController playerController = boss.character.GetComponent<TarodevController.PlayerController>();
                    if (playerController != null)
                    {
                        playerController.PlayerTakeDamage(boss.damage); // is not getting called
                        boss.timeBtwAttack = boss.startTimeBtwAttack;
                        boss.SwitchState(boss.runningState);
                        boss.movementSpeed = 2f;
                        boss.Invoke("AttackComplete", boss.damageDelay);

                    }
                    if (playerController = null)
                    {
                        Debug.Log("Player is null!");
                    }
                }

            }
            else
            {
                boss.timeBtwAttack -= Time.deltaTime;
                boss.SwitchState(boss.runningState);
            }
            
        }
    }
}

public class HumanBossMediumState : HumanBossBaseState
{
    private int currentPart = 1;  // Keep track of the current part of the attack
    private float delayBetweenFires = 1f;  // The delay between each fire instantiation
    private float fireDuration = 2f;  // The duration of each fire effect
    private float fireLength = 1f; // ateşin oyuncudan uzaklığı.

    private float timer = 0f;  // Timer to track the delay between fires
    private GameObject firePrefab;  // Prefab for the fire effect

    public override void EnterState(HumanBossController boss)
    {
        // Reset the current part to 1 when entering the state
        currentPart = 1;

        // Load the fire prefab from resources or assign it manually
        firePrefab = Resources.Load<GameObject>("FirePrefab");
    }

    public override void UpdateState(HumanBossController boss)
    {
        // Check if the boss has completed all three parts of the attack
        if (currentPart > 3)
        {
            // Transition to a different state or perform any other actions
            // after completing the attack
            boss.SwitchState(boss.runningState);
            return;
        }

        // Increment the timer
        timer += Time.deltaTime;

        // Check if the delay between fires has passed
        if (timer >= delayBetweenFires)
        {
            timer = 0f;  // Reset the timer

            // Perform the appropriate action for the current part of the attack
            switch (currentPart)
            {
                case 1:
                    // Perform the first part of the attack
                    // e.g., play animation, apply damage, etc.
                    CreateFire(boss.transform.position);
                    break;

                case 2:
                    // Perform the second part of the attack
                    // e.g., play animation, apply damage, etc.
                    CreateFire(boss.transform.position + boss.transform.right * fireLength);
                    break;

                case 3:
                    // Perform the third part of the attack
                    // e.g., play animation, apply damage, etc.
                    CreateFire(boss.transform.position + boss.transform.right * 2 * fireLength);
                    break;
            }

            // Increase the current part for the next update
            currentPart++;
        }
    }

    private void CreateFire(Vector3 position)
    {
        // Instantiate the fire effect at the specified position
        GameObject fire = Instantiate(firePrefab, position, Quaternion.identity);

        // Destroy the fire effect after the specified duration
        Destroy(fire, fireDuration);

        // Apply damage to the player or handle the collision accordingly
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<Health>().TakeDamage(1f);
        }
    }

    public override void OnCollisionEnter(HumanBossController boss, Collision2D collision)
    {
        // Handle collision events during the attack if necessary
        // This method will be called when the boss collides with something
    }
}





public class HumanBossLongState : HumanBossBaseState
{
    public override void EnterState(HumanBossController boss)
    {

    }

    public override void UpdateState(HumanBossController boss)
    {

    }
    public override void OnCollisionEnter(HumanBossController boss, Collision2D collision)
    {

    }
}

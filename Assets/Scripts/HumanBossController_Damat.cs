using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TarodevController;
using System;

public class HumanBossController_Damat : MonoBehaviour
{
    #region outside_connections
    public GameObject gM;
    public GameObject character;
    public Transform eyeRay;
    public Transform midRay;
    public Transform groundCheck;
    public Rigidbody2D rg2d;
    public Transform attackPos;
    public LayerMask whatIsEnemies;
    public GameObject fireObject;

    #endregion

    #region Ranges
    [Range(0f, 50f)]
    public float viewRange;

    [Range(0f, 50f)]
    public float meleeRange;

    [Range(0f, 50f)]
    public float mediumRange;

    [Range(0f, 50f)]
    public float longRange;
    [Range(0f, 10f)]
    public float bulletRange;
    [Range(0f, 10f)]
    public float pushForce;
    [Range(0f, 10f)]
    public float distance;
    [Range(0f, 2000f)]
    public float pushDistance;
    public float maxMovementSpeed;
    public float bulletTime;
    public float movementSpeed;
    public float jumpPower;
    public float jumpTime;
    #endregion

    #region variables
    float jumpTime2 = 0;
    public float damageDelay;
    public int health = 4;
    public float damage;
    int Count;
    [SerializeField] public float timeBtw_shortAttack;
    [SerializeField] public float startTimeBtw_shortAttack;
    [SerializeField] public float timeBtw_midAttack;
    [SerializeField] public float startTimeBtw_midAttack;
    [SerializeField] public float timeBtw_longAttack;
    [SerializeField] public float startTimeBtw_longAttack;
    public bool grounded = true;
    public bool pathBlocked = false;
    public bool pathBlocked_ButCANJump;
    public bool stopMoving;
    public bool playerIsInRange = false;
    public bool playerIsInMidRangeHorizontal = false;
    bool isFacing_Left;
    public Vector2 endPos_Player;
    public Vector2 FacingDirection = Vector2.left;
    #endregion

    #region State_Machine States
    public HumanBossBaseState currentState;
    public HumanBossRunState runningState = new HumanBossRunState();
    public HumanBossMeleeState meleeState = new HumanBossMeleeState();
    public HumanBossMediumState mediumState = new HumanBossMediumState();
    public HumanBossLongState longState = new HumanBossLongState();
    #endregion

    #region Animations
    public bool isAttackingShort, isAttackingMedium, isAttackingLong;
    private bool isTakingDamage;
    private bool isDying;
    public bool is_jumping;
    public Animator animator;
    private string currentAnimaton;

    public const string ENEMY_IDLE = "Boss1_Idle";
    public const string ENEMY_MOVEMENT = "Boss1_Movement";
    public const string ENEMY_TAKEDAMAGE = "Boss1_TakeDamage";
    public const string ENEMY_ATTACK1 = "Boss1_MeleeAttack";
    public const string ENEMY_ATTACK2 = "Boss1_MediumAttack";
    public const string ENEMY_ATTACK3 = "Boss1_LongAttack";
    public const string ENEMY_JUMP = "Boss1_Jump";
    public const string ENEMY_JUMPATTACK = "Boss1_JumpAttack";
    public const string ENEMY_DEATH = "Boss1_Explode";
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
        //Debug.Log(timeBtw_longAttack + " current mid attack");

        #region Flipping
        if (character != null)
        {
            if (transform.position.x < character.transform.position.x && transform.localScale != new Vector3(-1f, 1f, 1f))
            {
                //turn object
                transform.localScale = new Vector3(-1f, 1f, 1f);
                isFacing_Left = true;
                FacingDirection = Vector2.right;
                Debug.Log("Flipped to left");
            }
            if (transform.position.x > character.transform.position.x && transform.localScale != new Vector3(1f, 1f, 1f))
            {
                //turn object ro other side
                transform.localScale = new Vector3(1f, 1f, 1f);
                isFacing_Left = false;
                FacingDirection = Vector2.left;

                Debug.Log("Flipped to right");


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
        //parth blocked but can jump part
        var castDist = 1;
        if (isFacing_Left)
        {
            castDist = -1;
        }
        Vector2 endPos = midRay.position + Vector3.left * castDist;
        RaycastHit2D Midray = Physics2D.Linecast(midRay.position, endPos, 1 << LayerMask.NameToLayer("Default"));

        if (Midray.collider != null)
        {
            if (Midray.collider.gameObject.CompareTag("Ground"))
            {
                pathBlocked_ButCANJump = true;
            }
        }
        else
            pathBlocked_ButCANJump = false;

        // Player in range detection rays


        Vector2 endPos_Player = midRay.position + Vector3.left * distance;

        RaycastHit2D hit_Player = Physics2D.Raycast(midRay.position, FacingDirection, distance, 1 << LayerMask.NameToLayer("Player"));

        if (hit_Player.collider != null)
        {
            if (hit_Player.collider.CompareTag("Player"))
            {
                playerIsInRange = true;
                Debug.DrawRay(midRay.position, FacingDirection * distance, Color.red);
            }
        }
        else
        {
            playerIsInRange = false;
            Debug.DrawRay(midRay.position, FacingDirection * distance, Color.green);
        }

        if (character.transform.position.y < 1)
        {
            playerIsInMidRangeHorizontal = true;
        }
        else if (character.transform.position.y > 1)
        {
            playerIsInMidRangeHorizontal = false;
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
        if (!isAttackingShort && !isAttackingMedium && !isAttackingLong && !isTakingDamage && !isDying)
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
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {//so it can do collisions when we change collision states? every frame too while we are in our states
        currentState.OnCollisionEnter(this, collision);
    }

    // Callback to draw gizmos only if the object is selected.
    void OnDrawGizmos()
    {
        // Draw attack range sphere
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, meleeRange);



        // Draw view range wire cube
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(viewRange * 2, viewRange * 2, 0f));

        // Draw min range wire cube
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(mediumRange * 2, mediumRange * 2, 0f));

        // Draw close attack range sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, longRange);



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
            Debug.Log("HumanBoss1 DİED");
            Invoke(nameof(Die), 0.9f);
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

    public void AttackCompleteShort()
    {
        isAttackingShort = false;
        playerIsInRange = false;
        movementSpeed = 1f;

    }
    public void AttackCompleteMedium()
    {
        isAttackingMedium = false;
        Deb.ug("AttackCompleteMedium");
    }

    public void AttackCompleteLong()
    {
        isAttackingLong = false;
    }

    public void HumanBossAttackInitiater()
    {
        Deb.ug("İnitiating Attack");
        Debug.Log("is player is in range " + playerIsInRange);

        if (playerIsInRange == true && isAttackingShort == false)
        {
            Debug.Log("is player is in range " + playerIsInRange);

            SwitchState(meleeState);
            Debug.Log("melee attack");
        }
        else if (Vector3.Distance(transform.position, character.transform.position) <= mediumRange && playerIsInMidRangeHorizontal == true && isAttackingMedium == false)
        {
            SwitchState(mediumState);
            Debug.Log("medium attack");
        }
        else if (Vector3.Distance(transform.position, character.transform.position) <= longRange && isAttackingLong == false)
        {
            SwitchState(longState);
            Deb.ug("long attack");
        }
        else
        {
            Debug.Log(isAttackingLong + "is Running");
            SwitchState(runningState);
        }
    }

}





public abstract class HumanBossBaseState
{
    //Boss.SwitchState(Boss.HumanBossMeleeState); // tthis will switch states!

    public abstract void EnterState(HumanBossController_Damat boss);
    public abstract void UpdateState(HumanBossController_Damat boss);
    public abstract void OnCollisionEnter(HumanBossController_Damat boss, Collision2D collision);
}

public class HumanBossRunState : HumanBossBaseState
{
    public override void EnterState(HumanBossController_Damat boss)
    {
        Debug.Log("Boss2 run state entered");
    }

    public override void UpdateState(HumanBossController_Damat boss)
    {
        //Debug.Log("Boss2 run state updating");
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
            boss.timeBtw_shortAttack -= Time.deltaTime;
            boss.timeBtw_midAttack -= Time.deltaTime;
            boss.timeBtw_longAttack -= Time.deltaTime;
            if (Mathf.Abs(karPos.x - pos.x) < boss.viewRange)
            {
                if (!boss.stopMoving)
                {
                    // Move towards character
                    if (Mathf.Abs(karPos.x - pos.x) > (-2f + boss.meleeRange) && !(boss.pathBlocked && boss.grounded))
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
    }
    public override void OnCollisionEnter(HumanBossController_Damat boss, Collision2D collision)
    {

    }
}

public class HumanBossMeleeState : HumanBossBaseState
{
    float maxChargeDistance;
    float chargeDistance;
    Vector2 playerPosition;
    Vector2 bossPosition;
    public override void EnterState(HumanBossController_Damat boss)
    {
        Debug.Log("Boss2 melee state started");
        boss.isAttackingShort = true;
        maxChargeDistance = 3.5f;
        // Calculate the direction towards the player
        playerPosition = boss.character.transform.position;
        bossPosition = boss.transform.position;
        float direction = Mathf.Sign(playerPosition.x - bossPosition.x);

        // Set the boss's movement speed to a high value
        boss.movementSpeed = boss.maxMovementSpeed;

        // Move the boss towards the player until they collide
        Rigidbody2D rb2d = boss.GetComponent<Rigidbody2D>();
        rb2d.velocity = new Vector2(direction * boss.movementSpeed, rb2d.velocity.y);

    }

    public override void UpdateState(HumanBossController_Damat boss)
    {
        Deb.ug("Boss2 melee state updating");// we could implement a check for players new position if its differs from the enter state we misseed so we call attack ended
        playerPosition = boss.character.transform.position;
        bossPosition = boss.transform.position;
        chargeDistance = Vector2.Distance(bossPosition, playerPosition);
        Debug.Log(chargeDistance);

        if (chargeDistance > maxChargeDistance)
        {
            // Boss missed the player
            // boss.damageDelay = boss.animator.GetCurrentAnimatorStateInfo(0).length;
            boss.Invoke(nameof(boss.AttackCompleteShort), boss.damageDelay);
        }
    }
    public override void OnCollisionEnter(HumanBossController_Damat boss, Collision2D collision)
    {
        Deb.ug("Boss2 melee Collision happaned");

        Vector2 karPos = boss.character.transform.position;
        Vector2 pos = boss.transform.position;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Apply push force to the player
            // Apply push effect to the player

            var pushDirection = (karPos - pos).normalized;
            // Apply the push force
            boss.rg2d.AddForce(pushDirection * 5f, ForceMode2D.Impulse);

            boss.ChangeAnimationState("ENEMY_ATTACK");
            // boss.damageDelay = boss.animator.GetCurrentAnimatorStateInfo(0).length;

            // Apply damage to the player
            TarodevController.PlayerController playerController = boss.character.GetComponent<TarodevController.PlayerController>();
            Deb.ug("Boss2 melee state player is null??");

            if (playerController != null)
            {
                playerController.PlayerTakeDamage(boss.damage); // is not getting called
                boss.timeBtw_shortAttack = boss.startTimeBtw_shortAttack;
                boss.SwitchState(boss.runningState);

                boss.Invoke(nameof(boss.AttackCompleteShort), boss.damageDelay);
                Deb.ug("Boss2 melee state player is NOT null");


            }
            if (playerController = null)
            {
                boss.timeBtw_shortAttack -= Time.deltaTime;
                boss.isAttackingShort = false;
                boss.SwitchState(boss.runningState);
                Debug.Log("Player is null!");
            }

            boss.timeBtw_shortAttack -= Time.deltaTime;
            boss.SwitchState(boss.runningState);


        }
    }
}
public class HumanBossMediumState : HumanBossBaseState
{
    private float delayBetweenFires = 1f;
    private float fireDuration = 2f;
    private float fireLength = (float)Math.PI;

    private float timer = 0f;
    private readonly object stateLock = new object();

    // Add a state enum to keep track of the current state within the medium attack
    private enum MediumAttackState
    {
        InitiatingPart1,
        InitiatingPart2,
        InitiatingPart3,
        Completed
    }

    private MediumAttackState mediumAttackState = MediumAttackState.InitiatingPart1;

    public override void EnterState(HumanBossController_Damat boss)
    {
        Debug.Log("Enetrstate midium state is " + mediumAttackState);
        boss.Invoke(nameof(boss.AttackCompleteMedium), 0.11f);
        if (mediumAttackState == MediumAttackState.Completed)
        {
            boss.isAttackingMedium = true;
            mediumAttackState = MediumAttackState.InitiatingPart1;
        }
        else if (mediumAttackState != MediumAttackState.InitiatingPart1)
        {
            boss.isAttackingMedium = true;
        }

    }

    public override void UpdateState(HumanBossController_Damat boss)
    {
        timer += Time.deltaTime;

        // Check if the delay between fires has passed
        if (timer >= delayBetweenFires && boss.character != null)
        {
            lock (stateLock)
            {
                Debug.Log("firing inside ");

                timer = 0f;
                Vector3 playerDirection1 = boss.character.transform.position - boss.transform.position;
                Debug.Log("player direction1 " + playerDirection1);

                playerDirection1 = new Vector3(playerDirection1.x, 0f, 0f);
                Debug.Log("player direction1 only X " + playerDirection1);

                // Perform the appropriate action based on the current state
                switch (mediumAttackState)
                {
                    case MediumAttackState.InitiatingPart1:



                        Debug.Log("player direction1 " + playerDirection1);
                        CreateFire(boss.transform.position + playerDirection1.normalized * fireLength, boss);
                        mediumAttackState = MediumAttackState.InitiatingPart2;
                        break;

                    case MediumAttackState.InitiatingPart2:
                        //Vector3 playerDirection2 = boss.character.transform.position - boss.transform.position;
                        CreateFire(boss.transform.position + playerDirection1.normalized * fireLength * 2, boss);
                        mediumAttackState = MediumAttackState.InitiatingPart3;
                        break;

                    case MediumAttackState.InitiatingPart3:
                        //Vector3 playerDirection3 = boss.character.transform.position - boss.transform.position;
                        // Find all GameObjects with a specific tag
                        GameObject[] objectsWithSpecificTag = GameObject.FindGameObjectsWithTag("Explosion");

                        // Check if there are at least 2 objects with the specified tag
                        if (objectsWithSpecificTag.Length >= 2)
                        {
                            CreateFire(boss.transform.position + playerDirection1.normalized * fireLength * 1, boss);
                        }
                        else
                        {
                            // Less than 2 objects with the specified tag were found
                            CreateFire(boss.transform.position + playerDirection1.normalized * fireLength * 3, boss);
                        }
                        mediumAttackState = MediumAttackState.Completed;
                        break;
                }
                Debug.Log("Current midium state is " + mediumAttackState);
                // Check if the boss has completed all three parts of the attack
                if (mediumAttackState == MediumAttackState.InitiatingPart2 || mediumAttackState == MediumAttackState.InitiatingPart3 || mediumAttackState == MediumAttackState.Completed)
                {
                    boss.damageDelay = boss.animator.GetCurrentAnimatorStateInfo(0).length;
                    boss.Invoke(nameof(boss.AttackCompleteMedium), boss.damageDelay);
                    return;
                }
            }
        }
    }

    private void CreateFire(Vector3 position, HumanBossController_Damat boss)
    {
        GameObject fire = GameObject.Instantiate(boss.fireObject, position, Quaternion.identity);
        GameObject.Destroy(fire, fireDuration);
        boss.timeBtw_midAttack = boss.startTimeBtw_midAttack;
    }

    public override void OnCollisionEnter(HumanBossController_Damat boss, Collision2D collision)
    {
        boss.damageDelay = boss.animator.GetCurrentAnimatorStateInfo(0).length;
        boss.Invoke(nameof(boss.AttackCompleteMedium), boss.damageDelay);
    }
}

public class HumanBossLongState : HumanBossBaseState
{
    public Vector3 distance_lasertoplayer;
    public override void EnterState(HumanBossController_Damat boss)
    {
        Deb.ug("Laser Enter state");
        boss.timeBtw_longAttack = boss.startTimeBtw_longAttack;
        boss.isAttackingLong = true;
        boss.ChangeAnimationState(HumanBossController_Damat.ENEMY_ATTACK3);
        distance_lasertoplayer = (boss.character.transform.position - boss.transform.position).normalized;


        GameObject go = GameObject.Instantiate(Resources.Load("Prefabs/LaserType6"), boss.transform.position, Quaternion.identity) as GameObject;
        if (go != null)
        {
            go.GetComponent<LASER_Gun>().distance_lasertoplayerLaser = distance_lasertoplayer;
        }
        else
        {
            // Handle the case where LASER_Gun component is not found on go.
            Debug.LogError("LASER_Gun component not found on the GameObject.");
        }
        boss.isAttackingLong = false;
        boss.SwitchState(boss.runningState);
    }
    public override void UpdateState(HumanBossController_Damat boss)
    {
        Deb.ug("Laser Update state");

    }
    public override void OnCollisionEnter(HumanBossController_Damat boss, Collision2D collision)
    {
    }
}






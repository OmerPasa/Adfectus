using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TarodevController;

public class HumanBossController_Keko : MonoBehaviour
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
    public LineRenderer teleportLineRenderer; // Assign the Line Renderer component in the inspector
    public Collider2D lineRendererCollisionObject;

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
    public float pushDistance = 200f;
    public float maxMovementSpeed;
    public float bulletTime;
    public float movementSpeed;
    public float jumpPower;
    public float jumpTime;
    #endregion

    #region variables
    private Material lineMaterial;
    private Color initialColor;
    private float startTime;
    public Vector3 teleportPosition;
    public int health = 4;
    public int Count;
    float jumpTime2 = 0;
    float distance = 1;
    public float damageDelay;
    public float damage;
    public float lineDuration;
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
    public bool canInstantiate = false;
    bool isFacing_Left;
    #endregion

    #region State_Machine States
    public HumanBoss2BaseState currentState;
    public HumanBoss2RunState runningState = new HumanBoss2RunState();
    public HumanBoss2MeleeState meleeState = new HumanBoss2MeleeState();
    public HumanBoss2MediumState mediumState = new HumanBoss2MediumState();
    public HumanBoss2LongState longState = new HumanBoss2LongState();
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
        canInstantiate = true;
    }
    void Update()
    {
        //so it can update every frame too while we are in our states
        currentState.UpdateState(this);
        //Debug.Log(timeBtw_longAttack + " current mid attack");

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
        Gizmos.DrawWireSphere(transform.position, meleeRange);

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
            Debug.Log("HumanBoss1 DÝED");
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
        Debug.Log("AI_JUMPÝNG");
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

    public void SwitchState(HumanBoss2BaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    public void AttackCompleteShort()
    {
        isAttackingShort = false;
        canInstantiate = true;
        movementSpeed = 2f;
    }
    public void AttackCompleteMedium()
    {
        isAttackingMedium = false;
        canInstantiate = true;
        Deb.ug("AttackCompleteMedium");
    }

    public void AttackCompleteLong()
    {
        isAttackingLong = false;
        canInstantiate = true;
    }

    public void TeleportBehindPlayer()
    {
        // Calculate the direction towards the player
        float direction = Mathf.Sign(character.transform.position.x - transform.position.x);
        Vector3 playerPosition = character.transform.position;
        teleportPosition.x = playerPosition.x + direction * 2.0f; // Adjust the distance as needed
        teleportPosition.y = transform.position.y;


        // Store the Line Renderer's material and initial color
        lineMaterial = teleportLineRenderer.material;
        initialColor = lineMaterial.color;

        // Start the timer
        startTime = Time.time;

        // Draw a line from the boss's current position to the teleportation destination
        teleportLineRenderer.positionCount = 2;
        teleportLineRenderer.SetPosition(0, transform.position);
        teleportLineRenderer.SetPosition(1, teleportPosition);
        // Teleport the boss to the calculated position
        transform.position = teleportPosition;
        GenerateCollider();
    }

    //generating mesh collider for line renderer
    public void GenerateCollider()
    {
        //MeshCollider collider = transform.Find("LineRenderer").gameObject.GetComponent<MeshCollider>();
        BoxCollider2D collider = lineRendererCollisionObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            //collider = transform.Find("LineRenderer").gameObject.AddComponent<MeshCollider>();
            collider = lineRendererCollisionObject.GetComponent<BoxCollider2D>();
        }

        collider.enabled = true;

        if (teleportLineRenderer.enabled)
        {
            // Set the BoxCollider2D position to match the LineRenderer
            lineRendererCollisionObject.transform.position = teleportLineRenderer.transform.position;

            // Calculate the center position and size based on LineRenderer's start and end points
            Vector3 startPosition = teleportLineRenderer.transform.TransformPoint(teleportLineRenderer.GetPosition(0));
            Vector3 endPosition = teleportLineRenderer.transform.TransformPoint(teleportLineRenderer.GetPosition(teleportLineRenderer.positionCount - 1));
            Vector3 center = (startPosition + endPosition) / 2f;
            float sizeX = Vector3.Distance(startPosition, endPosition);
            float sizeY = teleportLineRenderer.endWidth; // Set to your LineRenderer's width

            // Update BoxCollider2D's position and size
            collider.transform.position = center;
            collider.size = new Vector2(sizeX, sizeY);
        }

        StartCoroutine(HideTeleportLine(collider));
        // Coroutine to hide the teleport line after a duration
    }
    private IEnumerator HideTeleportLine(Collider2D collider)
    {
        float elapsedTime = 0f;

        while (elapsedTime < lineDuration)
        {
            float lerpValue = elapsedTime / lineDuration;
            Color newColor = Color.Lerp(initialColor, new Color(initialColor.r, initialColor.g, initialColor.b, 0f), lerpValue);
            lineMaterial.color = newColor;

            elapsedTime = Time.time - startTime;
            yield return null;
        }
        collider.enabled = false;
        collider.offset = Vector2.zero;
        collider.transform.position = Vector3.zero;
        AttackCompleteShort();// geçiçi sistem oturunca sil!!
        teleportLineRenderer.positionCount = 0; // Hide the line completely
                                                // Set the Line Renderer's color back to normal
        lineMaterial.color = initialColor;

    }
    public void HumanBossAttackInitiater()
    {
        if (isAttackingShort == false && isAttackingMedium == false)
        {

            //boss.timeBtwAttack ý mý silsek ?
            Deb.ug("Ýnitiating Attack");
            if (Vector3.Distance(transform.position, character.transform.position) <= meleeRange && timeBtw_shortAttack <= 0 && isAttackingMedium == false)
            {
                SwitchState(meleeState);
                Debug.Log("melee attack");
            }
            else if (Vector3.Distance(transform.position, character.transform.position) <= longRange && timeBtw_longAttack <= 0 && canInstantiate == true)
            {
                SwitchState(longState);
                Debug.Log("long attack");
            }
            else if (Vector3.Distance(transform.position, character.transform.position) <= mediumRange && timeBtw_midAttack <= 0 && isAttackingShort == false)
            {
                SwitchState(mediumState);
                Debug.Log("medium attack");
            }
            else if (isAttackingMedium == false && isAttackingShort == false)
            {
                SwitchState(runningState);
            }
        }

    }

}





public abstract class HumanBoss2BaseState
{
    //Boss.SwitchState(Boss.HumanBossMeleeState); // tthis will switch states!

    public abstract void EnterState(HumanBossController_Keko boss);
    public abstract void UpdateState(HumanBossController_Keko boss);
    public abstract void OnCollisionEnter(HumanBossController_Keko boss, Collision2D collision);
}

public class HumanBoss2RunState : HumanBoss2BaseState
{
    public override void EnterState(HumanBossController_Keko boss)
    {
        Debug.Log("Boss2 run state entered");
    }

    public override void UpdateState(HumanBossController_Keko boss)
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
                    if (Mathf.Abs(karPos.x - pos.x) > boss.meleeRange && !(boss.pathBlocked && boss.grounded))
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
    public override void OnCollisionEnter(HumanBossController_Keko boss, Collision2D collision)
    {

    }
}

public class HumanBoss2MeleeState : HumanBoss2BaseState
{// this short attack  will be done if player is near if player is again in the range attack
 // will be done  as swordsman runs through his enemy aka it will be teleported
 // to new location only remain will be the air that cutted
    float maxChargeDistance;
    float chargeDistance;
    Vector2 playerPosition;
    Vector2 bossPosition;
    public override void EnterState(HumanBossController_Keko boss)
    {
        Debug.Log("Boss2 melee state started");
        boss.isAttackingShort = true;
        maxChargeDistance = 5.5f;


        // Set the boss's movement speed 
        boss.movementSpeed = 0f;

        boss.Invoke(nameof(boss.TeleportBehindPlayer), 1f);

    }



    public override void UpdateState(HumanBossController_Keko boss)
    {
    }
    public override void OnCollisionEnter(HumanBossController_Keko boss, Collision2D collision)
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

public class HumanBoss2MediumState : HumanBoss2BaseState
{

    public override void EnterState(HumanBossController_Keko boss)
    {
        // Reset the current part to 1 when entering the state

    }

    public override void UpdateState(HumanBossController_Keko boss)
    {
        boss.SwitchState(boss.runningState);
        boss.isAttackingMedium = false;
    }


    public override void OnCollisionEnter(HumanBossController_Keko boss, Collision2D collision)
    {
        // Handle collision events during the attack if necessary
        // This method will be called when the boss collides with something
    }


}

public class HumanBoss2LongState : HumanBoss2BaseState
{
    public Vector3 distance_lasertoplayer;
    public override void EnterState(HumanBossController_Keko boss)
    {
        Deb.ug("Laser Enter state");
        boss.timeBtw_longAttack = boss.startTimeBtw_longAttack;
        boss.canInstantiate = true;
        boss.ChangeAnimationState(HumanBossController_Keko.ENEMY_ATTACK3);
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
        boss.canInstantiate = false;
        boss.SwitchState(boss.runningState);
    }
    public override void UpdateState(HumanBossController_Keko boss)
    {
        Deb.ug("Laser Update state");

    }
    public override void OnCollisionEnter(HumanBossController_Keko boss, Collision2D collision)
    {
    }
}






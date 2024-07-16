using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TarodevController;
using System.Threading;

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
    public LineRenderer teleportLineRenderer2; // Assign the Line Renderer component in the inspector
    public LineRenderer teleportLineRenderer3; // Assign the Line Renderer component in the inspector
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
    public int linesToDraw = 1;
    public int Shortattackhitcount = 1;
    public int health = 4;
    float jumpTime2 = 0;
    float distance = 1;
    public float damageDelay;
    public float damage;
    public float lineDuration;
    public float TeleportDistance;

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
    public bool canAttackTeleport2 = false;
    bool isFacing_Left;
    private Mutex canAttackTeleport2Mutex = new Mutex();
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
        currentState = runningState;
        currentState.EnterState(this);

        animator = GetComponent<Animator>();
        rg2d = GetComponent<Rigidbody2D>();
        gM = GameObject.Find("GameManager");
        character = GameObject.Find("Player");

        // Debug log to check initial assignments
        Debug.Log("Initialized Boss Controller");
        Debug.Log("GameManager: " + gM);
        Debug.Log("Character: " + character);
        Debug.Log("Rigidbody2D: " + rg2d);
        Debug.Log("Animator: " + animator);
    }

    void Update()
    {
        currentState.UpdateState(this);

        #region Flipping
        if (character != null)
        {
            if (transform.position.x < character.transform.position.x)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
                isFacing_Left = true;
            }
            else if (transform.position.x > character.transform.position.x)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                isFacing_Left = false;
            }
        }
        #endregion

        #region Raycast
        if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")))
        {
            grounded = true;
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
        Debug.DrawLine(eyeRay.position, endPos1, Color.green, Time.deltaTime * 10);
        #endregion

        #region IdleorMove animations
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
    {
        currentState.OnCollisionEnter(this, collision);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(viewRange * 2, viewRange * 2, 0f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(mediumRange * 2, mediumRange * 2, 0f));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, longRange);
    }

    #region BossDamageandRelated
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D called");
        Debug.Log("Collision with: " + collision.name);

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
            Debug.Log("HumanBoss1 DIED");
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
        gM.GetComponent<GameManager>().GameWon();
        Destroy(gameObject);
    }

    public void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }

    public void BossTakeDamage(int damageTOBoss)
    {
        health = health - damageTOBoss;
        Debug.Log(health + " health remains to kill the boss");
        if (health <= 0)
        {
            isDying = true;
            ChangeAnimationState(ENEMY_DEATH);
            Debug.Log("HumanBoss1 DIED");
            Invoke(nameof(Die), 0.9f);
        }
    }
    #endregion
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
        teleportLineRenderer.gameObject.GetComponent<BoxCollider2D>().enabled = false;
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
    #region TeleportBehindPlayer-ShortAttack
    public IEnumerator TeleportBehindPlayer(LineRenderer teleportLineRenderer, int linesToDraw)
    {
        for (int i = 0; i < linesToDraw; i++)
        {
            IncreaseShortAttackLines(teleportLineRenderer);
            CalculateTeleportPosition();
            SetLineRendererProperties(teleportLineRenderer);
            DrawTeleportLine(teleportLineRenderer);
            UpdateCollider(teleportLineRenderer);

            yield return new WaitForSeconds(1f); // Adding a delay between each teleport
        }
    }
    public void IncreaseShortAttackLines(LineRenderer teleportLineRenderer)
    {
        Debug.Log($"Shortattackhitcount {Shortattackhitcount}");

        // Determine the number of lines to draw based on the hit count
        int linesToDraw = 1;
        if (Shortattackhitcount == 1)
        {
            linesToDraw = 2;
        }
        else if (Shortattackhitcount >= 2)
        {
            linesToDraw = 3;
        }

        // Debugging: Check if teleportLineRenderer is null
        if (teleportLineRenderer == null)
        {
            Debug.LogError("teleportLineRenderer is null!");
            return;
        }

    }

    private void CalculateTeleportPosition()
    {
        float direction = Mathf.Sign(character.transform.position.x - transform.position.x);
        Vector3 playerPosition = character.transform.position;
        teleportPosition = new Vector3(playerPosition.x + direction * TeleportDistance, playerPosition.y); // Ensuring the boss teleports at the player's Y position
    }

    private void SetLineRendererProperties(LineRenderer teleportLineRenderer)
    {
        lineMaterial = teleportLineRenderer.material;
        initialColor = lineMaterial.color;
        startTime = Time.time;
    }

    private void DrawTeleportLine(LineRenderer teleportLineRenderer)
    {
        teleportLineRenderer.positionCount = 2;
        teleportLineRenderer.SetPosition(0, transform.position);
        teleportLineRenderer.SetPosition(1, teleportPosition);
        transform.position = teleportPosition;
    }

    private void UpdateCollider(LineRenderer teleportLineRenderer)
    {
        BoxCollider2D collider = teleportLineRenderer.GetComponent<BoxCollider2D>();
        collider.enabled = false;
        if (teleportLineRenderer.enabled)
        {
            Debug.Log("colliders ready");
            SetColliderPositionAndSize(collider, teleportLineRenderer);
        }
    }

    private void SetColliderPositionAndSize(BoxCollider2D collider, LineRenderer teleportLineRenderer)
    {
        Vector3 startPosition = teleportLineRenderer.GetPosition(0);
        Vector3 endPosition = teleportLineRenderer.GetPosition(teleportLineRenderer.positionCount - 1);
        Vector3 center = (startPosition + endPosition) / 2f;
        float sizeX = Vector3.Distance(startPosition, endPosition);
        float sizeY = teleportLineRenderer.endWidth;

        collider.transform.position = center;
        collider.size = new Vector2(sizeX, sizeY);

        StartCoroutine(HideTeleportLine(collider, teleportLineRenderer));
    }

    private IEnumerator HideTeleportLine(BoxCollider2D collider, LineRenderer teleportLineRenderer)
    {
        float firstLineDuration = lineDuration - 1f;
        yield return ColorLerpOverTime(initialColor, new Color(initialColor.r, initialColor.g, initialColor.b, 0f), firstLineDuration, lineMaterial);

        collider.enabled = true;
        lineMaterial.color = Color.red;

        yield return ColorLerpOverTime(Color.red, new Color(lineMaterial.color.r, lineMaterial.color.g, lineMaterial.color.b, 0f), 1f, lineMaterial);
        yield return new WaitForSeconds(1f);

        collider.offset = Vector2.zero;
        collider.transform.position = Vector3.zero;
        teleportLineRenderer.positionCount = 0;
        lineMaterial.color = initialColor;

        AttackCompleteShort();
    }


    private IEnumerator ColorLerpOverTime(Color startColor, Color endColor, float duration, Material targetMaterial)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float lerpValue = elapsedTime / duration;
            targetMaterial.color = Color.Lerp(startColor, endColor, lerpValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    #endregion



    public void HumanBossAttackInitiater()
    {

        Deb.ug("Ýnitiating Attack");
        if (Vector3.Distance(transform.position, character.transform.position) <= meleeRange && isAttackingShort == false)
        {
            canAttackTeleport2 = true;
            Debug.Log("canAttackTeleport2: in attacklines" + canAttackTeleport2);
            SwitchState(meleeState);
            Debug.Log("melee attack");
        }
        else if (Vector3.Distance(transform.position, character.transform.position) <= mediumRange && isAttackingMedium == false)
        {
            SwitchState(mediumState);
            Debug.Log("medium attack");
        }
        else if (Vector3.Distance(transform.position, character.transform.position) <= longRange && isAttackingLong == true)
        {
            SwitchState(longState);
            Debug.Log("long attack");
        }
        else
        {
            SwitchState(runningState);
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
        boss.StartCoroutine(boss.TeleportBehindPlayer(boss.teleportLineRenderer, boss.linesToDraw));
        //boss.Invoke(nameof(boss.TeleportBehindPlayer), 1f);
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
                playerController.PlayerTakeDamage(boss.damage);
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






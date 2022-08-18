using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
public class PlayerScriptKanlıay : MonoBehaviour
{
    public ParticleSystem dust;
    [SerializeField]
    private float runSpeed;

    [SerializeField]
    public Transform groundCheck;

    private Animator animator;
    private string currentAnimaton;
    private float xAxis;
    private float yAxis;
    private Rigidbody2D rb2d;
    private bool isJumpPressed;
    [SerializeField]
    private float jumpForce;
    private int groundMask;
    private bool isGrounded;
    private bool isAttackPressed;
    private bool isAttacking;
    private bool isntDead;
    private bool DustActive;
    private bool TakingDamage;
    AudioSource AfterFiringMusic;
    public AudioSource BackGroundM;
    public bool isFacingLeft = false;

    [SerializeField]
    private float attackDelay;
    private float damageDelay;
    public int maxHealth = 10;
    public int Playerhealth;
    public healthbar_control healthbar;
    public MainMenu mainMenu;

    //Animation States
    const string PLAYER_IDLE = "Player_Idle";
    const string PLAYER_RUN = "Player_Run";
    const string PLAYER_JUMP = "Player_Jump";
    const string PLAYER_ATTACK = "Player_Attack";
    const string PLAYER_AIR_ATTACK = "Player_AirAttack";
    const string PLAYER_DEATH = "Player_Death";
    const string PLAYER_TAKEDAMAGE = "Player_TakeDamage";

    //=====================================================
    // Start is called before the first frame update
    //=====================================================
    void Start()
    {
        isntDead = true;
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Playerhealth = maxHealth;
    }
    void Update()
    {
        if (Playerhealth <= 0)
        {
            isntDead = false;
            ChangeAnimationState(PLAYER_DEATH);
            Invoke("Die", 2f);
        }
        //Checking for inputs
        xAxis = Input.GetAxisRaw("Horizontal");
        
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            rb2d.velocity = new Vector2(runSpeed, rb2d.velocity.y);
            transform.localScale = new Vector3(1, 1, 1);
            isFacingLeft = false;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            rb2d.velocity = new Vector2(-runSpeed, rb2d.velocity.y);
            transform.localScale = new Vector3(-1, 1, 1);
            isFacingLeft = true;
        }else if (isGrounded)
        {
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }
        
        
        //space jump key pressed?
        if (Input.GetKeyDown(KeyCode.Space))
        { 
            isJumpPressed = true;
        }

        //space Atatck key pressed?
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.LeftControl))
        {
            isAttackPressed = true;
        }

        //Q tuşuna basıcak ve farenin gösterdiği yere 
        //GRAPLİNG-GUN
    }

    //=====================================================
    // Physics based time step loop
    //=====================================================
    private void FixedUpdate()
    {
        ///=================================================
        //Ground and waterchecks
        if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")))
        {
            isGrounded = true;
            Debug.Log("isGROUNDED_Player");
        }
        else
        {
            isGrounded = false;
        }

        if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Water")))
        {
            Playerhealth -= 10;
        }

        //------------------------------------------
        //animation checks
        Debug.Log("Xaxis is " + xAxis);

            if (xAxis != 0)
            {
                ChangeAnimationState(PLAYER_RUN);
            }
            else
            {
                ChangeAnimationState(PLAYER_IDLE);
            }
        


        //Check if trying to jump 
        if (isJumpPressed && isGrounded)
        {
            rb2d.AddForce(new Vector2(0, jumpForce));
            isJumpPressed = false;
            ChangeAnimationState(PLAYER_JUMP);
            
        }

        //attack
        if (isAttackPressed)
        {
            isAttackPressed = false;

            if (!isAttacking)
            {
                isAttacking = true;
                if(isGrounded)
                {
                    ChangeAnimationState(PLAYER_ATTACK);
                }
                else
                {
                    ChangeAnimationState(PLAYER_AIR_ATTACK);
                }
                Invoke("AttackComplete", attackDelay);
            }
        }
        //Debug.Log("SPEED FOR PLAYER" +  rb2d.velocity.magnitude);

        if (rb2d.velocity.magnitude > 1)
        {
            DustActive = true;
        }else
        {
            DustActive = false;
        }
        if (DustActive)
        {
            dust.Play();
        }
    }
    void AttackComplete()
    {
        isAttacking = false;
        Debug.Log("ATTACKCOMPLETE");
    }
    public void Die()
    {
        Destroy(gameObject);
    }
    public void PlayerTakeDamage(int damage)
    {
        TakingDamage = true;
        Playerhealth -= damage;
        healthbar.SetHealth(Playerhealth);
        Debug.Log("damageTaken");
        ChangeAnimationState(PLAYER_TAKEDAMAGE);
        Debug.Log("ANİMATİON CHANGED TO TAKEDAMAGE!!!!!!!!");
        damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
        Invoke("DamageDelayComplete", damageDelay);
    }
    void DamageDelayComplete()
    {
        TakingDamage = false;
    }
    void OnCollisionEnter2D(Collision2D water) 
    {
        if (water.gameObject.tag == "Water")
        {
            mainMenu.GameIsOver();
        }
    }

    //=====================================================
    // mini animation manager
    //=====================================================
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimaton == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimaton = newAnimation;
    }
    void CreateDust()
    {
        dust.Play();
    }

}

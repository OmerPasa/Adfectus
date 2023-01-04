using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// Right now it only contains movement and jumping, but it should be pretty easy to expand... I may even do it myself
    /// if there's enough interest. You can play and compete for best times here: https://tarodev.itch.io/
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/GqeHHnhHpz
    /// </summary>
    /// 
    /// 
    /// OUR NAMİNG CONVENTİON !!!!    
    /// 
    /// PUBLİC  GameDeveloper
    /// PRİVATE gameDeveloper
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        // Public for external hooks
        public GameObject CurrentOneWayPlatform;
        [SerializeField] private BoxCollider2D playerCollider;
        [SerializeField] private BoxCollider2D bCol2d;
        [SerializeField] public Collider2D Col2DHit = null;
        public float AttackRange;

        public Transform AttackPos;
        public LayerMask WhatIsEnemies;
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool JumpingThisFrame { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public Vector3 RawMovement { get; private set; }
        public Vector2 GetAxisRaw;
        public bool Grounded => _colDown;
        public bool HasDashed;
        private bool isAttacking;
        public float DashSpeed;
        private float dashTime;
        public float StartDashTime;
        private int direction;
        public int DamageBoss = 1;
        public bool IsFacingLeft;
        private InputManager inputManager;

        //Animation States
        const string PLAYER_IDLE = "Player_Idle";
        const string PLAYER_RUN = "Player_Run";
        const string PLAYER_JUMP = "Player_Jump";
        const string PLAYER_ATTACK = "Player_Attack";
        const string PLAYER_AIR_ATTACK = "Player_Jump";
        const string PLAYER_DEATH = "Player_Death";
        const string PLAYER_TAKEDAMAGE = "Player_TakeDamage";

        private bool playerRunning, playerJumping, playerAttaking, playerTakingDamage, playerDying, oneWaying;

        private Animator animator;
        private Rigidbody2D rb2d;
        AudioSource AfterFiringMusic;
        public AudioSource BackGroundM;
        public GameObject GameManager_;
        public SpriteRenderer HealthSprite;
        public SpriteRenderer RangeImage;
        public ParticleSystem DashEffect;
        private string currentAnimaton;


        [SerializeField]
        private float attackDelay;
        private float jumpDelay = 0.2f;
        public float damageDelay ;
        private float maxHealth = 1;
        private float gizmoScale = 0;
        public float Scale;
        public float DamageToPlayer;
        public float RightRay;
        public float LeftRay;
        
        [SerializeField]
        public float Playerhealth;
        private Vector3 idleVelocity = new Vector3(0, 0, 0);
        private Vector3 lastPosition;
        private float currentHorizontalSpeed, currentVerticalSpeed;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

        void Start()
        {
            RangeImage.color = new Color(0, 0, 0, 0);
            playerDying = false;
            rb2d = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            AfterFiringMusic = GetComponent<AudioSource>();
            BackGroundM = GetComponent<AudioSource>();
            HealthSprite = GetComponent<SpriteRenderer>();
            Playerhealth = maxHealth;
            dashTime = StartDashTime;
            hitBufferList.Clear();
            bCol2d = GetComponent<BoxCollider2D>();
        }
        // This is horrible, but for some reason colliders are not fully established when update starts...
        private bool _active;
        void Awake(){
            DamageToPlayer = PlayerPrefs.GetFloat("damageToPlayer");
            Debug.Log("damagetoplayerin game is " + DamageToPlayer);
            Invoke(nameof(Activate), 0.5f);

            inputManager = new InputManager();
            inputManager.Player.Enable();
            inputManager.Player.Attack.performed += Attack_performed;
            //inputManager.Player.Dash.performed += Dash_performed;
            //inputManager.Player.Jump.performed += Jump_performed;
            //inputManager.Player.Move.performed += Movement_performed;
        }
        void Activate() => _active = true;
 
        private void Update()
        {
            RangeImage.color = new Color(0, 0, 0, 0);
            
            if (Playerhealth <= 0)
            {
                playerDying = true;
                GameManager_.GetComponent<GameManager>().EndGame();
                Debug.Log("game resetting");

                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                changeAnimationState(PLAYER_DEATH);
                Invoke("Die", 2f);
            }
            if (transform.position.y <= -4)
            {
                transform.position = new Vector3(0, -2);
            }
            if (!_active) return;
            // Calculate velocity
            Velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;

            gizmoScale = 0;

            gatherInput();
            RunCollisionChecks();

            CalculateWalk(); // Horizontal movement
            calculateJumpApex(); // Affects fall speed, so calculate before gravity
            calculateGravity(); // Vertical movement
            calculateJump(); // Possibly overrides vertical

            HandleDashing();//Checks dash movement codes.
            moveCharacter(); // Actually perform the axis movement

        }


        #region Gather Input
        private void gatherInput()
        {
            GetAxisRaw = inputManager.Player.Move.ReadValue<Vector2>();
            if (inputManager.Player.Onewayplatform.triggered)
            {
                Debug.Log(" Oneway triggered");
                if (Col2DHit != null)
                {
                    StartCoroutine(disableCollusion(Col2DHit));
                }
            }

            Input = new FrameInput
            {
                JumpDown = inputManager.Player.Jump.triggered, //make fix usinghold and jump jump actionto start this bool  
                JumpUp = inputManager.Player.Jump.triggered,
                X = GetAxisRaw.x,
            };
            //prevents further pushes and animation glitch.
            if (Input.JumpDown || Input.JumpUp)
            {
                playerJumping = true;
                //Debug.Log("PlayerisJumping");
            }

            if (Input.X < 0 && IsFacingLeft)
            {
                flip();
            }
            else if (Input.X > 0 && !IsFacingLeft)
            {
                flip();
            }


            if (Input.JumpDown)
            {
                _lastJumpPressed = Time.time;
            }
        }

        public void Attack_performed(InputAction.CallbackContext context)
        {
            Debug.Log("Attacking");
            isAttacking = true;

        }
        #endregion

        #region flip
        private void flip()
        {
            Vector3 currentScale = gameObject.transform.localScale;
            currentScale.x *= -1;
            gameObject.transform.localScale = currentScale;
            IsFacingLeft = !IsFacingLeft;
        }
        #endregion

        #region Collisions


        [Header("COLLISION")][SerializeField] private Bounds _characterBounds;


        private void onTriggerEnter2D(Collider2D laser)
        {
            if (laser.gameObject.tag == "Laser")
            {
                PlayerTakeDamage(DamageToPlayer);
                Debug.Log("amount of damage " + DamageToPlayer);
                Debug.Log("DamageTaken by Player");
            }
        }


        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private int _detectorCount = 3;
        [SerializeField] private float _detectionRayLength = 0.1f;
        [SerializeField][Range(0.1f, 0.3f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground

        private RayRange _raysUp, _raysRight, _raysDown, _raysLeft;
        private bool _colUp, _colRight, _colDown, _colLeft;

        private float _timeLeftGrounded;

        
        RaycastHit2D hitL;
        RaycastHit2D hitR;

        // We use these raycast checks for pre-collision information
        private void RunCollisionChecks()
        {
            LeftRay = transform.position.x - (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) + 0.1f;
            RightRay = transform.position.x + (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) - 0.1f;

            Vector2 startPositionLeft = new Vector2(LeftRay, transform.position.y + (bCol2d.bounds.extents.y - 0.1f));
            Vector2 startPositionRight = new Vector2(RightRay, transform.position.y + (bCol2d.bounds.extents.y - 0.1f));



            // Generate ray ranges. 
            CalculateRayRanged();


            // Ground
            LandingThisFrame = false;
            var groundedCheck = RunDetection(_raysDown);
            if (_colDown && !groundedCheck) _timeLeftGrounded = Time.time; // Only trigger when first leaving
            else if (!_colDown && groundedCheck)
            {
                _coyoteUsable = true; // Only trigger when first touching
                LandingThisFrame = true;
            }

            _colDown = groundedCheck;
            
            // The rest
            _colUp = RunDetection(_raysUp);
            _colLeft = RunDetection(_raysLeft);
            _colRight = RunDetection(_raysRight);

            bool RunDetection(RayRange range)
            {
                return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, _detectionRayLength, _groundLayer));
            }
            Debug.DrawRay(startPositionLeft, transform.TransformDirection(Vector2.up), Color.green, 1f);
            Debug.DrawRay(startPositionRight, transform.TransformDirection(Vector2.up), Color.green, 1f);
            hitL = Physics2D.Raycast(startPositionLeft, transform.TransformDirection(Vector2.up), 1f, _groundLayer);   //  a function that slightly changes x value and applies it to array.
            hitR = Physics2D.Raycast(startPositionRight, transform.TransformDirection(Vector2.up), 1f, _groundLayer);
            
            if (hitL.collider != null || hitR.collider != null && playerJumping)
            {
                Col2DHit = hitL.collider != null ? hitL.collider : hitR.collider;
                if (Col2DHit.gameObject.CompareTag("OneWayPlatform") && playerJumping && !oneWaying)
                {
                    //Debug.Log($"Raycast called.tag was {col2DHit}.");
                    CurrentOneWayPlatform = Col2DHit.gameObject;
                    //Debug.Log($"currentonewayplatform is {currentOneWayPlatform}.");
                    CurrentOneWayPlatform.SetActive(false);
                    oneWaying = true;
                    playerJumping = false;
                    //Debug.Log($"Raycast called.tag was {col2DHit.tag}.");
                    Debug.Log($"Currentonewayplatform is {CurrentOneWayPlatform}.");
                    Invoke("oneWayPlatform", 0.3f);
                }
            }    
                //Raycast2D hit = Physics2D.Raycast(transform.position, out transform.TransformDirection(Vector2.up) hit, 4f, _groundLayer);



            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(AttackPos.position, AttackRange, WhatIsEnemies);

            if (isAttacking)
            {
                RangeImage.color = new Color(0, 0, 0, 1);
                CinemachineShake.Instance.ShakeCamera(6f, .1f);

                if (enemiesInRange.Length >= 1)
                {
                    //for giving every one of enemies damage.
                    for (int i = 0; i < enemiesInRange.Length; i++)
                    {
                        RangeImage.color = new Color(0, 0, 0, 1);
                        LoopController.isObjectiveCompleted = true;
                        isAttacking = true;
                        changeAnimationState(PLAYER_ATTACK);
                        damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
                        Invoke("attackComplete", damageDelay);
                        enemiesInRange[i].GetComponent<BossMainScript>().BossTakeDamage(DamageBoss);
                    }
                }
                Invoke("attackComplete", damageDelay);
            }
        }

        private void CalculateRayRanged()
        {
            // This is crying out for some kind of refactor. 
            var b = new Bounds(transform.position, _characterBounds.size);

            _raysDown = new RayRange(b.min.x + _rayBuffer, b.min.y, b.max.x - _rayBuffer, b.min.y, Vector2.down);
            _raysUp = new RayRange(b.min.x + _rayBuffer, b.max.y, b.max.x - _rayBuffer, b.max.y, Vector2.up);
            _raysLeft = new RayRange(b.min.x, b.min.y + _rayBuffer, b.min.x, b.max.y - _rayBuffer, Vector2.left);
            _raysRight = new RayRange(b.max.x, b.min.y + _rayBuffer, b.max.x, b.max.y - _rayBuffer, Vector2.right);
        }


        private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
        {
            for (var i = 0; i < _detectorCount; i++)
            {
                var t = (float)i / (_detectorCount - 1);
                yield return Vector2.Lerp(range.Start, range.End, t);
            }
        }

        private void OnDrawGizmos()
        {
            // Bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + _characterBounds.center, _characterBounds.size);

            // Rays
            if (!Application.isPlaying)
            {
                CalculateRayRanged();
                Gizmos.color = Color.blue;
                foreach (var range in new List<RayRange> { _raysUp, _raysRight, _raysDown, _raysLeft })
                {
                    foreach (var point in EvaluateRayPositions(range))
                    {
                        Gizmos.DrawRay(point, range.Dir * _detectionRayLength);
                    }
                }
            }

            if (!Application.isPlaying) return;

            // Draw the future position. Handy for visualizing gravity
            Gizmos.color = Color.red;
            var move = new Vector3(currentHorizontalSpeed, currentVerticalSpeed) * Time.deltaTime;
            Gizmos.DrawWireCube(transform.position + move, _characterBounds.size);
        }

        #endregion


        #region Walk

        [Header("WALKING")][SerializeField] private float _acceleration = 90;
        [SerializeField] private float _moveClamp = 13;
        [SerializeField] private float _deAcceleration = 60f;
        [SerializeField] private float _apexBonus = 2;

        private void CalculateWalk()
        {
            if (Input.X != 0)
            {
                // Set horizontal move speed
                currentHorizontalSpeed += Input.X * _acceleration * Time.deltaTime;

                // clamped by max frame movement
                currentHorizontalSpeed = Mathf.Clamp(currentHorizontalSpeed, -_moveClamp, _moveClamp);

                // Apply bonus at the apex of a jump
                var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
                currentHorizontalSpeed += apexBonus * Time.deltaTime;
            }
            else
            {
                // No input. Let's slow the character down
                currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, 0, _deAcceleration * Time.deltaTime);
            }

            if (currentHorizontalSpeed > 0 && _colRight || currentHorizontalSpeed < 0 && _colLeft)
            {
                // Don't walk through walls
                currentHorizontalSpeed = 0;
            }
            if (!playerAttaking && !playerDying && !playerJumping)
            {
                if (currentHorizontalSpeed != 0.0f)
                {
                    changeAnimationState(PLAYER_RUN);
                }
                else if (currentHorizontalSpeed == 0.0f)
                {
                    changeAnimationState(PLAYER_IDLE);
                }
            }
        }

        #endregion

        #region Gravity

        [Header("GRAVITY")][SerializeField] private float fallClamp = -40f;
        [SerializeField] private float minFallSpeed = 80f;
        [SerializeField] private float maxFallSpeed = 120f;
        private float _fallSpeed;

        private void calculateGravity()
        {
            if (_colDown)
            {
                // Move out of the ground
                if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
            }
            else
            {
                // Add downward force while ascending if we ended the jump early
                var fallSpeed = _endedJumpEarly && currentVerticalSpeed > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

                // Fall
                currentVerticalSpeed -= fallSpeed * Time.deltaTime;

                // Clamp
                if (currentVerticalSpeed < fallClamp) currentVerticalSpeed = fallClamp;
            }
        }

        #endregion

        #region Jump

        [Header("JUMPING")][SerializeField] private float _jumpHeight = 30;
        [SerializeField] private float _jumpApexThreshold = 10f;
        [SerializeField] private float _coyoteTimeThreshold = 0.1f;
        [SerializeField] private float _jumpBuffer = 0.1f;
        [SerializeField] private float _jumpEndEarlyGravityModifier = 3;
        private bool _coyoteUsable;
        private bool _endedJumpEarly = true;
        private float _apexPoint; // Becomes 1 at the apex of a jump
        private float _lastJumpPressed;
        private bool CanUseCoyote => _coyoteUsable && !_colDown && _timeLeftGrounded + _coyoteTimeThreshold > Time.time;
        private bool HasBufferedJump => _colDown && _lastJumpPressed + _jumpBuffer > Time.time;

        private void calculateJumpApex()
        {
            if (!_colDown)
            {
                // Gets stronger the closer to the top of the jump
                _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
                _fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, _apexPoint);
            }
            else
            {
                _apexPoint = 0;
            }
        }

        private void calculateJump()
        {
            // Jump if: grounded or within coyote threshold || sufficient jump buffer
            if (Input.JumpDown && CanUseCoyote && playerJumping || HasBufferedJump)
            {
                currentVerticalSpeed = _jumpHeight;
                _endedJumpEarly = false;
                _coyoteUsable = false;
                _timeLeftGrounded = float.MinValue;
                JumpingThisFrame = true;
                changeAnimationState(PLAYER_JUMP);
                Invoke("jumploop", jumpDelay);
            }
            else
            {
                JumpingThisFrame = false;
            }

            // End the jump early if button released
            if (!_colDown && Input.JumpUp && !_endedJumpEarly && playerJumping && Velocity.y > 0)
            {
                // currentVerticalSpeed = 0;
                _endedJumpEarly = true;
                changeAnimationState(PLAYER_JUMP);
                Invoke("jumploop", jumpDelay);
            }

            if (_colUp)
            {
                if (currentVerticalSpeed > 0) currentVerticalSpeed = 0;
            }
        }

        #endregion

        #region Dash

        private void HandleDashing()
        {
            if (direction == 0)
            {
                if (inputManager.Player.DashLeft.triggered && !HasDashed)
                {
                    createDust();
                    direction = 1;
                }
                else if (inputManager.Player.DashRight.triggered && !HasDashed)
                {
                    createDust();
                    direction = 2;
                }
            }
            else
            {
                if (dashTime <= 0)
                {
                    direction = 0;
                    dashTime = StartDashTime;
                    rb2d.velocity = Vector2.zero;
                }
                else
                {
                    dashTime -= Time.deltaTime;

                    if (direction == 1)
                    {
                        rb2d.velocity = Vector2.left * DashSpeed;
                        Physics.IgnoreLayerCollision(8, 9, true);
                        Invoke("dashRecovery", 2f);
                    }
                    else if (direction == 2)
                    {
                        rb2d.velocity = Vector2.right * DashSpeed;
                        Physics2D.IgnoreLayerCollision(8, 9, true);
                        Invoke("dashRecovery", 2f);
                    }

                }
            }
        }

        #endregion

        #region Move

        [Header("MOVE")]
        [SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
        private int freeColliderIterations;

        // We cast our bounds before moving to avoid future collisions

        private void moveCharacter()
        {
            playerRunning = true;
            var pos = transform.position;
            RawMovement = new Vector3(currentHorizontalSpeed, currentVerticalSpeed); // Used externally

            var move = RawMovement * Time.deltaTime;
            var furthestPoint = pos + move;

            // check furthest movement. If nothing hit, move and don't do extra checks
            var hit = Physics2D.OverlapBox(furthestPoint, _characterBounds.size, 0, _groundLayer);
            if (!hit)
            {
                transform.position += move;
                return;
            }

            // otherwise increment away from current pos; see what closest position we can move to
            var positionToMoveTo = transform.position;
            for (int i = 1; i < freeColliderIterations; i++)
            {
                // increment to check all but furthestPoint - we did that already
                var t = (float)i / freeColliderIterations;
                var posToTry = Vector2.Lerp(pos, furthestPoint, t);

                if (Physics2D.OverlapBox(posToTry, _characterBounds.size, 0, _groundLayer))
                {
                    transform.position = positionToMoveTo;

                    // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                    if (i == 1)
                    {
                        if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
                        var dir = transform.position - hit.transform.position;
                        transform.position += dir.normalized * move.magnitude;
                    }

                    return;
                }

                positionToMoveTo = posToTry;
            }
        }

        #endregion

        #region Health

        //on trigger () Will trigger the player take damage and it will done most of the other work
        //then set_Health will give the color of current health.
        public void Die()
        {
            Destroy(gameObject);
        }
        public void PlayerTakeDamage(float damage)
        {
            playerTakingDamage = true;
            Playerhealth -= damage;
            set_Health(Playerhealth);
            Debug.Log("Player_Taken_Damage");
            changeAnimationState(PLAYER_TAKEDAMAGE);
            damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
            Invoke("damageDelayComplete", damageDelay);
        }

        // set new health to the sprite filter as a new color.
        void set_Health(float Playerhealth)
        {
            HealthSprite.color = new Color(Playerhealth, Playerhealth, Playerhealth, 1);
        }
        void damageDelayComplete()
        {
            playerTakingDamage = false;
        }
        #endregion

        private void onCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("OneWayPlatform"))
            {
                CurrentOneWayPlatform = collision.gameObject;

            }
        }

        private void onCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("OneWayPlatform"))
            {
                CurrentOneWayPlatform = null;
            }
        }
        private IEnumerator disableCollusion(Collider2D platformCollider)
        {
            //BoxCollider2D platformCollider = CurrentOneWayPlatform.GetComponent<BoxCollider2D>();
            Debug.Log($"current disabled collusion {platformCollider.gameObject.name}");
            Physics2D.IgnoreCollision(playerCollider, platformCollider);
            yield return new WaitForSeconds(0.6f);
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
        private void oneWayPlatform()
        {
            CurrentOneWayPlatform?.gameObject.SetActive(true);
            Debug.Log($"currentonewayplatform from inside LAST part is {CurrentOneWayPlatform}.");
            oneWaying = false;
            CurrentOneWayPlatform = null;
            Col2DHit = null;
            //Debug.Log($"col2DHit from inside LAST part is COLLider {col2DHit}.");
            //col2DHit.gameObject.SetActive(true);
        }
        void jumploop()
        {
            playerJumping = false;
        }

        void attackComplete()
        {
            isAttacking = false;
            RangeImage.color = new Color(0, 0, 0, 0);
            Debug.Log("aTTACKCOMPLETEBOSS");
        }
        void dashRecovery()
        {
            Physics2D.IgnoreLayerCollision(8, 9, false);
        }
        void createDust()
        {
            DashEffect.Play();
        }


        //=====================================================
        // mini animation manager
        //=====================================================
        private void changeAnimationState(string newAnimation)
        {
            if (currentAnimaton == newAnimation) return;

            animator.Play(newAnimation);
            currentAnimaton = newAnimation;
        }
    }
}
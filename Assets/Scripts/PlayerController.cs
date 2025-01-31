using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cinemachine;

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
    /// OUR NAM�NG CONVENT�ON !!!!    
    /// 
    /// PUBL�C  GameDeveloper
    /// PR�VATE gameDeveloper
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        // Public for external hooks
        public GameObject currentOneWayPlatform;
        [SerializeField] private BoxCollider2D playerCollider;
        [SerializeField] private BoxCollider2D bCol2d;
        [SerializeField] public Collider2D col2DHit = null;
        public float attackRange;
        public Transform attackPos;


        [Header("Squash & Stretch")]
        [SerializeField] private Transform visualJump;
        [SerializeField] private float jumpingScaleAmount;
        [SerializeField] private float jumpAnimationDur;
        [SerializeField] private float landingScaleAmount;
        [SerializeField] private float landAnimationDur;
        [SerializeField] private float dodgeScaleAmount;
        [SerializeField] private float dodAnimationDur;
        [Space]
        public LayerMask whatIsEnemies;
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool JumpingThisFrame { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public Vector3 RawMovement { get; private set; }
        public Vector2 GetAxisRaw;
        public bool Grounded => _colDown;
        private bool wasGrounded;
        public bool _hasDashed;
        private bool isAttacking;
        public bool isFacingLeft;
        private bool CanAttack;
        private int direction;
        public int damageBoss = 1;

        public int missedBeatPenalty;
        public int maxScore;
        public int playerScore;

        public float startDashTime;
        private float dashTime;
        public float dashSpeed;
        public float pushForce = 15f;
        private InputManager inputManager;


        [Header("Animations")]
        //Animation States
        [SerializeField] private string currentAnimaton;
        [SerializeField] private Animator animator;

        const string PLAYER_IDLE = "Player_Idle";
        const string PLAYER_RUN = "Player_Run";
        const string PLAYER_JUMP = "Player_Jump";
        const string PLAYER_ATTACK = "Player_Attack";
        const string PLAYER_AIR_ATTACK = "Player_Jump";
        const string PLAYER_DEATH = "Player_Death";
        const string PLAYER_TAKEDAMAGE = "Player_TakeDamage";

        bool playerRunning, playerJumping, playerAttaking, playerTakingDamage, playerDying, Onewaying;

        private Rigidbody2D rb2d;
        AudioSource AfterFiringMusic;
        public AudioSource BackGroundM;
        public GameObject GameManager_;
        public SpriteRenderer Healthsprite;
        public SpriteRenderer RangeImage;
        public ParticleSystem dashEffect;
        [SerializeField]
        public CinemachineVirtualCamera virtualCamera;

        [SerializeField]
        private float attackDelay;
        private float jumpDelay = 0.2f;
        private float damageDelay = 2f;
        private float maxHealth = 1;
        public float damageToPlayer;
        public float rightRay;
        public float leftRay;
        public float movementSpeed = 0.1f;
        public float offsetSpeed = 0.5f;
        private float offset = 0f;

        public TimingWindow timingWindow; // Assign this in the Unity Inspector

        private float beatTimestamp; // Store the timestamp of the beat


        [SerializeField]
        public static float Playerhealth = 1;
        private Vector3 IdleVelocity = new Vector3(0, 0, 0);
        private Vector3 _lastPosition;
        private float _currentHorizontalSpeed, _currentVerticalSpeed;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

        void Start()
        {
            RangeImage.color = new Color(0, 0, 0, 0);
            playerDying = false;
            rb2d = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
            AfterFiringMusic = GetComponent<AudioSource>();
            BackGroundM = GetComponent<AudioSource>();
            Healthsprite = GetComponentInChildren<SpriteRenderer>();
            Playerhealth = maxHealth;
            dashTime = startDashTime;
            hitBufferList.Clear();
            bCol2d = GetComponent<BoxCollider2D>();
            //virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
        // This is horrible, but for some reason colliders are not fully established when update starts...
        private bool _active;
        void Awake()
        {
            damageToPlayer = PlayerPrefs.GetFloat("damageToPlayer");
            Debug.Log("damagetoplayerin game is " + damageToPlayer);
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
            RangeImage.color = new Color(0, 0, 0, 0.2f);
            if (Playerhealth <= 0)
            {
                playerDying = true;
                GameManager_.GetComponent<GameManager>().EndGame();
                Deb.ug("game resetting");

                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                ChangeAnimationState(PLAYER_DEATH);
                Invoke("Die", 2f);
            }
            if (transform.position.y <= -4)
            {
                transform.position = new Vector3(0, -2);
            }
            if (!_active) return;
            // Calculate velocity
            Velocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;

            GatherInput();
            RunCollisionChecks();

            CalculateWalk(); // Horizontal movement
            CalculateJumpApex(); // Affects fall speed, so calculate before gravity
            CalculateGravity(); // Vertical movement
            CalculateJump(); // Possibly overrides vertical

            HandleDashing();//Checks dash movement codes.
            MoveCharacter(); // Actually perform the axis movement
            if (!wasGrounded && Grounded)
                ApplyLandingAnimation();

            wasGrounded = Grounded;
        }

        private void FixedUpdate()
        {
            // Check if the player has gone above the threshold
            //Debug.Log("y axis of player " + transform.position.y);
            if (transform.position.y > 0.75f && virtualCamera != null)
            {
                virtualCamera.gameObject.SetActive(true);
            }
            else
            {
                virtualCamera.gameObject.SetActive(false);
            }

        }
        #region Gather Input
        private void GatherInput()
        {
            GetAxisRaw = inputManager.Player.Move.ReadValue<Vector2>();
            if (inputManager.Player.Onewayplatform.triggered)
            {
                if (currentOneWayPlatform != null)
                {
                    StartCoroutine(DisableCollusion());
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
                ApplyJumpAnimation();
                //FlipBool(ref isFacingLeft);
                //Debug.Log("PlayerisJumping");
            }

            if (Input.X < 0 && isFacingLeft)
            {
                Flip();
            }
            else if (Input.X > 0 && !isFacingLeft)
            {
                Flip();
            }


            if (Input.JumpDown)
            {
                _lastJumpPressed = Time.time;
            }
        }
        public bool FlipBool(ref bool value)
        {
            value = !value;
            return value;
        }

        public void Attack_performed(InputAction.CallbackContext context)
        {
            Debug.Log("Attacking");
            isAttacking = true;
        }
        public void BeatPress()
        {
            CanAttack = true;
            beatTimestamp = Time.time;
        }

        void CheckTiming()
        {
            float timingDifference = Mathf.Abs(beatTimestamp - timingWindow.perfectTiming);
            float accuracy = 1.0f - (timingDifference / (timingWindow.windowSize / 2f));

            // Ensure that accuracy is clamped between 0 and 1
            accuracy = Mathf.Clamp01(accuracy);

            // Calculate the score based on accuracy (you can define your scoring system)
            int score = Mathf.RoundToInt(accuracy * maxScore);

            if (timingWindow.IsTimingPerfect(beatTimestamp))
            {
                // The player's attack timing was within the timing window (on beat)
                Debug.Log("Perfect Timing for Attack!");
            }
            else
            {
                // The player's attack timing was outside the timing window (missed beat)
                Debug.Log("Missed Beat for Attack");
                // Apply a penalty to the score for missing the beat
                score -= missedBeatPenalty;
            }

            // Handle scoring and game logic using the 'score' variable
            UpdateScore(score);
        }

        void UpdateScore(int score)
        {
            // Update the player's score and display it
            playerScore += score;
            Debug.Log("Score: " + playerScore);

            // You can update the UI to display the player's score
            // Example: scoreText.text = "Score: " + playerScore;
        }

        #endregion

        #region Flip
        private void Flip()
        {
            Vector3 currentScale = gameObject.transform.localScale;
            currentScale.x *= -1;
            gameObject.transform.localScale = currentScale;
            isFacingLeft = !isFacingLeft;
        }
        #endregion

        #region Collisions


        [Header("COLLISION")] [SerializeField] private Bounds _characterBounds;


        private void OnTriggerEnter2D(Collider2D laser)
        {
            if (laser.gameObject.tag == "Laser")
            {
                PlayerTakeDamage(damageToPlayer);
                Debug.Log("amount of damage " + damageToPlayer);
                Debug.Log("DamageTaken by Player");
            }
        }

        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private int _detectorCount = 3;
        [SerializeField] private float _detectionRayLength = 0.1f;
        [SerializeField] [Range(0.1f, 0.3f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground

        private RayRange _raysUp, _raysRight, _raysDown, _raysLeft;
        private bool _colUp, _colRight, _colDown, _colLeft;
        private float _timeLeftGrounded;
        RaycastHit2D hitL;
        RaycastHit2D hitR;

        // We use these raycast checks for pre-collision information
        private void RunCollisionChecks()
        {
            leftRay = transform.position.x - (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) + 0.1f;
            rightRay = transform.position.x + (bCol2d.size.x * transform.localScale.x / 2.0f) + (bCol2d.offset.x * transform.localScale.x) - 0.1f;

            Vector2 startPositionLeft = new Vector2(leftRay, transform.position.y + (bCol2d.bounds.extents.y - 0.1f));
            Vector2 startPositionRight = new Vector2(rightRay, transform.position.y + (bCol2d.bounds.extents.y - 0.1f));



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
                col2DHit = hitL.collider != null ? hitL.collider : hitR.collider;
                if (col2DHit.gameObject.CompareTag("OneWayPlatform") && playerJumping && !Onewaying)
                {
                    //Debug.Log($"Raycast called.tag was {col2DHit}.");
                    currentOneWayPlatform = col2DHit.gameObject;
                    //Debug.Log($"currentonewayplatform is {currentOneWayPlatform}.");
                    currentOneWayPlatform.SetActive(false);
                    Onewaying = true;
                    playerJumping = false;
                    //Debug.Log($"Raycast called.tag was {col2DHit.tag}.");
                    Debug.Log($"currentonewayplatform is {currentOneWayPlatform}.");
                    Invoke("OneWayPlatform", 0.3f);
                }
            }
            //Raycast2D hit = Physics2D.Raycast(transform.position, out transform.TransformDirection(Vector2.up) hit, 4f, _groundLayer);



            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);

            if (isAttacking && CanAttack)
            {
                if (enemiesInRange.Length >= 1)
                {
                    RangeImage.color = new Color(0, 0, 0, 1);
                    CinemachineShake.Instance.ShakeCamera(6f, .1f);
                    //for giving every one of enemies damage.
                    for (int i = 0; i < enemiesInRange.Length; i++)
                    {
                        LoopController.isObjectiveCompleted = true;
                        isAttacking = true;
                        ChangeAnimationState(PLAYER_ATTACK);
                        damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
                        enemiesInRange[i].GetComponent<BossMainScript>().BossTakeDamage(damageBoss);
                    }
                }
                Invoke("AttackComplete", damageDelay);
            }
        }
        public void setExactHitTime(int counterQ) //this does nothing
        {
            float exactHitTime = (float)counterQ * TimeB.quarterBeatDuration;
            //TimeB.timePassed
            //Debug.Log("exactHitTime: " + exactHitTime);
            //Debug.Log("TimeB.timePassed: " + TimeB.timePassed);
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
            var move = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed) * Time.deltaTime;
            Gizmos.DrawWireCube(transform.position + move, _characterBounds.size);
        }

        #endregion


        #region Walk

        [Header("WALKING")] [SerializeField] private float _acceleration = 90;
        [SerializeField] private float _moveClamp = 13;
        [SerializeField] private float _deAcceleration = 60f;
        [SerializeField] private float _apexBonus = 2;

        private void CalculateWalk()
        {
            if (Input.X != 0)
            {
                // Set horizontal move speed
                _currentHorizontalSpeed += Input.X * _acceleration * Time.deltaTime;

                // clamped by max frame movement
                _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed, -_moveClamp, _moveClamp);

                // Apply bonus at the apex of a jump
                var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
                _currentHorizontalSpeed += apexBonus * Time.deltaTime;
            }
            else
            {
                // No input. Let's slow the character down
                _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 0, _deAcceleration * Time.deltaTime);
            }

            if (_currentHorizontalSpeed > 0 && _colRight || _currentHorizontalSpeed < 0 && _colLeft)
            {
                // Don't walk through walls
                _currentHorizontalSpeed = 0;
            }
            if (!playerAttaking && !playerDying && !playerJumping)
            {
                if (_currentHorizontalSpeed != 0.0f)
                {
                    ChangeAnimationState(PLAYER_RUN);
                }
                else if (_currentHorizontalSpeed == 0.0f)
                {
                    ChangeAnimationState(PLAYER_IDLE);
                }
            }
        }

        #endregion

        #region Gravity

        [Header("GRAVITY")] [SerializeField] private float _fallClamp = -40f;
        [SerializeField] private float _minFallSpeed = 80f;
        [SerializeField] private float _maxFallSpeed = 120f;
        private float _fallSpeed;

        private void CalculateGravity()
        {
            if (_colDown)
            {
                // Move out of the ground
                if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
            }
            else
            {
                // Add downward force while ascending if we ended the jump early
                var fallSpeed = _endedJumpEarly && _currentVerticalSpeed > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

                // Fall
                _currentVerticalSpeed -= fallSpeed * Time.deltaTime;

                // Clamp
                if (_currentVerticalSpeed < _fallClamp) _currentVerticalSpeed = _fallClamp;
            }
        }

        #endregion

        #region Jump

        [Header("JUMPING")] [SerializeField] private float _jumpHeight = 30;
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

        private void CalculateJumpApex()
        {
            if (!_colDown)
            {
                // Gets stronger the closer to the top of the jump
                _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
                _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
            }
            else
            {
                _apexPoint = 0;
            }
        }

        private void CalculateJump()
        {
            // Jump if: grounded or within coyote threshold || sufficient jump buffer
            if (Input.JumpDown && CanUseCoyote && playerJumping || HasBufferedJump)
            {
                _currentVerticalSpeed = _jumpHeight;
                _endedJumpEarly = false;
                _coyoteUsable = false;
                _timeLeftGrounded = float.MinValue;
                JumpingThisFrame = true;

                ChangeAnimationState(PLAYER_JUMP);
                Invoke("Jumploop", jumpDelay);
            }
            else
            {
                JumpingThisFrame = false;
            }

            // End the jump early if button released
            if (!_colDown && Input.JumpUp && !_endedJumpEarly && playerJumping && Velocity.y > 0)
            {
                // _currentVerticalSpeed = 0;
                _endedJumpEarly = true;
                ChangeAnimationState(PLAYER_JUMP);
                Invoke("Jumploop", jumpDelay);
            }

            if (_colUp)
            {
                if (_currentVerticalSpeed > 0) _currentVerticalSpeed = 0;
            }
        }
        private void ApplyJumpAnimation()
        {
            visualJump.DOScale(new Vector3(1 - jumpingScaleAmount, 1 + jumpingScaleAmount, 1f), jumpAnimationDur * 0.5f).OnComplete(
                () => visualJump.DOScale(new Vector3(1f, 1f, 1f), jumpAnimationDur * 0.5f)
            );
        }

        private void ApplyLandingAnimation()
        {
            visualJump.DOScale(new Vector3(1 - landingScaleAmount, 1 + landingScaleAmount, 1f), landAnimationDur * 0.5f).OnComplete(
              () => visualJump.DOScale(new Vector3(1f, 1f, 1f), landAnimationDur * 0.5f)
            );
        }

        private void ApplyDodgeAnimation()
        {
            visualJump.DOScale(new Vector3(1 + dodgeScaleAmount, 1 - dodgeScaleAmount, 1f), dodAnimationDur * 0.5f).OnComplete(
                () => visualJump.DOScale(new Vector3(1f, 1f, 1f), dodAnimationDur * 0.5f)
                );
        }
        #endregion

        #region Dash

        private void HandleDashing()
        {
            if (direction == 0)
            {
                if (inputManager.Player.DashLeft.triggered && !_hasDashed)
                {
                    CreateDust();
                    direction = 1;
                }
                else if (inputManager.Player.DashRight.triggered && !_hasDashed)
                {
                    CreateDust();
                    direction = 2;
                }
            }
            else
            {
                if (dashTime <= 0)
                {
                    direction = 0;
                    dashTime = startDashTime;
                    rb2d.velocity = Vector2.zero;
                }
                else
                {
                    dashTime -= Time.deltaTime;

                    if (direction == 1)
                    {
                        rb2d.velocity = Vector2.left * dashSpeed;
                        Physics.IgnoreLayerCollision(8, 9, true);
                        Invoke("dashRecovery", 2f);
                    }
                    else if (direction == 2)
                    {
                        rb2d.velocity = Vector2.right * dashSpeed;
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
        private int _freeColliderIterations = 10;

        // We cast our bounds before moving to avoid future collisions

        private void MoveCharacter()
        {
            playerRunning = true;
            var pos = transform.position;
            RawMovement = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed); // Used externally

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
            for (int i = 1; i < _freeColliderIterations; i++)
            {
                // increment to check all but furthestPoint - we did that already
                var t = (float)i / _freeColliderIterations;
                var posToTry = Vector2.Lerp(pos, furthestPoint, t);

                if (Physics2D.OverlapBox(posToTry, _characterBounds.size, 0, _groundLayer))
                {
                    transform.position = positionToMoveTo;

                    // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                    if (i == 1)
                    {
                        if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
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
        //then Set_Health will give the color of current health.
        public void Die()
        {
            Destroy(gameObject);
        }
        public void PlayerTakeDamage(float damage)
        {
            playerTakingDamage = true;
            Playerhealth -= damage;
            Set_Health(Playerhealth);
            ChangeAnimationState(PLAYER_TAKEDAMAGE);
            damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
            Invoke("DamageDelayComplete", damageDelay);
            Debug.Log("Player has taken damage");
        }

        // set new health to the sprite filter as a new color.
        void Set_Health(float Playerhealth)
        {
            Healthsprite.color = new Color(Playerhealth, Playerhealth, Playerhealth, 1);
        }
        void DamageDelayComplete()
        {
            playerTakingDamage = false;
            ChangeAnimationState(PLAYER_IDLE);
        }
        #endregion

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("collusion happned");
            if (collision.gameObject.CompareTag("Boss2"))
            {
                // Calculate the push direction away from the boss
                Vector3 bossPosition = collision.gameObject.transform.position;
                Vector3 playerPosition = transform.position;

                //Calculating the direction
                Vector3 pushDirection = (playerPosition - bossPosition).normalized;

                // Apply the push force
                rb2d.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                Debug.Log("Player has been pushed by the boss2");
                ApplyDodgeAnimation();


            }
            if (collision.gameObject.CompareTag("OneWayPlatform"))
            {
                currentOneWayPlatform = collision.gameObject;

            }

        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("OneWayPlatform"))
            {
                currentOneWayPlatform = null;
            }
        }
        private IEnumerator DisableCollusion()
        {
            Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();
            Debug.Log($"current disabled collusion {platformCollider.gameObject.name}");
            Physics2D.IgnoreCollision(playerCollider, platformCollider);
            yield return new WaitForSeconds(0.6f);
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
        private void OneWayPlatform()
        {
            currentOneWayPlatform?.gameObject.SetActive(true);
            Debug.Log($"currentonewayplatform from inside LAST part is {currentOneWayPlatform}.");
            Onewaying = false;
            currentOneWayPlatform = null;
            col2DHit = null;
            //Debug.Log($"col2DHit from inside LAST part is COLLider {col2DHit}.");
            //col2DHit.gameObject.SetActive(true);
        }
        void Jumploop()
        {
            playerJumping = false;
        }



        void AttackComplete()
        {
            isAttacking = false;
            CanAttack = false;
            RangeImage.color = new Color(0, 0, 0, 0);
            Debug.Log("ATTACKCOMPLETEPlayer");
        }
        void dashRecovery()
        {
            Physics2D.IgnoreLayerCollision(8, 9, false);
        }
        void CreateDust()
        {
            dashEffect.Play();
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
    }

    public class TimingWindow
    {
        public float perfectTiming;  // The perfect timing for the action (e.g., the beat)
        public float windowSize;     // The size of the timing window in seconds

        public bool IsTimingPerfect(float actualTiming)
        {
            // Check if the actual timing falls within the timing window
            return Mathf.Abs(actualTiming - perfectTiming) <= windowSize / 2f;
        }
    }
}
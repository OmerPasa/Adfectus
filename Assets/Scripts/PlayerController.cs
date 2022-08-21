using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        // Public for external hooks
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool JumpingThisFrame { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public Vector3 RawMovement { get; private set; }
        public bool Grounded => _colDown;
        public bool _hasDashed;
        public float dashSpeed;
        private float dashTime;
        public float startDashTime;
        private int direction;
        public bool isFacingLeft;

        //Animation States
    const string PLAYER_IDLE = "Player_Idle";
    const string PLAYER_RUN = "Player_Run";
    const string PLAYER_JUMP = "Player_Jump";
    const string PLAYER_ATTACK = "Player_Attack";
    const string PLAYER_AIR_ATTACK = "Player_Jump";
    const string PLAYER_DEATH = "Player_Death";
    const string PLAYER_TAKEDAMAGE = "Player_TakeDamage";

    bool playerRunning , playerJumping,playerAttaking,playerTakingDamage,playerDying;
    
        private Animator animator;
        private Rigidbody2D rb2d;
        AudioSource AfterFiringMusic;
        public AudioSource BackGroundM;
        public SpriteRenderer sprite;
        public GameObject dashEffect;
        private string currentAnimaton;
        

        [SerializeField]
        private float attackDelay;
        private float jumpDelay = 0.7f;
        private float damageDelay = 2f;
        private float maxHealth = 1;
        [SerializeField]
        public static float Playerhealth = 1;
        private Vector3 IdleVelocity = new Vector3(0,0,0);
        public MainMenu mainMenu;
        private Vector3 _lastPosition;
        private float _currentHorizontalSpeed, _currentVerticalSpeed;

        void Start()
    {
        playerDying = false;
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        AfterFiringMusic = GetComponent<AudioSource>();
        BackGroundM = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        Playerhealth = maxHealth;
        dashTime = startDashTime;
        
    }
        // This is horrible, but for some reason colliders are not fully established when update starts...
        private bool _active;
        void Awake() => Invoke(nameof(Activate), 0.5f);
        void Activate() => _active = true;

        private void Update()
        {
            if (Playerhealth <= 0)
            {
                playerDying = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                ChangeAnimationState(PLAYER_DEATH);
                Invoke("Die", 3f);
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
        }


        #region Gather Input

        private void GatherInput()
        {

            Input = new FrameInput
            {
                JumpDown = UnityEngine.Input.GetButtonDown("Jump"),
                JumpUp = UnityEngine.Input.GetButtonUp("Jump"),
                X = UnityEngine.Input.GetAxisRaw("Horizontal")
            };
             //prevents further pushes and animation glitch.
            if (Input.JumpDown || Input.JumpUp)
            {
             playerJumping = true;   
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
                PlayerTakeDamage(0.1f);
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

        // We use these raycast checks for pre-collision information
        private void RunCollisionChecks()
        {
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

        [Header("WALKING")][SerializeField] private float _acceleration = 90;
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
                }else if (_currentHorizontalSpeed == 0.0f)
                {
                    ChangeAnimationState(PLAYER_IDLE);
                }
            }
        }

        #endregion

        #region Gravity

        [Header("GRAVITY")][SerializeField] private float _fallClamp = -40f;
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
            if (Input.JumpDown && CanUseCoyote && playerJumping|| HasBufferedJump)
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

        #endregion

        #region Dash
        private void HandleDashing()
        {
            if (direction == 0)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Q) && !_hasDashed)
                {
                    Instantiate(dashEffect, transform.position, Quaternion.identity);
                    direction = 1;
                }else if (UnityEngine.Input.GetKeyDown(KeyCode.E) && !_hasDashed)
                {
                    Instantiate(dashEffect, transform.position, Quaternion.identity);
                    direction = 2;
                }
            }else
            {
                if (dashTime <= 0)
                {
                    direction = 0;
                    dashTime = startDashTime;
                    rb2d.velocity = Vector2.zero;
                }else
                {
                    dashTime -= Time.deltaTime;

                    if (direction == 1)
                    {
                        rb2d.velocity = Vector2.left * dashSpeed;
                    }else if (direction == 2)
                    {
                        rb2d.velocity = Vector2.right * dashSpeed;
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

        private void MoveCharacter() {
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
        Debug.Log("damageTaken");
        ChangeAnimationState(PLAYER_TAKEDAMAGE);
        Debug.Log("ANİMATİON CHANGED TO TAKEDAMAGE!!!!!!!!");
        damageDelay = animator.GetCurrentAnimatorStateInfo(0).length;
        Invoke("DamageDelayComplete", damageDelay);////DLELETE 
    }

    // set new health to the sprite filter as a new color.
    void Set_Health(float Playerhealth)
    {
        sprite.color = new Color (Playerhealth, Playerhealth, Playerhealth, 1);
    }
    void DamageDelayComplete()
    {
        playerTakingDamage = false;
    }
    void OnCollisionEnter2D(Collision2D water) 
    {
        if (water.gameObject.tag == "Water")
        {
            Destroy(gameObject);
        }
    }
        #endregion

        void Jumploop()
        {
            Debug.Log("playerJumping");
            playerJumping = false;
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
}
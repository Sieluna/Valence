using UnityEngine;

// FPSWalkerEnhanced
// From Unify Community Wiki

// https://wiki.unity3d.com/index.php/FPSWalkerEnhanced#FPSWalkerEnhanced.cs

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("How fast the player moves when walking (default move speed).")]
    [SerializeField] private float walkSpeed = 6.0f;

    [Tooltip("How fast the player moves when running.")]
    [SerializeField] private float runSpeed = 11.0f;

    [Tooltip("If true, diagonal speed (when strafing + moving forward or back) can't exceed normal move speed; otherwise it's about 1.4 times faster.")]
    [SerializeField] public bool limitDiagonalSpeed = true;

    [Tooltip("If checked, the run key toggles between running and walking. Otherwise player runs if the key is held down.")]
    [SerializeField] private bool toggleRun = false;

    [Tooltip("How high the player jumps when hitting the jump button.")]
    [SerializeField] private float jumpSpeed = 8.0f;

    [Tooltip("How fast the player falls when not standing on anything.")]
    [SerializeField] private float gravity = 20.0f;

    [Tooltip("Units that player can fall before a falling function is run. To disable, type \"infinity\" in the inspector.")]
    [SerializeField] private float fallingThreshold = 10.0f;

    [Tooltip("If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down.")]
    [SerializeField] private bool slideWhenOverSlopeLimit = false;

    [Tooltip("If checked and the player is on an object tagged \"Slide\", he will slide down it regardless of the slope limit.")]
    [SerializeField] private bool slideOnTaggedObjects = false;

    [Tooltip("How fast the player slides when on slopes as defined above.")]
    [SerializeField] private float slideSpeed = 12.0f;

    [Tooltip("If checked, then the player can change direction while in the air.")]
    [SerializeField] private bool airControl = false;

    [Tooltip("Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast.")]
    [SerializeField] private float antiBumpFactor = .75f;

    [Tooltip("Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping.")]
    [SerializeField] private int antiBunnyHopFactor = 1;

    private Vector3 m_moveDirection = Vector3.zero;
    private bool m_grounded = false;
    private CharacterController m_controller;
    private Transform m_transform;
    private float m_speed;
    private RaycastHit m_hit;
    private float m_fallStartLevel;
    private bool m_falling;
    private float m_slideLimit;
    private float m_rayDistance;
    private Vector3 m_contactPoint;
    private bool m_playerControl = false;
    private int m_jumpTimer;

    private void Start()
    {
        // Saving component references to improve performance.
        m_transform = GetComponent<Transform>();
        m_controller = GetComponent<CharacterController>();

        // Setting initial values.
        m_speed = walkSpeed;
        m_rayDistance = m_controller.height * .5f + m_controller.radius;
        m_slideLimit = m_controller.slopeLimit - .1f;
        m_jumpTimer = antiBunnyHopFactor;
    }


    private void Update()
    {
        // If the run button is set to toggle, then switch between walk/run speed. (We use Update for this...
        // FixedUpdate is a poor place to use GetButtonDown, since it doesn't necessarily run every frame and can miss the event)

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        if (toggleRun && m_grounded && Input.GetButtonDown("Run"))
        {
            m_speed = (m_speed == walkSpeed ? runSpeed : walkSpeed);
        }

        // If both horizontal and vertical are used simultaneously, limit speed (if allowed), so the total doesn't exceed normal move speed
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed) ? .7071f : 1.0f;

        if (m_grounded)
        {
            bool sliding = false;
            // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
            // because that interferes with step climbing amongst other annoyances
            if (Physics.Raycast(m_transform.position, -Vector3.up, out m_hit, m_rayDistance))
            {
                if (Vector3.Angle(m_hit.normal, Vector3.up) > m_slideLimit)
                {
                    sliding = true;
                }
            }
            // However, just raycasting straight down from the center can fail when on steep slopes
            // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
            else
            {
                Physics.Raycast(m_contactPoint + Vector3.up, -Vector3.up, out m_hit);
                if (Vector3.Angle(m_hit.normal, Vector3.up) > m_slideLimit)
                {
                    sliding = true;
                }
            }

            // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
            if (m_falling)
            {
                m_falling = false;
                if (m_transform.position.y < m_fallStartLevel - fallingThreshold)
                {
                    OnFell(m_fallStartLevel - m_transform.position.y);
                }
            }

            // If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down
            if (!toggleRun)
            {
                m_speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            }

            // If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
            if ((sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && m_hit.collider.tag == "Slide"))
            {
                Vector3 hitNormal = m_hit.normal;
                m_moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                Vector3.OrthoNormalize(ref hitNormal, ref m_moveDirection);
                m_moveDirection *= slideSpeed;
                m_playerControl = false;
            }
            // Otherwise recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
            else
            {
                m_moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
                m_moveDirection = m_transform.TransformDirection(m_moveDirection) * m_speed;
                m_playerControl = true;
            }

            // Jump! But only if the jump button has been released and player has been grounded for a given number of frames
            if (!Input.GetButton("Jump"))
            {
                m_jumpTimer++;
            }
            else if (m_jumpTimer >= antiBunnyHopFactor)
            {
                m_moveDirection.y = jumpSpeed;
                m_jumpTimer = 0;
            }
        }
        else
        {
            // If we stepped over a cliff or something, set the height at which we started falling
            if (!m_falling)
            {
                m_falling = true;
                m_fallStartLevel = m_transform.position.y;
            }

            // If air control is allowed, check movement but don't touch the y component
            if (airControl && m_playerControl)
            {
                m_moveDirection.x = inputX * m_speed * inputModifyFactor;
                m_moveDirection.z = inputY * m_speed * inputModifyFactor;
                m_moveDirection = m_transform.TransformDirection(m_moveDirection);
            }
        }

        // Apply gravity
        m_moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller, and set grounded true or false depending on whether we're standing on something
        m_grounded = (m_controller.Move(m_moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }


    // Store point that we're in contact with for use in FixedUpdate if needed
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        m_contactPoint = hit.point;
    }


    // This is the place to apply things like fall damage. You can give the player hitpoints and remove some
    // of them based on the distance fallen, play sound effects, etc.
    private void OnFell(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
    }
}
using UnityEngine;
using UnityEngine.UI;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float velocity = 5f;
    public float sprintAdittion = 3.5f;
    public float jumpForce = 18f;
    public float jumpTime = 0.85f;
    [Space]
    public float gravity = 9.8f;
    public float crouch_speed = 0.5f;

    [Space]
    [Header("Crouch Physics")]
    public float standingHeight = 2.0f;
    public float crouchingHeight = 1.0f;
    public float standingCenterY = 1.0f;
    public float crouchingCenterY = 0.5f;

    [Space]
    [Header("Weapon")]
    public KeyCode armWeaponKey = KeyCode.F;
    public GameObject weaponModel;
    public HitscanGun playerGunScript;

    [Space]
    [Header("Aim Down Sights (ADS)")]
    public float defaultFOV = 60f;
    public float zoomedFOV = 40f;
    public float fovSmoothSpeed = 10f;

    [Space]
    [Header("UI")]
    public Image crosshairImage;

    [Space]
    [Header("Animation Smoothing")]
    [Tooltip("Animasyon geçişlerinin ne kadar yumuşak (yavaş) olacağı. Düşük = Yavaş, Yüksek = Hızlı.")]
    public float animationSmoothSpeed = 15f;

    float jumpElapsedTime = 0;

    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;
    bool isArmed = false;

    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    Animator animator;
    CharacterController cc;

    private float smoothedSpeed = 0f;
    private Camera mainCamera;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");

        cc.height = standingHeight;
        cc.center = new Vector3(cc.center.x, standingCenterY, cc.center.z);

        if (weaponModel != null)
        {
            weaponModel.SetActive(false);
        }

        if (crosshairImage != null)
        {
            crosshairImage.gameObject.SetActive(false);
        }

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = defaultFOV;
        }
    }

    void Update()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        if (Input.GetKeyDown(armWeaponKey))
        {
            isArmed = !isArmed;

            if (weaponModel != null)
            {
                weaponModel.SetActive(isArmed);
            }
        }

        if (isArmed && Input.GetButton("Fire1"))
        {
            if (playerGunScript != null)
            {
                playerGunScript.TryToShoot();
            }
        }

        if (inputCrouch)
        {
            if (isCrouching)
            {
                if (!CanStandUp())
                {
                    isCrouching = true;
                }
                else
                {
                    isCrouching = false;
                    cc.height = standingHeight;
                    cc.center = new Vector3(cc.center.x, standingCenterY, cc.center.z);
                }
            }
            else
            {
                isCrouching = true;
                cc.height = crouchingHeight;
                cc.center = new Vector3(cc.center.x, crouchingCenterY, cc.center.z);
                cc.Move(Vector3.down * 0.05f);
            }
        }

        if (cc.isGrounded && animator != null)
        {
            animator.SetBool("crouch", isCrouching);

            float minimumSpeed = 0.9f;
            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;

            float targetSpeed = 0f;

            if (cc.velocity.magnitude > 0.1f)
            {
                if (isCrouching)
                {
                    targetSpeed = 1.0f;
                }
                else
                {
                    targetSpeed = isSprinting ? 2.0f : 1.0f;
                }
            }

            smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, Time.deltaTime * animationSmoothSpeed);
            animator.SetFloat("Speed", smoothedSpeed);
        }

        if (animator != null)
        {
            animator.SetBool("air", cc.isGrounded == false);
            animator.SetBool("isArmed", isArmed);
        }

        if (inputJump && cc.isGrounded)
        {
            isJumping = true;
            if (isCrouching)
            {
                isCrouching = false;
                cc.height = standingHeight;
                cc.center = new Vector3(cc.center.x, standingCenterY, cc.center.z);
            }
        }

        HeadHittingDetect();
        HandleAiming();
    }

    private void HandleAiming()
    {
        if (mainCamera == null) return;

        bool isAiming = isArmed && (Input.GetMouseButton(1) || Input.GetMouseButtonDown(1));

        float targetFOV = isAiming ? zoomedFOV : defaultFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);

        if (crosshairImage != null)
        {
            bool showCrosshair = isArmed;
            crosshairImage.gameObject.SetActive(showCrosshair);
        }
    }

    private void FixedUpdate()
    {
        float velocityAdittion = 0;
        if (isSprinting)
            velocityAdittion = sprintAdittion;
        if (isCrouching)
            velocityAdittion = -(velocity * crouch_speed);

        float directionX = inputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
        float directionZ = inputVertical * (velocity + velocityAdittion) * Time.deltaTime;
        float directionY = 0;

        if (isJumping)
        {
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        directionY = directionY - gravity * Time.deltaTime;

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        forward = forward * directionZ;
        right = right * directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 moviment = verticalDirection + horizontalDirection;
        cc.Move(moviment);
    }

    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }

    bool CanStandUp()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = (standingHeight / 2f) * headHitDistance;

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            return false;
        }
        return true;
    }
}
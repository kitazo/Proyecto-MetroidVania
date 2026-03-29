using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerControllerComplete : MonoBehaviour
{
    [Header("Movimiento Horizontal")]
    [SerializeField] private float walkSpeed = 5f;
    
    [Header("Mecánica de Sprint / Dash")]
    [SerializeField] private float sprintSpeed = 13f; 
    [SerializeField] private float sprintDuration = 0.3f;
    [SerializeField] private int maxSprintCharges = 3;    
    
    [Header("Salto y Doble Salto")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int maxJumps = 2; 
    
    [Header("Mecánica de Paredes")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 15f); 
    [SerializeField] private float wallJumpDuration = 0.2f; 

    [Header("Detección")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    public Animator animator; 
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isSprinting; 
    private int currentSprintCharges;
    private int jumpsRemaining; 
    private bool isWallJumping;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSprintCharges = maxSprintCharges;
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        Inputs();
        CheckSurroundings();
        DetermineWallSlideState();
        UpdateAnimations(); 

        if (isGrounded && !isSprinting) currentSprintCharges = maxSprintCharges;

        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            jumpsRemaining = maxJumps;
            // Opcional: Resetear triggers al tocar el suelo
            if(animator != null) animator.ResetTrigger("DoubleJump");
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isWallSliding)
            {
                WallJump();
                jumpsRemaining = maxJumps - 1;
            }
            else if (jumpsRemaining > 0)
            {
                // LÓGICA DE DOS SALTOS
                if (jumpsRemaining == maxJumps) 
                {
                    // Primer Salto
                    if(animator != null) animator.SetTrigger("Jump");
                }
                else 
                {
                    // Segundo Salto
                    if(animator != null) animator.SetTrigger("DoubleJump");
                }

                Jump(jumpForce);
                jumpsRemaining--;
            }
        }

        if (Input.GetMouseButtonDown(0)) Attack();
    }

    void FixedUpdate() => Move();

    private void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetMouseButtonDown(1) && currentSprintCharges > 0 && !isSprinting && horizontalInput != 0)
            StartCoroutine(SprintRoutine());
        
        if (!isWallJumping)
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            else if (horizontalInput < 0 && isFacingRight) Flip();
        }
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer);
    }

    private void Move()
    {
        if (isWallJumping) return; 
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);

        if (isWallSliding && rb.linearVelocity.y < -wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
    }

    private void DetermineWallSlideState()
    {
        if (isTouchingWall && !isGrounded && horizontalInput != 0)
        {
            float directionToWall = isFacingRight ? 1 : -1;
            if (Mathf.Sign(horizontalInput) == directionToWall)
            {
                isWallSliding = true;
                return;
            }
        }
        isWallSliding = false;
    }

    private void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void WallJump()
    {
        isWallSliding = false;
        Flip(); 
        Vector2 forceToApply = new Vector2(wallJumpForce.x * (isFacingRight ? 1 : -1), wallJumpForce.y);
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(forceToApply, ForceMode2D.Impulse);
        StartCoroutine(WallJumpRoutine()); 
        if(animator != null) animator.SetTrigger("Jump");
    }

    private void Attack() { if (animator != null) animator.SetTrigger("Attack"); }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    private IEnumerator SprintRoutine()
    {
        isSprinting = true;
        currentSprintCharges--; 
        yield return new WaitForSeconds(sprintDuration);
        isSprinting = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("movement", Mathf.Abs(horizontalInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(groundCheck.position, checkRadius); }
        if (wallCheck != null) { Gizmos.color = Color.blue; Gizmos.DrawWireSphere(wallCheck.position, checkRadius); }
    }
}
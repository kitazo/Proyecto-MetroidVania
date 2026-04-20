using UnityEngine;

public class EnemyFollow : EnemyBase 
{
    [Header("Configuración de Seguimiento")]
    public Transform player;
    public float detectionRadius = 5f;
    public float stoppingDistance = 0.8f; 
    public float speed = 3f;
    public int contactDamage = 10;
    public float damageCooldown = 1f;   // segundos entre golpes
    private float lastDamageTime;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    
    private bool EnMovimiento; 
    private bool isFacingRight = false; 
    [Header("Detección de Borde")]
    public float edgeCheckDistance = 0.5f;   // qué tan adelante revisa
    public float groundCheckDepth = 0.3f;    // qué tan abajo lanza el rayo
    public LayerMask groundLayer;            // asigna el layer del suelo en el Inspector

    private bool isGroundAhead = true;

    private bool CheckGroundAhead(float directionX)
{
    Vector2 origin = new Vector2(
        transform.position.x + (directionX > 0 ? edgeCheckDistance : -edgeCheckDistance),
        transform.position.y
    );

    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDepth + 0.5f, groundLayer);
    Debug.DrawRay(origin, Vector2.down * (groundCheckDepth + 0.5f), hit ? Color.green : Color.red);

    return hit.collider != null;
}

    protected override void Start()
{
    base.Start(); 
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>(); 
    
    if (player == null)
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;

            // Ignora TODOS los colliders del enemigo contra el jugador
            Collider2D playerCol = p.GetComponent<Collider2D>();
            Collider2D[] myColliders = GetComponents<Collider2D>();

            if (playerCol != null)
            {
                foreach (Collider2D col in myColliders)
                    Physics2D.IgnoreCollision(col, playerCol, true);
            }
        }
    }
}

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius && distanceToPlayer > stoppingDistance)
{
    float directionX = player.position.x - transform.position.x;

    // Ahora revisa en la dirección REAL hacia el jugador, no hacia donde mira
    isGroundAhead = CheckGroundAhead(directionX);

    if (isGroundAhead)
    {
        movement = new Vector2(directionX, 0).normalized;
        EnMovimiento = true;

        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();
    }
    else
    {
        movement = Vector2.zero;
        EnMovimiento = false;
    }
}
else
{
    movement = Vector2.zero;
    EnMovimiento = false;
}

        if (animator != null) 
        {
            animator.SetBool("enMovimiento", EnMovimiento);
        }
    }

    void FixedUpdate()
    {
        if (EnMovimiento)
        {
            rb.linearVelocity = new Vector2(movement.x * speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Flip()
    {
        // Cambiamos el estado
        isFacingRight = !isFacingRight;

        // Multiplicamos la escala X por -1 para que el sprite se voltee
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }


    private void OnTriggerStay2D(Collider2D other)
{
    Debug.Log("Trigger: " + other.name + " | Tag: " + other.tag);

    if (Time.time < lastDamageTime + damageCooldown) return;

    if (other.CompareTag("Player"))
    {
        Debug.Log("Tag Player detectado");
        
        // Busca el script también en el padre por si el collider está en un hijo
        PlayerControllerComplete p = other.GetComponent<PlayerControllerComplete>();
        if (p == null) p = other.GetComponentInParent<PlayerControllerComplete>();
        
        Debug.Log("Script encontrado: " + (p != null));
        
        if (p != null)
        {
            p.TakeDamage(contactDamage, transform);
            lastDamageTime = Time.time;
        }
    }
}
}
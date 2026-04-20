using UnityEngine;

public class EnemyFollow : EnemyBase 
{
    [Header("Configuración de Seguimiento")]
    public Transform player;
    public float detectionRadius = 5f;
    public float stoppingDistance = 0.8f; 
    public float speed = 3f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    
    private bool EnMovimiento; 
    private bool isFacingRight = false; // Cambia a true si tu sprite original mira a la derecha

    protected override void Start()
    {
        base.Start(); 
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius && distanceToPlayer > stoppingDistance)
        {
            // Calculamos la dirección hacia el jugador
            float directionX = player.position.x - transform.position.x;
            movement = new Vector2(directionX, 0).normalized;
            
            EnMovimiento = true;

            // --- LÓGICA DEL FLIP ---
            // Si el jugador está a la derecha (positivo) y miramos a la izquierda, giramos.
            if (directionX > 0 && !isFacingRight)
            {
                Flip();
            }
            // Si el jugador está a la izquierda (negativo) y miramos a la derecha, giramos.
            else if (directionX < 0 && isFacingRight)
            {
                Flip();
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
}
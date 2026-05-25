using UnityEngine;
using Unity.Netcode;

public class EnemyFollow : EnemyBase 
{
    [Header("Configuración de Seguimiento")]
    public Transform player;
    public float detectionRadius = 7f; 
    public float stoppingDistance = 0.8f; 
    public float speed = 3f;

    [Header("IA: Detección de Borde")]
    public float edgeCheckDistance = 0.5f;   
    public float groundCheckDepth = 1.5f; 
    public LayerMask groundLayer;            

    private Vector2 movement;
    private bool EnMovimiento; 

    //Sincroniza la dirección de mirada en todos los clientes
    public NetworkVariable<bool> networkIsFacingRight = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> networkIsMoving = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkIsFacingRight.OnValueChanged += OnFacingRightChanged;

        networkIsMoving.OnValueChanged += (prev, newVal) =>
        {
            if (anim != null) anim.SetBool("enMovimiento", newVal);
        };
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        networkIsFacingRight.OnValueChanged -= OnFacingRightChanged;

        networkIsMoving.OnValueChanged -= (prev, newVal) =>
        {
            if (anim != null) anim.SetBool("enMovimiento", newVal);
        };
    }

    protected override void Start()
    {
        base.Start(); 
        
        if (IsServer && player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        //Toda la IA e inputs ocurren en el Servidor
        if (!IsServer) return;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) 
            {
                player = p.transform;
                Debug.Log($"[EnemyFollow] Jugador encontrado tarde: {p.name}");
            }
            //Si aún no hay jugador, esperamos al siguiente frame sin hacer nada más
            SetNetworkMoving(false);
            return;
        }

        if (isDead || isStunned) 
        {
            SetNetworkMoving(false);
            return; 
        }

        if (networkIsPossessed.Value)
        {
            //Sincroniza hacia dónde mira basado en cómo lo mueve el P2
            if (rb.linearVelocity.x > 0.1f && !networkIsFacingRight.Value) 
                networkIsFacingRight.Value = true;
            else if (rb.linearVelocity.x < -0.1f && networkIsFacingRight.Value) 
                networkIsFacingRight.Value = false;

            SetNetworkMoving(Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            float directionX = player.position.x - transform.position.x;
            if (directionX > 0 && !networkIsFacingRight.Value) 
                networkIsFacingRight.Value = true;
            else if (directionX < 0 && networkIsFacingRight.Value) 
                networkIsFacingRight.Value = false;

            //Movimiento solo si hay suelo adelante
            if (distanceToPlayer > stoppingDistance)
            {
                if (CheckGroundAhead(directionX))
                {
                    movement = new Vector2(directionX, 0).normalized;
                    EnMovimiento = true;
                }
                else
                {
                    EnMovimiento = false; 
                }
            }
            else 
            {
                EnMovimiento = false;
            }
        }
        else 
        {
            EnMovimiento = false;
        }

        SetNetworkMoving(EnMovimiento);
    }

    private void SetNetworkMoving(bool value)
    {
        if (networkIsMoving.Value != value)
            networkIsMoving.Value = value;
    }

    void FixedUpdate()
    {
        //La física la sigue calculando exclusivamente el servidor
        if (!IsServer) return;

        //Si está poseído, la física de movimiento horizontal la maneja EnemyBase mediante MoveAsPossessed()
        if (networkIsPossessed.Value) return;

        if (isDead || isStunned) return;

        if (EnMovimiento)
            rb.linearVelocity = new Vector2(movement.x * speed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private bool CheckGroundAhead(float dirX)
    {
        Vector2 origin = new Vector2(
            transform.position.x + (dirX > 0 ? edgeCheckDistance : -edgeCheckDistance),
            transform.position.y + 0.2f 
        );

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDepth, groundLayer);
        Debug.DrawRay(origin, Vector2.down * groundCheckDepth, hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }

    private void OnFacingRightChanged(bool previousValue, bool isRight)
    {
        Vector3 localScale = transform.localScale;
        localScale.x = isRight ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x); 
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //El daño por contacto solo lo aplica y calcula el Servidor
        if (!IsServer) return;

        if (isDead || isStunned) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControllerComplete p = collision.gameObject.GetComponent<PlayerControllerComplete>();
            if (p != null) 
            {
                p.TakeDamage(contactDamage, transform); 
            }
        }
    }
}
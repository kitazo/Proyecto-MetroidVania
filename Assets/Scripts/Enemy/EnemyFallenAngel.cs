using System.Collections;
using UnityEngine;
using Unity.Netcode; 

public class EnemyFlyingShooter : EnemyBase
{
    [Header("Referencias de Ataque")]
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Configuración de Vuelo")]
    public float detectionRange = 12f;   
    public float speed = 4f;            
    public float followDistance = 5f;    
    public float flapForce = 7f; //Fuerza con la que vuela hacia arriba al presionar Espacio

    [Header("Efecto de Flote Sinusoidal")]
    public float waveSpeed = 3f;        
    public float waveMagnitude = 1f;    

    [Header("Configuración de Disparo")]
    public float fireCooldown = 2f;     
    private float fireTimer;

    [Header("Efecto Visual de Daño")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;

    private bool isFacingRight = false;
    private float timeCounter;

    protected override void Start()
    {
        maxHealth = 30; 
        contactDamage = 10;    
        base.Start(); 

        if (rb != null) rb.gravityScale = 0f;

        if (IsServer && player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        fireTimer = fireCooldown;
    }

    //Al ser poseído, le damos gravedad para que el P2 tenga que mantenerlo volando con Espacio
    public override void SetPossessed(bool value)
    {
        base.SetPossessed(value);
        if (rb != null)
        {
            rb.gravityScale = value ? 1f : 0f;
            if (!value) rb.linearVelocity = Vector2.zero; //Frena la caída al soltarlo
        }
    }

    void Update()
    {
        if (!IsServer) return;

        //Disminuir timer de disparo aunque esté poseído
        if (fireTimer > 0f) fireTimer -= Time.deltaTime;

        if (networkIsPossessed.Value) 
        {
            //P2 Controla el vuelo: actualizamos hacia donde mira basado en su movimiento horizontal
            if (rb.linearVelocity.x > 0.1f && !isFacingRight) ManejarGiroMirada();
            else if (rb.linearVelocity.x < -0.1f && isFacingRight) ManejarGiroMirada();
            return;
        }

        if (isDead || isStunned || player == null)
        {
            if (rb != null && !isDead) rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ManejarGiroMiradaManual();
            ManejarMovimientoVolador();
            
            if (fireTimer <= 0f)
            {
                DispararProyectil();
                fireTimer = fireCooldown;
            }
        }
        else
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, Mathf.Sin(Time.time * waveSpeed) * waveMagnitude * 0.5f);
            }
        }
    }

    //Polimorfismo para disparar con Clic / J
    public override void AttackAsPossessed()
    {
        if (!IsServer || fireTimer > 0f) return;
        DispararProyectil();
        fireTimer = fireCooldown;
    }

    //Polimorfismo para volar hacia arriba (Aletear) con Espacio
    public override void SpecialActionAsPossessed()
    {
        if (!IsServer || rb == null) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); //Anulamos la caída para un salto limpio
        rb.AddForce(Vector2.up * flapForce, ForceMode2D.Impulse);
    }

    void ManejarMovimientoVolador()
    {
        Vector2 direccionAlPlayer = (player.position - transform.position).normalized;
        Vector2 posicionObjetivo = (Vector2)player.position - (direccionAlPlayer * followDistance);
        Vector2 movimientoBase = (posicionObjetivo - (Vector2)transform.position).normalized * speed;

        timeCounter += Time.deltaTime;
        float floteVertical = Mathf.Sin(timeCounter * waveSpeed) * waveMagnitude;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(movimientoBase.x, movimientoBase.y + floteVertical);
            if (anim != null) anim.SetBool("enMovimiento", rb.linearVelocity.magnitude > 0.2f);
        }
    }

    void DispararProyectil()
    {
        if (projectilePrefab == null || firePoint == null) return;

        TriggerAttackAnimClientRpc();

        //Si está poseído, dispara hacia donde mira, si no, dispara hacia el jugador
        Vector2 direccionDisparo;
        if (networkIsPossessed.Value)
        {
            direccionDisparo = isFacingRight ? Vector2.right : Vector2.left;
        }
        else
        {
            direccionDisparo = (player.position - firePoint.position).normalized;
        }

        float angulo = Mathf.Atan2(direccionDisparo.y, direccionDisparo.x) * Mathf.Rad2Deg;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angulo));
        projectile.GetComponent<NetworkObject>().Spawn();
    }

    [ClientRpc]
    private void TriggerAttackAnimClientRpc()
    {
        if (anim != null) anim.SetTrigger("Attack");
    }

    void ManejarGiroMiradaManual()
    {
        if (player.position.x > transform.position.x && !isFacingRight) ManejarGiroMirada();
        else if (player.position.x < transform.position.x && isFacingRight) ManejarGiroMirada();
    }

    private void ManejarGiroMirada()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }


    protected override void OnTakeDamageEffects(Vector3 sourcePosition)
    {
        base.OnTakeDamageEffects(sourcePosition);

        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(FlashRoutine));
            StartCoroutine(nameof(FlashRoutine));
        }
    }

    protected override void OnDieEffects()
    {
        base.OnDieEffects(); //Llama método normal

        this.enabled = false; 
        if (rb != null) rb.gravityScale = 1f;
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
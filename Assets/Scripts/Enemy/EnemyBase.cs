using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class EnemyBase : NetworkBehaviour 
{
    [Header("Estadísticas Base")]
    public int maxHealth = 100; 
    public float deathDelay = 1.5f; 
    public int contactDamage = 10;

    [Header("Ajustes de Combate")]
    public float knockbackForce = 7f;   
    public float stunDuration = 0.3f;  

    [Header("Recompensa de Tiempo")]
    public float reduccionDeTiempo = 10f; 
    
    protected Animator anim;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected bool isStunned;
    public bool isDead;

    public NetworkVariable<int> networkHealth = new NetworkVariable<int>(
        100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> networkIsPossessed = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    protected Color originalColor;

    public override void OnNetworkSpawn()
    {
        if (IsServer) 
        { 
            networkHealth.Value = maxHealth; 
            networkIsPossessed.Value = false; 
        }

        networkIsPossessed.OnValueChanged += OnPossessionChanged;
    }

    public override void OnNetworkDespawn() 
    { 
        networkIsPossessed.OnValueChanged -= OnPossessionChanged; 
    }

    protected virtual void Start() 
    {
        anim = GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>(); 
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) 
        {
            originalColor = spriteRenderer.color;
        }
    }

    public virtual void TakeDamage(int damage, Transform damageSource) 
    {
        if (!IsServer || isDead) 
        {
            return;
        }

        networkHealth.Value -= damage;

        if (networkHealth.Value <= 0) 
        {
            Die();
        }
        else 
        {
            Vector3 sourcePos = damageSource != null ? damageSource.position : transform.position;
            TakeDamageEffectsClientRpc(sourcePos);
            StopAllCoroutines(); 
            ApplyKnockback(damageSource);
            StartCoroutine(StunRoutine());
        }
    }

    private void ApplyKnockback(Transform source) 
    { 
        if (rb == null || source == null) 
        {
            return;
        }

        Vector2 direction = (transform.position - source.position).normalized; 
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(new Vector2(direction.x * knockbackForce, 3f), ForceMode2D.Impulse); 
    }

    private IEnumerator StunRoutine() 
    { 
        isStunned = true; 
        yield return new WaitForSeconds(stunDuration); 
        isStunned = false; 
    }

    protected virtual void Die() 
    {
        if (!IsServer || isDead) 
        {
            return;
        }

        isDead = true;

        if (networkIsPossessed.Value) 
        {
            networkIsPossessed.Value = false;
        }

        SyncTimePenaltyClientRpc(-reduccionDeTiempo);
        DieEffectsClientRpc();
    }

    [ClientRpc]
    private void SyncTimePenaltyClientRpc(float penalty)
    {
        if (GameManager.instance != null) 
        {
            GameManager.instance.ModificarTiempo(penalty);
        }
    }

    public virtual void SetPossessed(bool value) 
    { 
        if (!IsServer) 
        {
            return;
        }

        networkIsPossessed.Value = value; 
    }

    private void OnPossessionChanged(bool previousValue, bool newValue) 
    { 
        if (spriteRenderer != null) 
        {
            spriteRenderer.color = newValue ? new Color(1f, 0.5f, 1f) : originalColor;
        }
    }

    public virtual void MoveAsPossessed(float horizontal) 
    { 
        if (!networkIsPossessed.Value || rb == null || !IsServer) 
        {
            return;
        }

        rb.linearVelocity = new Vector2(horizontal * 3f, rb.linearVelocity.y); 
    }

    public virtual void AttackAsPossessed() { }
    
    public virtual void SpecialActionAsPossessed() { }


    [ClientRpc]
    protected virtual void TakeDamageEffectsClientRpc(Vector3 sourcePosition)
    {
        OnTakeDamageEffects(sourcePosition);
    }


    /// Lógica visual de recibir daño. Se Sobrescribe esto en las subclases
    protected virtual void OnTakeDamageEffects(Vector3 sourcePosition)
    {
        if (anim != null) 
        {
            anim.SetTrigger("Hurt"); 
        }
    }

    [ClientRpc]
    protected virtual void DieEffectsClientRpc()
    {
        OnDieEffects();
    }

    //Lógica visual de muerte. Se Sobrescribe esto en las subclases
    protected virtual void OnDieEffects()
    {
        isDead = true;

        if (anim != null) 
        {
            anim.SetTrigger("Die");
        }

        Collider2D col = GetComponent<Collider2D>(); 

        if (col != null) 
        {
            col.enabled = false;
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts) 
        { 
            if (script != this && !(script is NetworkBehaviour)) 
            {
                script.enabled = false; 
            }
        }

        if (rb != null) 
        { 
            rb.linearVelocity = Vector2.zero; 
            rb.gravityScale = 0f; 
        }

        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        if (spriteRenderer == null) 
        { 
            yield return new WaitForSeconds(deathDelay); 

            if (IsServer) 
            {
                GetComponent<NetworkObject>().Despawn(); 
            }

            yield break; 
        }

        float timer = 0f; 
        Color baseColor = spriteRenderer.color;

        while (timer < deathDelay) 
        { 
            timer += Time.deltaTime; 
            float alpha = Mathf.Lerp(1f, 0f, timer / deathDelay); 
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha); 
            yield return null; 
        }

        if (IsServer) 
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
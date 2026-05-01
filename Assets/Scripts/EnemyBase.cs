using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour 
{
    [Header("Estadísticas Base")]
    public int health = 100;
    public float deathDelay = 1.5f; 

    [Header("Ajustes de Combate")]
    public float knockbackForce = 7f;   
    public float stunDuration = 0.3f;  
    
    protected Animator anim;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected bool isStunned;
    protected bool isDead;

    protected virtual void Start() 
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void TakeDamage(int damage, Transform damageSource) 
    {
        if (isDead) return;

        health -= damage;
        if (anim != null) anim.SetTrigger("Hurt");

        if (health <= 0) 
        {
            Die();
        }
        else 
        {
            StopAllCoroutines(); 
            ApplyKnockback(damageSource);
            StartCoroutine(StunRoutine());
        }
    }

    private void ApplyKnockback(Transform source)
    {
        if (rb == null) return;
        Vector2 direction = (transform.position - source.position).normalized;
        rb.linearVelocity = Vector2.zero; 
        // Aplicamos fuerza en X y un pequeño salto en Y para despegarlo del suelo
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
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger("Die");

        // Desactivamos colisiones e IA para que no interfiera mientras muere
        GetComponent<Collider2D>().enabled = false;
        MonoBehaviour followScript = GetComponent<EnemyFollow>();
        if (followScript != null) followScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f; 
        }

        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float timer = 0;
        Color originalColor = spriteRenderer.color;

        while (timer < deathDelay)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / deathDelay);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
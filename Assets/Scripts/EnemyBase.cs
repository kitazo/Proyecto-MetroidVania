<<<<<<< HEAD
using System.Collections;
=======
>>>>>>> f52a03deb5e1221587606df02a48d62654c155a1
using UnityEngine;

public class EnemyBase : MonoBehaviour 
{
<<<<<<< HEAD
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
=======
    [Header("Estadisticas Base")]
    public int health = 100;
    public float deathDelay = 1.0f; // Tiempo que dura la animación de muerte antes de borrar el objeto

    protected Animator anim; // Referencia protegida para que los hijos (como EnemyFollow) la usen

    protected virtual void Start() 
    {
        // Buscamos el Animator en el objeto
        anim = GetComponent<Animator>();
    }

    public virtual void TakeDamage(int damage) 
    {
        health -= damage;
        Debug.Log("Vida del enemigo: " + health);

        if (anim != null)
        {
            anim.SetTrigger("Hurt"); 
        }
>>>>>>> f52a03deb5e1221587606df02a48d62654c155a1

        if (health <= 0) 
        {
            Die();
        }
<<<<<<< HEAD
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
=======
    }

    protected virtual void Die() 
{
    Debug.Log("Enemigo muerto");

    if (anim != null)
        anim.SetTrigger("Die");

    Collider2D col = GetComponent<Collider2D>();
    if (col != null) col.enabled = false;

    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f; // ← Evita que "caiga" durante la animación
    }

    Destroy(gameObject, deathDelay);
}
>>>>>>> f52a03deb5e1221587606df02a48d62654c155a1
}
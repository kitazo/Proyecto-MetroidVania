using UnityEngine;

public class EnemyBase : MonoBehaviour 
{
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

        if (health <= 0) 
        {
            Die();
        }
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
}
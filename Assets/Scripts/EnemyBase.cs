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

        // Si tienes una animación de "Recibir Golpe", actívala aquí
        if (anim != null)
        {
            anim.SetTrigger("Hurt"); // Crea un Trigger llamado "Hurt" en tu Animator
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
        {
            // Activamos la animación de muerte
            anim.SetTrigger("Die"); // Crea un Trigger llamado "Die" en tu Animator
        }

        // DESACTIVAR FÍSICAS: Muy importante para que el cadáver no te bloquee el paso
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero; // Detenerlo en seco

        // Destruir el objeto después del tiempo de la animación
        Destroy(gameObject, deathDelay);
    }
}
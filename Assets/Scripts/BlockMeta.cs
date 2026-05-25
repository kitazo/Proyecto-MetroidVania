using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MetaBloqueada : MonoBehaviour
{
    [Header("Requisito para desbloquear")]
    [Tooltip("Arrastra desde la Jerarquía el Jefe que debe morir para abrir esta meta")]
    public GameObject bossRequerido;

    private Collider2D metaCollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        metaCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //Al asignar un jefe bloqueamos la meta hasta que el jugador lo derrote
        if (bossRequerido != null)
        {
            metaCollider.enabled = false;
            
            //Hace que la meta se vea semitransparente para indicar que está bloqueada
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f); 
            }
        }
    }

    void Update()
    {
        if (bossRequerido == null)
        {
            DesbloquearMeta();
        }
    }

    private void DesbloquearMeta()
    {
        //Activa la colisión
        if (metaCollider != null) metaCollider.enabled = true;
        
        //Restaura el color original
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        this.enabled = false;
    }
}
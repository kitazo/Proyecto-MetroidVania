using UnityEngine;
using TMPro;

public class LevelGoal : MonoBehaviour
{
    [Header("Requisito para ganar")]
    public GameObject bossRequerido;

    [Header("Mensaje de Bloqueo")]
    public TextMeshProUGUI textoAviso;
    public string mensaje = "¡Debes derrotar al jefe para pasar!";

    private SpriteRenderer spriteRenderer;
    private bool estaDesbloqueada = false;
    private bool hasTriggered = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (textoAviso != null)
            textoAviso.gameObject.SetActive(false);

        if (bossRequerido == null)
            DesbloquearMeta();
        else if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
    }

    void Update()
    {
        if (!estaDesbloqueada && bossRequerido == null)
            DesbloquearMeta();
    }

    private void DesbloquearMeta()
    {
        estaDesbloqueada = true;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        if (textoAviso != null) textoAviso.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        FileLogger.Write($"[LevelGoal] Colisión con Player. estaDesbloqueada={estaDesbloqueada} hasTriggered={hasTriggered}");

        if (estaDesbloqueada)
        {
            if (!hasTriggered)
            {
                hasTriggered = true;
                PlayerControllerComplete player = collision.GetComponent<PlayerControllerComplete>();

                FileLogger.Write($"[LevelGoal] player={player != null} IsOwner={player?.IsOwner}");

                if (player != null && player.IsOwner)
                {
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = Vector2.zero;

                    FileLogger.Write($"[LevelGoal] GameManager.instance={GameManager.instance != null}");
                    FileLogger.Write($"[LevelGoal] ElapsedTime={GameManager.instance?.ElapsedTime}");

                    if (GameManager.instance != null)
                    {
                        FileLogger.Write("[LevelGoal] Llamando ReachGoalServerRpc...");
                        player.ReachGoalServerRpc(GameManager.instance.ElapsedTime);
                        FileLogger.Write("[LevelGoal] ReachGoalServerRpc enviado OK.");
                    }
                }
            }
        }
        else
        {
            if (textoAviso != null)
            {
                textoAviso.text = mensaje;
                textoAviso.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            if (textoAviso != null)
                textoAviso.gameObject.SetActive(false);
    }
}
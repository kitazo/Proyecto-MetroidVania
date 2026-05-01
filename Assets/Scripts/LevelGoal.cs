using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Le avisa al GameManager que el jugador llegó a la meta
            if (GameManager.instance != null)
            {
                GameManager.instance.WinLevel();
            }
        }
    }
}
using UnityEngine;

public class TrapBase : MonoBehaviour
{
    public int damage = 20;
    public float cooldown = 1.5f;
    private float lastTrigger;

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (Time.time < lastTrigger + cooldown) return;

        lastTrigger = Time.time;
        var p = col.GetComponent<PlayerControllerComplete>();
        if (p != null) p.TakeDamage(damage, transform);
    }
}
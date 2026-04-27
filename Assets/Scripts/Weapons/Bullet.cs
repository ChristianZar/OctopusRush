using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;
    public int damage = 20;

    private Coroutine lifeRoutine;
    private bool returned;

    void OnEnable()
    {
        returned = false;
        if (lifeRoutine != null) StopCoroutine(lifeRoutine);
        lifeRoutine = StartCoroutine(LifeTimer());
    }

    IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifeTime);
        lifeRoutine = null;
        ReturnSafely();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        SharkHealth sharkHealth = other.GetComponentInParent<SharkHealth>();
        if (sharkHealth != null)
        {
            sharkHealth.TakeDamage(damage, transform.position);
            ReturnSafely();
            return;
        }

        if (!other.isTrigger)
            ReturnSafely();
    }

    void ReturnSafely()
    {
        if (returned) return;
        returned = true;
        GetComponent<PoolReturn>()?.ReturnToPool();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDamageScript : MonoBehaviour
{
    public float fieldOfImpact;
    public float force;
    public LayerMask layertoHit;

    void Start()
    {
        Explode();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        float damageToPlayer = PlayerPrefs.GetFloat("damageToPlayer");
        TarodevController.PlayerController playerController = other.GetComponent<TarodevController.PlayerController>();
        if (playerController != null)
        {
            playerController.PlayerTakeDamage(damageToPlayer); // is not getting called
        }
    }

    private void Explode()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, fieldOfImpact, layertoHit);

        foreach (Collider2D obj in objects)
        {
            Vector2 direction = (obj.transform.position - transform.position).normalized;

            obj.GetComponent<Rigidbody2D>().AddForce(direction * force);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fieldOfImpact);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDamageScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        float damageToPlayer = PlayerPrefs.GetFloat("damageToPlayer");
        TarodevController.PlayerController playerController = other.GetComponent<TarodevController.PlayerController>();
        if (playerController != null)
        {
            playerController.PlayerTakeDamage(damageToPlayer); // is not getting called
        }
    }
}

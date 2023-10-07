using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LASER_Gun : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public EdgeCollider2D edgeCollider;
    public float laserSpeed;
    public Vector3 playerDirection;
    public Vector3 distance_lasertoplayerLaser;
    public void Update()
    {
        //transform.Translate(distance_lasertoplayerLaser * laserSpeed * Time.deltaTime);
        transform.position += distance_lasertoplayerLaser * (laserSpeed * Time.deltaTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }


}

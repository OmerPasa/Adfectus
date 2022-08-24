using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainScript : MonoBehaviour
{
    public Transform[]waypoints;
    public Transform target;
    Rigidbody2D rb2d;
    Vector2 moveDirection;
    public int moveSpeed;
    private int waypointIndex;
    private float dist;
    public bool Chasing;
    private void Awake() {
    rb2d = GetComponent<Rigidbody2D>();    
    }
    void Start()
    {
        target = GameObject.Find("Player").transform;
        waypointIndex= 0;
    }

    void Update()
    {
        if (target && Chasing)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb2d.rotation = angle;
            moveDirection = direction;
        }else
        {
            Patrol();
        }
        dist = Vector2.Distance(transform.position, waypoints[waypointIndex].position);
        if (dist < 1f)
        {
            IncreaseIndex();
        }
    }

    private void FixedUpdate() {
        if (target && Chasing)
        {
            rb2d.velocity = new Vector2(moveDirection.x , moveDirection.y) * moveSpeed;
        }
    }
    void Patrol()
    {
        Chasing = false;
        transform.position = Vector2.MoveTowards(transform.position, waypoints[waypointIndex].position, moveSpeed * Time.deltaTime);
        //transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }
    void IncreaseIndex()
    {
        waypointIndex++;
        if (waypointIndex >= waypoints.Length)
        {
            waypointIndex = 0;
        }
    }
}

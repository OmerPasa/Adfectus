using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooth : MonoBehaviour
{
    public Vector3 directionVec = Vector3.zero;

    Transform target;
    float speed;

    public int mode = 1;
    //0 stopped tooth
    //1 tooth firing
    //2 tooth ended

    // Start is called before the first frame update
    void Start()
    {
        speed = GetComponent<Attack>().data.f1;
        /*if (GetComponent<Attack>().data.b1){}*/
        target = LoopData.player.transform;
        directionVec = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, directionVec);
    }

    // Update is called once per frame
    void Update()
    {
        switch (mode)
        {
            case 0:
                break;
            case 1:
                transform.position += directionVec * speed * multiplier() * Time.deltaTime;
                break;
            case 2:
                Destroy(gameObject);
                break;
        }
    }

    float multiplier()
    {
        float dist = (LoopData.boss.transform.position - transform.position).magnitude;
        //return ((dist + 0.8f) / dist);

        float limit = 5;
        if (dist > limit)
        {
            return 1;
        }

        return ((5 - dist) * (5 - dist) / 20) + 1;
    }
}

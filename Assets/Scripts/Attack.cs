using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public AttackData data;

    void Update()
    {
        if (data == null)
        {
            return;
        }

        if (data.duration.CounterEQ == TimeB.Counter_Q)
        {
            endAttack();
        }
    }

    public void endAttack()
    {
        switch (data.type)
        {
            case -1:
                //GetComponent<BossMainScript>().Chasing = false;
                break;
            case 1:
                GetComponent<LaserT0>().mode = 2;
                //bosMainScr.currentAttackData = this;
                break;
            case 2:
                GetComponent<LaserT1>().mode = 2;
                break;

            default:
                Destroy(gameObject);
                break;
        }
    }

}

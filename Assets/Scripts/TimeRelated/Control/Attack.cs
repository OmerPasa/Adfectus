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

        if (data.duration.CounterEQ == TimeB.CounterQ)
        {
            endAttack();
        }
    }

    public void endAttack()
    {
        switch (data.type)
        {
            case -5:
                break;
            case -4:
                break;
            case -3:
                break;
            case -2:
                GetComponent<BossMainScript>().BossCollider.enabled = false;
                LoopData.boss.GetComponent<BossMainScript>().Weakened = false;
                break;
            case -1:
                GetComponent<BossMainScript>().Chasing = false;
                break;
            case 1:
                GetComponent<LaserT0>().mode = 2;
                //bosMainScr.currentAttackData = this;
                break;
            case 2:
                GetComponent<LaserT1>().mode = 2;
                break;
            case 3:
                break;

            default:
                Destroy(gameObject);
                break;
        }
    }

}

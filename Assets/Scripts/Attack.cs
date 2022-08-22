using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int type;
    public int beat;
    public int beatQuarter;

    public int startBeatQuarter;
    public int duration;


    public int handData1;
    public GameObject handParcasiData1;


    bool sent = false;


    void Update()
    {
        /*if (Zaman.beat == eklemBeat[index] && Zaman.beatQuarter == eklemBeatQuarter[index])
        {

        }*/



        if (startBeatQuarter + duration == Zaman.beatQuarterCounter && !sent)
        {
            endAttack();
            sent = true;
        }
    }

    public void endAttack()
    {
        switch (type)
        {
            case 1:
                GetComponent<LaserT0>().mode = 2;
                //bosMainScr.currentAttackData = this;
                break;
            case 3:
                GetComponent<BossMainScript>().Chasing = true;
                break;

            default:
                Destroy(gameObject);
                break;
        }
    }

}

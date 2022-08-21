using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int type;
    public int beat;
    public int beatQuarter;
    public int beatQQuarter;

    public int startBeatQuarter;
    public int duration;

    bool sent = false;


    void Update()
    {
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
                break;
            case 3:
                //GetComponent<el>().eklemSayisi = 4;
                break;

            default:
                Destroy(gameObject);
                break;
        }
    }

}

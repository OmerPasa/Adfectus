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

    void Update()
    {
        if (startBeatQuarter + duration == Zaman.beatQuarterCounter)
        {
            endAttack();
        }
    }

    public void endAttack()
    {
        switch (type)
        {
            case 1:
                GetComponent<Laser>().mode = 2;
                break;

            default:
                Destroy(gameObject);
                break;
        }
    }

}

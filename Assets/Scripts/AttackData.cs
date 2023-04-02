using System;
using UnityEngine;

public class AttackData
{
    static string[] prefabPaths = { "Prefabs/Null",
    "Prefabs/LaserType0",
    "Prefabs/LaserType1",
    "Prefabs/Tooth",
    "Prefabs/LaserType2",
    "Prefabs/LaserType3",
    "Prefabs/LaserType4",
     "Prefabs/LaserType5" };
    // (-1 = Chase, -2 = Weakened, -3 = objective pass, -4 = fade in, -5 = fade out)

    public int type;
    public int extraAttackCount;

    public Duration duration; //list of different objects with different start durations

    public float f1; public float f2; public float f3;
    public bool b1;

    public GameObject g1;

    public AttackData(AttackData data, Duration dur)
    {
        type = data.type; duration = dur;
        f1 = data.f1; f2 = data.f2; f3 = data.f3;
        b1 = data.b1;
        g1 = data.g1;
    }

    public AttackData
    (int type, int W, int Q, int duration,
        int extraAttackCount = 0,
        float f1 = -1f, float f2 = -1f, float f3 = -1f,
        bool b1 = false,
        GameObject g1 = null
    )
    {
        this.type = type;
        this.extraAttackCount = extraAttackCount;

        this.duration = new Duration(W, Q, duration); //base duration for attackdata calculations on boss

        this.f1 = f1;
        this.f2 = f2;
        this.f3 = f3;
        this.b1 = b1;

        this.g1 = g1;

    }

    public void action(Vector3 pos, int counter_Q)
    {
        if (type == 0)
        {
            return;
        }

        int[] wAndQ = Duration.durToTime(counter_Q);
        AttackData clone = new AttackData(this, new Duration(wAndQ[0], wAndQ[1], duration.duration));

        if (type > 0)
        {
            create(clone, pos, counter_Q);
            return;
        }

        switch (type)
        {
            case -1:
                LoopData.boss.GetComponent<Attack>().data = clone;
                LoopData.boss.GetComponent<BossMainScript>().Chasing = true;
                break;
            case -2:
                LoopData.boss.GetComponent<Attack>().data = clone;
                Debug.Log("Case2");
                LoopData.boss.GetComponent<BossMainScript>().Weakened = true;
                LoopData.boss.GetComponent<BossMainScript>().BossCollider.enabled = false;
                break;
            case -3:
                LoopController.isObjectiveCompleted = true;
                break;
            case -4:
                LoopData.boss.GetComponent<BossMainScript>().fadeIn(clone);
                break;
            case -5:
                LoopData.boss.GetComponent<BossMainScript>().fadeOut(clone);
                break;
            case -16:
                LoopData.player.GetComponent<TarodevController.PlayerController>().setExactHitTime(counter_Q + duration.duration);
                //bundan emin deilim
                break;
        }

    }

    void denemekIcinKod(AttackData clone)
    {
        LoopData.player.GetComponent<TarodevController.PlayerController>().setExactHitTime(clone.duration.CounterEQ);

    }

    public void create(AttackData clone, Vector3 position, int startBeatQ)
    {
        denemekIcinKod(clone); //sonradan silinmeli, gerçeği yansıtmamaktadır

        GameObject go = GameObject.Instantiate(Resources.Load(prefabPaths[type]), position, Quaternion.identity) as GameObject;
        //Debug.Log("c. " + clone.duration.CounterSQ + " " + clone.duration.CounterEQ + " ");
        go.GetComponent<Attack>().data = clone;
        Debug.Log("clone number " + clone.type);
        LoopData.boss.GetComponent<BossMainScript>().attackAnim(clone);
        Debug.Log("cloning");

        //replaced with duration.counterSQ
        //go.GetComponent<Attack>().startQ = startBeatQ;

        LoopData.currentAttacks.Add(go);
    }
}
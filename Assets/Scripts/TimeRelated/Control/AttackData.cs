using System;
using UnityEngine;
using TarodevController;
using UnityEngine.SceneManagement;
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
    // -1 = Chase, -2 = Weakened, -3 = objective pass, -4 = fade in, -5 = fade out,
    // -9 nothing
    // -10 = human boss state change

    public int type;
    public int extraAttackCount;

    public Duration duration; //list of different objects with different start durations

    public float f1, f2, f3;
    public bool b1;

    public GameObject g1;
    public HumanBossBaseState s1;

    public AttackData(AttackData data, Duration dur)
    {
        type = data.type; duration = dur;
        f1 = data.f1; f2 = data.f2; f3 = data.f3;
        b1 = data.b1;
        g1 = data.g1;
        s1 = data.s1;
    }

    public AttackData
    (int type, int W, int Q, int duration,
        int extraAttackCount = 0,
        float f1 = -1f, float f2 = -1f, float f3 = -1f,
        bool b1 = false,
        GameObject g1 = null, HumanBossBaseState s1 = null
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
        this.s1 = s1;

    }


    public void action(Vector3 pos, int counter_Q)
    {
        Debug.Log("action " + type + " " + counter_Q);
        if (type == 0)
        {
            return;
        }

        int[] wAndQ = Duration.durToTime(counter_Q);
        AttackData clone = new AttackData(this, new Duration(wAndQ[0], wAndQ[1], duration.duration));

        if (SceneManager.GetActiveScene().name == "BossDamat")
        {
            HumanBossController_Damat hbc = LoopData.boss.GetComponent<HumanBossController_Damat>();
            hbc.HumanBossAttackInitiater();
        }
        else if (SceneManager.GetActiveScene().name == "BossKeko")
        {
            HumanBossController_Keko hbc_D = LoopData.boss.GetComponent<HumanBossController_Keko>();
            hbc_D.HumanBossAttackInitiater();
        }
        PlayerController pc = LoopData.player.GetComponent<PlayerController>();
        pc.BeatPress();
        if (clone.s1 != null)
        {
            stateSenderForHumanBossController(clone);
        }

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

            case -9:
                break;

            case -10:
                stateSenderForHumanBossController(clone);
                break;

            case -16:
                LoopData.player.GetComponent<TarodevController.PlayerController>().setExactHitTime(counter_Q + duration.duration);
                //bundan emin deilim
                break;
        }

    }

    void stateSenderForHumanBossController(AttackData clone)
    {
        //LoopData.boss.GetComponent<Attack>().data = clone;
        LoopData.boss.GetComponent<HumanBossController_Damat>().SwitchState(clone.s1);
    }

    void denemekIcinKod(AttackData clone)
    {
        LoopData.player.GetComponent<TarodevController.PlayerController>().setExactHitTime(clone.duration.CounterEQ);

    }

    public void create(AttackData clone, Vector3 position, int startBeatQ)
    {

        denemekIcinKod(clone); //sonradan silinmeli, gerçeği yansıtmamaktadır

        //GameObject go = GameObject.Instantiate(Resources.Load(prefabPaths[type]), position, Quaternion.identity) as GameObject;
        //Debug.Log("c. " + clone.duration.CounterSQ + " " + clone.duration.CounterEQ + " ");
        //go.GetComponent<Attack>().data = clone;
        //Debug.Log("clone number " + clone.type);

        if (TimeDriver.part == 0) // this must be changed, we can not set animations for each part manually. Or we must?
        {
            LoopData.boss.GetComponent<BossMainScript>().attackAnim(clone);
        }
        //Debug.Log("cloning");

        //replaced with duration.counterSQ
        //go.GetComponent<Attack>().startQ = startBeatQ;

        //LoopData.currentAttacks.Add(go);

    }
}
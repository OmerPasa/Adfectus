using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAndBeat : MonoBehaviour
//boss scripti
//atakları doğru TimeBda spawn eder
{
    public GameObject player;
    public GameObject boss;
    public AudioSource audioSource;

    public int currentLoopPos = 0;
    public int currentAttackPos = 0;
    public int durStart = 0;

    int prevBeatQC = -1;
    bool active = false;


    void Start()
    {
        StartCoroutine(StartAttack());
    }

    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(3);

        LoopData.player = player;
        LoopData.boss = boss;
        audioSource.Play();
        active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
        {
            return;
        }

        TimeB.tick(Time.deltaTime * 1000);
        if (TimeB.Counter_Q == prevBeatQC)
        {//eğer Qbeat değişmediyse kontrol etmeye gerek yok
            return;
        }

        int patInd = LoopController.currentLoop[currentLoopPos, 0]; //pattern index
        int dur = LoopController.currentLoop[currentLoopPos, 1] + durStart; //döngü süresi
        AttackData currentAttackData = LoopData.patterns[patInd][currentAttackPos]; //atak datası

        if (TimeB.W % dur == currentAttackData.duration.beatSW && TimeB.Q == currentAttackData.duration.beatSQ)
        { //atak TimeBı geldiyse
            currentAttackData.action(transform.position, TimeB.Counter_Q); //atak oluştur
            currentAttackPos++;
        }


        if (currentAttackPos >= LoopData.patterns[patInd].Length)
        {//atakPos kontrolü
            currentAttackPos = 0;
            currentLoopPos++;

            if (currentLoopPos >= LoopController.currentLoop.GetLength(0))
            {//loopPos kontrolü
                currentLoopPos = 0;
            }
        }

        prevBeatQC = TimeB.Counter_Q;

    }

    public AttackData getNextAttack()
    {
        int patInd = LoopController.currentLoop[currentLoopPos, 0]; //pattern index
        //int dur = currentLoop[currentLoopPos, 1] + durStart; //loop duration

        int nextAttackPos = currentAttackPos + 1;

        if (nextAttackPos < LoopData.patterns[patInd].Length)
        {
            return LoopData.patterns[patInd][nextAttackPos];
        }

        nextAttackPos = 0;

        if (currentLoopPos + 1 < LoopController.currentLoop.GetLength(0))
        {
            return LoopData.patterns[LoopController.currentLoop[currentLoopPos + 1, 0]][0];
        }

        return LoopData.patterns[LoopController.currentLoop[0, 0]][0];



    }
}



public class Duration
{
    public int beatSW = 0;
    public int beatSQ = 0;
    public int CounterSQ = 0;

    public int duration = 0;

    public int beatEW = 0;
    public int beatEQ = 0;
    public int CounterEQ = 0;

    public Duration(int beat_W, int beat_Q, int duration)
    {
        this.beatSW = beat_W;
        this.beatSQ = beat_Q;
        this.CounterSQ = getBeatC(beat_W, beat_Q);

        this.duration = duration;

        int[] endTime = addDurationToTime(beat_W, beat_Q, duration);
        this.beatEW = endTime[0];
        this.beatEQ = endTime[1];
        this.CounterEQ = getBeatC(endTime[0], endTime[1]);

    }

    public float getDurInSeconds()
    {
        return duration * TimeB.quarterBeatDuration;
    }

    //example: (W: 3, Q: 1) -> dur: 13
    public static int getBeatC(int W, int Q)
    {
        return W * 4 + Q;
    }

    // example: dur:7 -> [W:1, Q:3]
    public static int[] durToTime(int dur)
    {
        int[] time = new int[2];
        time[0] = dur / 4;
        time[1] = dur % 4;
        return time;
    }

    //example: (W:2, Q:1, dur:5) -> (W:3, Q:2)
    public static int[] addDurationToTime(int add1W, int add1Q, int dur)
    {
        int[] add2 = durToTime(dur);
        int[] total = new int[3];

        int tempQ = add1Q + add2[1];
        int tempW = add1W + add2[0];
        if (tempQ >= 4)
        {
            tempQ -= 4;
            tempW++;
        }
        total[0] = tempW;
        total[1] = tempQ;

        return total;
    }
}




public static class TimeB
{
    public static float timePassed = 0;
    public static float bpm = 112.0f;

    public static int W = 0;
    public static int Q = 0;
    public static int Counter_Q = 0;

    static float beatDuration = 60000 / bpm;
    public static float quarterBeatDuration = beatDuration / 4;

    static bool isFirstFrame = true;

    public static void reset()
    {
        timePassed = 0;
        W = 0;
        Q = 0;
        Counter_Q = 0;
        isFirstFrame = true;
    }

    public static void baslat(float time)
    {
        timePassed += time;
    }

    public static void tick(float time)
    {
        if (isFirstFrame)
        {
            isFirstFrame = false;
            return;
        }
        timePassed += time;
        W = (int)(timePassed / beatDuration);
        Q = (int)(timePassed / quarterBeatDuration) % 4;

        Counter_Q = W * 4 + Q; //(int)(timePassed / quarterBeatDuration);
    }

    public static bool isBeatReached(int bQC)
    {
        return Counter_Q >= bQC;
    }

}

public static class LoopController
{
    public static int[,] currentLoop = LoopData.loops[0];



}

//data about loops, player, boss etc
public static class LoopData
{
    public static GameObject player;
    public static GameObject boss;
    public static List<GameObject> currentAttacks = new List<GameObject>();


    public static int[][,] loops = {
    //patternIndex, duration
        new int[,] {
            { 0, 4 },
            { 1, 4 }
        },
        new int[,] {
            { 0, 4 },
            { 1, 4 },
            { 1, 4 }
        },
        new int[,] {
            { 4, 4 },
            { 0, 4 }
        },
        new int[,] {
            { 1, 4 },
            { 4, 4 },
            { 3, 4 },
            { 4, 4 },
            { 1, 4 },
            { 4, 4 },
            { 5, 4 },
            { 5, 4 }
        },
        new int[,] {
            { 1, 4 },
            { 4, 4 },
            { 3, 4 },
            { 4, 4 },
            { 1, 4 },
            { 4, 4 },
            { 5, 4 },
            { 5, 4 }
        }
    };

    //AttactType, W, Q, other params...
    public static AttackData[][] patterns =
    {   //0
        new AttackData[] {
            new AttackData(1, 0, 0, 1),
            new AttackData(1, 1, 0, 1),
            new AttackData(1, 2, 0, 1),
            new AttackData(1, 3, 0, 1)
        },//1
        new AttackData[] { //5-6
            new AttackData(1, 1, 0, 2),
            new AttackData(2, 2, 0, 4, f1: 270, f2: 240),
            new AttackData(2, 3, 0, 4, f1: 270, f2: 300),
        },//2
        new AttackData[] { //7-8
            new AttackData(1, 1, 0, 2),
            new AttackData(1, 1, 3, 2),
            new AttackData(1, 2, 2, 2),
            new AttackData(1, 3, 1, 2),
        },//3
        new AttackData[] { //7-8 alternative
            new AttackData(1, 0, 3, 2),
            new AttackData(2, 1, 2, 4, f1: 270, f2: 240),
            new AttackData(1, 2, 1, 2),
            new AttackData(2, 3, 1, 4, f1: 270, f2: 300),
        },//4
        new AttackData[] { //6-7
            new AttackData(0, 3, 0, 4), //empty
        },//5
        new AttackData[] { //11-12 unkown
            new AttackData(2, 1, 0, 4, f1: 270, f2: 240),
            new AttackData(2, 2, 2, 4, f1: 270, f2: 300),
            new AttackData(-1, 2, 2, 4, f1: 4),
            //new AttackData(3, 2, 2, 3, handData1:4),
            //new AttackData(4, 2, 2, 3, handPartData1:GetGameObject(3)),
            //new AttackData(4, 3, 2, 3, handPartData1:GetGameObject(3)),
        }
    };


    public static GameObject GetGameObject(int type)
    {
        foreach (GameObject go in LoopData.currentAttacks)
        {
            if (go.GetComponent<Attack>().data.type == type)
            {
                return go;
            }
        }

        return null;
    }
}

public class AttackData
{
    static string[] prefabPaths = { "Prefabs/Null", "Prefabs/LaserType0", "Prefabs/LaserType1" };
    // (-1 = Chase)

    public int type;
    //public int W;
    //public int Q;
    public Duration duration; //list of different objects with different start durations

    public float f1; public float f2;
    public bool b1;

    public GameObject g1;

    public AttackData(AttackData data, Duration dur)
    {
        type = data.type; duration = dur;
        f1 = data.f1; f2 = data.f2;
        b1 = data.b1;
        g1 = data.g1;
    }

    public AttackData
    (int type, int W, int Q, int duration,
        float f1 = -1f, float f2 = -1f,
        bool b1 = false,
        GameObject g1 = null
    )
    {
        this.type = type;

        this.duration = new Duration(W, Q, duration); //base duration for attackdata calculations on boss, id:0

        this.f1 = f1;
        this.f2 = f2;
        this.b1 = b1;

        this.g1 = g1;

    }

    public void action(Vector3 pos, int Counter_Q)
    {
        if (type == 0)
        {
            return;
        }



        int[] wAndQ = Duration.durToTime(Counter_Q);
        AttackData clone = new AttackData(this, new Duration(wAndQ[0], wAndQ[1], duration.duration));

        if (type > 0)
        {
            create(clone, pos, Counter_Q);
            return;
        }

        switch (type)
        {
            case -1:
                LoopData.boss.GetComponent<Attack>().data = clone;

                /*1LoopData.boss.GetComponent<BossMainScript>().Chasing = true;
                LoopData.boss.GetComponent<BossMainScript>().ChasingTime = duration;
                LoopData.boss.GetComponent<BossMainScript>().ChasingTimeEnd = f1;

                if (TimeB.isBeatReached(ChasingTimeEnd))
                {
                    LoopData.boss.GetComponent<BossMainScript>().Chasing = false;
                }

                LoopData.boss.GetComponent<BossMainScript>().Chasing_start(data);*/
                break;

        }


    }

    public void create(AttackData clone, Vector3 position, int startBeatQ)
    {
        GameObject go = GameObject.Instantiate(Resources.Load(prefabPaths[type]), position, Quaternion.identity) as GameObject;
        Debug.Log("c. " + clone.duration.CounterSQ + " " + clone.duration.CounterEQ + " ");
        go.GetComponent<Attack>().data = clone;

        //replaced with duration.counterSQ
        //go.GetComponent<Attack>().startQ = startBeatQ;

        LoopData.currentAttacks.Add(go);
    }
}
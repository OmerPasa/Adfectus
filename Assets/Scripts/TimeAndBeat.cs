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

    public int[,] currentLoop = Patterns.loop4;

    public int currentLoopPos = 0;
    public int currentAttackPos = 0;
    public int durStart = 0;

    int prevBeatQC = -1;
    bool started = false;


    void Start()
    {
        StartCoroutine(StartAttack());
    }

    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(3);

        Patterns.player = player;
        Patterns.boss = boss;
        audioSource.Play();
        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!started)
        {
            return;
        }

        TimeB.tick(Time.deltaTime * 1000);
        if (TimeB.beatQuarterCounter == prevBeatQC)
        {//eğer Qbeat değişmediyse kontrol etmeye gerek yok
            return;
        }

        int patInd = currentLoop[currentLoopPos, 0]; //pattern index
        int dur = currentLoop[currentLoopPos, 1] + durStart; //döngü süresi

        AttackData currentAttackData = Patterns.patterns[patInd][currentAttackPos]; //atak datası

        if (TimeB.beat % dur == currentAttackData.beat && TimeB.beatQuarter == currentAttackData.beatQuarter)
        { //atak TimeBı geldiyse
            currentAttackData.action(transform.position, TimeB.beatQuarterCounter); //atak oluştur
            currentAttackPos++;
        }

        if (currentAttackPos >= Patterns.patterns[patInd].Length)
        {//atakPos kontrolü
            currentAttackPos = 0;
            currentLoopPos++;

            if (currentLoopPos >= currentLoop.GetLength(0))
            {//loopPos kontrolü
                currentLoopPos = 0;
            }
        }

        prevBeatQC = TimeB.beatQuarterCounter;

    }

    public AttackData getNextAttack()
    {
        int patInd = currentLoop[currentLoopPos, 0]; //pattern index
        //int dur = currentLoop[currentLoopPos, 1] + durStart; //loop duration

        int nextAttackPos = currentAttackPos + 1;

        if (nextAttackPos < Patterns.patterns[patInd].Length)
        {
            return Patterns.patterns[patInd][nextAttackPos];
        }

        nextAttackPos = 0;

        if (currentLoopPos + 1 < currentLoop.GetLength(0))
        {
            return Patterns.patterns[currentLoop[currentLoopPos + 1, 0]][0];
        }

        return Patterns.patterns[currentLoop[0, 0]][0];



    }
}

public static class TimeB
{
    public static float timePassed = 0;
    public static float bpm = 112.0f;
    public static int beat = 0;
    public static int beatQuarter = 0;

    public static int beatQuarterCounter = 0;
    static float beatDuration = 60000 / bpm;
    public static float quarterBeatDuration = beatDuration / 4;

    static bool isFirstFrame = true;

    public static void reset()
    {
        timePassed = 0;
        beat = 0;
        beatQuarter = 0;
        beatQuarterCounter = 0;
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
        beat = (int)(timePassed / beatDuration);
        beatQuarter = (int)(timePassed / quarterBeatDuration) % 4;

        beatQuarterCounter = beat * 4 + beatQuarter; //(int)(timePassed / quarterBeatDuration);

    }

}

public static class Patterns
{
    public static GameObject player;
    public static GameObject boss;
    public static List<GameObject> currentAttacks = new List<GameObject>();

    //patternIndex, duration
    public static int[,] loop1 = {
        { 0, 4 },
        { 1, 4 }
    };
    public static int[,] loop2 = {
        { 0, 4 },
        { 1, 4 },
        { 1, 4 }
    };
    public static int[,] loop3 = {
        { 4, 4 },
        { 0, 4 }
    };

    public static int[,] loop4 = {
        { 1, 4 },
        { 4, 4 },
        { 3, 4 },
        { 4, 4 },
        { 1, 4 },
        { 4, 4 },
        { 5, 4 },
        { 5, 4 }
    };

    public static int[,] loop5 = {
        { 1, 4 },
        { 4, 4 },
        { 3, 4 },
        { 4, 4 },
        { 1, 4 },
        { 4, 4 },
        { 5, 4 },
        { 5, 4 }
    };

    //AttactType, beat, beatQuarter, other params...
    public static AttackData[][] patterns =
    {   //0
        new AttackData[] {
            new AttackData(1, 0, 0, 1),
            new AttackData(1, 1, 0, 1),
            new AttackData(1, 2, 0, 1),
            new AttackData(1, 3, 0, 1)
        },//1
        new AttackData[] { //5-6
            new AttackData(1, 1, 0, 4),
            new AttackData(2, 2, 0, 2, f1: 270, f2: 220, b1: true),
            new AttackData(2, 3, 0, 4, f1: 270, f2: 310, b1: false),
        },//2
        new AttackData[] { //7-8
            new AttackData(1, 1, 0, 2),
            new AttackData(1, 1, 3, 2),
            new AttackData(1, 2, 2, 2),
            new AttackData(1, 3, 1, 2),
        },//3
        new AttackData[] { //7-8 alternative
            new AttackData(1, 0, 3, 2),
            new AttackData(2, 1, 2, 4, f1: 270, f2: 220, b1: true),
            new AttackData(1, 2, 1, 2),
            new AttackData(2, 3, 1, 4, f1: 270, f2: 310, b1: false),
        },//4
        new AttackData[] { //6-7
            new AttackData(0, 3, 0, 4), //empty
        },//5
        new AttackData[] { //11-12 unkown
            new AttackData(2, 1, 0, 4, f1: 270, f2: 220, b1: true),
            new AttackData(2, 2, 2, 4, f1: 270, f2: 310, b1: false),
            //new AttackData(3, 2, 2, 3, handData1:4),
            //new AttackData(4, 2, 2, 3, handPartData1:GetGameObject(3)),
            //new AttackData(4, 3, 2, 3, handPartData1:GetGameObject(3)),
        }
    };


    public static GameObject GetGameObject(int type)
    {
        foreach (GameObject go in Patterns.currentAttacks)
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
    static string[] prefabPaths = { "Prefabs/Null", "Prefabs/LaserType0", "Prefabs/LaserType1", "Prefabs/Chase" };

    public int type;
    public int beat;
    public int beatQuarter;
    public int duration;

    public float f1;
    public float f2;
    public bool b1;

    public int handData1;
    public GameObject g1;

    public AttackData(int type, int beat, int beatQuarter, int duration, float f1 = -1f, float f2 = -1f, bool b1 = false, int handData1 = -1, GameObject g1 = null)
    {
        this.type = type;
        this.beat = beat;
        this.beatQuarter = beatQuarter;
        this.duration = duration;

        this.f1 = f1;
        this.f2 = f2;
        this.b1 = b1;

        this.handData1 = handData1;
        this.g1 = g1;

    }

    public void action(Vector3 pos, int beatQuarterCounter)
    {
        if (type == 0)
        {
            return;
        }

        if (type > 0) //type 0 ise boş
        {
            create(pos, beatQuarterCounter);
            return;
        }

        switch (type)
        {
            case -1:
                //Patterns.boss.GetComponent<BossMainScript>().fonksiyonAdi(this);
                break;

        }


    }

    public void create(Vector3 position, int startBeatQ)
    {
        GameObject go = GameObject.Instantiate(Resources.Load(prefabPaths[type]), position, Quaternion.identity) as GameObject;
        go.GetComponent<Attack>().data = this;
        go.GetComponent<Attack>().startBeatQuarter = startBeatQ;

        Patterns.currentAttacks.Add(go);
    }
}
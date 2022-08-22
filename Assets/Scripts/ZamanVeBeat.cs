using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZamanVeBeat : MonoBehaviour
//boss scripti
//atakları doğru zamanda spawn eder
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

        Zaman.tick(Time.deltaTime * 1000);
        if (Zaman.beatQuarterCounter == prevBeatQC)
        {//eğer Qbeat değişmediyse kontrol etmeye gerek yok
            return;
        }

        int patInd = currentLoop[currentLoopPos, 0]; //pattern index
        int dur = currentLoop[currentLoopPos, 1] + durStart; //döngü süresi

        AttackData currentAttackData = Patterns.patterns[patInd][currentAttackPos]; //atak datası

        if (Zaman.beat % dur == currentAttackData.beat && Zaman.beatQuarter == currentAttackData.beatQuarter)
        { //atak zamanı geldiyse
            currentAttackPos++;
            currentAttackData.create(transform.position, Zaman.beatQuarterCounter); //atak oluştur
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

        prevBeatQC = Zaman.beatQuarterCounter;

    }
}

public static class Zaman
{
    public static float gecenSure = 0;
    public static float bpm = 112.0f;
    public static int beat = 0;
    public static int beatQuarter = 0;

    public static int beatQuarterCounter = 0;
    static float beatDuration = 60000 / bpm;
    static float quarterBeatDuration = beatDuration / 4;

    static bool isFirstFrame = true;

    public static void reset()
    {
        gecenSure = 0;
        beat = 0;
        beatQuarter = 0;
        beatQuarterCounter = 0;
        isFirstFrame = true;
    }

    public static void baslat(float time)
    {
        gecenSure += time;
    }

    public static void tick(float time)
    {
        if (isFirstFrame)
        {
            isFirstFrame = false;
            return;
        }
        gecenSure += time;
        beat = (int)(gecenSure / beatDuration);
        beatQuarter = (int)(gecenSure / quarterBeatDuration) % 4;

        beatQuarterCounter = beat * 4 + beatQuarter; //(int)(gecenSure / quarterBeatDuration);

    }

}

public static class Patterns
{
    public static GameObject player;
    public static GameObject boss;
    public static List<GameObject> currentAttacks = new List<GameObject>();

    //hangi patternin ne kadar süreyle calışacağı
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
        { 4, 4 },
        { 4, 4 }
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
    //ritmin vakitleri ve türleri.
    public static AttackData[][] patterns =
    {
        new AttackData[] {
            new AttackData(1, 0, 0, 1),
            new AttackData(1, 1, 0, 1),
            new AttackData(1, 2, 0, 1),
            new AttackData(1, 3, 0, 1)
        },
        new AttackData[] { //5-6
            new AttackData(1, 1, 0, 2),
            new AttackData(1, 2, 0, 2),
            new AttackData(1, 3, 0, 2),
        },
        new AttackData[] { //7-8
            new AttackData(1, 1, 0, 2),
            new AttackData(1, 1, 3, 2),
            new AttackData(1, 2, 2, 2),
            new AttackData(1, 3, 1, 2),
        },
        new AttackData[] { //7-8 alternative
            new AttackData(1, 0, 3, 2),
            new AttackData(1, 1, 2, 2),
            new AttackData(1, 2, 1, 2),
            new AttackData(1, 3, 1, 2),
        },
        new AttackData[] { //6-7
            new AttackData(0, 3, 0, 4), //boş
        },
        new AttackData[] { //11-12 unkown
            new AttackData(1, 1, 0, 3),

            new AttackData(3, 2, 2, 3, handData1:4),
            new AttackData(4, 2, 2, 3, handParcasiData1:GetGameObject(3)),
            new AttackData(4, 3, 2, 3, handParcasiData1:GetGameObject(3)),
        }
    };


    public static GameObject GetGameObject(int type)
    {
        foreach (GameObject go in Patterns.currentAttacks)
        {
            if (go.GetComponent<Attack>().type == type)
            {
                return go;
            }
        }

        return null;
    }
}

public class AttackData
{
    static string[] prefabPaths = { "Prefabs/Null", "Prefabs/LaserType0", "Prefabs/LaserType1" , "Prefabs/Chase"};
}

    public int type;
    public int beat;
    public int beatQuarter;
    public int duration;

    public int handData1;
    public GameObject handParcasiData1;

    public AttackData(int type, int beat, int beatQuarter, int duration, int handData1 = -1, GameObject handParcasiData1 = null)
    {
        this.type = type;
        this.beat = beat;
        this.beatQuarter = beatQuarter;
        this.duration = duration;
        this.handData1 = handData1;
        this.handParcasiData1 = handParcasiData1;
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
        if (type == 0) return;

        GameObject go = GameObject.Instantiate(Resources.Load(prefabPaths[type]), position, Quaternion.identity) as GameObject;
        go.GetComponent<Attack>().type = type;
        go.GetComponent<Attack>().beat = beat;
        go.GetComponent<Attack>().beatQuarter = beatQuarter;
        go.GetComponent<Attack>().startBeatQuarter = startBeatQ;
        go.GetComponent<Attack>().duration = duration;

        switch (type)
        {
            case 3:
                go.GetComponent<Attack>().handData1 = handData1;
                break;
            case 4:
                go.GetComponent<Attack>().handParcasiData1 = handParcasiData1;
                break;
        }

        Patterns.currentAttacks.Add(go);
    }
}
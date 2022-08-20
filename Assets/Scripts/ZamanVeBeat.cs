using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZamanVeBeat : MonoBehaviour
//boss scripti
//atakları doğru zamanda spawn eder
{
    public GameObject player;
    public AudioSource audioSource;

    public GameObject laser;
    GameObject copy;


    public int[,] currentLoop = Patterns.loop1;

    public int currentLoopPos = 0;
    public int currentAttackPos = 0;
    public int durStart = 0;

    int prevBeatQC = -1;

    void Start()
    {
        Patterns.player = player;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Zaman.tick(Time.deltaTime * 1000);
        if (Zaman.beatQuarterCounter == prevBeatQC)
        {//eğer Qbeat değişmediyse kontrol etmeye gerek yok
            return;
        }

        int patInd = currentLoop[currentLoopPos, 0]; //pattern index
        int dur = currentLoop[currentLoopPos, 1] + durStart; //döngü süresi

        AttackData currentAttackData = Patterns.patterns[patInd][currentAttackPos]; //attak datası

        if (Zaman.beat % dur == currentAttackData.beat && Zaman.beatQuarter == currentAttackData.beatQuarter)
        { //attak zamanı geldiyse
            currentAttackPos++;
            currentAttackData.create(transform.position, Zaman.beatQuarterCounter); //attak oluştur
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
    public static int bpm = 120;
    public static int beat = 0;
    public static int beatQuarter = 0;
    public static int beatQuarterCounter = 0;
    static int beatDuration = 60000 / bpm;
    static int quarterBeatDuration = beatDuration / 4;

    static bool isFirstFrame = true;


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
    public static List<GameObject> currentAttacks = new List<GameObject>();


    public static int[,] loop1 = {
        { 0, 4 },
        { 1, 4 }
    };
    public static int[,] loop2 = {
        { 0, 4 },
        { 1, 4 },
        { 1, 4 }
    };

    public static AttackData[][] patterns =
    {
        new AttackData[] {
            new AttackData(1, 0, 0, 3),
            new AttackData(1, 1, 2, 1),
            new AttackData(1, 2, 0, 1),
            new AttackData(1, 3, 0, 3)
        },
        new AttackData[] {
            new AttackData(1, 0, 0, 3),
            new AttackData(1, 1, 2, 1),
            new AttackData(1, 2, 0, 3),
            new AttackData(1, 3, 0, 1)
        }
    };
}

public class AttackData
{
    static string[] prefabPaths = { "Prefabs/Null", "Prefabs/LaserType0", "Prefabs/LaserType1" };


    public int type;
    public int beat;
    public int beatQuarter;
    public int duration;

    public AttackData(int type, int beat, int beatQuarter, int duration)
    {
        this.type = type;
        this.beat = beat;
        this.beatQuarter = beatQuarter;
        this.duration = duration;
    }

    public GameObject create(Vector3 position, int startBeatQ)
    {
        GameObject go = GameObject.Instantiate(Resources.Load(prefabPaths[type]), position, Quaternion.identity) as GameObject;
        go.GetComponent<Attack>().type = type;
        go.GetComponent<Attack>().beat = beat;
        go.GetComponent<Attack>().beatQuarter = beatQuarter;
        go.GetComponent<Attack>().startBeatQuarter = startBeatQ;
        go.GetComponent<Attack>().duration = duration;

        Patterns.currentAttacks.Add(go);
        return go;
    }
}
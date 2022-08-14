using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZamanVeBeat : MonoBehaviour
{
    public GameObject player;
    public AudioSource audioSource;

    public GameObject laser;
    GameObject copy;


    public int[,] currentLoop = Patterns.loop1;

    public int currentLoopPos = 0;
    public int currentAttackPos = 0;
    public int durStart = 0;
    int prevBeat = -1;
    int prevBeatQ = -1;

    void Start()
    {
        Patterns.player = player;
        audioSource.Play();
        Zaman.tick(-Time.deltaTime * 1000);
    }

    // Update is called once per frame
    void Update()
    {
        Zaman.tick(Time.deltaTime * 1000);
        //Debug.Log(Zaman.gecenSure / 1000);
        //Debug.Log(Zaman.beat.ToString() + ", " + Zaman.beatQuarter.ToString() + ". " + Zaman.beatQuarterCounter.ToString());

        if (Zaman.beatQuarter == prevBeatQ)
        {
            return;
        }

        /*if (Zaman.beat % 4 == 3 && Zaman.beat != prevBeat)
        {
            Debug.Log("Beat" + Zaman.beat.ToString() + ", " + prevBeat.ToString());
            copy = Spawner.spawn(laser, transform.position, Quaternion.identity);
            //copy.GetComponent<Laser>().target = GameObject.Find("player").transform;
        }
        else if (Zaman.beatQuarter % 4 == 0 && Zaman.beatQuarter != prevBeatQ)
        {
            Debug.Log("Q" + Zaman.beatQuarter.ToString() + ", " + prevBeatQ.ToString());
            copy.GetComponent<Laser>().mode = 2;
        }*/


        int patInd = currentLoop[currentLoopPos, 0];
        int dur = currentLoop[currentLoopPos, 1] + durStart;

        AttackData currentAttackData = Patterns.patterns[patInd][currentAttackPos];
        Debug.Log("c: " + currentAttackData.beat.ToString() + ", " + currentAttackData.beatQuarter.ToString());
        Debug.Log("ccc: " + Zaman.beat.ToString() + ", " + Zaman.beatQuarter.ToString());
        Debug.Log(currentAttackPos.ToString() + ", " + patInd.ToString());

        if (Zaman.beat % dur == currentAttackData.beat && Zaman.beatQuarter == currentAttackData.beatQuarter)
        {

            currentAttackPos++;


            currentAttackData.create(transform.position, Zaman.beatQuarterCounter);
        }

        if (currentAttackPos >= Patterns.patterns[patInd].Length)
        {
            currentAttackPos = 0;
            currentLoopPos++;
            //Debug.Log("L: " + Patterns.patterns.Length.ToString() + ", " + patInd.ToString());
            if (currentLoopPos >= Patterns.patterns.Length)
            {
                currentLoopPos = 0;
            }
        }



        prevBeat = Zaman.beat;
        prevBeatQ = Zaman.beatQuarter;

    }
}

public static class Spawner
{
    public static GameObject spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GameObject.Instantiate(prefab, position, rotation);
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


    public static void baslat(float time)
    {
        gecenSure += time;
    }

    public static void tick(float time)
    {
        gecenSure += time;
        beat = (int)(gecenSure / beatDuration) + 1;
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
        { 1, 4 },
        { 2, 4 },
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
            new AttackData(1, 2, 0, 1),
            new AttackData(1, 3, 0, 3)
        }
    };

    /*
    public static AttackData[] pattern1 =
    {
        new AttackData(1, 0, 0, 4),
    };
    public static AttackData[] pattern2 =
    {
        new AttackData(1, 0, 0, 4),
        new AttackData(1, 2, 0, 1),
    };
    public static AttackData[] pattern3 =
    {
        new AttackData(1, 0, 0, 4),
        new AttackData(1, 2, 0, 1),
        new AttackData(1, 3, 1, 1),
    };*/



}

public class AttackData : MonoBehaviour
{
    static string[] prefabPaths = { "Prefabs/Null", "Prefabs/Laser" };


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
        GameObject go = Instantiate(Resources.Load(prefabPaths[type]), position, Quaternion.identity) as GameObject;
        go.GetComponent<Attack>().type = type;
        go.GetComponent<Attack>().beat = beat;
        go.GetComponent<Attack>().beatQuarter = beatQuarter;
        go.GetComponent<Attack>().startBeatQuarter = startBeatQ;
        go.GetComponent<Attack>().duration = duration;

        Patterns.currentAttacks.Add(go);
        return go;
    }
}

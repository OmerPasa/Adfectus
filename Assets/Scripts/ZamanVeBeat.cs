using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZamanVeBeat : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Zaman.tick(Time.deltaTime * 1000);
        Debug.Log(Zaman.gecenSure / 1000);
        Debug.Log(Zaman.beat);

    }
}

public static class Zaman
{
    public static float gecenSure = 0;
    public static int bpm = 120;
    public static int beat = 1;
    static int beatDuration = 60000 / bpm;


    public static void baslat(float time)
    {
        gecenSure += time;
    }

    public static void tick(float time)
    {
        gecenSure += time;
        beat = (int)(gecenSure / beatDuration) + 1;
    }

}

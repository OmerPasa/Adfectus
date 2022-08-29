using System.Collections.Generic;
using UnityEngine;

//data about loops, player, boss etc
public static class LoopData
{
    public static GameObject player;
    public static GameObject boss;
    public static List<GameObject> currentAttacks = new List<GameObject>();


    public static int[][,] loops = {
    //patternIndex, duration
        new int[,] {//start
            { 1, 4 },
            { 4, 4 },
            { 3, 4 },
            { 4, 4 },
            { 1, 4 },
            { 4, 4 },
            { 5, 4 },
            { 5, 4 }
        },
        new int[,] {//boss1
            { 1, 4 },
            { 4, 4 },
            { 3, 4 },
            { 4, 4 },
            { 1, 4 },
            { 4, 4 },
            { 5, 4 },
            { 5, 4 }
        },
        new int[,] {//boss2
            { 1, 4 },
            { 4, 4 },
            { 3, 4 },
            { 4, 4 },
            { 1, 4 },
            { 4, 4 },
            { 5, 4 },
            { 5, 4 }
        },
       new int[,] {//boss3
            { 1, 4 },
            { 4, 4 },
            { 3, 4 },
            { 4, 4 },
            { 1, 4 },
            { 4, 4 },
            { 5, 4 },
            { 5, 4 },
            { 1, 4 },
            { 4, 4 },
            { 5, 4 },
            { 5, 4 }
        },
        new int[,] {//rise
            { 1, 4 },
            { 4, 4 },
            { 3, 4 },
            { 4, 4 }
        },
        new int[,] {//boss4
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
            new AttackData(-1, 0, 0, 1),
            new AttackData(-1, 1, 0, 1),
            new AttackData(-1, 2, 0, 1),
            new AttackData(-1, 3, 0, 1)
        },//1
        new AttackData[] { //5-6
            new AttackData(1, 1, 0, 2),
            new AttackData(2, 2, 0, 4, f1: 260, f2: 230),
            new AttackData(3, 3, 0, 4, f1: 5),
        },//2
        new AttackData[] { //7-8
            new AttackData(-1, 1, 0, 2),
            new AttackData(-1, 1, 3, 2),
            new AttackData(-1, 2, 2, 2),
            new AttackData(-1, 3, 1, 2),
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
            new AttackData(0, 3, 0, 4), //empty
            //new AttackData(-1, 2, 2, 4, f1: 4),
            //new AttackData(3, 2, 2, 3, handData1:4),
            //new AttackData(4, 2, 2, 3, handPartData1:GetGameObject(3)),
            //new AttackData(4, 3, 2, 3, handPartData1:GetGameObject(3)),
        }
    };

    public static int loopTotalSize(int loopIndex)
    {//returns totatl Q of the loop
        int total = 0;
        for (int i = 0; i < loops[loopIndex].GetLength(0); i++)
        {
            total += loops[loopIndex][i, 1];
        }
        return total * 4;
    }


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
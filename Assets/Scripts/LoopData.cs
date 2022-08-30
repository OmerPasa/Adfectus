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
            { 17, 16 },
        },
        new int[,] {//boss1
            { 6, 4 },
            { 7, 4 },
            { 8, 4 },
            { 7, 4 },
            { 6, 4 },
            { 7, 4 },
            { 9, 8 }
        },
        new int[,] {//boss2
            { 10, 8 },
            { 10, 8 },
            { 10, 8 },
            { 9, 8 }
        },
       new int[,] {//boss3
            { 11, 4 },
            { 12, 4 },
            { 11, 4 },
            { 12, 4 },
            { 11, 4 },
            { 12, 4 },
            { 9, 8 }
        },
        new int[,] {//rise
            { 18, 16 },
        },
        new int[,] {//boss4
            { 13, 4 },
            { 14, 4 },
            { 15, 4 },
            { 15, 4 },
            { 13, 4 },
            { 14, 4 },
            { 9, 8 }
        },
        new int[,] {//end
            { 4, 4 },
            { 4, 4 },
            { 4, 4 },
            { 4, 4 }
        },

    };

    //AttactType, W, Q, other params...
    public static AttackData[][] patterns =
    {   //0
        new AttackData[] {
            new AttackData(-1, 0, 0, 1),
            new AttackData(-1, 1, 0, 1),
            new AttackData(-1, 2, 0, 1),
            new AttackData(-1, 3, 0, 1)
        },
        
        //1
        new AttackData[] { //5-6
            new AttackData(1, 1, 0, 2),
            new AttackData(7, 1, 3, 4, f1: 260, f2: 230),
            new AttackData(2, 2, 0, 4, f1: 260, f2: 230),
            new AttackData(3, 3, 0, 4, f1: 5),
        },
        
        //2
        new AttackData[] { //7-8
            new AttackData(-1, 1, 0, 2),
            new AttackData(-1, 1, 3, 2),
            new AttackData(-1, 2, 2, 2),
            new AttackData(-1, 3, 1, 2),
        },
        
        //3
        new AttackData[] { //7-8 alternative
            new AttackData(1, 0, 3, 2),
            new AttackData(2, 1, 2, 4, f1: 270, f2: 240),
            new AttackData(1, 2, 1, 2),
            new AttackData(2, 3, 1, 4, f1: 270, f2: 300),
        },
        
        //4
        new AttackData[] { //6-7
            new AttackData(0, 3, 0, 4), //empty
        },
        
        //5
        new AttackData[] { //11-12 unkown
            new AttackData(2, 1, 0, 4, f1: 270, f2: 240),
            new AttackData(2, 2, 2, 4, f1: 270, f2: 300),
            new AttackData(0, 3, 0, 4), //empty
            //new AttackData(-1, 2, 2, 4, f1: 4),
            //new AttackData(3, 2, 2, 3, handData1:4),
            //new AttackData(4, 2, 2, 3, handPartData1:GetGameObject(3)),
            //new AttackData(4, 3, 2, 3, handPartData1:GetGameObject(3)),
        },
        
        //6
        new AttackData[] { //5-6 f
            new AttackData(7, 0, 2, 1, f1: 210, f2: 170),
            new AttackData(2, 1, 0, 4, f1: 210, f2: 170),

            new AttackData(7, 1, 2, 1, f1: 330, f2: 370),
            new AttackData(2, 2, 0, 4, f1: 330, f2: 370),

            new AttackData(7, 2, 2, 1, f1: 270, f2: 230),
            new AttackData(2, 3, 0, 4, f1: 270, f2: 310, extraAttackCount: 1),
            new AttackData(2, 3, 0, 4, f1: 270, f2: 230)
        },

        //7
        new AttackData[] { //6-7 f
            new AttackData(3, 0, 1, 4, f1: 5),
            new AttackData(3, 0, 2, 4, f1: 5),
            new AttackData(3, 0, 3, 4, f1: 5),
            new AttackData(3, 1, 1, 4, f1: 3),
            new AttackData(3, 1, 3, 4, f1: 5),
            new AttackData(3, 2, 1, 4, f1: 5),
            new AttackData(3, 2, 2, 4, f1: 5),
            new AttackData(3, 2, 3, 4, f1: 5),
            new AttackData(3, 3, 1, 4, f1: 5),
            new AttackData(3, 3, 3, 4, f1: 5)
        },

        //8
        new AttackData[] { //7-8 f
            new AttackData(7, 0, 2, 1, f1: 210, f2: 190),
            new AttackData(2, 1, 0, 3, f1: 210, f2: 190),

            new AttackData(7, 1, 1, 1, f1: 330, f2: 350),
            new AttackData(2, 1, 3, 3, f1: 330, f2: 350),

            new AttackData(7, 2, 0, 1, f1: 250, f2: 230),
            new AttackData(2, 2, 2, 3, f1: 250, f2: 230),

            new AttackData(7, 2, 3, 1, f1: 290, f2: 310),
            new AttackData(2, 3, 1, 3, f1: 290, f2: 310)
        },

        //9
        new AttackData[] { //weakening f
            new AttackData(-2, 0, 0, 32), // -2 has a bug???????????????????????????
            new AttackData(0, 7, 3, 1) //empty
        },

        //10
        new AttackData[] { //13-15 f
            new AttackData(7, 2, 0, 1, f1: 270, f2: 270),
            new AttackData(5, 2, 2, 16, f1: 270, f2: 270, extraAttackCount: 1),
            new AttackData(6, 2, 2, 16, f1: 270, f2: 270),

            new AttackData(1, 4, 0, 1),
            new AttackData(1, 4, 2, 1),
            new AttackData(1, 5, 0, 1),
            new AttackData(1, 5, 2, 1),

            new AttackData(0, 7, 3, 1) //empty
        },

        //11
        new AttackData[] { //21-22 f
            new AttackData(7, 0, 2, 1, f1: 360, f2: 330, extraAttackCount: 1),
            new AttackData(7, 0, 2, 1, f1: 320, f2: 290),
            new AttackData(2, 1, 0, 4, f1: 360, f2: 330, extraAttackCount: 1),
            new AttackData(2, 1, 0, 4, f1: 320, f2: 290),

            new AttackData(7, 1, 2, 1, f1: 180, f2: 210, extraAttackCount: 1),
            new AttackData(7, 1, 2, 1, f1: 220, f2: 250),
            new AttackData(2, 2, 0, 4, f1: 180, f2: 210, extraAttackCount: 1),
            new AttackData(2, 2, 0, 4, f1: 220, f2: 250),

            new AttackData(7, 2, 2, 1, f1: 270, f2: 270),
            new AttackData(5, 3, 0, 4, f1: 270, f2: 270, extraAttackCount: 1),
            new AttackData(6, 3, 0, 4, f1: 270, f2: 270),
        },

        //12
        new AttackData[] { //22-23 f
            new AttackData(3, 0, 1, 4, f1: 5),
            new AttackData(3, 0, 2, 4, f1: 5),
            new AttackData(3, 0, 3, 4, f1: 5),
            new AttackData(3, 1, 1, 4, f1: 5),
            new AttackData(3, 1, 2, 4, f1: 5),
            new AttackData(3, 1, 3, 4, f1: 5),
            new AttackData(3, 2, 1, 4, f1: 5),
            new AttackData(3, 2, 2, 4, f1: 5),
            new AttackData(3, 2, 3, 4, f1: 5),
            new AttackData(3, 3, 1, 4, f1: 5),
            new AttackData(3, 3, 2, 4, f1: 5),
            new AttackData(3, 3, 3, 4, f1: 5)
        },

        //13
        new AttackData[] { //33-34 f
            new AttackData(3, 1, 0, 4, f1: 5),
            new AttackData(3, 1, 2, 4, f1: 5),
            new AttackData(3, 2, 0, 4, f1: 5, extraAttackCount: 2),

            new AttackData(7, 2, 0, 1, f1: 220, f2: 290),
            new AttackData(7, 2, 0, 1, f1: 320, f2: 250),
            new AttackData(2, 2, 2, 6, f1: 220, f2: 290, extraAttackCount: 1),
            new AttackData(2, 2, 2, 6, f1: 320, f2: 250),
        },

        //14
        new AttackData[] { //34-35 f
            new AttackData(3, 0, 1, 4, f1: 5),
            new AttackData(3, 0, 2, 4, f1: 5),
            new AttackData(3, 0, 3, 4, f1: 5),
            new AttackData(3, 1, 2, 4, f1: 5),
            new AttackData(3, 2, 0, 4, f1: 5,  extraAttackCount: 2),

            new AttackData(7, 2, 0, 1, f1: 260, f2: 220),
            new AttackData(7, 2, 0, 1, f1: 280, f2: 320),
            new AttackData(2, 2, 2, 6, f1: 260, f2: 220, extraAttackCount: 1),
            new AttackData(2, 2, 2, 6, f1: 280, f2: 320),
        },

        //15
        new AttackData[] { //35-36 f
            new AttackData(3, 0, 1, 4, f1: 5),
            new AttackData(3, 0, 2, 4, f1: 5, extraAttackCount: 2),

            new AttackData(7, 0, 2, 1, f1: 240, f2: 240),
            new AttackData(7, 0, 2, 1, f1: 300, f2: 300),

            new AttackData(3, 0, 3, 4, f1: 5),

            new AttackData(2, 1, 0, 3, f1: 240, f2: 240, extraAttackCount: 1),
            new AttackData(2, 1, 0, 3, f1: 300, f2: 300),

            new AttackData(3, 1, 3, 4, f1: 5),
            new AttackData(3, 2, 1, 4, f1: 5),
            new AttackData(3, 2, 2, 4, f1: 5),
            new AttackData(3, 2, 3, 4, f1: 5),
            new AttackData(3, 3, 1, 4, f1: 5),
            new AttackData(3, 3, 2, 4, f1: 5),
            new AttackData(3, 3, 3, 4, f1: 5),
        },

        //16
        new AttackData[] { //pass the objective  (unused)
            new AttackData(-3, 3, 0, 4),
        },

        //17
        new AttackData[] { //instant fade out f
            new AttackData(-5, 0, 0, 0),
            new AttackData(-3, 1, 0, 4),
            new AttackData(-4, 8, 0, 32),
        },

        //18
        new AttackData[] { //rise f
            new AttackData(-5, 0, 0, 24),
            new AttackData(-3, 3, 0, 4),
            new AttackData(-4, 8, 0, 32),
        },
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
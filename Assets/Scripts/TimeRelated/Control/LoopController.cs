public static class LoopController
{
    public static int prevIndex = 0;
    public static int currentIndex = 0;
    public static int[,] currentLoop = LoopData.loops[currentIndex];


    public static bool loopAllLoops = false;
    public static bool isObjectiveCompleted = false;
    public static bool needChange = false;
    public static int changeIndex = 0;


    public static bool currentLoopEnd()
    {
        //changeindex is when it will change to the next loop
        changeIndex += LoopData.loopTotalSize(currentIndex);
        if (!isObjectiveCompleted)
        {
            //play the same loop
            return false;
        }

        isObjectiveCompleted = false;
        needChange = true;
        currentLoop = getNextLoop();
        return true;
    }

    public static int[,] getNextLoop()
    {
        prevIndex = currentIndex;
        currentIndex++;
        if (currentIndex >= LoopData.loops.Length)
        {
            currentIndex = 0;
        }

        if (currentIndex == 0 && !loopAllLoops)
        {
            end();
        }
        return LoopData.loops[currentIndex];
    }

    public static void end()
    {
        //ğ
    }

    public static void reset()
    {
        prevIndex = 0;
        currentIndex = 0;
        currentLoop = LoopData.loops[currentIndex];
        isObjectiveCompleted = false;
        needChange = false;
        changeIndex = 0;
    }

}
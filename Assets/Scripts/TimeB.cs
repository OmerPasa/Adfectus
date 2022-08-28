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
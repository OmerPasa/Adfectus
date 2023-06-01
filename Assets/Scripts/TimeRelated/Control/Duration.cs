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
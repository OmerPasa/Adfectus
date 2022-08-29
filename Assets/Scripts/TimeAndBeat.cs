using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAndBeat : MonoBehaviour
//boss scripti
//atakları doğru TimeBda spawn eder
{
    public GameObject player;
    public GameObject boss;
    public AudioSource[] audioSources;

    public int currentLoopPos = 0;
    public int currentAttackPos = 0;
    public int durStart = 0;

    int prevBeatQC = -1;
    bool active = false;


    void Start()
    {
        StartCoroutine(StartAttack());
    }

    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(3);

        LoopData.player = player;
        LoopData.boss = boss;
        play(0);
        currentLoopPos = 0;
        active = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            LoopController.isObjectiveCompleted = !LoopController.isObjectiveCompleted;
        }

        if (!active)
        {
            return;
        }

        TimeB.tick(Time.deltaTime * 1000);
        if (TimeB.Counter_Q == prevBeatQC)
        {//eğer Qbeat değişmediyse kontrol etmeye gerek yok
            return;
        }
        Debug.Log("bQC: " + TimeB.Counter_Q);

        if (LoopController.needChange && LoopController.changeIndex > TimeB.Counter_Q)
        {//loop değişmesi gerekiyor ve değişme zamanı gelmediyse
            prevBeatQC = TimeB.Counter_Q;
            return;
        }
        if (LoopController.needChange && LoopController.changeIndex == TimeB.Counter_Q)
        {//loop değişmesi gerekiyor ve değişme zamanı geldiyse
            LoopController.needChange = false;
            playUpdate();
            Debug.Log("Loop changed");
        }

        int patInd = LoopController.currentLoop[currentLoopPos, 0]; //pattern index
        int dur = LoopController.currentLoop[currentLoopPos, 1] + durStart; //döngü süresi
        AttackData currentAttackData = LoopData.patterns[patInd][currentAttackPos]; //atak datası

        if (TimeB.W % dur == currentAttackData.duration.beatSW && TimeB.Q == currentAttackData.duration.beatSQ)
        { //atak TimeBı geldiyse
            currentAttackData.action(transform.position, TimeB.Counter_Q); //atak oluştur
            currentAttackPos++;
        }


        if (currentAttackPos >= LoopData.patterns[patInd].Length)
        {//atakPos kontrolü
            currentAttackPos = 0;
            currentLoopPos++;

            if (currentLoopPos >= LoopController.currentLoop.GetLength(0))
            {//loopPos kontrolü
                LoopController.currentLoopEnd();
                Debug.Log("Loop end. " + LoopController.currentIndex + " " + LoopController.prevIndex);

                currentLoopPos = 0;

            }
        }
        prevBeatQC = TimeB.Counter_Q;
    }

    public AttackData getNextAttack()
    {
        int patInd = LoopController.currentLoop[currentLoopPos, 0]; //pattern index
        //int dur = currentLoop[currentLoopPos, 1] + durStart; //loop duration

        int nextAttackPos = currentAttackPos + 1;

        if (nextAttackPos < LoopData.patterns[patInd].Length)
        {
            return LoopData.patterns[patInd][nextAttackPos];
        }

        nextAttackPos = 0;

        if (currentLoopPos + 1 < LoopController.currentLoop.GetLength(0))
        {
            return LoopData.patterns[LoopController.currentLoop[currentLoopPos + 1, 0]][0];
        }

        return LoopData.patterns[LoopController.currentLoop[0, 0]][0];
    }

    public void playUpdate()
    {
        stop(LoopController.prevIndex);
        play(LoopController.currentIndex);
    }

    public void play(int index)
    {
        audioSources[index].enabled = true;
        audioSources[index].Play();
    }

    public void stop(int index)
    {
        audioSources[index].Stop();
        audioSources[index].enabled = false;
    }
}
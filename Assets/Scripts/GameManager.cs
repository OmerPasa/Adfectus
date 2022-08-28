using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static bool gameHasEndedBadWay = false;
    static bool gameHasEndedGoodWay = false;
    public float RestartDelay = 1f;

    public void EndGame ()
    {
        if (gameHasEndedBadWay == false)
        {
            gameHasEndedBadWay =  true;
            Debug.Log("GAME OVER");
            Invoke("Restart", RestartDelay);
        }
    }

    public void GameWon ()
    {
        if (gameHasEndedGoodWay == false)
        {
            gameHasEndedGoodWay =  true;
            Debug.Log("GAME GameWon");
            //we may change a game scene or play video etc.
            Invoke("Restart", RestartDelay);
        }
    }
    void Restart () 
    {
        SceneManager.LoadScene("MainGame");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject blackCurtain;
    static bool gameHasEndedBadWay = false;
    public GameObject badEnding;
    static bool gameHasEndedGoodWay = false;
    public GameObject goodEnding;
    public float restartDelay = 1f;

    public void EndGame ()
    {
        if (gameHasEndedBadWay == false)
        {
            blackCurtain.transform.position = Vector3.Lerp(blackCurtain.transform.position,transform.position, Time.time);
            badEnding.SetActive(true);// also implement fading
            gameHasEndedBadWay =  true;
            Debug.Log("GAME OVER");
            Invoke("Restart", restartDelay);
        }
    }

    public void GameWon ()
    {
        if (gameHasEndedGoodWay == false)
        {
            blackCurtain.transform.position = Vector3.Lerp(blackCurtain.transform.position,transform.position, Time.time);
            goodEnding.SetActive(true); //also implement fading
            gameHasEndedGoodWay =  true;
            Debug.Log("GAME GameWon");
            //we may change a game scene or play video etc.
            Invoke("Restart", restartDelay);
        }
    }
    void Restart () 
    {
        SceneManager.LoadScene("MainGame");
    }
}

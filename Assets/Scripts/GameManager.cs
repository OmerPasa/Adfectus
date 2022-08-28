using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static bool gameHasEnded = false;
    public float RestartDelay = 1f;
    public void EndGame ()
    {
        if (gameHasEnded == false)
        {
            gameHasEnded =  true;
            Debug.Log("GAME OVER");
            Invoke("Restart", RestartDelay);
        }
    }
    void Restart () 
    {
        SceneManager.LoadScene("MainGame");
    }
}

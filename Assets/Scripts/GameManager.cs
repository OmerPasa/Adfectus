using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public SpriteRenderer Renderer;
    static bool gameHasEndedBadWay = false;
    public GameObject badEnding;
    static bool gameHasEndedGoodWay = false;
    public GameObject goodEnding;
    public float restartDelay = 10f;

    private void Start() {
        gameHasEndedBadWay= false;
        gameHasEndedGoodWay= false;
    }

    public void EndGame ()
    {
        if (gameHasEndedBadWay == false)
        {
            for (float i = 0; i <= 1.0f; i += 0.01f)
            {
            Renderer.color = new Color(20, 10, 10, i);
            }
            badEnding.SetActive(true);
            gameHasEndedBadWay =  true;
            Debug.Log("GAME OVER");
            Invoke("Restart", restartDelay);
        }
    }

    public void GameWon ()
    {
        if (gameHasEndedGoodWay == false)
        {
            for (float i = 0; i <= 1.0f; i += 0.01f)
            {
            Renderer.color = new Color(20, 10, 10, i);
            }
            goodEnding.SetActive(true); //also implement fading
            gameHasEndedGoodWay =  true;
            Debug.Log("GAME GameWon");
            //we may change a game scene or play video etc.
            Invoke("Restart", restartDelay);
        }
    }
    void Restart () 
    {
        SceneManager.LoadScene("MEnu");
    }
}

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
    public float restartDelay = 13f;


    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Optional: Keep the GameManager persistent across scenes
    }

    private void Start()
    {
        gameHasEndedBadWay = false;
        gameHasEndedGoodWay = false;
        StartCoroutine(CheckAndActivate());
    }

    public void EndGame()
    {
        if (gameHasEndedBadWay == false)
        {
            for (float i = 0; i <= 1.0f; i += 0.01f)
            {
                Renderer.color = new Color(20, 10, 10, i);
            }
            badEnding.SetActive(true);
            gameHasEndedBadWay = true;
            Debug.Log("GAME OVER");
            Invoke("Restart", restartDelay);
        }
    }

    public void GameWon()
    {
        if (gameHasEndedGoodWay == false)
        {
            for (float i = 0; i <= 1.0f; i += 0.01f)
            {
                Renderer.color = new Color(20, 10, 10, i);
            }
            goodEnding.SetActive(true); //also implement fading
            gameHasEndedGoodWay = true;
            Debug.Log("GAME GameWon");
            //we may change a game scene or play video etc.
            Invoke("Restart", restartDelay);
        }
    }
    void Restart()
    {
        SceneManager.LoadScene("MEnu");
    }
    public GameObject[] objects;

    public void BossGETDamage(int damageTOBoss)
    {
        if (SceneManager.GetActiveScene().name == "BossDamat")
        {
            GetComponent<HumanBossController_Damat>().BossTakeDamage(damageTOBoss);
        }
        else if (SceneManager.GetActiveScene().name == "BossKeko")
        {
            GetComponent<HumanBossController_Keko>().BossTakeDamage(damageTOBoss);
        }
    }

    IEnumerator CheckAndActivate()
    {
        while (true)
        {
            foreach (GameObject obj in objects)
            {
                if (!obj.activeSelf)
                {
                    yield return new WaitForSeconds(1.5f);
                    obj.SetActive(true);
                }
            }
            yield return null;
        }
    }


}

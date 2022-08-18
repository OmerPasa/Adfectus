using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class quit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Quit", 50f);
    }

    void Quit()
    {
        Application.Quit();
    }
    
}

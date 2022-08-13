using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cozunurlukAyarla : MonoBehaviour
{
    int x = 1280;
    int y = 720;

    // Start is called before the first frame update
    void Start()
    {
        Ayarla();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                Screen.SetResolution(x, y, FullScreenMode.Windowed);
            }
            else
            {
                Ayarla();
            }

        }
    }

    void Ayarla()
    {
        //Screen.SetResolution(1024, 576, FullScreenMode.FullScreenWindow);

        if (Screen.width / x > Screen.height / y)
        {
            Screen.SetResolution(Screen.currentResolution.height / y * x, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.width / x * y, FullScreenMode.FullScreenWindow);
        }
    }

}

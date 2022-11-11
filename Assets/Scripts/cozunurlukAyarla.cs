using UnityEngine;

public class cozunurlukAyarla : MonoBehaviour
{
    public bool setAtSceneStart = false;
    float x = 1280.0f;
    float y = 720.0f;
    private InputManager inputManager;

    void Start()
    {
        inputManager = new InputManager();
        if (setAtSceneStart)
            Ayarla();
    }

    void Update()
    {
        if (inputManager.Player.FullScreen.triggered)
        {
            if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                Screen.SetResolution((int)x, (int)y, FullScreenMode.Windowed);
            }
            else
            {
                Ayarla();
            }

        }
    }

    void Ayarla()
    {
        bool isWide = (Screen.width / x > Screen.height / y);
        if (isWide)
        {
            Screen.SetResolution((int)(Screen.currentResolution.height * x / y), Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(Screen.currentResolution.width, (int)(Screen.currentResolution.width * y / x), FullScreenMode.FullScreenWindow);
        }
    }

}

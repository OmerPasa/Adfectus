using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthbar_control : MonoBehaviour
{
    public MainMenu mainMenu;
    public Slider slider;
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }
    
    public void SetHealth(int health)
    {
        slider.value = health;
        if (health <= 0)
        {
            mainMenu.GameIsOver();
            Debug.Log("Game is ooverrrr");
        }
    }
}

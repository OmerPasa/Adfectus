using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthbar_control : MonoBehaviour
{
    public GameManager GameManager;
    public Slider slider;
    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }
    
    public void SetHealth(float health)
    {
        slider.value = health;
        if (health <= 0)
        {
            GameManager.EndGame();
            Debug.Log("Game is ooverrrr");
        }
    }
}

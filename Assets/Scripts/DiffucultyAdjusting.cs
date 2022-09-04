using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiffucultyAdjusting : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    public static float _damageToPlayer = 0.1f;
    public static void SetFloat(string damageToPlayer, float damageToPlayerNumber)
    {
        PlayerPrefs.SetFloat(damageToPlayer, damageToPlayerNumber);
    }

    private void Update() 
    {
        _slider.onValueChanged.AddListener((v) => {
            _damageToPlayer = v;
        });
        Debug.Log("damagetoplayer is " + _damageToPlayer);
        SetFloat("damageToPlayer", _damageToPlayer);
    }
}

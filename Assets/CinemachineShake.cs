using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private float shakeTimer;
    private void Awake()
    {
        Instance =  this;
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    
    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannalPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannalPerlin.m_AmplitudeGain = intensity;
    }

    private void Update() 
    {
        if (shakeTimer > 0 ) 
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f) 
            {
                //Timer is over
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannalPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cinemachineBasicMultiChannalPerlin.m_AmplitudeGain = 0f;
            }
        }
    }
}

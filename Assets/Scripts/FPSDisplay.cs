// Date Created:        May 16, 2020
// Created By:          Peter Reynolds

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField]
    private Text text;

    // Update is called once per frame
    void Update()
    {
        text.text = $"FPS: {1.0f/Time.smoothDeltaTime}\nParticles: {ParticleController.SGetParticleCount()}";
    }
}

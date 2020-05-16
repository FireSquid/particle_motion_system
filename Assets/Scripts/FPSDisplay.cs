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
        text.text = $"FPS: {1.0f/Time.smoothDeltaTime}\nParticles: {((ParticleController.particles != null) ? (ParticleController.particles.Count) : (0))}";
    }
}

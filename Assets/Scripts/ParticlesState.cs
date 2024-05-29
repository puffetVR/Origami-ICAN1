using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesState : MonoBehaviour
{
    ParticleSystem ps;

    private void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }

    public void ParticlesPlay()
    {
        ps.Play();
    }

    public void ParticlesStop() { ps.Stop(); }
}

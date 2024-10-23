using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Desturctive : MonoBehaviour
{
    private List<Vector3> samplers = new List<Vector3>();
    void Start()
    {
        PoissonDiskSampler3D sampler = new PoissonDiskSampler3D(10, 5, 7.5f, 1f);
        foreach (Vector3 sample in sampler.Samples()) {
            samplers.Add(sample);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 sample in samplers) {
            Gizmos.DrawWireCube(sample, Vector3.one * 0.1f);
        }
    }
}

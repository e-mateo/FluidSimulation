using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluid
{
    public float particuleRadius;
    public int kernelParticule;
    public float kernalRadius;

    public float mass;
    public float radius;
    public float densityRest;
    public float viscosity;
    public float threshold;
    public float restitution;
    public float gasStiffness;
    public float particuleMass;
    public float surfaceTension;
    public float buoyancyDiffusion;
}

public class Water : Fluid
{
    public Water()
    {
        kernelParticule = 20;
        densityRest = 998.29f;
        viscosity = 3.5f;
        threshold = 7.065f;
        restitution = 0f;
        gasStiffness = 3f;
        particuleMass = 0.02f;
        surfaceTension = 0.0728f;
        buoyancyDiffusion = 0f;
        particuleRadius = Mathf.Pow((3f * particuleMass) / (4f * Mathf.PI * densityRest), 1f / 3f);
        kernalRadius = 0.0457f;
    }
}

public class Mucus : Fluid
{
    public Mucus()
    {
        kernelParticule = 40;
        densityRest = 1000f;
        viscosity = 36f;
        threshold = 5f;
        restitution = 0f;
        gasStiffness = 5f;
        particuleMass = 0.04f;
        surfaceTension = 6f;
        buoyancyDiffusion = 0f;
        particuleRadius = Mathf.Pow((3f * particuleMass) / (4f * Mathf.PI * densityRest), 1f / 3f);
        kernalRadius = 0.0726f;
    }
}



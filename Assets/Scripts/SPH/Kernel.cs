using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kernel
{
    protected float p_radius = 0f;

    public void InitKernelData(float kernelSize)
    {
        p_radius = kernelSize;
    }

    public float GetRadius() { return p_radius; }

    public float W_Default(Vector3 r)
    {
        if(r.magnitude > p_radius)
            return 0;
        else
            return (315f * Mathf.Pow(Mathf.Pow(p_radius,2f) - Mathf.Pow(r.magnitude, 2f), 3f)) / (64f * Mathf.PI * Mathf.Pow(p_radius, 9f));
    }

    public Vector3 GradW_Default(Vector3 r)
    {
        return (-945)/(32 * Mathf.PI * Mathf.Pow(p_radius, 9)) * r * (Mathf.Pow((p_radius * p_radius) - (r.magnitude * r.magnitude), 2));
    }

    public float LapW_Default(Vector3 r)
    {
        return (-945) / (32 * Mathf.PI * Mathf.Pow(p_radius, 9)) * (Mathf.Pow((p_radius * p_radius) - (r.magnitude * r.magnitude), 2)) * (3* Mathf.Pow(p_radius,2) - 7 * Mathf.Pow(r.magnitude, 2));
    }

    public float W_Pressure(Vector3 r)
    {
        if (r.magnitude > p_radius)
            return 0;
        else
            return (15f * Mathf.Pow((p_radius - r.magnitude), 3f)) / (Mathf.PI * Mathf.Pow(p_radius, 6));
    }

    public Vector3 GradW_Pressure(Vector3 r)
    {
        return (-45f / (Mathf.PI * Mathf.Pow(p_radius, 6))) * (r.normalized) * Mathf.Pow(p_radius - r.magnitude, 2);
    }

    public float LaplacienW_Viscosity(Vector3 r)
    {
        if (r.magnitude > p_radius)
            return 0;
        else
            return (45f * (p_radius - r.magnitude)) / (Mathf.PI * Mathf.Pow(p_radius, 6));
    }
}
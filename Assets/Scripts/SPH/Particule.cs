using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particule : MonoBehaviour
{
    public Fluid fluid;

    private float m_delta;
    private float m_pressure;
    private float m_massDensity;

    private Vector3 m_gravityForce;
    private Vector3 m_surfaceForce;
    private Vector3 m_internalForce;
    private Vector3 m_externalForce;

    private Vector3 m_curPosition;
    private Vector3 m_curVelocity;
    private Vector3 m_acceleration;

    public List<int> neighborIndex;
    public float pressurePerDensitySquare;
    public float massPerDensity;

    public void SetInitialValues(Vector3 pos, Vector3 speed, Fluid fluidPart, float dt)
    {
        m_curPosition = pos;
        m_curVelocity = speed;

        fluid = fluidPart;
        m_delta = dt;
    }

    public void AdvanceParticule()
    {
        float dt = m_delta * Time.fixedDeltaTime;

        //Update the currentVelocity with the acceleration
        m_curVelocity += dt * m_acceleration;

        //Update the currentPosition with the currentVelocity
        m_curPosition += dt * m_curVelocity;

        //Update the transform of the particule
        transform.position = m_curPosition;
    }

    public void SetPressure(float pressure) { m_pressure = pressure; }
    public void SetMassDensity(float massDensity) { m_massDensity = massDensity; }
    public void SetAcceleration(Vector3 force) { m_acceleration = force; }
    public void SetPosition(Vector3 pos) { m_curPosition = pos; }
    public void SetVelocity(Vector3 vel) { m_curVelocity = vel; }
    public void SetGravityForce(Vector3 force) { m_gravityForce = force; }
    public void SetSurfaceForce(Vector3 force) { m_surfaceForce = force; }
    public void SetInternalForce(Vector3 force) { m_internalForce = force; }
    public void SetExternalForce(Vector3 force) { m_externalForce = force; }
    public float GetPressure() { return m_pressure; }
    public float GetMassDensity() { return m_massDensity; }
    public Vector3 GetPosition() { return m_curPosition; }
    public Vector3 GetVelocity() { return m_curVelocity; }
    public Vector3 GetAcceleration() { return m_acceleration; }
    public Vector3 GetGravityForce() { return m_gravityForce; }
    public Vector3 GetSurfaceForce() { return m_surfaceForce; }
    public Vector3 GetInternalForce() { return m_internalForce; }
    public Vector3 GetExternalForce() { return m_externalForce; }
}
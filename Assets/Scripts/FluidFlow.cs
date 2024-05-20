using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FluidFlow : MonoBehaviour
{
    // Script for the Part1 of the exercice

    // Start is called before the first frame update
    [SerializeField] private TMP_Text canvasText;
    [SerializeField] private TMP_Text canvasText2;
    [SerializeField] private float m_height;
    [SerializeField] private float m_orificeSection;
    [SerializeField] private float m_speed;

    [SerializeField] private Transform m_groundObject;
    [SerializeField] private Transform m_ceilingObject;
    [SerializeField] private Transform m_wallOne;
    [SerializeField] private Transform m_wallTwo;
    [SerializeField] private Transform m_background;

    private float m_cubeSection;
    private float m_vOrif;
    private float m_time;
    private float m_maxTime;
    private float m_g;
    private float m_waterTowerHeight;
    private LineRenderer m_lineRenderer;

    private float distMax = 0;
    private Vector3 m_startPosition;
    private bool m_isRunning;
    private bool m_isFinish;

    void Start()
    {
        m_startPosition = transform.position;
        StartSimulation();
    }

    void Update()
    {
        if (!m_isRunning)
        {
            m_lineRenderer.enabled = false;
            return;
        }

        //Compute new height
        float previousHeight = transform.localScale.y;
        float height = ComputeFluidHeight();

        //Compute the water trajectory
        ComputeFluidTrajectory(previousHeight, height);

        //Update simulation time
        if (!m_isFinish)
            m_time += m_speed * Time.deltaTime;
    }

    public void RunSimulation()
    {
        m_isRunning = !m_isRunning;
        m_isFinish = false;
    }

    public void StartSimulation()
    {
        //Init values

        transform.position = m_groundObject.position + Vector3.up *0.2f + Vector3.up * m_height/2.0f;
        SetupWallSize();
        transform.localScale = new Vector3(4.0f, m_height, 4.0f);
        m_cubeSection = transform.localScale.x * transform.localScale.z;
        m_time = 0.0f;
        m_g = 9.81f;
        m_lineRenderer = GetComponent<LineRenderer>();
        m_lineRenderer.positionCount = 50;
        m_lineRenderer.startWidth = 0.2f;
        m_lineRenderer.endWidth = 0.2f;
        m_waterTowerHeight = transform.position.y;
        m_maxTime = Mathf.Sqrt(2 * m_waterTowerHeight / m_g);
    }

    private void ComputeFluidTrajectory(float previousHeight, float height)
    {
        //To draw the water trajectory we used a line renderer composed of 50 points

        if (previousHeight >= height)
        {
            m_lineRenderer.enabled = true;
            transform.localScale = new Vector3(4.0f, height, 4.0f);
            transform.position -= new Vector3(0, (previousHeight - height) / 2.0f, 0);

            //Update orifice velocity 
            m_vOrif = Mathf.Sqrt(2 * m_g * height);

            for (int i = 0; i < 50; i++) //loop over each point
            {
                float time = (m_maxTime / 50.0f) * i;

                //Compute movement equation to find each position
                float x = m_vOrif * time + transform.position.x + transform.localScale.x / 2.0f;
                float y = (-m_g / 2.0f) * time * time + (m_waterTowerHeight - m_height / 2.0f);

                if (y > -0.5f) //Cap the y position to the ground if its below
                {
                    m_lineRenderer.SetPosition(i, new Vector3(x, y, transform.position.z)); //Set the position of the point i
                }
                else
                {
                    m_lineRenderer.SetPosition(i, m_lineRenderer.GetPosition(i - 1)); //Set the position of the point i
                }

                //Update maximum distance
                if (x - transform.localScale.x / 2.0f > distMax)
                {
                    distMax = m_vOrif * m_maxTime + transform.position.x;
                }

            }
            DrawUI();
        }
        else
        {
            m_lineRenderer.enabled = false;
            m_isFinish = true;
        }
    }

    private float ComputeFluidHeight()
    {
       return  Mathf.Pow((-m_orificeSection * Mathf.Sqrt(2 * m_g) * m_time / (2*m_cubeSection) )+ Mathf.Sqrt(m_height), 2);
    }

    public void ChangeHeight(string text)
    {
        m_height = float.Parse(text);
        StartSimulation();
    }

    private void SetupWallSize()
    {
        m_ceilingObject.position = m_groundObject.position + Vector3.up * 0.4f + Vector3.up * m_height;
        m_wallOne.position = m_groundObject.position + Vector3.up * 0.7f + Vector3.right *     2.0f + Vector3.up * m_height / 2.0f ;
        m_wallOne.localScale = new Vector3(m_height-0.20f , 0.4f,4);
        m_wallTwo.position = m_groundObject.position + Vector3.up * 0.2f + Vector3.right * -2.0f +Vector3.up * m_height / 2.0f;
        m_wallTwo.localScale = new Vector3(m_height+0.8f, 0.4f,4);

        m_background.position = m_groundObject.position + Vector3.up * 0.2f + Vector3.forward * 2.0f + Vector3.up * m_height / 2.0f;
        m_background.localScale = new Vector3(4, 0.4f, m_height + 0.8f);
    }

    private void DrawUI()
    {
        float timer = m_time;

        int hours = (int)(timer) / 3600;
        int minute = (int)(timer - (hours * 3600)) / 60;
        float seconds = (timer - (hours * 3600) - (minute * 60));

        canvasText.text = "Timer : " + hours + " h: " + minute +" m: "+  seconds.ToString("F3") + "s";
        canvasText2.text = "Distance : " + distMax.ToString() + "m";
    }


}

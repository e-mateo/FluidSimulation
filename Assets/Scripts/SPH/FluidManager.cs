using System.Collections;
using System.Collections.Generic;
using Unity.Burst;  
using UnityEngine;

public class FluidManager : MonoBehaviour
{

    //The FluidManager class is the class that will run the simuation
    //It will calculate SPH Forces for each particule

    [SerializeField] private float m_scaleSizeParticule;
    [SerializeField] private float m_timeStep;
    [SerializeField] private GameObject m_particulePrefab;
    [SerializeField] private GameObject m_obstacles;
    [SerializeField] private GameObject m_shaderMat;

    private Kernel m_kernel;
    private Water m_water;
    private Mucus m_mucus;

    private int m_partCount;
    private Vector3 m_gravity = Vector3.up * -9.82f;
    private List<Particule> m_particules;
    private Material m_fluidMaterial;

    private void Start()
    {
        m_kernel = new Kernel();
        m_water = new Water();
        m_mucus = new Mucus();
        m_particules = new List<Particule>();
        m_fluidMaterial = m_shaderMat.GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        for (int i = 0; i < m_particules.Count; i++)
        {
            //Handle collisions with obstacles like spheres, cubes and capsules
            //Particules don't collide with each others, they only collide with obstacle objects
            HandleCollision(m_particules[i]);

            //Search Neighbors
            List<int> N = SearchNeighborIndex(m_particules[i]);
            m_particules[i].neighborIndex = N; //Store neighbors

            ComputeMassDensity(i, N);
            ComputePressure(i);
        }

        for (int i = 0; i < m_particules.Count; i++)
        {
            List<int> N = m_particules[i].neighborIndex; //Get Neighbors
            ComputeInternalForces(i, N); //Internal Forces : Pressure Force + Viscosity Force 
            ComputeExternalForces(i); //External Forces : Gravity Force
            Vector3 SPHForce = m_particules[i].GetInternalForce() + m_particules[i].GetExternalForce();
            ComputeAcceleration(i, SPHForce);
        }
    }

    private void FixedUpdate()
    {
        //Advance particules depending of their acceleration
        for (int i = 0; i < m_particules.Count; i++)
            m_particules[i].AdvanceParticule();
    }

    public void LaunchWater()
    {
        StopAllCoroutines();
        DeleteParticules();

        m_kernel.InitKernelData(m_water.kernalRadius); //Setup the support radius of the kernel

        m_partCount = 275;
        m_fluidMaterial.color = (Color.cyan + Color.blue) / 2f;
        StartCoroutine(CreateParticulesCoroutine(m_water));
    }

    public void LaunchMucus()
    {
        StopAllCoroutines();
        DeleteParticules();

        m_kernel.InitKernelData(m_mucus.kernalRadius); //Setup the support radius of the kernel

        m_partCount = 350;
        m_fluidMaterial.color = Color.green;
        StartCoroutine(CreateParticulesCoroutine(m_mucus));
    }

    private void DeleteParticules()
    {
        foreach(Particule p in m_particules)
            Destroy(p.gameObject);

        m_particules.Clear();
    }

    private List<int> SearchNeighborIndex(Particule p)
    {
        List<int> neighbors = new List<int>();

        //Find each particule that is close enough to our particule p
        //We use a circle with a radius equal to our support kernel radius to find our neighbor particules

        for (int i = 0; i < m_particules.Count; i++) //Loop over each particules 
        {
            float distance = Vector3.Distance(p.GetPosition(), m_particules[i].GetPosition()); 
            if (distance <= m_kernel.GetRadius()) 
                neighbors.Add(i);
        }

        return neighbors;
    }

    private void ComputeInternalForces(int indexParticule, List<int> neighborIndices)
    {
        //This function computes the pressure force and the viscosity force

        //Init values
        Particule p = m_particules[indexParticule];
        Vector3 pressureForce = Vector3.zero;
        Vector3 viscosityForce = Vector3.zero;

        //Loop over each neighbor
        for (int i = 0; i < neighborIndices.Count; i++) 
        {
            int indexNeighbor = neighborIndices[i];
            if (indexParticule == indexNeighbor) continue; //Ignore the particule p

            Particule neighbor = m_particules[indexNeighbor];

            Vector3 dir = p.GetPosition() - neighbor.GetPosition();
            pressureForce += (p.pressurePerDensitySquare + neighbor.pressurePerDensitySquare) * neighbor.fluid.particuleMass * m_kernel.GradW_Pressure(dir); //Use the Gradient of the Pressure Kernel
            viscosityForce += (neighbor.GetVelocity() - p.GetVelocity()) * (neighbor.massPerDensity) * m_kernel.LaplacienW_Viscosity(dir); //Use the Laplacian of the Viscosity Kernel
        }

        pressureForce = -p.GetMassDensity() * pressureForce;
        viscosityForce = p.fluid.viscosity * viscosityForce;

        p.SetInternalForce(pressureForce + viscosityForce);
    }

    private void ComputeExternalForces(int i)
    {
        //This function computes the gravity force

        Particule p = m_particules[i];
        Vector3 gravityF = p.GetMassDensity() * m_gravity;

        p.SetGravityForce(gravityF);
        p.SetExternalForce(gravityF);
    }

    private void ComputeAcceleration(int i,Vector3 SPHForce)
    {
        Vector3 acceleration = SPHForce / m_particules[i].GetMassDensity();
        m_particules[i].SetAcceleration(acceleration);
    }

    private void ComputeMassDensity(int particuleIndex, List<int> neighborIndices)
    {
        //Init values
        Particule p = m_particules[particuleIndex];
        float massDensity = 0f;

        //The list 'neighborIndices' stores indexes of each neighbor;
        //Those indices are then used to get neighbor particules in the 'm_particules' List;

        for (int i = 0; i < neighborIndices.Count; i++) //loop over 
        {
            int neighborIndex = neighborIndices[i];
            Particule neighbor = m_particules[neighborIndex];
            massDensity += neighbor.fluid.particuleMass * m_kernel.W_Default(p.GetPosition() - neighbor.GetPosition()); //Use the default Kernel
        }

        p.SetMassDensity(massDensity);

        //Compute the mass per density that will be used in future calculations
        p.massPerDensity = p.fluid.particuleMass / massDensity;
    }

    private void ComputePressure(int particuleIndex)
    {
        Particule p = m_particules[particuleIndex];
        float pressure = p.fluid.gasStiffness * (p.GetMassDensity() - p.fluid.densityRest);
        p.SetPressure(pressure);

        //Compute the pressurePerDensitySquare that will be used in future calculations
        p.pressurePerDensitySquare = pressure / (p.GetMassDensity() * p.GetMassDensity());
    }

    public void HandleCollision(Particule p)
    {
        //Loop over each obstacle
        for (int i = 0; i < m_obstacles.transform.childCount; i++) 
        {
            GameObject go = m_obstacles.transform.GetChild(i).gameObject;
            Obstacles obstacle = go.GetComponent<Obstacles>();

            if (obstacle.GetComponent<Obstacles>() != null)
            {
                TypeObstacle type = obstacle.GetTypeObstacle();

                //The container boolean helps us know if the object should act like a container or like an obstacle
                bool container = obstacle.IsContainer();

                switch (type) //Check the type of the obstacle (Sphere, Capsule, Box) and Handle the right collision
                {
                    case TypeObstacle.SPHERE:
                        HandleSphereCollision(p, go, container);
                        break;
                    case TypeObstacle.CAPSULE:
                        HandleCapsuleCollision(p, go, container);
                        break;
                    case TypeObstacle.CUBE:
                        HandleBoxesCollision(p, go, container);
                        break;
                }
            }
        }
    }

    public void HandleSphereCollision(Particule p, GameObject obstacle, bool container)
    {
        Vector3 center = obstacle.transform.position;
        float radius = obstacle.transform.localScale.x / 2f;

        //The value F will help us know is the particule is inside the object (F < 0) or outside (F > 0)
        float F = Mathf.Pow((p.GetPosition() - center).magnitude, 2) - Mathf.Pow(radius, 2);

        if ((F < 0 && !container) || (F > 0 && container))
        {
            //First we calculate different values to handle our collision
            Vector3 contactPoint = center + radius * (p.GetPosition() - center).normalized; 
            float depth = Mathf.Abs((center - p.GetPosition()).magnitude - radius); 
            Vector3 normal = Mathf.Sign(F) * (center - p.GetPosition()).normalized;
            float dt = m_timeStep * Time.fixedDeltaTime;

            //We reset the position of the particule to the contact point
            p.SetPosition(contactPoint);

            //Then we calculate its new velocity
            Vector3 newVelocity = p.GetVelocity() - (1 + p.fluid.restitution * (depth / (dt * p.GetVelocity().magnitude))) * (Vector3.Dot(p.GetVelocity(), normal) * normal);
            p.SetVelocity(newVelocity);

            //Finally we update its transform
            p.transform.position = p.GetPosition();
        }
    }

    public void HandleCapsuleCollision(Particule p, GameObject obstacle, bool container)
    {
        CapsuleCollider collider = obstacle.GetComponent<CapsuleCollider>();
        Vector3 center = obstacle.transform.position;
        float height = collider.height * obstacle.transform.localScale.y;
        float radius = collider.radius * obstacle.transform.localScale.x;
        Vector3 P0 = center - obstacle.transform.up * (height / 2f - radius); //P0 is the center of the bottom sphere
        Vector3 P1 = center + obstacle.transform.up * (height / 2f - radius); //P1 is the center of the top sphere
        Vector3 x = p.GetPosition(); 

        Vector3 q = P0 + (Mathf.Min(1, Mathf.Max(0, -Vector3.Dot((P0 - x), (P1 - P0)) / Mathf.Pow((P1 - P0).magnitude, 2)))) * (P1 - P0);

        //The value F will help us know is the particule is inside the object (F < 0) or outside (F > 0)
        float FCapsule = (q - x).magnitude - radius;

        if ((FCapsule < 0 && !container) || (FCapsule > 0 && container))
        {
            //First we calculate different values to handle our collision
            Vector3 contactPoint = q + radius * (x - q).normalized;
            float depth = Mathf.Abs(FCapsule);
            Vector3 normal = Mathf.Sign(FCapsule) * (q - x).normalized;
            float dt = m_timeStep * Time.fixedDeltaTime;

            //We reset the position of the particule to the contact point
            p.SetPosition(contactPoint);

            //Then we calculate its new velocity
            Vector3 newVelocity = p.GetVelocity() - (1 + p.fluid.restitution * (depth / (dt * p.GetVelocity().magnitude))) * (Vector3.Dot(p.GetVelocity(), normal) * normal);
            p.SetVelocity(newVelocity);

            //Finally we update its transform
            p.transform.position = p.GetPosition();
        }
    }

    public void HandleBoxesCollision(Particule p, GameObject obstacle, bool container)
    {
        Quaternion rotation = Quaternion.Euler(-obstacle.transform.eulerAngles);
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotation);
        Vector3 extend = obstacle.transform.localScale / 2f;
        Vector3 x =  p.transform.position;
        
        //Calculate the local position of the particule (local to the box)
        Vector4 xlocal4 = rotationMatrix * (x - obstacle.transform.position);
        Vector3 xlocal = new Vector3(xlocal4.x, xlocal4.y, xlocal4.z);

        //The value F will help us know is the particule is inside the object (F < 0) or outside (F > 0)
        float Fbox = MaxComponentVector(VectorAbs(xlocal) - extend);

        if (Fbox > 0 && container) //We only handle Boxes as containers
        {
            //Calculate local and global contactPoint
            Vector3 contactPointLocal = Vector3.Min(extend, Vector3.Max(-extend, xlocal));
            Vector4 contactPointLocalRot = (rotationMatrix.transpose * contactPointLocal);
            Vector3 contactPoint = obstacle.transform.position + new Vector3(contactPointLocalRot.x, contactPointLocalRot.y, contactPointLocalRot.z);

            float depth = (contactPoint - p.transform.position).magnitude;
            Vector3 normal = Mathf.Sign(Fbox) * (rotationMatrix * VectorAbs(contactPointLocal - xlocal)).normalized;
            float dt = m_timeStep * Time.fixedDeltaTime;

            //We reset the position of the particule to the contact point
            p.SetPosition(contactPoint);

            //Then we calculate its new velocity
            Vector3 newVelocity = p.GetVelocity() - (1 + p.fluid.restitution * (depth / (dt * p.GetVelocity().magnitude))) * (Vector3.Dot(p.GetVelocity(), normal) * normal);
            p.SetVelocity(newVelocity);

            //Finally we update its transform
            p.transform.position = p.GetPosition();
        }
    }

    public Vector3 VectorAbs(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    public float MaxComponentVector(Vector3 vec)
    {
        if (vec.x > vec.y && vec.x > vec.z)
            return vec.x;
        else if (vec.y > vec.z)
            return vec.y;
        else
            return vec.z;
    }

    IEnumerator CreateParticulesCoroutine(Fluid fluid)
    {
        //A coroutine that will instanciate each particule
        int direction = 1;

        while(m_particules.Count <= m_partCount)
        {
            float particuleSize = fluid.particuleRadius * 2;

            float x = this.transform.position.x;
            float y = this.transform.position.y;

            //Create the particule gameobject
            GameObject particuleObject = Instantiate(m_particulePrefab, new Vector3(x, y, 0), Quaternion.identity);
            particuleObject.transform.localScale = new Vector3(particuleSize, particuleSize, particuleSize) * m_scaleSizeParticule;
            particuleObject.transform.SetParent(this.transform, true);

            //Add the particule component
            Particule particule = particuleObject.AddComponent<Particule>();
            particule.SetInitialValues(new Vector3(x, y, 0), new Vector3(-4 * direction, -4, 0), fluid, m_timeStep);
            m_particules.Add(particule);
            direction *= -1;
            yield return new WaitForSeconds(0.005f);
        }

        yield return null;
    }
}
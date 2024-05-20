using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TypeObstacle
{
    SPHERE,
    CUBE,
    CAPSULE,
}

public class Obstacles : MonoBehaviour
{
    //Obstacles is a little component attach to our obstacle objects
    //It stores the type of obstacle and if the obstacle is a container or not

    [SerializeField] TypeObstacle obstacle;
    [SerializeField] bool container;

    public TypeObstacle GetTypeObstacle() { return obstacle; }
    public bool IsContainer() { return container; }
}

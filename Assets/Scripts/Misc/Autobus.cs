using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autobus : MonoBehaviour
{
    public float velocidad;
  
    void Update()
    {
        transform.Translate(-velocidad, 0, 0);
        if (transform.position.x < -100)
        {
            transform.Translate(200, 0, 0);
        }
    }
}
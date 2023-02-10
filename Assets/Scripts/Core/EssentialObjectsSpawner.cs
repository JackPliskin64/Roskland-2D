using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;
    public Vector3 offset;

    public void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0)
        //If there is a grid, then spawn at it's center
        {
            var spawnPos = new Vector3(0, 0, 0);

            var grid = FindObjectOfType<Grid>();

            if (grid != null)
            {
                spawnPos = grid.transform.position;
            }
        
            Instantiate(essentialObjectsPrefab, spawnPos + offset, Quaternion.identity);
        }
    }
}

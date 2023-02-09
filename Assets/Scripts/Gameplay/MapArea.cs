using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Fighter> wildFighters;

    public Fighter GetRandomWildFighter()
    {
        var wildFighter = wildFighters[Random.Range(0, wildFighters.Count)];
        wildFighter.Init();
        return wildFighter;
    }
}

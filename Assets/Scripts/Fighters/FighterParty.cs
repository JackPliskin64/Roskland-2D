using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FighterParty : MonoBehaviour
{
    [SerializeField] List<Fighter> fighters;

    public List<Fighter> Fighters { get { return fighters; } }

    private void Start()
    {
        foreach (var fighter in fighters)
        {
            fighter.Init();
        }
    }

    public Fighter GetHealthyFighter()
    {
       return fighters.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddFighter(Fighter newFighter)
    {
        if (fighters.Count < 6)
        {
            fighters.Add(newFighter);
        }
        else
        {
            //Add to the PC once it's implemented
        }
    }
}

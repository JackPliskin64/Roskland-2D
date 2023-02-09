using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightersDB
{
    static Dictionary<string, FighterBase> fighters;

    public static void Init()
    {
        fighters = new Dictionary<string, FighterBase>();

        var fighterArray = Resources.LoadAll<FighterBase>("");

        foreach (var fighter in fighterArray)
        {
            if (fighters.ContainsKey(fighter.FighterName))
            {
                Debug.LogError($"Existen dos luchadores con el nombre {fighter.fighterName}");
                continue;
            }

            fighters[fighter.fighterName] = fighter;
        }
    }

    public static FighterBase GetFighterByName(string name)
    {
        if (!fighters.ContainsKey(name))
        {
            Debug.Log($"El luchador con nombre {name} no se encontró en la base de datos.");
            return null;
        }

        return fighters[name];
    }
}

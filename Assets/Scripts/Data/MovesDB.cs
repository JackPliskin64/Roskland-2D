using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovesDB
{
    static Dictionary<string, MoveBase> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MoveBase>();

        var moveList = Resources.LoadAll<MoveBase>("");

        foreach (var move in moveList)
        {
            if (moves.ContainsKey(move.MoveName))
            {
                Debug.LogError($"Existen dos movimientos con el nombre {move.moveName}");
                continue;
            }

            moves[move.MoveName] = move;
        }
    }

    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.Log($"El movimiento con nombre {name} no se encontró en la base de datos.");
            return null;
        }

        return moves[name];
    }
}

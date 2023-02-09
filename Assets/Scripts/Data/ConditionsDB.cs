using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
            {
                ConditionID.Veneno, 
                new Condition()
                {
                    Name = "Veneno",
                    StartMessage = "ha sido envenenado.",
                    OnAfterTurn = (Fighter fighter) =>
                    {
                        fighter.UpdateHP(fighter.MaxHP / 8);
                        fighter.StatusChanges.Enqueue($"{fighter.Base.FighterName} sufrió daño por el veneno.");
                    }
                }
            },
        {
                ConditionID.Fuego,
                new Condition()
                {
                    Name = "Ardiendo",
                    StartMessage = "está ardiendo.",
                    OnAfterTurn = (Fighter fighter) =>
                    {
                        fighter.UpdateHP(fighter.MaxHP / 16);
                        fighter.StatusChanges.Enqueue($"{fighter.Base.FighterName} sufrió daño por el fuego.");
                    }
                }
        },

        {
                ConditionID.Parálisis,
                new Condition()
                {
                    Name = "Paralizado",
                    StartMessage = "está paralizado.",
                    OnBeforeMove = (Fighter fighter) =>
                    {
                        if (Random.Range(1, 5) == 1)
                        {
                            fighter.StatusChanges.Enqueue($"¡{fighter.Base.FighterName} está paralizado, no se puede mover!");
                            return false;
                        }
                       
                        return true;
                        
                    }
                }
        },
        
        {
                ConditionID.Hielo,
                new Condition()
                {
                    Name = "Congelado",
                    StartMessage = "está congelado.",
                    OnBeforeMove = (Fighter fighter) =>
                    {
                        if (Random.Range(1, 5) == 1)
                        {
                            fighter.CureStatus();
                            fighter.StatusChanges.Enqueue($"¡{fighter.Base.FighterName} ya no está congelado!");
                            return false;
                        }

                        return false;

                    }
                }
        },

        {
                ConditionID.Sueño,
                new Condition()
                {
                    Name = "Dormido",
                    StartMessage = "se ha dormido.",
                    OnStart = (Fighter fighter) =>
                    {
                        //Sleep for 1-3 turns
                        fighter.StatusTime = Random.Range(1,4);
                        Debug.Log($"Will be asleep for {fighter.StatusTime} moves.");
                    },

                    OnBeforeMove = (Fighter fighter) =>
                    {
                        if (fighter.StatusTime == 0)
                        {
                            fighter.CureStatus();
                            fighter.StatusChanges.Enqueue($"¡{fighter.Base.FighterName} se ha despertado!");
                            return true;

                        }

                       fighter.StatusTime--;
                       fighter.StatusChanges.Enqueue($"{fighter.Base.FighterName} está durmiendo.");
                       return false;

                    }
                }
        },
        //Volatile Status COnditions
        {
                ConditionID.Confusión,
                new Condition()
                {
                    Name = "Confusión",
                    StartMessage = "está confuso.",
                    OnStart = (Fighter fighter) =>
                    {
                        //Confused for 1-4 turns
                        fighter.VolatileStatusTime = Random.Range(1,5);
                        Debug.Log($"Will be confused for {fighter.VolatileStatusTime} moves.");
                    },

                    OnBeforeMove = (Fighter fighter) =>
                    {
                        if (fighter.VolatileStatusTime == 0)
                        {
                            fighter.CureVolatileStatus();
                            fighter.StatusChanges.Enqueue($"¡{fighter.Base.FighterName} ya no está confuso!");
                            return true;

                        }

                       fighter.VolatileStatusTime--;

                        //50% chance to do a move
                        if (Random.Range(1, 3) == 1)
                        {
                            return true;
                        }

                        //Hurt by confusion
                       fighter.StatusChanges.Enqueue($"¡{fighter.Base.FighterName} está confuso!");
                       fighter.UpdateHP(fighter.MaxHP / 8);
                       fighter.StatusChanges.Enqueue($"¡Está tan confuso que se hirió a sí mismo!");
                       return false;

                    }
                }
        }
    };

    public static float GetStausBonus(Condition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.Id == ConditionID.Sueño || condition.Id == ConditionID.Hielo)
        {
            return 2f;
        }
        else if (condition.Id == ConditionID.Parálisis || condition.Id == ConditionID.Veneno || condition.Id == ConditionID.Fuego)
        {
            return 1.5f;
        }

        return 1f;
    }
}

public enum ConditionID
{
    none, Veneno, Fuego, Sueño, Parálisis, Hielo, Confusión
}

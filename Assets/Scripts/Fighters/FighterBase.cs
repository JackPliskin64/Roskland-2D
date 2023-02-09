using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "FighterBase", menuName = "Fighter/Create new fighter")]
public class FighterBase : ScriptableObject
{
    [SerializeField] public string fighterName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] public Sprite sprite;

    [SerializeField] public fighterType fighterType1;
    [SerializeField] public fighterType fighterType2;

    //Base stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int deffense;
    [SerializeField] int spAttack;
    [SerializeField] int spDeffense;
    [SerializeField] int speed;
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;
    
    [SerializeField] List<LearnableMove> learnableMoves;

    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }

        return -1;
    }
    
    public string FighterName { get { return fighterName; } }

    public string Description { get { return description; } }

    public int MaxHP { get { return maxHp; } }

    public int Attack { get { return attack; } }

    public int Deffense { get { return deffense; } }

    public int Speed { get { return speed; } }

    public int SpAttack { get { return spAttack; } }

    public int SpDeffense { get { return spDeffense; } }

    public List<LearnableMove> LearnableMoves { get { return learnableMoves; } }

    public int CatchRate => catchRate;

    public int ExpYield => expYield;

    public GrowthRate GrowthRate => growthRate; 



    [System.Serializable]
    public class LearnableMove
    {
        [SerializeField] MoveBase moveBase;
        [SerializeField] int level;

        public MoveBase Base { get { return moveBase; } }
        public int Level { get { return level; } }  
    }
}

public class TypeChart
{
    static float[][] chart =
    {
        //                  MEL ROS GIG PET MAE EMO CAL COG FEM PAL MET VIO
        /*MEL*/ new float[] {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
        /*ROS*/ new float[] {1f, 0.5f, 1f, 2f, 1f, 1f, 2f, 0.5f, 2f, 0.5f , 1f, 2f },
        /*GIG*/ new float[] {2f, 2f, 1f, 2f, 0.5f, 2f, 2f, 1f, 2f, 1f, 1f, 2f },
        /*PET*/ new float[] {2f, 0.5f, 0.5f, 0.5f, 2f, 2f, 2f, 1f, 0.5f, 1f, 1f, 0.5f },
        /*MAE*/ new float[] {2f, 2f, 2f, 2f, 1f, 2f, 2f, 2f, 2f, 2f, 2f, 2f },
        /*EMO*/ new float[] {2f, 1f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 0.5f },
        /*CAL*/ new float[] {1f, 1f, 0.5f, 1f, 1f, 1f, 0.5f, 1f, 2f, 1f, 1f, 0.5f },
        /*COG*/ new float[] {2f, 1f, 0.5f, 1f, 1f, 1f, 2f, 2f, 2f, 1f, 1f, 1f },
        /*FEM*/ new float[] {0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f, 0.5f, 1f, 0.5f, 1f, 1f, 2f },
        /*PAL*/ new float[] {1f, 2f, 1f, 2f, 1f, 1f, 2f, 1f, 2f, 0.5f, 1f, 1f },
        /*MET*/ new float[] {1f, 1f, 0.5f, 1f, 0.5f, 2f, 2f, 1f, 0.5f, 2f, 0.5f, 1f },
        /*VIO*/ new float[] {1f, 0.5f, 0.5f, 2f, 0.5f, 1f, 1f, 1f, 0.5f, 1f, 1f, 0.5f },
        //MARICON
        //SOLLAO
        //ERMITAÑO
        
    };

    public static float GetEffectiveness(fighterType attackType, fighterType defenseType)
    {
        if (attackType == fighterType.None || defenseType == fighterType.None)
        {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
public enum fighterType
{
    None,
    Melvin,
    Roscas,
    GIGACHAD,
    Petarda,
    CalzMaster,
    Emo,
    Calzones,
    Cogorzo,
    Feminazi,
    Palanquero,
    Metalero, 
    Violador

}

public enum GrowthRate
{
    Fast, MediumFast
}

public enum Stat
{
    Ataque,
    Defensa,
    AtaqueSP,
    DefensaSP,
    Velocidad, 

    //These two are not actual stats, they're used to boost the moveAccuracy
    Precisión,
    Evasión
}



using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Fighter
{
    [SerializeField] FighterBase _base;
    [SerializeField] int level;

    public Fighter (FighterBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public int Exp { get; set; }
    public FighterBase Base { get { return _base; } }
    public int Level { get { return level; } }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }

    public Condition VolatileStatus { get; private set; }

    public int VolatileStatusTime { get; set; } 
    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }

    public event System.Action OnStatusChanged;

    public void Init()
    {
        //Generate moves
        Moves = new List<Move>();
        foreach(var move in Base.LearnableMoves)
        {
            if(move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }

            if(Moves.Count >= FighterBase.MaxNumOfMoves)
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(Level);
        
        CalculateStats();
        HP = MaxHP;

        StatusChanges = new Queue<string>();

        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;

    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Ataque, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defensa, Mathf.FloorToInt((Base.Deffense * Level) / 100f) + 5);
        Stats.Add(Stat.AtaqueSP, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.DefensaSP, Mathf.FloorToInt((Base.SpDeffense * Level) / 100f) + 5);
        Stats.Add(Stat.Velocidad, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Ataque, 0},
            {Stat.Defensa, 0},
            {Stat.AtaqueSP, 0},
            {Stat.DefensaSP, 0},
            {Stat.Velocidad, 0},
            {Stat.Precisi�n, 0},
            {Stat.Evasi�n, 0},
        };
    }
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        //Comprobamos si el boost es negativo o positivo
        if(boost >= 0) { statVal = Mathf.FloorToInt(statVal * boostValues[boost]); }
        else { statVal = Mathf.FloorToInt(statVal / boostValues[-boost]); }

        return statVal;
    }

    public void ApplyBoosts(List<MoveBase.StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"�{stat} de {Base.FighterName} aument�!");
            }
            else
            {
                StatusChanges.Enqueue($"�{stat} de {Base.FighterName} disminuy�!");
            }
        }
    }

    public bool CheckForlevelUp()
    {
        if (Exp > Base.GetExpForLevel(Level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public FighterBase.LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(FighterBase.LearnableMove moveToLearn)
    {
        if (Moves.Count > FighterBase.MaxNumOfMoves)
        {
            return;
        }
        Moves.Add(new Move(moveToLearn.Base));
    }
    public int MaxHP { get; private set; }

    public int Attack
    {
        get { return GetStat(Stat.Ataque); }
    }

    public int Deffense
    {
        get { return GetStat(Stat.Defensa); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Velocidad); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.AtaqueSP); }
    }

    public int SpDeffense
    {
        get { return GetStat(Stat.DefensaSP); }
    }

    public DamageDetails TakeDamage(Move move, Fighter attacker)
    {
        float critical = 1f;
        if(Random.value * 100 <= 6.25f)
        {
            critical = 2f;
        }
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.fighterType1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.fighterType2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveBase.MoveCategory.Especial) ? attacker.SpAttack : attacker.Attack;
        float deffense = (move.Base.Category == MoveBase.MoveCategory.Especial) ? attacker.SpDeffense : Deffense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / deffense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionId)
    {
        if(Status != null) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.FighterName} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.FighterName} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();
        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoosts();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

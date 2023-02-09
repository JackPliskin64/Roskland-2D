using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Fighter _fighter;

    Dictionary<ConditionID, Color> statusColors;


    public void SetData(Fighter fighter)
    {
        _fighter = fighter;
        SetLevel();
        hpBar.SetHP((float) fighter.HP / fighter.MaxHP);
        nameText.text = fighter.Base.FighterName;
        SetExpSmooth();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.Veneno, psnColor },
            {ConditionID.Fuego, brnColor },
            {ConditionID.Sueño, slpColor },
            {ConditionID.Parálisis, parColor },
            {ConditionID.Hielo, frzColor },
        };

        SetStatusText();
        _fighter.OnStatusChanged += SetStatusText; 
    }

    void SetStatusText()
    {
        if (_fighter.Status == null)
        {
            statusText.text = ""; 
        }
        else
        {
            statusText.text = _fighter.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_fighter.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _fighter.Level;

    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if(expBar == null) { yield break; }

        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();

        
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _fighter.Base.GetExpForLevel(_fighter.Level);
        int nextlevelExp = _fighter.Base.GetExpForLevel(_fighter.Level + 1);

        float normalizedExp = (float)(_fighter.Exp - currLevelExp) / (nextlevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public IEnumerator UpdateHP()
    {
        if (_fighter.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_fighter.HP / _fighter.MaxHP);
            _fighter.HpChanged = false;
        }

    }

}


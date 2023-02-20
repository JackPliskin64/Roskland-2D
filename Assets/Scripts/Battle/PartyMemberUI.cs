using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyMemberUI: MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;

    Fighter _fighter;


    public void SetData(Fighter fighter)
    {
        _fighter = fighter;

        nameText.text = fighter.Base.FighterName;
        levelText.text = "Lvl " + fighter.Level;
        hpBar.SetHP((float)fighter.HP / fighter.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        if (selected) { nameText.color = GlobalSettings.i.HighlightedColor; }
        else { nameText.color = Color.black; }
    }
}

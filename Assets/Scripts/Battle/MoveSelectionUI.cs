using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<TMP_Text> moveTexts;
    [SerializeField] Color highlightedColor;

    int currentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].MoveName;
        }

        moveTexts[currentMoves.Count].text = newMove.MoveName;

        UpdateMoveSelection(currentSelection);
    }

    public void HandleMoveSelection(Action <int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --currentSelection;
        }
        currentSelection = Mathf.Clamp(currentSelection, 0, FighterBase.MaxNumOfMoves);

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < FighterBase.MaxNumOfMoves+1; i++)
        {
            if (i == selection)
            {
                moveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
    }
}

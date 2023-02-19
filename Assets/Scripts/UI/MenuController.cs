using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    List<TMP_Text> menuItems;

    int selectedItem = 0;

    public void Awake()
    {
            menuItems = menu.GetComponentsInChildren<TMP_Text>().ToList();
    }
    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void HandleUpdate()
    {

        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedItem;
        }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --selectedItem;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if(prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }
 
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == selectedItem)
            {
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            }

            else
            {
                menuItems[i].color = Color.black;
            }
        }
    }
}

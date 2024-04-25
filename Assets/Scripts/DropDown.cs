using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Michsky.MUIP;

public class DropDown : MonoBehaviour
{
    public CustomDropdown dropdown;
    public List<GameObject> dropDownList;

    public void changeDrop()
    {
        for(int i = 0; i < dropDownList.Count; i++)
        {
            if(i == dropdown.index)
            //if(i == dropdown.value)
            {
                dropDownList[i].SetActive(true);
            }
            else
            {
                dropDownList[i].SetActive(false);
            }
        }
    }
}

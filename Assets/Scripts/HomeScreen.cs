using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HomeScreen : MonoBehaviour
{
    [Header ("Панели")]
    public GameObject panelHome;
    public GameObject panelCart;
    public GameObject panelUser;

    public void clickHome()
    {
        panelHome.SetActive (true);
        panelCart.SetActive (false);
        panelUser.SetActive (false);
    }

    public void clickCart()
    {
        panelHome.SetActive(false);
        panelCart.SetActive(true);
        panelUser.SetActive(false);
    }

    public void clickUser()
    {
        panelHome.SetActive(false);
        panelCart.SetActive(false);
        panelUser.SetActive(true);
    }

}

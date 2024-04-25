using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject signUpPanel;
    public GameObject profilePanel;
    public GameObject forgotPasswordPanel;
    public GameObject adminPanel;
    public GameObject buttonsPanel;
    public GameObject homePanel;


    // Список всех панелей для упрощения управления ими
    private List<GameObject> allPanels;

    private void Awake()
    {
        allPanels = new List<GameObject> { loginPanel, signUpPanel, profilePanel, buttonsPanel, forgotPasswordPanel, adminPanel };
    }

    // Универсальный метод для активации нужной панели
    public void ActivatePanel(GameObject panelToActivate)
    {
        foreach (var panel in allPanels)
        {
            panel.SetActive(panel == panelToActivate);
        }
        buttonsPanel.SetActive(panelToActivate == profilePanel);
    }

    // Добавляем метод для вызова из UI
    public void SwitchPanel(string panelName)
    {
        switch (panelName)
        {
            case "loginPanel":
                ActivatePanel(loginPanel);
                break;
            case "signUpPanel":
                ActivatePanel(signUpPanel);
                break;
            case "forgotPasswordPanel":
                ActivatePanel(forgotPasswordPanel);
                break;
            case "profilePanel":
                ActivatePanel(profilePanel);
                break;
            case "adminPanel":
                ActivatePanel(adminPanel);
                break;
            case "homePanel":
                ActivatePanel(homePanel);
                break;
            default:
                Debug.LogError("Panel name is incorrect or not implemented");
                break;
        }
    }
}



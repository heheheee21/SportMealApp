using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Michsky.MUIP;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public GameObject orderPrefab; // ������ ��� ����������� ������
    public GameObject itemPrefab; // ������ ��� ����������� �������
    public Transform contentPanel; // ������������ ������� ��� ���� �������

    // �������� ���� �����, ����� ��������� ������ � ���������� ��
    public void LoadAndDisplayAllItemsForAdmin()
    {
        DatabaseReference cartsRef = FirebaseDatabase.DefaultInstance.GetReference("carts");

        cartsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("������ ��� �������� ������: " + task.Exception);
                // ��������� ������
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // ������� ������ ���������
                foreach (Transform child in contentPanel)
                {
                    Destroy(child.gameObject);
                }

                // ���� �� ���� �������������
                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    string userId = userSnapshot.Key;
                    //Debug.Log("������������: " + userId);

                    // ���� �� ���� ������� ������������
                    foreach (DataSnapshot orderSnapshot in userSnapshot.Children)
                    {
                        // ��������� ������� ��������� � ������
                        if (!orderSnapshot.HasChild("items") || orderSnapshot.Child("items").ChildrenCount == 0)
                        {
                            Debug.LogWarning($"����� {orderSnapshot.Key} ������������ {userId} �� �������� �������");
                            continue;
                        }
                        // ������� ���� ������
                        GameObject orderObject = Instantiate(orderPrefab, contentPanel);
                        SetupOrderLayout(orderObject);  // ��������� ������ ��� ������ ������� ������

                        TMP_Text EMailText = orderObject.transform.Find("EMail").GetComponent<TMP_Text>();
                        TMP_Text AddressText = orderObject.transform.Find("Address").GetComponent<TMP_Text>();
                        ButtonManager deleteButton = orderObject.transform.Find("DeleteButton").GetComponent<ButtonManager>();

                        EMailText.text = orderSnapshot.Child("email").Value.ToString();
                        AddressText.text = orderSnapshot.Child("deliveryAddress").Value.ToString();
                        deleteButton.onClick.AddListener(() => DeleteOrder(userId, orderSnapshot.Key, orderObject));
                        // ���� �� ���� ������� � ������
                        foreach (DataSnapshot itemSnapshot in orderSnapshot.Child("items").Children)
                        {
                            string itemName = itemSnapshot.Child("itemName").Value.ToString();
                            string itemQuantity = itemSnapshot.Child("quantity").Value.ToString();

                            // ������� UI ������� ��� ������ � ��������� ��� ������ orderObject
                            CreateItemElement(orderObject.transform, itemName, itemQuantity);  // ��������� �����
                        }
                    }
                }
                
            }
        });
    }

    void SetupOrderLayout(GameObject orderObject)
    {
        VerticalLayoutGroup vlg = orderObject.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = orderObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }


    // �������� UI �������� ��� ������
    void CreateItemElement(Transform parent, string name, string quantity)
    {
        Debug.Log("������ ������� ������");
        GameObject newItem = Instantiate(itemPrefab, parent);  // ��������� �����

        TMP_Text nameText = newItem.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text quantityText = newItem.transform.Find("Quantity").GetComponent<TMP_Text>();

        nameText.text = name;
        quantityText.text = quantity;
    }
    // �������� ������ �� �� � UI
    void DeleteOrder(string userId, string orderId, GameObject orderObject)
    {
        DatabaseReference orderRef = FirebaseDatabase.DefaultInstance.GetReference($"carts/{userId}/{orderId}");
        orderRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("����� ������: " + orderId);
                Destroy(orderObject);  // ��� ������ ����� � ��� ��������� ������
            }
            else
            {
                Debug.LogError("������ ��� �������� ������: " + task.Exception);
            }
        });
    }
}
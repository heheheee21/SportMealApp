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
    public GameObject orderPrefab; // Префаб для отображения заказа
    public GameObject itemPrefab; // Префаб для отображения товаров
    public Transform contentPanel; // Родительский элемент для всех товаров

    // Вызовите этот метод, чтобы загрузить данные и отобразить их
    public void LoadAndDisplayAllItemsForAdmin()
    {
        DatabaseReference cartsRef = FirebaseDatabase.DefaultInstance.GetReference("carts");

        cartsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Ошибка при загрузке данных: " + task.Exception);
                // Обработка ошибок
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // Очистка старых элементов
                foreach (Transform child in contentPanel)
                {
                    Destroy(child.gameObject);
                }

                // Идем по всем пользователям
                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    string userId = userSnapshot.Key;
                    //Debug.Log("Пользователь: " + userId);

                    // Идем по всем заказам пользователя
                    foreach (DataSnapshot orderSnapshot in userSnapshot.Children)
                    {
                        // Проверяем наличие элементов в заказе
                        if (!orderSnapshot.HasChild("items") || orderSnapshot.Child("items").ChildrenCount == 0)
                        {
                            Debug.LogWarning($"Заказ {orderSnapshot.Key} пользователя {userId} не содержит товаров");
                            continue;
                        }
                        // Создаем блок заказа
                        GameObject orderObject = Instantiate(orderPrefab, contentPanel);
                        SetupOrderLayout(orderObject);  // Настройка макета для нового объекта заказа

                        TMP_Text EMailText = orderObject.transform.Find("EMail").GetComponent<TMP_Text>();
                        TMP_Text AddressText = orderObject.transform.Find("Address").GetComponent<TMP_Text>();
                        ButtonManager deleteButton = orderObject.transform.Find("DeleteButton").GetComponent<ButtonManager>();

                        EMailText.text = orderSnapshot.Child("email").Value.ToString();
                        AddressText.text = orderSnapshot.Child("deliveryAddress").Value.ToString();
                        deleteButton.onClick.AddListener(() => DeleteOrder(userId, orderSnapshot.Key, orderObject));
                        // Идем по всем товарам в заказе
                        foreach (DataSnapshot itemSnapshot in orderSnapshot.Child("items").Children)
                        {
                            string itemName = itemSnapshot.Child("itemName").Value.ToString();
                            string itemQuantity = itemSnapshot.Child("quantity").Value.ToString();

                            // Создаем UI элемент для товара и размещаем его внутри orderObject
                            CreateItemElement(orderObject.transform, itemName, itemQuantity);  // Изменение здесь
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


    // Создание UI элемента для товара
    void CreateItemElement(Transform parent, string name, string quantity)
    {
        Debug.Log("создаю элемент товара");
        GameObject newItem = Instantiate(itemPrefab, parent);  // Изменение здесь

        TMP_Text nameText = newItem.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text quantityText = newItem.transform.Find("Quantity").GetComponent<TMP_Text>();

        nameText.text = name;
        quantityText.text = quantity;
    }
    // Удаление заказа из БД и UI
    void DeleteOrder(string userId, string orderId, GameObject orderObject)
    {
        DatabaseReference orderRef = FirebaseDatabase.DefaultInstance.GetReference($"carts/{userId}/{orderId}");
        orderRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Заказ удален: " + orderId);
                Destroy(orderObject);  // Это удалит заказ и все вложенные товары
            }
            else
            {
                Debug.LogError("Ошибка при удалении заказа: " + task.Exception);
            }
        });
    }
}
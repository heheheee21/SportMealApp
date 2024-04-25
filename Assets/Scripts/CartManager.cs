using Firebase.Auth;
using Firebase.Database;
using Michsky.MUIP;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class CartItem
{
    public GameObject itemPrefab;
    public int quantity;
    public float price;
    public string itemName; // Добавьте поле для хранения имени товара
    public Sprite itemImage; // Добавьте это поле для хранения изображения товара

    // Измените конструктор, чтобы он также принимал изображение
    public CartItem(GameObject prefab, float itemPrice, string itemName, Sprite image)
    {
        itemPrefab = prefab;
        quantity = 1;
        price = itemPrice;
        this.itemName = itemName; // Инициализируйте имя товара
        itemImage = image; // Сохраните изображение товара
    }

}
public class CartDataa
{
    public string email;
    public string deliveryAddress;
    public List<CartItem> items;
    // Конструктор по умолчанию
    public CartDataa() { }

    // Конструктор с параметрами
    public CartDataa(string email, string deliveryAddress, List<CartItem> items)
    {
        this.email = email;
        this.deliveryAddress = deliveryAddress;
        this.items = items;
    }
}




public class CartManager : MonoBehaviour
{
    public NotificationManager NotirificationError;
    public List<CartItem> cartItems; // Список всех товаров в корзине
    [Header("Данные заказа")]
    public TMP_InputField deliveryAddressInputField; // Поле для ввода адреса доставки
    public static CartManager Instance; // Singleton для доступа к менеджеру корзины
    public GameObject cartItemPrefab; // Префаб UI элемента корзины
    public Transform cartItemsContainer; // Родительский объект, куда будут помещаться элементы корзины в UI
    public TMP_Text totalQuantityText; // Добавь ссылку на текстовый объект в инспекторе
    public TMP_Text totalPriceText; // Добавь ссылку на текстовый объект в инспекторе
    public ButtonManager ButtonCart; // Добавь ссылку на текстовый объект в инспекторе
    public TMP_Text emptyCartText; // Добавьте ссылку на текстовый объект в инспекторе

    private float totalPrice = 0; // Общая сумма покупки
    public delegate void QuantityChanged(GameObject itemPrefab, int newQuantity);
    public event QuantityChanged OnQuantityChanged;

    void Awake()
    {
        UpdateCartUI();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Позволяет объекту не уничтожаться при загрузке новой сцены
        }
        else
        {
            Destroy(gameObject);
        }

        cartItems = new List<CartItem>();
        // Установите видимость текстовых элементов сразу
        if (totalQuantityText != null)
            totalQuantityText.gameObject.SetActive(false);
        if (totalPriceText != null)
            totalPriceText.gameObject.SetActive(false);
        if (ButtonCart != null)
            ButtonCart.gameObject.SetActive(false);
        if (emptyCartText != null)
            emptyCartText.gameObject.SetActive(true); // Показываем текст пустой корзины при старте

        UpdateCartUI(); // Вызываем обновление UI
    }

    // Словарь для связи префабов товаров с их контроллерами
    private Dictionary<GameObject, ItemController> itemControllers = new Dictionary<GameObject, ItemController>();

    // Метод для регистрации ItemController при их создании
    public void RegisterItemController(GameObject itemPrefab, ItemController controller)
    {
        if (!itemControllers.ContainsKey(itemPrefab))
        {
            itemControllers.Add(itemPrefab, controller);
        }
    }

    // Добавление товара в корзину
    public void AddToCart(GameObject itemPrefab, float price, Sprite image)
    {
        Debug.Log("Добавление в корзину: " + itemPrefab.name);
        CartItem existingItem = cartItems.Find(x => x.itemPrefab == itemPrefab);
        // Прежде всего, проверим, действительно ли itemPrefab - это корневой объект staffHome
        Transform nameTransform = itemPrefab.transform.parent.Find("Name");

        TMP_Text nameTextComponent = nameTransform.GetComponent<TMP_Text>();
        if (nameTextComponent == null)
        {
            Debug.LogError("Не найден компонент TMP_Text на объекте 'Name'.");
            return;
        }

        string itemName = nameTextComponent.text;
        if (existingItem != null)
        {
            existingItem.quantity++;
            Debug.Log("Товар уже в корзине, новое количество: " + existingItem.quantity);
        }
        else
        {

            CartItem newItem = new CartItem(itemPrefab, price, itemName, image);
            cartItems.Add(newItem);
            Debug.Log("Новый товар добавлен в корзину: " + itemPrefab.name);
            OnQuantityChanged?.Invoke(itemPrefab, newItem.quantity); // Важно вызвать событие здесь
        }
        UpdateCartUI();
    }

    // Удаление товара из корзины
    public void RemoveFromCart(GameObject itemPrefab)
    {
        CartItem existingItem = cartItems.Find(x => x.itemPrefab == itemPrefab);

        if (existingItem != null)
        {
            existingItem.quantity--;
            if (existingItem.quantity <= 0)
            {
                cartItems.Remove(existingItem); // Если количество товара стало 0, удаляем его из списка
            }
        }
    }

    // Метод для увеличения количества товара в корзине
    public void IncreaseItemQuantity(GameObject itemPrefab)
    {
        var item = cartItems.Find(x => x.itemPrefab == itemPrefab);
        if (item != null)
        {
            item.quantity++;
            OnQuantityChanged?.Invoke(itemPrefab, item.quantity); // Уведомляем подписчиков о изменении количества
            UpdateCartUI();
        }
    }

    public void DecreaseItemQuantity(GameObject itemPrefab)
    {
    var item = cartItems.Find(x => x.itemPrefab == itemPrefab);
    if (item != null)
    {
        item.quantity--;
        OnQuantityChanged?.Invoke(itemPrefab, item.quantity); // Обеспечьте вызов события здесь
        if (item.quantity <= 0)
        {
            RemoveItemFromCart(item);
        }
        UpdateCartUI();
    }
}

    public int GetQuantity(GameObject itemPrefab)
    {
        CartItem item = cartItems.Find(x => x.itemPrefab == itemPrefab);
        return item != null ? item.quantity : 0;
    }

    // Метод для полного удаления товара из корзины
    public void RemoveItemFromCart(CartItem item)
    {
        cartItems.Remove(item);
        UpdateCartUI();
    }

    // Метод для обновления UI корзины (предстоит реализовать)
    public void UpdateCartUI()
    {
        int totalQuantity = 0;
        totalPrice = 0;
        // Удалить текущие элементы интерфейса корзины
        foreach (Transform child in cartItemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Создать новые элементы интерфейса корзины
        foreach (CartItem cartItem in cartItems)
        {
            GameObject newItem = Instantiate(cartItemPrefab, cartItemsContainer);
            TMP_Text nameText = newItem.transform.Find("Name").GetComponent<TMP_Text>();
            TMP_Text priceText = newItem.transform.Find("Price").GetComponent<TMP_Text>();
            TMP_Text quantityText = newItem.transform.Find("QuantityControl/Quantity").GetComponent<TMP_Text>();
            Image itemImageComponent = newItem.transform.Find("Image").GetComponent<Image>();
            itemImageComponent.sprite = cartItem.itemImage;
            newItem.transform.Find("ButtonCart").gameObject.SetActive(false);
            var quantityControl = newItem.transform.Find("QuantityControl");
            if (quantityControl == null)
            {
                Debug.LogError("QuantityControl не найден!");
            }
            else
            {
                quantityControl.gameObject.SetActive(true);
            }


            nameText.text = cartItem.itemName;
            priceText.text = $"{cartItem.price} ₽";
            quantityText.text = cartItem.quantity.ToString();
            totalQuantity += cartItem.quantity;
            totalPrice += cartItem.quantity * cartItem.price;
            
            
            ButtonManager buttonPlus = newItem.transform.Find("QuantityControl/ButtonPlus")?.GetComponent<ButtonManager>();
            ButtonManager buttonMinus = newItem.transform.Find("QuantityControl/ButtonMinus")?.GetComponent<ButtonManager>();


            buttonPlus.onClick.AddListener(() => IncreaseItemQuantity(cartItem.itemPrefab));
            buttonMinus.onClick.AddListener(() => DecreaseItemQuantity(cartItem.itemPrefab));
        }
        totalQuantityText.gameObject.SetActive(totalQuantity > 0);
        totalPriceText.gameObject.SetActive(totalQuantity > 0);
        // Обновляем текстовые поля для общего количества и стоимости
        if (totalQuantityText != null)
            totalQuantityText.text = $"{totalQuantity}";
        if (totalPriceText != null)
            totalPriceText.text = $"Сумма покупки: {totalPrice} ₽";
        if (ButtonCart != null)
        {
            ButtonCart.gameObject.SetActive(totalQuantity > 0);
        }

        if (emptyCartText != null)
            emptyCartText.gameObject.SetActive(totalQuantity == 0);
        foreach (var cartItem in cartItems)
        {
            if (itemControllers.TryGetValue(cartItem.itemPrefab, out ItemController controller))
            {
                controller.UpdateQuantityText(cartItem.quantity); // Обновляем UI с новым количеством
            }
        }
    }
    public void SaveCartData(string deliveryAddress)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            CartDataa cartData = new CartDataa
            {
                email = user.Email,
                deliveryAddress = deliveryAddress,
                items = cartItems
            };

            string json = JsonUtility.ToJson(cartData);
            // Получаем ссылку на базу данных
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("carts");
            // Генерируем новый уникальный ключ для заказа
            string key = reference.Child(user.UserId).Push().Key;
            // Сохраняем данные заказа под уникальным ключом
            reference.Child(user.UserId).Child(key).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Ошибка при сохранении заказа: " + task.Exception.ToString());
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Заказ сохранен в Firebase под ключом: " + key);
                }
            });
        }
        else
        {
            Debug.LogError("Пользователь не авторизован");
        }
    }

    public void OnSaveCartButtonPressed()
    {
        string deliveryAddress = deliveryAddressInputField.text; // Получаем адрес из поля ввода
        if (!string.IsNullOrEmpty(deliveryAddress) && deliveryAddress.Length > 5) // Пример проверки длины адреса
        {
            SaveCartData(deliveryAddress); // Сохраняем данные корзины с адресом доставки
        }
        else
        {
            NotirificationError.Open();
            Debug.LogError("Введите корректный адрес доставки.");
            // Вы можете также здесь обновить UI, чтобы сообщить пользователю о необходимости ввести адрес
        }
    }



}
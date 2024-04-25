using UnityEngine;
using UnityEngine.UI;
using Michsky.MUIP;
using TMPro;

public class ItemController : MonoBehaviour
{
    public ButtonManager buttonCart; // Ссылка на кнопку добавления в корзину
    public GameObject quantityControl; // Ссылка на контроллер количества
    public ButtonManager buttonMinus; // Ссылка на кнопку уменьшения количества
    public ButtonManager buttonPlus; // Ссылка на кнопку увеличения количества
    public TMP_Text textQuantity; // Ссылка на текстовое поле количества
    public float price;
    public Image itemImage;
    private int quantity = 1; // Начальное количество товара

    void Start()
    {
        Debug.Log("Метод Start вызван.");
        buttonCart.onClick.AddListener(ActivateQuantityControl); // Подписываемся на события нажатия кнопок
        buttonMinus.onClick.AddListener(DecreaseQuantity);
        buttonPlus.onClick.AddListener(IncreaseQuantity);
        CartManager.Instance.OnQuantityChanged += HandleQuantityChanged;

    }

    private void OnDestroy()
    {
        if (CartManager.Instance != null)
        {
            CartManager.Instance.OnQuantityChanged -= HandleQuantityChanged;
        }
    }

    public void SetData(float newPrice)
    {
        price = newPrice;
        // Тут можно обновить текст цены на UI, если он есть
        // priceText.text = price.ToString() + " ₽";
    }
    private void HandleQuantityChanged(GameObject itemPrefab, int newQuantity)
    {
        if (itemPrefab == gameObject) // Проверяем, что событие касается именно этого товара
        {
            UpdateQuantityText(newQuantity);
        }
    }
    void ActivateQuantityControl()
    {
        if (itemImage == null || itemImage.sprite == null)
        {
            Debug.LogError("Компонент изображения не настроен или не содержит sprite");
            return;
        }

        // Теперь передаём Sprite из компонента Image вместе с товаром
        CartManager.Instance.AddToCart(gameObject, price, itemImage.sprite);
        UpdateQuantityText(CartManager.Instance.GetQuantity(gameObject));
        buttonCart.gameObject.SetActive(false); // Деактивируем кнопку добавления в корзину
        quantityControl.SetActive(true); // Активируем контроллер количества
    }

    void IncreaseQuantity()
    {
        CartManager.Instance.IncreaseItemQuantity(gameObject);
        quantity = CartManager.Instance.GetQuantity(gameObject);  
        UpdateQuantityText(quantity);
    }

    void DecreaseQuantity()
    {
        CartManager.Instance.DecreaseItemQuantity(gameObject);
        quantity = CartManager.Instance.GetQuantity(gameObject); 
        if (quantity < 1)
        {
            DeactivateQuantityControl();
        }
        else
        {
            UpdateQuantityText(quantity);
        }
    }

    public void UpdateQuantityText(int newQuantity)
    {
        if (newQuantity <= 0)
        {
            // Товар удалён из корзины
            DeactivateQuantityControl();
        }
        else
        {
            quantity = newQuantity;
            textQuantity.text = quantity.ToString();
            // Убедитесь, что контроллеры количества активированы, если товар присутствует
            quantityControl.SetActive(true);
            buttonCart.gameObject.SetActive(false);
        }
    }

    void DeactivateQuantityControl()
    {
        quantityControl.SetActive(false); // Деактивируем контроллер количества
        buttonCart.gameObject.SetActive(true); // Активируем кнопку добавления в корзину
        quantity = 1; // Сбрасываем количество на 1
    }
}
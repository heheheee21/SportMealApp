using UnityEngine;
using UnityEngine.UI;
using TMPro; // Для работы с текстовыми полями TextMeshPro
using UnityEngine.Networking; // Для работы с UnityWebRequest и DownloadHandlerTexture
using System.Collections; // Для использования IEnumerator


public class ProductDisplay : MonoBehaviour
{
    public Image productImage;
    public TMP_Text productNameText;
    public TMP_Text productPriceText;
    public TMP_Text productDescriptionText;

    // Метод для установки данных товара
    public void SetProductData(Product productData)
    {
        // Здесь должен быть код для загрузки изображения из URL.
        // Это может потребовать дополнительный код или использование сторонней библиотеки для Unity.
        StartCoroutine(LoadImage(productData.ImageURL));

        productNameText.text = productData.Name;
        productPriceText.text = productData.Price.ToString() + " ₽";
        productDescriptionText.text = productData.Description;
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);

        yield return imageRequest.SendWebRequest();

        if (imageRequest.isNetworkError || imageRequest.isHttpError)
        {
            Debug.LogError("Ошибка загрузки изображения: " + imageRequest.error);
        }
        else
        {
            Texture2D tex = ((DownloadHandlerTexture)imageRequest.downloadHandler).texture;
            productImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}

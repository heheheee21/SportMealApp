using Firebase.Extensions;
using Firebase.Firestore;
using Michsky.MUIP;
using System.Collections.Generic;
using TMPro;
//using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class FirestoreLoader : MonoBehaviour
{
    public delegate void ProductsLoadedHandler(List<Product> products);
    public static event ProductsLoadedHandler ProductsLoaded;

    public Transform productsContainer; // ������������ ������� UI ��� �������
    public GameObject productPrefab; // ������ ������
    public CustomDropdown dropdown; // ������� ���������� Dropdown
    private Dictionary<string, GameObject> productObjects = new Dictionary<string, GameObject>();

    private void Start()
    {
        ProductsLoaded += OnProductsLoaded; // �������� �� ������� �������� �������
        LoadProductsFromFirestore(dropdown.index); // �������� ������� � ����������� �� ���������� ������� Dropdown

    }

    private void OnDestroy()
    {
        ProductsLoaded -= OnProductsLoaded;
    }

    public void ChangeDropdown()
    {
        LoadProductsFromFirestore(dropdown.index); // ��������� ������ � ����������� �� �������� ������ � Dropdown
    }

    // �������� ������ � ������� �� Firestore �� ���������
    private void LoadProductsFromFirestore(int categoryIndex)
    {
        string category = categoryIndex switch
        {
            0 => "protein",
            1 => "creatine",
            2 => "vitamin",
            _ => "All"
        };

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        Query query = db.Collection("staff");
        if (category != "All")
        {
            query = query.WhereEqualTo("category", category);
        }

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("������ ��� ��������� ������: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            List<Product> products = new List<Product>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Debug.Log($"Document {document.Id} data: {document.ToDictionary()}");

                Product newProduct = new Product
                {
                    Id = document.Id,
                    Name = document.GetValue<string>("name"),
                    Price = document.GetValue<double>("price"),
                    Description = document.GetValue<string>("description"),
                    ImageURL = document.GetValue<string>("image")
                };
                Debug.Log("������");

                products.Add(newProduct);
            }

            ProductsLoaded?.Invoke(products);
        });
    }
    private void OnProductsLoaded(List<Product> products)
    {
        foreach (Product product in products)
        {
            if (!productObjects.TryGetValue(product.Id, out GameObject existingProduct))
            {
                GameObject productObject = Instantiate(productPrefab, productsContainer);
                productObject.SetActive(true);
                ItemController itemController = productObject.transform.Find("CartController").GetComponent<ItemController>();

                if (itemController != null)
                {
                    itemController.SetData((float)product.Price);
                }

                ProductDisplay display = productObject.GetComponentInChildren<ProductDisplay>();
                if (display != null)
                {
                    display.SetProductData(product);
                }

                productObjects[product.Id] = productObject; // ��������� ������ � �������
            }
            else
            {
                existingProduct.SetActive(true); // ���������� ������������ ������
            }
        }

        // �������� ������� �� ����������� � ������� ���������
        foreach (var kvp in productObjects)
        {
            if (!products.Exists(p => p.Id == kvp.Key))
            {
                kvp.Value.SetActive(false);
            }
        }
    }

}

public class Product
{
    public string Id;
    public string Name;
    public double Price;
    public string Description;
    public string ImageURL;
    public string Category; // �������� ���� ��� ���������
}

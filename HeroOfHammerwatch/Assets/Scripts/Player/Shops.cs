using System;
using UnityEngine;

public class Shops : MonoBehaviour
{
    [Serializable]
    public class ShopPlace
    {
        public GameObject place;
        public GameObject ui;
	}

	[Header("Collision Objects")]
    [SerializeField] private ShopPlace[] objects;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		foreach (var shop in objects)
        {
            if (collision.gameObject == shop.place)
            {
                shop.ui.SetActive(true);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
			}
		}
	}

    private void OnCollisionExit2D(Collision2D collision)
    {
        foreach (var shop in objects)
        {
            if (collision.gameObject == shop.place)
            {
                shop.ui.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
			}
        }
	}
}

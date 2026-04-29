using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{

    public class PropsAltar : MonoBehaviour
    {
        [SerializeField] Button btn;

        public List<SpriteRenderer> runes;
        public float lerpSpeed;

        private Color curColor;
        private Color targetColor;

        private void Awake()
        {
            targetColor = runes[0].color;
        }

        private void Start()
        {
            targetColor.a = 0.0f;
            btn.gameObject.SetActive(false);
		}

		private void OnTriggerEnter2D(Collider2D other)
        {
            targetColor.a = 1.0f;
            btn.gameObject.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            targetColor.a = 0.0f;
            btn.gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            curColor = Color.Lerp(curColor, targetColor, lerpSpeed * Time.deltaTime);

            foreach (var r in runes)
            {
                r.color = curColor;
            }
        }
    }
}

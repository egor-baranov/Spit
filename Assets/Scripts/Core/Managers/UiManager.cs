using UnityEngine;
using UnityEngine.UI;

namespace Core.Managers {
    public class UiManager : MonoBehaviour {
        public static UiManager Instance { get; private set; }

        private Text ScoreText => deathPanel.transform.Find("Score").GetComponent<Text>();

        [SerializeField] private GameObject deathPanel;

        public void OnDeath(int score) {
            deathPanel.SetActive(true);
            ScoreText.text = $"Score: {score}";
        }

        public void OnRestart() => GameManager.OnRestart();

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;
        }
    }
}
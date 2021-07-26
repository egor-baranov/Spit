using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Managers {
    public class UiManager : MonoBehaviour {
        public static UiManager Instance { get; private set; }

        private Text ScoreText => deathPanel.transform.Find("Score").GetComponent<Text>();


        [SerializeField] private GameObject deathPanel;

        [SerializeField] private Text currentScoreText;

        public void OnDeath(int score) {
            deathPanel.SetActive(true);
        }

        public void DisplayScore(int score, int sec, int kills, int shots) {
            currentScoreText.text = $"Score: {score} ({sec} sec, {kills} kills, {shots} shots)";
            ScoreText.text = $"Score: {score}\n" +
                             $"Time in game: {sec}\n" +
                             $"Enemies killed: {kills}\n" +
                             $"Soul shots performed: {shots}";
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
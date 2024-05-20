using System.Collections;
using BeatWalker.Config;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatWalker
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI health;
        
        private TimingManager _tm;
        private int _totalPoints;
        private int _currentHealth;
        
        private void Awake()
        {
            _tm = FindObjectOfType<TimingManager>();
        }

        private void Start()
        {
            _totalPoints = 0;
            _currentHealth = gameConfig.PlayerHealth;
            score.text = $"Score: {_totalPoints}";
            health.text = $"Health: {_currentHealth}";
            StartCoroutine(OnStartPlaying(gameConfig.SongDelay));
        }

        private IEnumerator OnStartPlaying(float delay)
        {
            yield return new WaitForSeconds(delay);
            _tm.StartPlaying();
        }

        public void GameCompleted(float delay)
        {
            StartCoroutine(CompleteGameAfterDelay(delay));
        }

        private static IEnumerator CompleteGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("Game Completed");
        }

        public void AddPoints(int points)
        {
            _totalPoints += points;
            score.text = $"Score: {_totalPoints}";
        }

        public void DecrementHealth()
        {
            _currentHealth--;
            health.text = $"Health: {_currentHealth}";
            if (_currentHealth <= 0)
                SceneManager.LoadScene("GameOver");
        }
    }
}
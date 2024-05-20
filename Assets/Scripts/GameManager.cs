using System.Collections;
using BeatWalker.Config;
using UnityEngine;

namespace BeatWalker
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        
        private TimingManager _tm;
        
        private void Awake()
        {
            _tm = FindObjectOfType<TimingManager>();
        }

        private void Start()
        {
            StartCoroutine(OnStartPlaying(gameConfig.SongDelay));
        }

        private IEnumerator OnStartPlaying(float delay)
        {
            yield return new WaitForSeconds(delay);
            _tm.StartPlaying();
        }
    }
}
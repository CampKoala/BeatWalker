using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatWalker.Config;
using BeatWalker.Utils;
using UnityEngine;

namespace BeatWalker
{
    public class TimingManager : MonoBehaviour
    {
        [SerializeField] private string song;
        [SerializeField] private Player player;
        [SerializeField] private Target target;
        [SerializeField] [Min(1)] private int reserveCapacity;
        [SerializeField] [Min(1)] private int batchSize; // The number of lines/enemies to queue up per batch
        [SerializeField] [Min(1)] private int minLinesQueued; // The minimum numbers of lines that are currently queued
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private AudioSource audioSource;
        
        private SongTimingConfig _songTimingConfig;
        private Queue<Line> _reserveLines;
        private Queue<Enemy> _reserveTapEnemies;
        private Queue<Enemy> _reserveHoldEnemies;

        private float _startTime;
        private int _index;

        private Vector2 _targetPosition;
        private Vector2 _playerPosition;
        private float _lineTravelTime;
        
        public float CurrentTime => Time.timeSinceLevelLoad - _startTime;
        
        private void Awake()
        {
            _songTimingConfig = new SongTimingConfig(song);
            target.Init(this, gameConfig);
            
            _playerPosition = player.transform.position;
            _targetPosition = target.transform.position;
            _lineTravelTime = (gameConfig.LineStartY - _targetPosition.y) / gameConfig.LineSpeed;
            _reserveLines = new Queue<Line>(Enumerable.Range(0, reserveCapacity).Select(_ => CreateNewLine()));
            _reserveTapEnemies = new Queue<Enemy>(Enumerable.Range(0, reserveCapacity)
                .Select(_ => CreateNewEnemy(gameConfig.TapEnemy)));
            _reserveHoldEnemies = new Queue<Enemy>(Enumerable.Range(0, reserveCapacity)
                .Select(_ => CreateNewEnemy(gameConfig.HoldEnemy)));
        }

        public void StartPlaying()
        {
            audioSource.Play();
            _startTime = Time.timeSinceLevelLoad;
            QueueNextLines();
        }

        private void QueueNextLines()
        {
            if (_index >= _songTimingConfig.Count && _reserveLines.Count == reserveCapacity)
            {
                Debug.Log("End of Song");
                return;
            }
    
            if (reserveCapacity - _reserveLines.Count > minLinesQueued)
                return;
            
            for (var i = 0; i < batchSize; ++i)
            {
#if UNITY_EDITOR
                while (_index < _songTimingConfig.Count && _songTimingConfig[_index].Time < _lineTravelTime)
                {
                    Debug.LogWarning($"Skipping Line {_index}, Time: {_songTimingConfig[_index].Time}, Line Travel Time: {_lineTravelTime}");
                    _index++;
                }
#endif

                if (_index >= _songTimingConfig.Count)
                    break;
                
                Debug.Assert(_reserveHoldEnemies.Count > 0 && _reserveTapEnemies.Count > 0 && _reserveLines.Count > 0, "Need to increase reserve objects");
                StartCoroutine(NextLine(_songTimingConfig[_index++]));
            }
        }

        private IEnumerator NextLine(SongTiming timing)
        {
            var line = _reserveLines.Dequeue();
            var (enemy, config) = timing.Type switch
            {
                SongTimingType.Hold => (_reserveHoldEnemies.Dequeue(), gameConfig.HoldEnemy),
                SongTimingType.LeftTap => (_reserveTapEnemies.Dequeue(), gameConfig.TapEnemy),
                SongTimingType.RightTap => (_reserveTapEnemies.Dequeue(), gameConfig.TapEnemy),
                _ => throw new ArgumentOutOfRangeException()
            };
                
            enemy.Prepare(Vector2Utils.FromPolar(_playerPosition, config.EnemySpawnRadius, config.EnemyAngle * (float) timing.Type));
            line.Prepare(timing, enemy);
            
            var waitTime = timing.Time - _lineTravelTime - CurrentTime;
            Debug.Assert(waitTime > 0.0f);
            yield return new WaitForSeconds(waitTime);
            
            enemy.Go();
            line.Go();
        }

        public void Return(Line line)
        {
            _reserveLines.Enqueue(line);
            QueueNextLines();
        }

        public void Return(Enemy enemy)
        {
            switch (enemy.Type)
            {
                case EnemyType.Hold:
                    _reserveHoldEnemies.Enqueue(enemy);
                    break;
                
                case EnemyType.Tap:
                    _reserveTapEnemies.Enqueue(enemy);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        } 

        private Line CreateNewLine()
        {
            var obj = Instantiate(gameConfig.LinePrefab);
            var line = obj.GetComponent<Line>();
            line.Init(this, gameConfig.TapLineLength, gameConfig.LineStartY, gameConfig.LineEndY, gameConfig.LineSpeed, gameConfig.MissedTimeOffset);
            return line;
        }

        private Enemy CreateNewEnemy(EnemyConfig config)
        {
            var obj = Instantiate(config.EnemyPrefab);
            var enemy = obj.GetComponent<Enemy>();
            var speed = (config.EnemySpawnRadius - config.EnemyDeathRadius) / _lineTravelTime;
            enemy.Init(this, _playerPosition, speed, config.Type);
            return enemy;
        }
    }
}
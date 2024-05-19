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
        [SerializeField] [Min(1)] private int reserveCapacity;
        [SerializeField] [Min(1)] private int batchSize; // The number of lines/enemies to queue up per batch
        [SerializeField] [Min(1)] private int minLinesQueued; // The minimum numbers of lines that are currently queued
        [SerializeField] private GameConfig gameConfig;
    
        private SongTimingConfig _songTimingConfig;
        private Queue<Line> _reserveLines;
        private Queue<Enemy> _reserveTapEnemies;
        private Queue<Enemy> _reserveHoldEnemies;

        private float _startTime;
        private int _index;

        private Vector2 _playerPosition;
        private float _lineTravelTime;

        private void Awake()
        {
            _songTimingConfig = new SongTimingConfig(song);
            _playerPosition = player.transform.position;
            _lineTravelTime = (gameConfig.LineStartY - _playerPosition.y) / gameConfig.LineSpeed;

            player.Init(gameConfig);
            
            _reserveLines = new Queue<Line>(Enumerable.Range(0, reserveCapacity).Select(_ => CreateNewLine()));
            _reserveTapEnemies = new Queue<Enemy>(Enumerable.Range(0, reserveCapacity).Select(_ => CreateNewEnemy(gameConfig.TapEnemy)));
            _reserveHoldEnemies = new Queue<Enemy>(Enumerable.Range(0, reserveCapacity).Select(_ => CreateNewEnemy(gameConfig.HoldEnemy)));
        }

        private void Start()
        {
            _startTime = Time.timeSinceLevelLoad;
            QueueNextLines();
        }

        private void QueueNextLines()
        {
            for (var i = 0; i < batchSize; ++i)
            {
                if (_index >= _songTimingConfig.Count || _reserveHoldEnemies.Count <= 0 || _reserveTapEnemies.Count <= 0 || _reserveLines.Count <= 0)
                    break;

                StartCoroutine(NextLine());
            }
        }

        private IEnumerator NextLine()
        {
            var timing = _songTimingConfig[_index++];
            var line = _reserveLines.Dequeue();
            var (enemy, config) = timing.Type switch
            {
                SongTimingConfig.TimingType.Hold => (_reserveHoldEnemies.Dequeue(), gameConfig.HoldEnemy),
                SongTimingConfig.TimingType.LeftTap => (_reserveTapEnemies.Dequeue(), gameConfig.TapEnemy),
                SongTimingConfig.TimingType.RightTap => (_reserveTapEnemies.Dequeue(), gameConfig.TapEnemy),
                _ => throw new ArgumentOutOfRangeException()
            };
                
            line.Prepare(timing, enemy);
            enemy.Prepare(Vector2Utils.FromPolar(_playerPosition, config.EnemySpawnRadius, config.EnemyAngle * (float) timing.Type));

            yield return new WaitForSeconds(Mathf.Clamp(
                timing.StartTime - Time.timeSinceLevelLoad - _startTime - _lineTravelTime, 0.0f, float.MaxValue));
            enemy.Go();
            line.Go();
        }

        public void Return(Line line)
        {
            _reserveLines.Enqueue(line);

            if (reserveCapacity - _reserveLines.Count <= minLinesQueued)
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
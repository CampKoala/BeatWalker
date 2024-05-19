using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatWalker.Config;
using BeatWalker.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

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
        private Queue<Enemy> _reserveEnemies;

        private float _startTime;
        private int _index;

        private Vector2 _playerPosition;
        private float _enemySpeed;
        private float _lineTravelTime;
        
        private bool _isLineEnter;
        private float _lineReferenceTime;
        private SongTimingConfig.TimingType _currentLineType;
        private bool _skipNextLineExit;

        private void Awake()
        {
            _songTimingConfig = new SongTimingConfig(song);
            _playerPosition = player.transform.position;

            _lineTravelTime = (gameConfig.LineStartY - _playerPosition.y) / gameConfig.LineSpeed;
            _enemySpeed = (gameConfig.EnemySpawnRadius - gameConfig.EnemyDeathRadius) / _lineTravelTime;

            player.Init(this);
            
            _reserveLines = new Queue<Line>(Enumerable.Range(0, reserveCapacity).Select(_ => CreateNewLine()));
            _reserveEnemies = new Queue<Enemy>(Enumerable.Range(0, reserveCapacity).Select(_ => CreateNewEnemy()));
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
                if (_index >= _songTimingConfig.Count || _reserveEnemies.Count <= 0 || _reserveLines.Count <= 0)
                    break;

                StartCoroutine(NextLine());
            }
        }

        private IEnumerator NextLine()
        {
            var timing = _songTimingConfig[_index++];
            var line = _reserveLines.Dequeue();
            var enemy = _reserveEnemies.Dequeue();

            line.Prepare(timing, enemy);
            enemy.Prepare(Vector2Utils.FromPolar(_playerPosition, gameConfig.EnemySpawnRadius, gameConfig.EnemyAngle * (float) timing.Type));

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

        public void Return(Enemy enemy) => _reserveEnemies.Enqueue(enemy);

        private Line CreateNewLine()
        {
            var obj = Instantiate(gameConfig.LinePrefab);
            var line = obj.GetComponent<Line>();
            line.Init(this, gameConfig.TapLineLength, gameConfig.LineStartY, gameConfig.LineEndY, gameConfig.LineSpeed, gameConfig.MissedTimeOffset);
            return line;
        }

        private Enemy CreateNewEnemy()
        {
            var obj = Instantiate(gameConfig.EnemyPrefab);
            var enemy = obj.GetComponent<Enemy>();
            enemy.Init(this, _playerPosition, _enemySpeed);
            return enemy;
        }

        public void OnLineEnter(SongTimingConfig.TimingType type)
        {
            _isLineEnter = true;
            _lineReferenceTime = Time.timeSinceLevelLoad;
            _currentLineType = type;
        }

        public void OnLineExit()
        {
            if (_skipNextLineExit)
            {
                _skipNextLineExit = false;
                return;
            }
            
            _isLineEnter = false;
            _lineReferenceTime = Time.timeSinceLevelLoad;
        }

        public void OnAttack(InputValue button)
        {
            var attackTime = Time.timeSinceLevelLoad - _lineReferenceTime;
            
            // Ignore Invalid Inputs
            if (_isLineEnter != button.isPressed || (_currentLineType != SongTimingConfig.TimingType.Hold && !button.isPressed))
                return;
            
            if (attackTime > gameConfig.MissedTimeOffset)
            {
                _skipNextLineExit = _isLineEnter && _currentLineType == SongTimingConfig.TimingType.Hold;
                Debug.Log($"Missed { (_isLineEnter ? "Line Enter": "Line Exit") }");
                return;
            }

            if (attackTime > gameConfig.LateTimeOffset)
            {
                Debug.Log("Late");
                return;
            }

            if (attackTime > gameConfig.EarlyTime)
            {
                Debug.Log("Perfect");
                return;
            }

            Debug.Log("Early");
        }
    }
}
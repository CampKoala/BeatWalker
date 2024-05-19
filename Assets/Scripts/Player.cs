using System;
using BeatWalker.Config;
using BeatWalker.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatWalker
{
    public class Player : MonoBehaviour
    {
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Hold = Animator.StringToHash("Hold");
        private static readonly int LeftTap = Animator.StringToHash("LeftTap");
        private static readonly int RightTap = Animator.StringToHash("RightTap");

        private bool _isLineEnter;
        private float _lineReferenceTime;
        private SongTimingConfig.TimingType _currentLineType;
        private Enemy _currentEnemy;
        private bool _skipNextLineExit;
        private GameConfig _gameConfig;
        private Animator _animator;

        public void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("LineEnter"))
                OnLineEnter(other.GetComponentInParent<Line>());

            if (other.CompareTag("LineExit"))
                OnLineExit();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("LineExit"))
            {
                _animator.SetBool(Hold, false);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag("Enemy"))
            {
                _animator.SetTrigger(Hit);
            }
        }

        public void Init(GameConfig config)
        {
            _gameConfig = config;
        }

        private void OnLineEnter(Line line)
        {
            _isLineEnter = true;
            _lineReferenceTime = Time.timeSinceLevelLoad;
            _currentLineType = line.TimingType;
            _currentEnemy = line.Enemy;
        }

        private void OnLineExit()
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
            // Ignore Invalid Inputs
            if (_isLineEnter != button.isPressed || (_currentLineType != SongTimingConfig.TimingType.Hold && !button.isPressed))
                return;
            
            var attackTime = Time.timeSinceLevelLoad - _lineReferenceTime;
            
            if (attackTime > _gameConfig.MissedTimeOffset)
            {
                _skipNextLineExit = _isLineEnter && _currentLineType == SongTimingConfig.TimingType.Hold;
                Debug.Log($"Missed { (_isLineEnter ? "Line Enter": "Line Exit") }");
                return;
            }

            // By this point the player has successfully tapped the line
            // TODO: Might want to use the projectile to damage the enemy
            _currentEnemy.OnDamage();
            
            // Start Playing the appropriate animation
            if (_currentLineType == SongTimingConfig.TimingType.Hold)
            {
                _animator.SetBool(Hold, button.isPressed);
            }
            else
            {
                _animator.SetTrigger(_currentLineType switch
                {
                    SongTimingConfig.TimingType.LeftTap => LeftTap,
                    SongTimingConfig.TimingType.RightTap => RightTap,
                    SongTimingConfig.TimingType.Hold => throw new ArgumentOutOfRangeException(),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            
            // Check for timing
            if (attackTime > _gameConfig.LateTimeOffset)
            {
                Debug.Log("Late");
                return;
            }

            if (attackTime > _gameConfig.EarlyTime)
            {
                Debug.Log("Perfect");
                return;
            }

            Debug.Log("Early");
        }
    }
}
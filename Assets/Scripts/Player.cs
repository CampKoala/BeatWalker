using System;
using System.Collections.Generic;
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
        private Queue<Line> _lines = new();
        private Line _currentLine;
        private Enemy _currentEnemy;
        private bool _skipNextLineExit;
        private GameConfig _gameConfig;
        private Animator _animator;

        public void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void Init(GameConfig config)
        {
            _gameConfig = config;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var line = other.GetComponentInParent<Line>();
            
            if (other.CompareTag("LineEnter"))
                OnLineEnter(line);
            else if (other.CompareTag("LineExit"))
                OnLineExit(line);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var line = other.GetComponentInParent<Line>();
            
            if (other.CompareTag("LineEnter") && _currentLine is not null && line != _currentLine)
                _lines.Remove(line); // Remove future line if current line hasn't been handled
            else if (other.CompareTag("LineExit") && line == _currentLine)
                HandleReleaseHold(true); // Automatically release if player missed release
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag("Enemy"))
                _animator.SetTrigger(Hit);
        }
        
        private void OnLineEnter(Line line)
        {
            _isLineEnter = true;
            _lineReferenceTime = Time.timeSinceLevelLoad;
            _lines.Enqueue(line);
        }

        private void OnLineExit(Line line)
        {
            if (line != _currentLine)
                return;
            
            if (_skipNextLineExit)
            {
                _skipNextLineExit = false;
            }
            else
            {
                _isLineEnter = false;
                _lineReferenceTime = Time.timeSinceLevelLoad;
            }
        }
        
        public void OnAttack(InputValue button)
        {
            if (_currentEnemy is null)
            {
                if (_lines.Count == 0)
                    return;

                _currentLine = _lines.Dequeue();
                _currentEnemy = _currentLine.Enemy;
            }
            
            // Ignore button release for taps
            if (_currentLine.Type != SongTimingConfig.TimingType.Hold && !button.isPressed)
                return;

            // Handle early release for holds
            if (_isLineEnter && !_skipNextLineExit && !button.isPressed)
            {
                _skipNextLineExit = true;
                _isLineEnter = false;
                HandleReleaseHold(false);
                Debug.Log("Released before line exit");
                return;
            }

            var attackTime = Time.timeSinceLevelLoad - _lineReferenceTime;
            
            if (attackTime > _gameConfig.MissedTimeOffset)
            {
                _skipNextLineExit = _isLineEnter && _currentLine.Type == SongTimingConfig.TimingType.Hold;
                Debug.Log($"Missed {(_isLineEnter ? "Line Enter" : "Line Exit")}");
            }
            else
            {
                HandleAttack(button);
                HandleAwardPoints(attackTime);
            }
        }

        private void HandleAttack(InputValue button)
        {
            Debug.Assert(_currentEnemy, "No enemy found to damage");

            switch (_currentLine.Type)
            {
                case SongTimingConfig.TimingType.Hold when button.isPressed:
                    _animator.SetBool(Hold, true);
                    _currentEnemy.OnDamage();
                    break;
                case SongTimingConfig.TimingType.Hold when !button.isPressed:
                    HandleReleaseHold(true);
                    break;
                case SongTimingConfig.TimingType.LeftTap:
                case SongTimingConfig.TimingType.RightTap:
                    _animator.SetTrigger(_currentLine.Type switch
                    {
                        SongTimingConfig.TimingType.LeftTap => LeftTap,
                        SongTimingConfig.TimingType.RightTap => RightTap,
                        SongTimingConfig.TimingType.Hold => throw new ArgumentOutOfRangeException(),
                        _ => throw new ArgumentOutOfRangeException()
                    });
                    _currentEnemy.OnDamage();
                    _currentEnemy = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void HandleReleaseHold(bool enemyDies)
        {
            if (_currentEnemy is null)
                return;
            
            _animator.SetBool(Hold, false);
            _currentEnemy.OnStopDamage(enemyDies);
            _currentEnemy = null;
        }
        
        private void HandleAwardPoints(float attackTime)
        {
            if (attackTime < _gameConfig.EarlyTime)
            {
                Debug.Log("Early");
            }
            else if (attackTime < _gameConfig.LateTimeOffset)
            {
                Debug.Log("Perfect");
            }
            else
            {
                Debug.Log("Late");
            }
        }
    }
}
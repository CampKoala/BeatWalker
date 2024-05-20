using System;
using System.Collections.Generic;
using BeatWalker.Config;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatWalker
{
    public class Target : MonoBehaviour
    {
        [SerializeField] private Player player;
        
        private GameConfig _gameConfig;
        private TimingManager _tm;

        private readonly List<(Line line, float time, bool isEnter)> _lines = new();
        private Line _currentLine;

        public void Init(TimingManager timingManager, GameConfig config)
        {
            _tm = timingManager;
            _gameConfig = config;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var line = other.GetComponentInParent<Line>();

            if (other.CompareTag("LineEnter"))
            {
                _lines.Add((line, _tm.CurrentTime, true));
            }
            else if (other.CompareTag("LineExit"))
            {
                _lines.Add((line, _tm.CurrentTime, false));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {

            if (other.CompareTag("LineEnter")) {
                var line = other.GetComponentInParent<Line>();
                if (!line || line == _currentLine) return;
                Debug.Log($"[Line {line.TimingIndex}] Remove missed line from queue: {line.Type}");
                _lines.RemoveAll(l => l.line == line); // Remove future line if current line hasn't been handled
            }
            else if (other.CompareTag("LineExit"))
            {
                var line = other.GetComponentInParent<Line>();
                if (!line || line != _currentLine) return;
                
                Debug.Log($"[Line {line.TimingIndex}] Automatically release hold");
                _lines.RemoveAll(l => l.line == line);
                HandleReleaseHold(line, true); // Automatically release if player missed release
            }
        }

        public void OnAttack(InputValue button)
        {
            Line line;
            float referenceTime;
            bool isLineEnter;
            
            if (_currentLine is not null)
            {
                // Handle early release for holds
                if (_lines.Count == 0)
                {
                    Debug.Log($"[Line {_currentLine.TimingIndex}] Missed Line Exit, released too early");
                    HandleReleaseHold(_currentLine, false);
                    return;
                }
                
                (line, referenceTime, isLineEnter) = _lines[0];

                if (line != _currentLine)
                {
                    Debug.Log($"[Line {_currentLine.TimingIndex}] Released in somebody else's line exit ({line.TimingIndex})");
                    _currentLine = null;
                    return;
                }
                
                Debug.Assert(!isLineEnter, $"[Line {line.TimingIndex}] This should always be a line exit");
            }
            else
            {
                // TODO: This looks gross, make it better
                if (_lines.Count == 0)
                {
                    if (button.isPressed)
                        Debug.Log("Missed, no line currently");
                    return;
                }
                
                (line, referenceTime, isLineEnter) = _lines[0];
                while (_lines.Count > 0 && !isLineEnter)
                {
                    Debug.Log($"[Line {line.TimingIndex}] Missed the line enter for this line");
                    _lines.RemoveAt(0);
                    
                    if (_lines.Count == 0)
                    {
                        if (button.isPressed)
                            Debug.Log("Missed, no line currently");
                        return;
                    }
                    (line, referenceTime, isLineEnter) = _lines[0];
                }

                if (_lines.Count == 0)
                {
                    if (button.isPressed)
                        Debug.Log("Missed, no line currently");
                    return;
                }
                
                (line, referenceTime, isLineEnter) = _lines[0];
                Debug.Assert(isLineEnter, $"[Line {line.TimingIndex}] This should always be a line enter");
            }
    
            if (isLineEnter != button.isPressed)
                return;
            
            _lines.RemoveAt(0);
            var attackTime = _tm.CurrentTime - referenceTime;

            if (attackTime > _gameConfig.MissedTimeOffset)
            {
                Debug.Log($"[Line {line.TimingIndex}] Missed {(isLineEnter ? "Line Enter" : "Line Exit")}");
                if (!isLineEnter) HandleReleaseHold(line, true);
                _currentLine = null;
            }
            else
            {
                HandleAttack(line, button);
                HandleAwardPoints(attackTime);
            }
        }

        private void HandleAttack(Line line, InputValue button)
        {
            Debug.Assert(line.isActiveAndEnabled, "Line is inactive during attack");
            Debug.Assert(line.Enemy, "No enemy found to damage");

            switch (line.Type)
            {
                case SongTimingType.Hold when button.isPressed:
                    HandleHold(line);
                    break;
                case SongTimingType.Hold when !button.isPressed:
                    HandleReleaseHold(line, true);
                    break;
                case SongTimingType.LeftTap:
                case SongTimingType.RightTap:
                    HandleTap(line);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleTap(Line line)
        {
            player.Tap(line.Type);
            line.Enemy.OnDamage();
            line.Return();
            _currentLine = null;
        }

        private void HandleHold(Line line)
        {
            player.Hold(true);
            line.Enemy.OnDamage();
            _currentLine = line;
        }

        private void HandleReleaseHold(Line line, bool enemyDies)
        {
            player.Hold(false);
            line.Enemy.OnStopDamage(enemyDies);
            _currentLine = null;
        }

        private void HandleAwardPoints(float attackTime)
        {
            if (attackTime < _gameConfig.EarlyTime)
            {
                // Debug.Log("Early");
            }
            else if (attackTime < _gameConfig.LateTimeOffset)
            {
                // Debug.Log("Perfect");
            }
            else
            {
                // Debug.Log("Late");
            }
        }
    }
}
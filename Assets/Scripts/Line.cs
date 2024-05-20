using System;
using BeatWalker.Config;
using UnityEngine;

namespace BeatWalker
{
    public class Line : MonoBehaviour
    {
        private float _tapLength;
        private float _speed;
        private float _reactionTime;

        private TimingManager _tm;
        private Transform _transform;
        private BoxCollider2D _lineEnterCollider;
        private BoxCollider2D _lineExitCollider;

        private float _startY;
        private float _endY;
        private Vector2 _endPosition;
        private bool _isGoing;

        public Enemy Enemy { get; private set; }
        public SongTimingType Type { get; private set; }
        public int TimingIndex { get; private set; }

        public void Init(TimingManager timingManager, float tapLength, float startY, float endY, float speed,
            float reactionTime)
        {
            _tm = timingManager;
            _tapLength = tapLength;
            _startY = startY;
            _endY = endY;
            _speed = speed;
            _reactionTime = reactionTime;
            _transform = transform;

            foreach (var c in GetComponentsInChildren<BoxCollider2D>())
            {
                if (c.CompareTag("LineEnter"))
                    _lineEnterCollider = c;
                else if (c.CompareTag("LineExit"))
                    _lineExitCollider = c;
            }

            if (!_lineEnterCollider || !_lineExitCollider)
                throw new Exception("Missing Enter and Exit Colliders");
        }

        private void Update()
        {
            if (!_isGoing)
                return;

            var current = _transform.position;

            if (Mathf.Approximately(current.y, _endPosition.y))
            {
                Return();
                return;
            }

            _transform.position = Vector2.MoveTowards(current, _endPosition, _speed * Time.deltaTime);
        }

        public void Prepare(SongTiming songTiming, Enemy enemy)
        {
            Debug.Assert(enemy, "An enemy was not set for this line");
            Enemy = enemy;
            
            Type = songTiming.Type;
            TimingIndex = songTiming.Index;
            var length = Type switch
            {
                SongTimingType.Hold => _speed * songTiming.Duration,
                SongTimingType.LeftTap => _tapLength,
                SongTimingType.RightTap => _tapLength,
                _ => throw new ArgumentOutOfRangeException()
            };
            UpdateTransform(length);
            UpdateColliders(Type, length);
            gameObject.SetActive(true);
        }

        private void UpdateTransform(float length)
        {
            _transform.localScale = new Vector2(_transform.localScale.x, length);
            _transform.position = new Vector2(0, _startY + length / 2);
            _endPosition = new Vector2(0, _endY - length / 2);
        }
        
        private void UpdateColliders(SongTimingType type, float length)
        {
            /* Note: Need to scale down the size because scaling the gameObject also scales the
             * colliders and the offset can be fixed to 0.5 because scaling the parent will
             * automatically scale the offset as well.
             */
            _lineEnterCollider.size = new Vector2(1, _reactionTime * _speed / length);
            _lineExitCollider.size = new Vector2(1, _reactionTime * _speed / length);

            if (type == SongTimingType.Hold)
            {
                _lineEnterCollider.offset = new Vector2(0, -0.5f);
                _lineExitCollider.offset = new Vector2(0, 0.5f);
                _lineExitCollider.enabled = true;
            }
            else
            {
                _lineEnterCollider.offset = Vector2.zero;
                _lineExitCollider.enabled = false;
            }
        }
        
        public void Go() => _isGoing = true;

        public void Return()
        {
            Debug.Assert(_isGoing, "A line should only be returned if it was going");
            gameObject.SetActive(false);
            _tm.Return(this);
            _isGoing = false;
        }
    }
}
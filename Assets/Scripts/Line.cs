using System;
using UnityEngine;

namespace BeatWalker
{
    public class Line : MonoBehaviour
    {
        private float _endY;
        private float _startY;
        private float _length;
        private float _speed;
        private TimingManager _tm;
        private bool _isGoing;
        private float _earlyTime;
        private float _reactionTime;

        private BoxCollider2D _lineEnterCollider;
        private BoxCollider2D _lineExitCollider;

        private void Awake()
        {
            foreach (var collider in GetComponentsInChildren<BoxCollider2D>())
            {
                if (collider.CompareTag("LineEnter"))
                    _lineEnterCollider = collider;
                else if (collider.CompareTag("LineExit"))
                    _lineExitCollider = collider;
            }
        }

        private void Update()
        {
            if (!_isGoing)
                return;

            var oldPosition = transform.position;

            if (oldPosition.y < _endY - _length / 2)
            {
                NotGo();
                return;
            }

            transform.position = new Vector3(oldPosition.x, oldPosition.y - _speed * Time.deltaTime, 0.0f);
        }

        private void NotGo()
        {
            gameObject.SetActive(false);
            _tm.NotGo(this);
            _isGoing = false;
        }

        public void Go(float duration)
        {
            _length = _speed * duration;
            
            transform.localScale = new Vector3(transform.localScale.x, _length, 0.0f);
            transform.position = new Vector3(transform.position.x, _startY + _length / 2, 0.0f);
            _isGoing = true;
            gameObject.SetActive(true);
            UpdateColliders();
        }

        public void Init(TimingManager timingManager, float startY, float endY, float speed, float earlyTime,
            float reactionTime)
        {
            _tm = timingManager;
            _startY = startY;
            _endY = endY;
            _speed = speed;
            _earlyTime = earlyTime;
            _reactionTime = reactionTime;
        }

        private void UpdateColliders()
        {
            /* Note: Need to scale down the size because scaling the gameObject also scales the
             * colliders and the offset can be fixed to 0.5 because scaling the parent will
             * automatically scale the offset as well.
            */
            _lineEnterCollider.size = new Vector2(1, _reactionTime * _speed / _length);
            _lineExitCollider.size = new Vector2(1, _reactionTime * _speed / _length);
        }
    }
}
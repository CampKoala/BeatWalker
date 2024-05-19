using System;
using BeatWalker.Config;
using UnityEngine;

namespace BeatWalker
{
    public class Enemy : MonoBehaviour
    {
        private Vector2 _target;
        private float _speed;
        private TimingManager _tm;
        private bool _isGoing;
        private Transform _transform;
        private Animator _animator;

        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int Hit = Animator.StringToHash("Hit");

        public EnemyType Type { get; private set; }

        public void Init(TimingManager timingManager, Vector2 target, float speed, EnemyType type)
        {
            _tm = timingManager;
            _target = target;
            _speed = speed;
            Type = type;

            _animator = GetComponent<Animator>();
            _transform = transform;
        }

        public void Update()
        {
            if (!_isGoing)
                return;

            _transform.position = Vector2.MoveTowards(_transform.position, _target, _speed * Time.deltaTime);
        }

        public void Prepare(Vector2 startPosition)
        {
            _transform.position = startPosition;
            gameObject.SetActive(true);
        }

        public void Go() => _isGoing = true;

        private void Return()
        {
            gameObject.SetActive(false);
            _isGoing = false;
            _tm.Return(this);
        }

        public void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag("Player"))
                Return();
        }

        public void OnDeathAnimationFinished()
        {
            _animator.SetBool(Die, false);
            Return();
        }

        public void OnDamage()
        {
            switch (Type)
            {
                case EnemyType.Tap:
                    _animator.SetBool(Die, true);
                    _isGoing = false;
                    break;
                case EnemyType.Hold when _isGoing:
                    _animator.SetBool(Hit, true);
                    _isGoing = false;
                    break;                
                case EnemyType.Hold when !_isGoing:
                    _animator.SetBool(Hit, false);
                    _animator.SetBool(Die, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
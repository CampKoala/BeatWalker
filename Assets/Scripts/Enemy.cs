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

        public void Init(TimingManager timingManager, Vector2 target, float speed)
        {
            _tm = timingManager;
            _target = target;
            _speed = speed;
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
            _transform.SetPositionAndRotation(startPosition,
                Quaternion.LookRotation(Vector3.forward, _target - startPosition));
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
    }
}
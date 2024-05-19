using UnityEngine;
using UnityEngine.Serialization;

namespace BeatWalker.Config
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "ScriptableObjects/EnemyConfig", order = 2)]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField] private GameObject enemyPrefab;
        public GameObject EnemyPrefab => enemyPrefab;
        
        [SerializeField] private float enemyAngle;
        public float EnemyAngle => enemyAngle;
        
        [SerializeField] private float enemySpawnRadius;
        public float EnemySpawnRadius => enemySpawnRadius;
        
        [SerializeField] private float enemyDeathRadius;
        public float EnemyDeathRadius => enemyDeathRadius;

        [SerializeField] private EnemyType type;
        public EnemyType Type => type;
    }
}
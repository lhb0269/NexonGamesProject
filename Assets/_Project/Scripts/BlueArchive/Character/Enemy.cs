using UnityEngine;

namespace NexonGame.BlueArchive.Character
{
    /// <summary>
    /// 적 유닛 데이터
    /// </summary>
    [System.Serializable]
    public class EnemyData
    {
        public string enemyName = "Enemy";
        public int maxHP = 1000;
        public int attack = 50;
        public int defense = 20;

        public EnemyData(string name, int hp, int atk, int def)
        {
            enemyName = name;
            maxHP = hp;
            attack = atk;
            defense = def;
        }
    }

    /// <summary>
    /// 적 유닛 런타임 인스턴스
    /// </summary>
    public class Enemy
    {
        public EnemyData Data { get; private set; }

        // 현재 상태
        public int CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;

        // 통계
        public int TotalDamageDealt { get; private set; }
        public int TotalDamageTaken { get; private set; }

        public Enemy(EnemyData data)
        {
            Data = data;
            CurrentHP = data.maxHP;
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
        }

        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        public int TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - Data.defense);
            CurrentHP = Mathf.Max(0, CurrentHP - actualDamage);
            TotalDamageTaken += actualDamage;

            Debug.Log($"[{Data.enemyName}] 데미지 받음: {actualDamage} (남은 HP: {CurrentHP})");

            if (!IsAlive)
            {
                Debug.Log($"[{Data.enemyName}] 격파됨!");
            }

            return actualDamage;
        }

        /// <summary>
        /// 공격을 수행합니다
        /// </summary>
        public int Attack()
        {
            int damage = Data.attack;
            TotalDamageDealt += damage;
            Debug.Log($"[{Data.enemyName}] 공격: {damage} 데미지");
            return damage;
        }

        /// <summary>
        /// HP를 회복합니다
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(Data.maxHP, CurrentHP + amount);
            Debug.Log($"[{Data.enemyName}] HP 회복: +{amount} (현재 HP: {CurrentHP})");
        }
    }
}

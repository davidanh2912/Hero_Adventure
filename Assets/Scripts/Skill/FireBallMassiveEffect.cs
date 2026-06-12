using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FireBallMassiveEffect : MonoBehaviour, ISkillEffect
{
    [Header("Spawn Settings")]
    [Tooltip("Khoảng cách X so với Player để spawn chiêu")]
    [SerializeField] private float spawnXOffset = 1.5f;
    [Tooltip("Khoảng cách Y so với Player để spawn chiêu")]
    [SerializeField] private float spawnYOffset = 0.5f;

    [Header("Cast Settings")]
    [Tooltip("Thời gian đứng tụ chiêu trước khi bay (giây)")]
    [SerializeField] private float castDuration = 0.5f;

    [Header("Movement Settings")]
    [Tooltip("Thời gian chiêu bay đến mục tiêu (giây)")]
    [SerializeField] private float travelDuration = 0.5f;
    [Tooltip("Kiểu di chuyển (DOTween Ease)")]
    [SerializeField] private Ease easeType = Ease.OutQuad;
    [Tooltip("Độ lệch X so với mục tiêu (số âm để lùi lại về phía Player)")]
    [SerializeField] private float targetXOffset = -1f;
    [Tooltip("Độ lệch Y so với Player khi bay đến mục tiêu (X là của enemy, Y là của Player + offset này)")]
    [SerializeField] private float targetYOffset = 1.5f;

    [Header("Hit Scale Settings")]
    [Tooltip("Scale của chiêu khi chạm đất/nổ")]
    [SerializeField] private float hitScale = 2f;
    [Tooltip("Thời gian chuyển tiếp phóng to scale khi nổ (giây)")]
    [SerializeField] private float scaleDuration = 0.15f;

    [Header("Damage Settings")]
    [Tooltip("Gây sát thương lan cho toàn bộ quái (True) hay chỉ quái được chọn (False)")]
    [SerializeField] private bool isAoE = true;
    [Tooltip("Thời gian chờ trước khi gây sát thương kể từ lúc chạm mục tiêu (giây)")]
    [SerializeField] private float damageDelay = 0.1f;
    [Tooltip("Thời gian chờ sau khi gây sát thương trước khi bắt đầu biến mất (giây)")]
    [SerializeField] private float beforeDestroyDelay = 0.6f;
    [Tooltip("Thời gian mờ dần (Fade out) của hiệu ứng trước khi hủy (giây)")]
    [SerializeField] private float fadeOutDuration = 0.2f;

    [Header("Animation States")]
    [Tooltip("Tên state cho lúc tụ chiêu")]
    [SerializeField] private string castStateName = "FireBallCast";
    [Tooltip("Tên state cho lúc bay")]
    [SerializeField] private string flightStateName = "FireBallFire";
    [Tooltip("Tên state cho lúc chạm đất/nổ")]
    [SerializeField] private string hitStateName = "FireBallDown";

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    public bool NeedsTargetSelection => false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    public IEnumerator Execute(Player caster, Enemy target, float damageMultiplier)
    {
        if (BattleManager.Instance == null || BattleManager.Instance.ActiveEnemies.Count == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        // --- 1. Giai đoạn tụ chiêu (Cast) ---
        Vector3 startPosition = caster.transform.position + new Vector3(spawnXOffset, spawnYOffset, 0f);
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        if (_animator != null && !string.IsNullOrEmpty(castStateName))
        {
            _animator.Play(castStateName);
        }

        if (castDuration > 0f)
        {
            yield return new WaitForSeconds(castDuration);
        }

        // --- 2. Giai đoạn bay (Flight) ---
        if (_animator != null && !string.IsNullOrEmpty(flightStateName))
        {
            _animator.Play(flightStateName);
        }

        // Đích đến: X ở giữa đám quái và Y của player + targetYOffset
        float targetX = target != null ? target.transform.position.x : 0f;
        float targetZ = target != null ? target.transform.position.z : 0f;

        var activeEnemiesList = BattleManager.Instance.ActiveEnemies;
        float sumX = 0f;
        float sumZ = 0f;
        int count = 0;
        foreach (var enemy in activeEnemiesList)
        {
            if (enemy != null && !enemy.IsDead())
            {
                sumX += enemy.transform.position.x;
                sumZ += enemy.transform.position.z;
                count++;
            }
        }
        if (count > 0)
        {
            targetX = sumX / count;
            targetZ = sumZ / count;
        }

        Vector3 targetPosition = new Vector3(targetX + targetXOffset, caster.transform.position.y + targetYOffset, targetZ);

        bool hasArrived = false;
        transform.DOMove(targetPosition, travelDuration)
            .SetEase(easeType)
            .OnComplete(() => hasArrived = true);

        yield return new WaitUntil(() => hasArrived);

        // --- 3. Giai đoạn chạm đất/nổ (Hit) ---
        if (_animator != null && !string.IsNullOrEmpty(hitStateName))
        {
            _animator.Play(hitStateName);
        }

        transform.DOScale(hitScale, scaleDuration);

        if (damageDelay > 0f)
        {
            yield return new WaitForSeconds(damageDelay);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAttackHit();
        }

        float rawDamage = caster.CalculateDamage(out bool isCrit) * damageMultiplier;

        if (isAoE && BattleManager.Instance != null)
        {
            var enemies = BattleManager.Instance.ActiveEnemies;
            if (enemies != null && enemies.Count > 0)
            {
                var enemyListCopy = new System.Collections.Generic.List<Enemy>(enemies);
                foreach (var enemy in enemyListCopy)
                {
                    if (enemy != null && !enemy.IsDead())
                    {
                        enemy.TakeDamage(rawDamage, isCrit);
                        enemy.transform.DOShakePosition(0.2f, 0.2f, 15, 90);
                    }
                }
            }
        }
        else
        {
            if (target != null && !target.IsDead())
            {
                target.TakeDamage(rawDamage, isCrit);
                target.transform.DOShakePosition(0.2f, 0.2f, 15, 90);
            }
        }

        if (beforeDestroyDelay > 0f)
        {
            yield return new WaitForSeconds(beforeDestroyDelay);
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.DOFade(0f, fadeOutDuration);
        }

        yield return new WaitForSeconds(fadeOutDuration);
        Destroy(gameObject);
    }
}

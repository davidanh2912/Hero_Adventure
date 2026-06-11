using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SwordSkillEffect : MonoBehaviour, ISkillEffect
{
    [Header("Visual Settings (DOTween Fallback)")]
    [Tooltip("Khoảng cách phía trên enemy để spawn kiếm")]
    [SerializeField] private float spawnYOffset = 5f;
    [Tooltip("Thời gian kiếm rơi xuống chạm enemy")]
    [SerializeField] private float fallDuration = 0.3f;
    [Tooltip("Góc xoay mặc định của kiếm khi spawn (ví dụ chỉ xuống dưới)")]
    [SerializeField] private Vector3 spawnRotation = new Vector3(0, 0, -90f);

    [Header("Animation Settings")]
    [Tooltip("Sử dụng Animator thay vì hiệu ứng di chuyển DOTween")]
    [SerializeField] private bool useAnimation = false;
    [Tooltip("Tên trigger để kích hoạt animation (để trống nếu tự động chạy default state)")]
    [SerializeField] private string animationTrigger = "";
    [Tooltip("Thời gian chờ trước khi gây sát thương kể từ lúc xuất hiện (giây)")]
    [SerializeField] private float damageDelay = 0.5f;
    [Tooltip("Tổng thời gian tồn tại của skill trước khi hủy (giây)")]
    [SerializeField] private float destroyDelay = 1.0f;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator Execute(Player caster, Enemy target, float damageMultiplier)
    {
        if (target == null || target.IsDead())
        {
            Destroy(gameObject);
            yield break;
        }

        if (useAnimation)
        {
            // Đặt vị trí tại tâm của enemy
            transform.position = target.transform.position;
            transform.rotation = Quaternion.identity;

            Animator animator = GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(animationTrigger))
            {
                animator.SetTrigger(animationTrigger);
            }

            // Chờ đến thời điểm gây sát thương trong animation
            yield return new WaitForSeconds(damageDelay);

            if (target != null && !target.IsDead())
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayAttackHit();
                }

                float rawDamage = caster.CalculateDamage(out bool isCrit) * damageMultiplier;
                target.TakeDamage(rawDamage, isCrit);

                // Rung lắc nhẹ kẻ địch
                target.transform.DOShakePosition(0.2f, 0.2f, 15, 90);
            }

            // Chờ nốt phần thời gian còn lại của skill rồi hủy
            float remainingDelay = Mathf.Max(0f, destroyDelay - damageDelay);
            yield return new WaitForSeconds(remainingDelay);
            Destroy(gameObject);
        }
        else
        {
            // Đặt vị trí xuất phát ở trên đầu enemy
            Vector3 startPosition = target.transform.position + Vector3.up * spawnYOffset;
            transform.position = startPosition;
            transform.rotation = Quaternion.Euler(spawnRotation);

            // Hiệu ứng rơi xuống sử dụng DOTween
            bool hasHit = false;
            transform.DOMove(target.transform.position, fallDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => hasHit = true);

            // Đợi đến khi kiếm chạm enemy
            yield return new WaitUntil(() => hasHit);

            if (target != null && !target.IsDead())
            {
                // Âm thanh va chạm
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayAttackHit();
                }

                // Tính toán sát thương
                float rawDamage = caster.CalculateDamage(out bool isCrit) * damageMultiplier;
                target.TakeDamage(rawDamage, isCrit);

                // Rung lắc nhẹ kẻ địch khi bị trúng kiếm
                target.transform.DOShakePosition(0.2f, 0.2f, 15, 90);
            }

            // Hiệu ứng mờ dần (Fade out) của kiếm trước khi biến mất
            if (_spriteRenderer != null)
            {
                _spriteRenderer.DOFade(0f, 0.2f);
            }

            yield return new WaitForSeconds(0.2f);

            // Hủy object skill
            Destroy(gameObject);
        }
    }
}

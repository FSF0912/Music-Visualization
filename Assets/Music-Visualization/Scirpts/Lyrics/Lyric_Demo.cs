using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace FSF.CollectionFrame
{
    public class Lyric_Demo : MonoBehaviour
    {
        [Header("Base Settings")]
        public Text Text;
        public CanvasGroup CanvasGroup;
        public AudioSource AudioSource;
        public List<LyricValueKey> Keys;
        public string LyricPath;
        public float fadeDuration = 0.1f;

        [Header("Animation Settings")]
        public bool enableMovementEffect = false;
        public MovementDirection movementDirection = MovementDirection.Left;
        public float movementDistance = 100f;
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public enum MovementDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        private int currentIndex;
        private int keyCount;
        private Coroutine fadeCoroutine;
        private Vector3 originalTextPosition;

        private void Start()
        {
            originalTextPosition = Text.rectTransform.anchoredPosition;
            
            if ((Keys == null || Keys.Count == 0) && !string.IsNullOrWhiteSpace(LyricPath))
            {
                Keys = LyricSpliter.Split(File.ReadAllText(LyricPath));
                keyCount = Keys.Count;
                currentIndex = 0;
                if (keyCount > 0)
                {
                    Text.text = Keys[0].Lyric;
                    ResetTextPosition();
                    SetTextAlpha(1f);
                }
            }
            else
            {
                keyCount = Keys.Count;
            }
        }

        private void Update()
        {
            if (keyCount == 0 || currentIndex >= keyCount - 1) return;

            float currentTime = AudioSource.time;
            int nextIndex = currentIndex + 1;
            
            if (currentTime >= Keys[nextIndex].Time)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                
                fadeCoroutine = StartCoroutine(enableMovementEffect ? 
                    FadeWithMovementTransition(Keys[nextIndex].Lyric) : 
                    FadeLyricTransition(Keys[nextIndex].Lyric));
                
                currentIndex = nextIndex;
            }
        }
        
        private void OnDisable()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            ResetTextPosition();
        }

        /// <summary>
        /// 普通渐变切换：先淡出，切换文本，再淡入
        /// </summary>
        private IEnumerator FadeLyricTransition(string newLyric)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                float curveValue = animationCurve.Evaluate(progress);
                float alpha = 1f - curveValue;
                SetTextAlpha(alpha);
                yield return null;
            }

            SetTextAlpha(0);
            Text.text = newLyric;

            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                float curveValue = animationCurve.Evaluate(progress);
                float alpha = curveValue;
                SetTextAlpha(alpha);
                yield return null;
            }

            SetTextAlpha(1f);
            fadeCoroutine = null;
        }

        /// <summary>
        /// 带位移效果的渐变切换
        /// </summary>
        private IEnumerator FadeWithMovementTransition(string newLyric)
        {
            Vector3 startPos = originalTextPosition;
            Vector3 outDirection = GetMovementDirectionVector(movementDirection);
            Vector3 inDirection = -outDirection;

            // 第一阶段：淡出并移出
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                float curveValue = animationCurve.Evaluate(progress);
                
                // 淡出
                float alpha = 1f - curveValue;
                SetTextAlpha(alpha);
                
                // 位移
                Vector3 newPosition = startPos + outDirection * (movementDistance * curveValue);
                Text.rectTransform.anchoredPosition = newPosition;
                
                yield return null;
            }

            // 切换文本并重置位置到进入方向
            Text.text = newLyric;
            Text.rectTransform.anchoredPosition = startPos + inDirection * movementDistance;
            SetTextAlpha(0);

            // 第二阶段：淡入并移入
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                float curveValue = animationCurve.Evaluate(progress);
                
                // 淡入
                float alpha = curveValue;
                SetTextAlpha(alpha);
                
                // 位移
                Vector3 newPosition = startPos + inDirection * (movementDistance * (1f - curveValue));
                Text.rectTransform.anchoredPosition = newPosition;
                
                yield return null;
            }

            // 恢复原始状态
            ResetTextPosition();
            SetTextAlpha(1f);
            fadeCoroutine = null;
        }

        /// <summary>
        /// 获取移动方向向量
        /// </summary>
        private Vector3 GetMovementDirectionVector(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.Left:
                    return Vector3.left;
                case MovementDirection.Right:
                    return Vector3.right;
                case MovementDirection.Up:
                    return Vector3.up;
                case MovementDirection.Down:
                    return Vector3.down;
                default:
                    return Vector3.left;
            }
        }

        /// <summary>
        /// 重置文本位置到原始位置
        /// </summary>
        private void ResetTextPosition()
        {
            Text.rectTransform.anchoredPosition = originalTextPosition;
        }

        /// <summary>
        /// 直接设置CanvasGroup的alpha值
        /// </summary>
        private void SetTextAlpha(float alpha)
        {
            CanvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// 设置淡入淡出持续时间
        /// </summary>
        public void SetFadeDuration(float duration)
        {
            fadeDuration = Mathf.Max(0.1f, duration);
        }

        /// <summary>
        /// 设置移动距离
        /// </summary>
        public void SetMovementDistance(float distance)
        {
            movementDistance = Mathf.Max(0f, distance);
        }

        /// <summary>
        /// 设置移动方向
        /// </summary>
        public void SetMovementDirection(MovementDirection direction)
        {
            movementDirection = direction;
        }

        /// <summary>
        /// 启用或禁用移动效果
        /// </summary>
        public void SetMovementEffectEnabled(bool enabled)
        {
            enableMovementEffect = enabled;
        }

        /// <summary>
        /// 设置动画曲线
        /// </summary>
        public void SetAnimationCurve(AnimationCurve curve)
        {
            if (curve != null && curve.length >= 2)
            {
                animationCurve = curve;
            }
        }

        /// <summary>
        /// 强制立即切换到指定索引的歌词
        /// </summary>
        public void JumpToLyric(int index)
        {
            if (index < 0 || index >= keyCount) return;
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            currentIndex = index;
            Text.text = Keys[currentIndex].Lyric;
            ResetTextPosition();
            SetTextAlpha(1f);
        }

        /// <summary>
        /// 重新开始歌词显示
        /// </summary>
        public void ResetLyrics()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            currentIndex = 0;
            if (keyCount > 0)
            {
                Text.text = Keys[0].Lyric;
                ResetTextPosition();
                SetTextAlpha(1f);
            }
        }

        /// <summary>
        /// 测试移动效果
        /// </summary>
        public void TestMovementEffect()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(FadeWithMovementTransition("测试移动效果"));
        }

        /// <summary>
        /// 重置为默认的缓动曲线
        /// </summary>
        public void ResetToDefaultCurve()
        {
            animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        /// <summary>
        /// 设置预设曲线
        /// </summary>
        public void SetPresetCurve(CurvePreset preset)
        {
            switch (preset)
            {
                case CurvePreset.EaseInOut:
                    animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    break;
                case CurvePreset.EaseIn:
                    animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                    animationCurve = new AnimationCurve(
                        new Keyframe(0, 0, 0, 0),
                        new Keyframe(1, 1, 2, 0)
                    );
                    break;
                case CurvePreset.EaseOut:
                    animationCurve = new AnimationCurve(
                        new Keyframe(0, 0, 0, 2),
                        new Keyframe(1, 1, 0, 0)
                    );
                    break;
                case CurvePreset.Bounce:
                    animationCurve = new AnimationCurve(
                        new Keyframe(0, 0),
                        new Keyframe(0.5f, 1.2f),
                        new Keyframe(0.8f, 0.9f),
                        new Keyframe(1, 1)
                    );
                    break;
            }
        }

        public enum CurvePreset
        {
            EaseInOut,
            EaseIn,
            EaseOut,
            Bounce
        }
    }
}
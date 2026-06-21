using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Lyric_Demo : MonoBehaviour
{
    [Header("Base Settings")]
    public Text Text;
    public CanvasGroup CanvasGroup;
    public AudioSource AudioSource;
    public List<LyricValueKey> Keys;
    public TextAsset LyricFile;
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
    private Coroutine fadeCoroutine;
    private Vector3 originalTextPosition;

    private void Start()
    {
        originalTextPosition = Text.rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (Keys.Count == 0 || currentIndex >= Keys.Count - 1) return;

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

    private IEnumerator FadeWithMovementTransition(string newLyric)
    {
        Vector3 startPos = originalTextPosition;
        Vector3 outDirection = GetMovementDirectionVector(movementDirection);
        Vector3 inDirection = -outDirection;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            float curveValue = animationCurve.Evaluate(progress);
            
            float alpha = 1f - curveValue;
            SetTextAlpha(alpha);
            
            Vector3 newPosition = startPos + outDirection * (movementDistance * curveValue);
            Text.rectTransform.anchoredPosition = newPosition;
            
            yield return null;
        }

        Text.text = newLyric;
        Text.rectTransform.anchoredPosition = startPos + inDirection * movementDistance;
        SetTextAlpha(0);

        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            float curveValue = animationCurve.Evaluate(progress);
            
            float alpha = curveValue;
            SetTextAlpha(alpha);
            
            Vector3 newPosition = startPos + inDirection * (movementDistance * (1f - curveValue));
            Text.rectTransform.anchoredPosition = newPosition;
            
            yield return null;
        }

        ResetTextPosition();
        SetTextAlpha(1f);
        fadeCoroutine = null;
    }

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

    private void ResetTextPosition()
    {
        Text.rectTransform.anchoredPosition = originalTextPosition;
    }

    private void SetTextAlpha(float alpha)
    {
        CanvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    public void SetFadeDuration(float duration)
    {
        fadeDuration = Mathf.Max(0.1f, duration);
    }

    public void SetMovementDistance(float distance)
    {
        movementDistance = Mathf.Max(0f, distance);
    }

    public void SetMovementDirection(MovementDirection direction)
    {
        movementDirection = direction;
    }

    public void SetMovementEffectEnabled(bool enabled)
    {
        enableMovementEffect = enabled;
    }

    public void SetAnimationCurve(AnimationCurve curve)
    {
        if (curve != null && curve.length >= 2)
        {
            animationCurve = curve;
        }
    }

    public void JumpToLyric(int index)
    {
        if (index < 0 || index >= Keys.Count) return;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        currentIndex = index;
        Text.text = Keys[currentIndex].Lyric;
        ResetTextPosition();
        SetTextAlpha(1f);
    }

    public void ResetLyrics()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        currentIndex = 0;
        if (Keys.Count > 0)
        {
            Text.text = Keys[0].Lyric;
            ResetTextPosition();
            SetTextAlpha(1f);
        }
    }

    public void ResetToDefaultCurve()
    {
        animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

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

# Audio Visualization Solution / 音乐可视化解决方案 / 音楽可視化ソリューション

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-57b9d3.svg?style=for-the-badge&logo=unity)](https://unity.com)
[![Latest Version](https://img.shields.io/github/v/release/FSF0912/Music-Visualization?style=for-the-badge)](https://github.com/FSF0912/Music-Visualization/releases)
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/FSF0912/Music-Visualization/blob/main/LICENSE)
[![GitHub Stars](https://img.shields.io/github/stars/FSF0912/Music-Visualization?style=for-the-badge)](https://github.com/FSF0912/Music-Visualization/stargazers)

![Visualization Sample 1](https://github.com/FSF0912/Music-Visualization/blob/main/SamplePic.png)
![Visualization Sample 2](https://github.com/FSF0912/Music-Visualization/blob/main/samplePic2.png)

> Images and lyrics are sourced from the internet. If there is any infringement, please contact me for removal.

## Overview

A real-time audio visualization system built with **Unity 2021.3 LTS** or higher.
Supports both **pre-recorded audio** (via `AudioSource`) and **live microphone input** visualization.
Features rhythmic geometric transformations driven by spectrum data, paired with a dynamic bilingual lyrics display system.

## Key Features

### 1. Real-Time Spectrum Visualization
- **Dual Input Modes**: Choose between `ByAudioSource` (pre-recorded clips) and `ByMicroPhone` (live capture).
- **Smooth Interpolation**: Configurable `lerpSpeed` for fluid bar height transitions.
- **Adjustable Sensitivity**: Fine-tune visual intensity via the `intensity` multiplier.
- **Sample Window**: Configurable FFT sample size — must be a power of two between 64 and 8192.
- **Auto-Generation**: One-click procedural generation of visualization bars from a prefab, with customizable spacing (`ElementSplit`).

### 2. Dynamic Lyrics Display
- **File Format**: `.lrc` (LyRiCs) with automatic scripted import into Unity `TextAsset`.
- **Encoding Auto-Detection**: Handles UTF-8, UTF-8 BOM, and GB18030/GBK encodings — essential for Chinese lyrics files. Falls back gracefully to system default.
- **Bilingual Support**: Parses multi-language lyrics sharing the same timestamp, merging them into a single entry with configurable ordering.
- **Rich Text Formatting**: Apply per-language rich text tags (bold, italic, color, etc.) using `*` placeholder patterns.
- **Animated Transitions**: Fade-in/fade-out with optional directional movement effects (left, right, up, down).
- **Curve Presets**: Choose from `EaseInOut`, `EaseIn`, `EaseOut`, or `Bounce` animation curves, or supply your own custom `AnimationCurve`.
- **Playback Controls**: Jump to any lyric index, reset to start, or adjust fade duration and movement distance at runtime.

## Getting Started

### System Requirements
- Unity 2021.3 or higher
- TuanJie Engine (Unity China edition) also supported

### Installation

[![Download Latest Release](https://img.shields.io/badge/Download-v1.1.0-blue)](https://github.com/FSF0912/Music-Visualization/releases/)

## Configuration Guide

### Audio Visualizer (`AudioVisualizer`)

Attach the `AudioVisualizer` component to a GameObject and configure:

| Parameter | Description |
|---|---|
| `Visualization Mode` | `ByAudioSource` or `ByMicroPhone` |
| `Source` | Reference to the target `AudioSource` (required for `ByAudioSource` mode) |
| `Length Sample` | FFT window size (64–8192, power of two) |
| `Lerp Speed` | Smoothing factor for bar height transitions (0.01–30) |
| `Intensity` | Global amplitude multiplier (min 0.01) |
| `Bars List` | Array of bar transforms — auto-populated if empty |

If `Bars List` is empty, the editor exposes additional fields for auto-generation:

| Parameter | Description |
|---|---|
| `Element Count` | Number of bars to generate |
| `Element Holder` | Parent `RectTransform` to hold generated bars |
| `Visual Bar Prefab` | Prefab to instantiate for each bar |
| `Element Split` | Horizontal spacing between consecutive bars |

### Lyrics Importer (`LrcImporter`)

Place `.lrc` files anywhere in your `Assets` folder. Unity automatically imports them as `TextAsset` objects using the scripted importer, which handles:

- **UTF-8 BOM** — Detected via byte-order marker
- **UTF-8 (strict)** — Validated; falls through to GB18030 on decode failure
- **GB18030 / GBK** — Common encoding for Chinese lyrics files
- **System default** — Last-resort fallback

No manual configuration required.

### Lyrics Parser (`LyricSpliter`)

```csharp
public static List<LyricValueKey> Split(TextAsset lrc, bool reverse = false, params string[] richTextSymbols)

public static List<LyricValueKey> Split(string lrcText, bool reverse, params string[] richTextSymbols)
```

#### Parameter: `reverse`

When parsing **two lyric lines sharing the same timestamp** (typical in bilingual `.lrc` files), both lines are merged into a single `LyricValueKey` entry separated by a newline.

- `reverse = false` — The first line in the file appears as the **first** line in the merged lyric.
- `reverse = true` — The first line in the file appears as the **second** line (order is inverted).

#### Parameter: `richTextSymbols`

An array of rich text formatting patterns applied per language line. Each pattern must contain a single `*` character as a placeholder for the text content.

```csharp
// Example: bold for the first language, italic for the second
LyricSpliter.Split(lrcFile, false, "<b>*</b>", "<i>*</i>");

// Example: color-coded languages
LyricSpliter.Split(lrcFile, true, "<color=#FF0000>*</color>", "<color=#00FF00>*</color>");
```

> The parser uses `ReadOnlySpan<char>` internally for zero-allocation line processing. Non-time tags (e.g. `[ar:Artist]`, `[ti:Title]`) are automatically skipped.

### Lyrics Display (`Lyric_Demo`)

Attach the `Lyric_Demo` component and wire up:

| Parameter | Description |
|---|---|
| `Text` | Target `UnityEngine.UI.Text` for lyrics display |
| `Canvas Group` | Controls transparency via `alpha` |
| `Audio Source` | Reference audio source for time tracking |
| `Keys` | Parsed `List<LyricValueKey>` from `LyricSpliter.Split()` |
| `Lyric File` | Source `.lrc` file (optional reference) |
| `Fade Duration` | Length of fade-in/fade-out transitions |

**Animation Settings** (optional):

| Parameter | Description |
|---|---|
| `Enable Movement Effect` | Toggle directional slide animation during transitions |
| `Movement Direction` | `Left`, `Right`, `Up`, or `Down` |
| `Movement Distance` | Pixel distance for the slide offset |
| `Animation Curve` | Custom easing curve for both fade and movement |

**Runtime API**:

```csharp
SetFadeDuration(float duration)           // Clamped to minimum 0.1s
SetMovementDistance(float distance)       // Clamped to minimum 0
SetMovementDirection(MovementDirection d)
SetMovementEffectEnabled(bool enabled)
SetAnimationCurve(AnimationCurve curve)   // Must have at least 2 keyframes
SetPresetCurve(CurvePreset preset)        // EaseInOut, EaseIn, EaseOut, Bounce
ResetToDefaultCurve()
JumpToLyric(int index)
ResetLyrics()
```

## Contribution & Support

[![GitHub Stars](https://img.shields.io/github/stars/FSF0912/Music-Visualization?style=social)](https://github.com/FSF0912/Music-Visualization/stargazers)

If you find this project useful, please consider:

- **Starring** the repository
- Reporting issues
- Submitting pull requests
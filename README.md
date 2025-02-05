# Audio Visualization Solution / 音乐可视化解决方案 / 音楽可視化ソリューション

![Visualization Sample 1](https://github.com/FSF0912/Music-Visualization/blob/main/SamplePic.png)  
![Visualization Sample 2](https://github.com/FSF0912/Music-Visualization/blob/main/samplePic2.png)

## Overview
A real-time audio visualization system developed with `Unity 2021.3 LTS`.  
Supports both **pre-recorded audio** and **live microphone input** visualization.  
Features rhythmic transformations of geometric objects synchronized with audio frequencies.

## Key Features
### 1. Dynamic Lyrics Display
- **File Format Support**: `.lrc (LyRiCs)`
- **Auto-Conversion**: Automatically parses lyric files into Unity-compatible data structures
- **Bilingual Support**: Handles translated lyrics with configurable ordering

### 2. Visualization Modes
- Spectrum Analyzer
- Waveform Monitor
- Beat Detection Visualization
- Customizable geometric transformations

## Getting Started
### System Requirements
- Unity 2021.3+ 

### Installation
[![Download Latest Release](https://img.shields.io/badge/Download-v1.0.0-blue)](https://github.com/FSF0912/Music-Visualization/releases/)

## Configuration Guide
### Lyrics Order Management
```csharp
LyricSpliter.Split(TextAsset lrc, bool reverseTranslate = false);

LyricSpliter.Split(string lrc, bool reverseTranslate = false);
```

#### Parameter Behavior
| Reverse Value | Ordering Scheme        |
|---------------|------------------------|
| `true`        | Original → Translation |
| `false`       | Translation → Original |

> **Note**: This configuration applies to standard bilingual lyrics format. Custom formats may require additional adjustments.

## Contribution & Support
[![GitHub Stars](https://img.shields.io/github/stars/FSF0912/Music-Visualization?style=social)](https://github.com/FSF0912/Music-Visualization/stargazers)

If you find this project useful, please consider:
- ⭐ **Starring** the repository
- 🐛 Reporting issues
- 🎨 Submitting pull requests

For commercial usage inquiries, please contact the maintainer.

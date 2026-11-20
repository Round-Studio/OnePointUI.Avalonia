/*
 * BedrockBoot - A launcher for Minecraft Bedrock Edition.
 * Copyright (C) 2025-2026 Round-Studio
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace OnePointUI.Avalonia.Styling.Controls.OnePointControls;

/// <summary>
///     不使用样式动画驱动：样式动画的 INFINITE 循环在 UI 线程洪峰（如批量解码图片）
///     下可能永久饿死，必须重建控件才能恢复。改用 DispatcherTimer + Stopwatch 按
///     真实时间插值，洪峰过后自动续播，弧段伸缩效果与原关键帧完全一致。
/// </summary>
public class ProgressRing : TemplatedControl
{
    public static readonly StyledProperty<int> RingWidthProperty =
        AvaloniaProperty.Register<ProgressRing, int>(nameof(RingWidth), 5);

    private const double DurationMs = 2500;

    // 与旧样式动画相同的关键帧（Cue, 角度, 扫掠），Cue 位置非均匀分布
    private static readonly (double Cue, double Angle, double Sweep)[] KeyFrames =
    {
        (0.0, -720, 0),
        (0.4, -240, 170),
        (0.8, 360, 0),
        (1.0, 720, 10)
    };

    private Arc? _fillArc;
    private DispatcherTimer? _timer;
    private Stopwatch? _stopwatch;

    public int RingWidth
    {
        get => GetValue(RingWidthProperty);
        set => SetValue(RingWidthProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _fillArc = e.NameScope.Find("Fill") as Arc;
        RestartTimer();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        StopTimer();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RestartTimer();
    }

    private void RestartTimer()
    {
        if (_fillArc == null || !IsLoaded) return;

        _stopwatch = Stopwatch.StartNew();
        _timer ??= CreateTimer();
        if (_timer.IsEnabled) return;
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
    }

    private DispatcherTimer CreateTimer()
    {
        // Render 优先级：低于常规输入/布局回调，UI 线程繁忙时动画帧主动让路，
        // 空闲恢复后按 Stopwatch 真实时间对齐相位，不会出现“停在同一帧”。
        var timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        timer.Tick += (_, _) => UpdateArc();
        return timer;
    }

    private void UpdateArc()
    {
        if (_fillArc == null || _stopwatch == null) return;

        var progress = _stopwatch.ElapsedMilliseconds / DurationMs % 1.0;

        // 按 Cue 定位区间：0-40%、40-80%、80-100%，段内线性插值
        var index = 0;
        for (var i = 0; i < KeyFrames.Length - 2; i++)
            if (progress >= KeyFrames[i + 1].Cue)
                index = i + 1;

        var from = KeyFrames[index];
        var to = KeyFrames[index + 1];
        var local = (progress - from.Cue) / (to.Cue - from.Cue);

        _fillArc.StartAngle = from.Angle + (to.Angle - from.Angle) * local;
        _fillArc.SweepAngle = from.Sweep + (to.Sweep - from.Sweep) * local;
    }
}

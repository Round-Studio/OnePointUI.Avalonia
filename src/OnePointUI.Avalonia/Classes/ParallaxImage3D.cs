using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;

namespace OnePointUI.Avalonia.Classes;

public sealed class ParallaxImage3D : Image
{
	public ParallaxImage3D()
	{
		base.RenderTransform = new TransformGroup
		{
			Children = 
			{
				(Transform)new Rotate3DTransform(),
				(Transform)new ScaleTransform()
			}
		};
	}

	private Rotate3DTransform GetRotate3DTransform()
	{
		return (Rotate3DTransform)((TransformGroup)base.RenderTransform).Children[0];
	}

	public void SetScale(double uniformScale)
	{
		if (base.RenderTransform is TransformGroup transformGroup)
		{
			((ScaleTransform)transformGroup.Children[1]).ScaleX = uniformScale;
			((ScaleTransform)transformGroup.Children[1]).ScaleY = uniformScale;
		}
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } classicDesktopStyleApplicationLifetime)
		{
			classicDesktopStyleApplicationLifetime.MainWindow.PointerMoved += MainImageOnPointerMoved;
			classicDesktopStyleApplicationLifetime.MainWindow.PointerExited += MainImageOnPointerExited;
		}
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } classicDesktopStyleApplicationLifetime)
		{
			classicDesktopStyleApplicationLifetime.MainWindow.PointerMoved -= MainImageOnPointerMoved;
			classicDesktopStyleApplicationLifetime.MainWindow.PointerExited -= MainImageOnPointerExited;
		}
	}

	private void MainImageOnPointerMoved(object? sender, PointerEventArgs e)
	{
		if (base.IsLoaded)
		{
			double num = (e.GetPosition(this).X / base.Bounds.Width - 0.5) * -25.0;
			double num2 = (0.0 - (e.GetPosition(this).Y / base.Bounds.Height - 0.5)) * -20.0;
			Rotate3DTransform rotate3DTransform = GetRotate3DTransform();
			rotate3DTransform.Depth = 300.0;
			rotate3DTransform.CenterX = num;
			rotate3DTransform.CenterY = num2;
			rotate3DTransform.AngleX = num / 2.0;
			rotate3DTransform.AngleY = num2 / 2.0;
		}
	}

	private void MainImageOnPointerExited(object? sender, PointerEventArgs e)
	{
		if (base.IsLoaded)
		{
			Rotate3DTransform rotate3DTransform = GetRotate3DTransform();
			rotate3DTransform.Depth = 0.0;
			rotate3DTransform.CenterX = base.Bounds.Center.X;
			rotate3DTransform.CenterY = base.Bounds.Center.Y;
			rotate3DTransform.AngleX = 0.0;
			rotate3DTransform.AngleY = 0.0;
		}
	}
}

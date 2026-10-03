namespace ThaiReference.Views.Drawables;

public abstract class BoxSceneDrawable : IDrawable
{
	protected static readonly Color BoxTop = Color.FromArgb("#9A9A94");
	protected static readonly Color BoxFront = Color.FromArgb("#6E6E68");
	protected static readonly Color BoxSide = Color.FromArgb("#52524E");
	protected static readonly Color BoxInnerBack = Color.FromArgb("#3A3A38");
	protected static readonly Color BoxInnerLeft = Color.FromArgb("#4A4A46");
	protected static readonly Color BoxInnerFloor = Color.FromArgb("#5A5A56");
	protected static readonly Color BallMid = Color.FromArgb("#E05A2E");
	protected static readonly Color BallLite = Color.FromArgb("#F0A070");
	protected static readonly Color BallDark = Color.FromArgb("#A33A18");

	protected const float BoxW = 64;
	protected const float BoxH = 48;
	protected const float BoxD = 20;

	public abstract void Draw(ICanvas canvas, RectF dirtyRect);

	protected readonly record struct BoxGeom(float Cx, float BaseY, float W, float H, float D)
	{
		public float FrontL => Cx - W / 2;
		public float FrontR => Cx + W / 2;
		public float FrontTop => BaseY - H;
		public float TopBackL => FrontL + D;
		public float TopBackR => FrontR + D;
		public float TopBackY => FrontTop - D;
		public float SideBottomY => BaseY - D;

		public (float X, float Y) TopCenter => (Cx + D * 0.5f, (FrontTop + TopBackY) * 0.5f);
	}

	protected static BoxGeom BoxAt(float cx, float baseY, float scale = 1f) =>
		new(cx, baseY, BoxW * scale, BoxH * scale, BoxD * scale);

	protected static void DrawSolidBox(ICanvas canvas, BoxGeom g)
	{
		DrawBoxSide(canvas, g);
		DrawBoxFront(canvas, g, 1f);
		DrawBoxTop(canvas, g);
	}

	protected static void DrawBoxFront(ICanvas canvas, BoxGeom g, float alpha)
	{
		var front = new PathF();
		front.MoveTo(g.FrontL, g.BaseY);
		front.LineTo(g.FrontR, g.BaseY);
		front.LineTo(g.FrontR, g.FrontTop);
		front.LineTo(g.FrontL, g.FrontTop);
		front.Close();

		canvas.FillColor = alpha >= 1f ? BoxFront : BoxFront.WithAlpha(alpha);
		canvas.FillPath(front);

        canvas.StrokeColor = Color.FromArgb("#B8B8B0");
        canvas.StrokeSize = 0.25f;
        canvas.DrawPath(front);
    }

	protected static void DrawBoxTop(ICanvas canvas, BoxGeom g)
	{
		var top = new PathF();
		top.MoveTo(g.FrontL, g.FrontTop);
		top.LineTo(g.FrontR, g.FrontTop);
		top.LineTo(g.TopBackR, g.TopBackY);
		top.LineTo(g.TopBackL, g.TopBackY);
		top.Close();

		canvas.FillColor = BoxTop;
		canvas.FillPath(top);

        canvas.StrokeColor = Color.FromArgb("#B8B8B0");
        canvas.StrokeSize = 0.25f;
        canvas.DrawPath(top);
    }

	protected static void DrawBoxSide(ICanvas canvas, BoxGeom g, float alpha = 1f)
	{
		var side = new PathF();
		side.MoveTo(g.FrontR, g.BaseY);
		side.LineTo(g.TopBackR, g.SideBottomY);
		side.LineTo(g.TopBackR, g.TopBackY);
		side.LineTo(g.FrontR, g.FrontTop);
		side.Close();

		canvas.FillColor = alpha >= 1f ? BoxSide : BoxSide.WithAlpha(alpha);
		canvas.FillPath(side);

		canvas.StrokeColor = Color.FromArgb("#B8B8B0");
		canvas.StrokeSize = 0.25f;
		canvas.DrawPath(side);
	}

	protected static void DrawOpenBoxInterior(ICanvas canvas, BoxGeom g, float floorAlpha = 1f)
	{
		var floor = new PathF();
		floor.MoveTo(g.FrontL, g.BaseY);
		floor.LineTo(g.FrontR, g.BaseY);
		floor.LineTo(g.TopBackR, g.SideBottomY);
		floor.LineTo(g.TopBackL, g.SideBottomY);
		floor.Close();
		canvas.FillColor = floorAlpha >= 1f ? BoxInnerFloor : BoxInnerFloor.WithAlpha(floorAlpha);
		canvas.FillPath(floor);

		var back = new PathF();
		back.MoveTo(g.TopBackL, g.SideBottomY);
		back.LineTo(g.TopBackR, g.SideBottomY);
		back.LineTo(g.TopBackR, g.TopBackY);
		back.LineTo(g.TopBackL, g.TopBackY);
		back.Close();
		canvas.FillColor = BoxInnerBack;
		canvas.FillPath(back);

		var left = new PathF();
		left.MoveTo(g.FrontL, g.BaseY);
		left.LineTo(g.TopBackL, g.SideBottomY);
		left.LineTo(g.TopBackL, g.TopBackY);
		left.LineTo(g.FrontL, g.FrontTop);
		left.Close();
		canvas.FillColor = BoxInnerLeft;
		canvas.FillPath(left);
	}

	protected static void DrawOpenBoxExterior(ICanvas canvas, BoxGeom g)
	{
		DrawBoxSide(canvas, g);
		DrawBoxTop(canvas, g);
		DrawBoxFront(canvas, g, 0.42f);
	}

	protected static void DrawSphere(ICanvas canvas, float x, float y, float r)
	{
		canvas.FillColor = BallMid;
		canvas.FillCircle(x, y, r);

		canvas.FillColor = BallLite;
		canvas.FillCircle(x - r * 0.30f, y - r * 0.32f, r * 0.22f);
	}

	// Floor of the box: offset so the box+ball cluster sits in the vertical middle of the canvas.
	protected static float SceneBaseY(RectF b) => b.Center.Y + BoxH * 0.55f;
	protected static float SceneCx(RectF b) => b.Center.X;

	public static IDrawable ForPositionId(int id) => id switch
	{
		1 => new OnBoxDrawable(),
		2 => new InBoxDrawable(),
		3 => new UnderBoxDrawable(),
		4 => new NextToBoxDrawable(),
		5 => new BetweenBoxDrawable(),
		6 => new InFrontBoxDrawable(),
		7 => new BehindBoxDrawable(),
		8 => new OutsideBoxDrawable(),
		9 => new AmongBoxDrawable(),
		10 => new AboveBoxDrawable(),
		13 => new LeftOfBoxDrawable(),
		14 => new RightOfBoxDrawable(),
		_ => new OnBoxDrawable(),
	};
}

public enum AroundSlot { Above, Under, Left, Right }

public sealed class AroundBoxDrawable : BoxSceneDrawable
{
	static readonly AroundSlot[] AllSlots =
		[AroundSlot.Above, AroundSlot.Under, AroundSlot.Left, AroundSlot.Right];

	public AroundSlot Slot { get; set; } = AroundSlot.Above;

	readonly Dictionary<AroundSlot, (float X, float Y, float R)> _balls = new();

	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect), SceneBaseY(dirtyRect));
		_balls.Clear();
		foreach (var slot in AllSlots)
			_balls[slot] = BallAt(g, slot);

		DrawBall(canvas, AroundSlot.Under);
		DrawOpenBoxInterior(canvas, g, floorAlpha: 0.42f);
		DrawOpenBoxExterior(canvas, g);
		DrawBall(canvas, AroundSlot.Left);
		DrawBall(canvas, AroundSlot.Right);
		DrawBall(canvas, AroundSlot.Above);
	}

	void DrawBall(ICanvas canvas, AroundSlot slot)
	{
		var (x, y, r) = _balls[slot];
		if (slot == Slot)
		{
			canvas.FillColor = BallMid.WithAlpha(0.38f);
			canvas.FillCircle(x, y, r * 1.55f);
		}
		DrawSphere(canvas, x, y, r);
	}

	public AroundSlot? HitTestBall(float x, float y)
	{
		AroundSlot? best = null;
		float bestDist = float.MaxValue;
		foreach (var (slot, (bx, by, r)) in _balls)
		{
			float dx = x - bx;
			float dy = y - by;
			float d2 = dx * dx + dy * dy;
			float hit = r + 10;
			if (d2 <= hit * hit && d2 < bestDist)
			{
				bestDist = d2;
				best = slot;
			}
		}
		return best;
	}

	public static AroundSlot SlotForPositionId(int id) => id switch
	{
		10 => AroundSlot.Above,
		3 => AroundSlot.Under,
		13 => AroundSlot.Left,
		14 => AroundSlot.Right,
		_ => AroundSlot.Above,
	};

	const float AroundBallR = 12f;
	const float AboveGap = 18f;
	const float SideGap = 32f;

	static (float X, float Y, float R) BallAt(BoxGeom g, AroundSlot slot)
	{
		var (topX, topY) = g.TopCenter;
		float leftX = (g.FrontL + g.TopBackL) * 0.5f;
		float rightX = (g.FrontR + g.TopBackR) * 0.5f;
		float sideY = (g.BaseY + g.FrontTop + g.SideBottomY + g.TopBackY) * 0.25f;

		return slot switch
		{
			AroundSlot.Above => (topX, topY - AroundBallR - AboveGap, AroundBallR),
			AroundSlot.Under => (g.Cx + g.D * 0.5f - 2, (g.BaseY + g.SideBottomY) * 0.5f + 3, AroundBallR),
			AroundSlot.Left => (leftX - SideGap, sideY, AroundBallR),
			AroundSlot.Right => (rightX + SideGap, sideY, AroundBallR),
			_ => (topX, topY - AroundBallR - AboveGap, AroundBallR),
		};
	}
}

public sealed class OnBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect), SceneBaseY(dirtyRect));
		DrawSolidBox(canvas, g);

		float r = 14;
		var (tx, ty) = g.TopCenter;
		DrawSphere(canvas, tx, ty - r, r);
	}
}

public sealed class InBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect), SceneBaseY(dirtyRect));
		DrawOpenBoxInterior(canvas, g);
		DrawSphere(canvas, g.Cx + g.D * 0.35f, g.BaseY - g.H * 0.42f, 12);
		DrawOpenBoxExterior(canvas, g);
	}
}

public sealed class UnderBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect), SceneBaseY(dirtyRect));
		float r = 12;
		float bx = g.Cx + g.D * 0.5f;
		float by = (g.BaseY + g.SideBottomY) * 0.5f;
		DrawSphere(canvas, bx - 2, by + 3, r);
		DrawOpenBoxInterior(canvas, g, floorAlpha: 0.42f);
		DrawOpenBoxExterior(canvas, g);
	}
}

public sealed class NextToBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect) + 28, SceneBaseY(dirtyRect));
		DrawSolidBox(canvas, g);
		DrawSphere(canvas, g.FrontL - 28, g.BaseY - 18, 15);
	}
}

public sealed class BetweenBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		float baseY = SceneBaseY(dirtyRect);
		float cx = SceneCx(dirtyRect);
		var left = BoxAt(cx - 52, baseY, 0.85f);
		var right = BoxAt(cx + 52, baseY, 0.85f);
		float ballX = (left.TopBackR + right.FrontL) * 0.5f;
		DrawSolidBox(canvas, left);
		DrawSphere(canvas, ballX, baseY - 16, 12);
		DrawSolidBox(canvas, right);
	}
}

public sealed class InFrontBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect), SceneBaseY(dirtyRect));
		DrawSolidBox(canvas, g);
		float r = 14;
		DrawSphere(canvas, g.Cx - 5, g.BaseY - r + 5, r);
	}
}

public sealed class BehindBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect), SceneBaseY(dirtyRect));
		float r = 12;
		float wallCx = (g.TopBackL + g.TopBackR) * 0.5f;
		DrawSphere(canvas, wallCx - 1, g.SideBottomY - r - 1, r);

		var back = new PathF();
		back.MoveTo(g.TopBackL, g.SideBottomY);
		back.LineTo(g.TopBackR, g.SideBottomY);
		back.LineTo(g.TopBackR, g.TopBackY);
		back.LineTo(g.TopBackL, g.TopBackY);
		back.Close();
		canvas.FillColor = BoxInnerBack.WithAlpha(0.55f);
		canvas.FillPath(back);

		var floor = new PathF();
		floor.MoveTo(g.FrontL, g.BaseY);
		floor.LineTo(g.FrontR, g.BaseY);
		floor.LineTo(g.TopBackR, g.SideBottomY);
		floor.LineTo(g.TopBackL, g.SideBottomY);
		floor.Close();
		canvas.FillColor = BoxInnerFloor;
		canvas.FillPath(floor);

		var left = new PathF();
		left.MoveTo(g.FrontL, g.BaseY);
		left.LineTo(g.TopBackL, g.SideBottomY);
		left.LineTo(g.TopBackL, g.TopBackY);
		left.LineTo(g.FrontL, g.FrontTop);
		left.Close();
		canvas.FillColor = BoxInnerLeft;
		canvas.FillPath(left);

		DrawOpenBoxExterior(canvas, g);
	}
}

public sealed class OutsideBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect) - 24, SceneBaseY(dirtyRect));
		DrawSolidBox(canvas, g);
		DrawSphere(canvas, g.TopBackR + 30, g.BaseY - 16, 15);
	}
}

public sealed class AmongBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		float baseY = SceneBaseY(dirtyRect);
		float cx = SceneCx(dirtyRect);
		const float scale = 0.68f;
		const float edgeGap = 2f;
		float w = BoxW * scale;
		float d = BoxD * scale;
		var back = BoxAt(cx, baseY - 14, scale);
		var left = BoxAt(back.FrontL - edgeGap - w / 2 - d, baseY, scale);
		var right = BoxAt(back.TopBackR - edgeGap, baseY, scale);

		DrawSolidBox(canvas, back);
		DrawSolidBox(canvas, left);
		DrawSolidBox(canvas, right);
		DrawSphere(canvas, cx - 8, baseY - 10, 10);
	}
}

public sealed class AboveBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect), SceneBaseY(dirtyRect));
		DrawSolidBox(canvas, g);
		float r = 13;
		var (tx, ty) = g.TopCenter;
		DrawSphere(canvas, tx, ty - r - 18, r);
	}
}

public sealed class LeftOfBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect) + 36, SceneBaseY(dirtyRect));
		DrawSolidBox(canvas, g);
		DrawSphere(canvas, g.FrontL - 32, g.BaseY - 16, 14);
	}
}

public sealed class RightOfBoxDrawable : BoxSceneDrawable
{
	public override void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var g = BoxAt(SceneCx(dirtyRect) - 36, SceneBaseY(dirtyRect));
		DrawSolidBox(canvas, g);
		DrawSphere(canvas, g.FrontR + 32, g.BaseY - 16, 14);
	}
}

#region
using System.Runtime.InteropServices;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Mino.Graphics.Text;
using Mino.Mathematics;
using Mino.Mathematics.TwoDim;
using Mino.Nio;
#endregion

namespace Mino.Graphics.Sprite;

/// <summary>
///     2D batched brush,
///     provides integrated 2D rendering solution.
/// </summary>
public unsafe class Brush : IDisposable {
	/// <summary>
	///		Called on flushing.
	/// </summary>
	public Action<Brush>? OnFlushed;
	/*
	 * STATE SWITCHING INFO
	 */
	private BrushState _state;
	private float _invWidth = 0;
	private float _invHeight = 0;
	/*
	 * STATIC DEFAULTS
	 * These pipes and resource sets are:
	 * [0] - color RGBA Half
	 * [1] - color RGBA Half, texture
	 */
	private RenderPipe[] _pipes = new RenderPipe[2];
	private ResourceSet[] _sets = new ResourceSet[2];
	/*
	 * STATICS
	 */
	private RenderTarget _renderTarget = null!;
	private Sampler _sampler = null!;
	private bool _isDirect;
	private MultiMesh _parent;
	private MultiMesh.Node _target = null!;
	private Encoder _encoder = null!;
	private BufferObject _ubo = null!;
	private Camera2D _camera = Camera2D.Normal(new Camera2D());
	/*
	 * Other variables
	 */
	private bool _disposed;
	public int _Drawcalls = 0;
	private Stack<ScissorDesc> _scissorStack = new Stack<ScissorDesc>();

	public Brush(MultiMesh parent) {
		_parent = parent;
		_isDirect = _parent.IsUltimate;
		
		initGfxResources();
	}

	/// <summary>
	///     Transform matrix stack.
	/// </summary>
	public MatrixStack<Matrix4x4> Transform = new MatrixStack<Matrix4x4>();

	/// <summary>
	///     Rendering color tint.
	/// </summary>
	public Color Color { get; set; } = Color.PureWhite;

	/// <summary>
	///     Current camera.
	///     This operation will flush the brush.
	/// </summary>
	public Camera2D Camera {
		get => _camera;
		set {
			SetViewProjection(value.ViewProjectionMatrix);
			_camera = value;
		}
	}

	/// <summary>
	///     Current texture sampler.
	///     This operation will flush the brush.
	/// </summary>
	public Sampler Sampler {
		get => _sampler;
		set {
			Flush();
			_sampler = value;
		}
	}

	/// <summary>
	///     Brush render target.
	///     This operation will flush the brush.
	/// </summary>
	public RenderTarget RenderTarget {
		get => _renderTarget;
		set {
			Flush();
			_renderTarget = value;
		}
	}

	/// <summary>
	///		Current brush states.
	/// </summary>
	public BrushState State {
		get => _state;
		set {
			Flush();
			_state = value;
		}
	}

	/// <summary>
	///     Current viewport.
	/// </summary>
	public Box2 CurrentViewport { get; private set; }

	/// <summary>
	///     Current scissor test.
	/// </summary>
	public ScissorDesc CurrentScissor { get; private set; } = ScissorDesc.Disabled;

	/// <summary>
	///     Brush depth value. By default is 0.0F (near).
	/// </summary>
	public float Depth { get; set; } = 0.0F;

	/// <summary>
	///     Brush drawing flags.
	/// </summary>
	public BrushFlag Flags { get; set; } = BrushFlag.None;

	/// <summary>
	///     Begins a render pass.
	///     This operation will flush the brush.
	/// </summary>
	public void Begin(in RenderPassDesc? desc = null) {
		_renderTarget.Acquire(desc);
	}

	/// <summary>
	///     Ends a render pass.
	///     This operation will flush the brush.
	/// </summary>
	public void End() {
		Flush(true);
		_renderTarget.Present();
	}

	/// <summary>
	///		Moves to next mesh node.
	/// </summary>
	public void NextNode() {
		if (_target != null) {
			_target.RecordedState = _state;
		}
		
		_target = _parent.Acquire();
	}
	
	/// <summary>
	///		Replays a mesh with this brush.
	/// </summary>
	/// <param name="mesh">The mesh to replay.</param>
	public void Replay(MultiMesh mesh) {
		if (mesh.IsUltimate) {
			return; // Ultimate mesh cannot be replayed.
		}
		
		BrushState oldState = State;
		
		foreach (MultiMesh.Node node in mesh) {
			if (node.IsEmpty) {
				continue;
			}
			
			State = node.RecordedState;
			draw(node);
		}
		
		State = oldState;
	}

	/// <summary>
	///     Flushes the brush, clears all buffers and submits the draw.
	/// </summary>
	/// <param name="force">True if a force state update is needed.</param>
	public void Flush(bool force = false) {
		if (_target == null) {
			return;
		}
		
		if (_parent.IsUltimate) {
			draw(_target, force);
			_target.Reset(); // Directly submit, dp not record.
		} else if(!_target.IsEmpty) {
			NextNode(); // Move to next node for recording.
		}
	}

	public void draw(MultiMesh.Node node, bool force = false) {
		if (_state._set == null || _state._pipe == null || _state._primitive == null) {
			return; // Unreachable...?
		}
		if (node.VertexCount <= 0 && !force) {
			return;
		}
		
		_Drawcalls++;
		
		ByteBuffer vBuf = node.VertexBuf;
		ByteBuffer iBuf = node.IndexBuf;

		_encoder.SetViewport(
			(int) CurrentViewport.MinX,
			(int) CurrentViewport.MinY,
			(int) CurrentViewport.Width,
			(int) CurrentViewport.Height
		);
		_encoder.SetScissor(CurrentScissor);

		// End of a drawing frame, we need to clear encoder's commands.
		// (forced flush)
		if (node.VertexCount <= 0) {
			if (force) {
				_encoder.QueuedExecute();
				_encoder.Reset();
			}
			return;
		}

		_encoder.SetRenderPipe(_state._pipe);

		if (node.Dirty) {
			node.Vbo.Submit<byte>(vBuf.AsSpan());
		}
		_encoder.SetBuffer(node.Vbo);

		_state._set.BindUniform(0, _ubo, sizeof(Matrix4x4));
		_encoder.SetResource(0, _state._set);

		switch (_state._primitive) {
			case BrushPrimitive.TextureSprite:
				_state._set.BindTexture(1, _state._tex!, _sampler);
				_encoder.SetTopology(Topology.Triangle);
				
				if (node.Dirty) {
					node.Ibo.Submit<byte>(iBuf.AsSpan());
				}
				_encoder.SetBuffer(node.Ibo);
				
				_encoder.DrawIndexed(node.IndexCount, 0);
				break;
			case BrushPrimitive.ColorSprite:
				_encoder.SetTopology(Topology.Triangle);
				
				if (node.Dirty) {
					node.Ibo.Submit<byte>(iBuf.AsSpan());
				}
				_encoder.SetBuffer(node.Ibo);
				
				_encoder.DrawIndexed(node.IndexCount, 0);
				break;
			case BrushPrimitive.ColorLine:
				_encoder.SetTopology(Topology.Line);
				_encoder.Draw(node.VertexCount, 0);
				break;
			case BrushPrimitive.ColorPoint:
				_encoder.SetTopology(Topology.Point);
				_encoder.Draw(node.VertexCount, 0);
				break;
		}

		_encoder.QueuedExecute();
		_encoder.Reset();

		// Avoid useless submissions.
		node.Dirty = false;
	}

	/// <summary>
	///     Sets drawing view-projection matrix.
	///     This operation will flush the brush.
	/// </summary>
	/// <param name="vpm">The matrix, column-majored.</param>
	public void SetViewProjection(in Matrix4x4 vpm) {
		Flush();
		// 64B view projection mat4.
		_ubo.Submit([vpm]);
	}

	/// <summary>
	///     Sets drawing viewport.
	///     This operation will flush the brush.
	/// </summary>
	/// <param name="box">Viewport box.</param>
	public void SetViewport(in Box2 box) {
		Flush();
		CurrentViewport = box;
	}

	/// <summary>
	///     Sets scissor test.
	///     This operation will flush the brush.
	/// </summary>
	/// <param name="box">Scissor region.</param>
	public void SetScissor(in Box2 box) {
		Flush();

		Box2 newBox = Box2.CreateByPoints(
			Camera.Project(box.Min, CurrentViewport),
			Camera.Project(box.Max, CurrentViewport)
		);
		CurrentScissor = new ScissorDesc {
			Enable = true,
			X = (int) MathF.Floor(newBox.MinX),
			Y = (int) MathF.Floor(newBox.MinY),
			Width = (int) MathF.Ceiling(newBox.Width),
			Height = (int) MathF.Ceiling(newBox.Height)
		};
	}

	public void DisableScissor() {
		Flush();
		CurrentScissor = ScissorDesc.Disabled;
	}

	/// <summary>
	///     Draws a texture.
	/// </summary>
	/// <param name="tex">Texture to draw.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	/// <param name="u">Src u.</param>
	/// <param name="v">Src v.</param>
	/// <param name="uw">Src width.</param>
	/// <param name="vh">Src height.</param>
	public void DrawTexture(FragileTexture? tex, float x, float y, float w, float h, float u, float v, float uw,
		float vh) {
		if (tex == null) {
			return;
		}
		assert(BrushPrimitive.TextureSprite);
		assert(tex.Pin());

		ByteBuffer vBuf = _target.VertexBuf;
		ByteBuffer iBuf = _target.IndexBuf;

		u *= _invWidth;
		v *= _invHeight;
		float u2 = u + uw * _invWidth;
		float v2 = v + vh * _invHeight;

		if ((Flags & BrushFlag.FlipX) != 0) {
			(u, u2) = (u2, u);
		}
		if ((Flags & BrushFlag.FlipY) != 0) {
			(v, v2) = (v2, v);
		}

		Transform.Top.Transform(x, y, Depth, out float x1, out float y1, out float d1);
		Transform.Top.Transform(x + w, y + h, Depth, out float x2, out float y2, out float d2);
		float dMid = (d1 + d2) * 0.5F;

		/* Vertex 0, 1, 2, 3 visualized.
		 *
		 * 0-----------1
		 * | \	       |
		 * |    \	   |
		 * |	  \    |
		 * |		 \ |
		 * 3-----------2
		 *
		 * Ensure CCW sort for culling.
		 */

		// 0
		vBuf.Write(new Vector3(x1, y1, d1));
		vBuf.Write(Color.AsHalves());
		vBuf.Write(new Vector2(u, v));
		// 1
		vBuf.Write(new Vector3(x2, y1, dMid));
		vBuf.Write(Color.AsHalves());
		vBuf.Write(new Vector2(u2, v));
		// 2
		vBuf.Write(new Vector3(x2, y2, d2));
		vBuf.Write(Color.AsHalves());
		vBuf.Write(new Vector2(u2, v2));
		// 3
		vBuf.Write(new Vector3(x1, y2, dMid));
		vBuf.Write(Color.AsHalves());
		vBuf.Write(new Vector2(u, v2));

		iBuf.Write((uint) _target.VertexCount + 0);
		iBuf.Write((uint) _target.VertexCount + 2);
		iBuf.Write((uint) _target.VertexCount + 1);
		iBuf.Write((uint) _target.VertexCount + 2);
		iBuf.Write((uint) _target.VertexCount + 0);
		iBuf.Write((uint) _target.VertexCount + 3);

		_target.Write(4, 6);
	}

	/// <summary>
	///     Draws a texture part.
	/// </summary>
	/// <param name="tex">Texture to draw.</param>
	/// <param name="dst">Draw destination.</param>
	/// <param name="src">Texture source.</param>
	public void DrawTexture(FragileTexture? tex, in Box2 dst, in Box2 src) {
		DrawTexture(tex, dst.MinX, dst.MinY, dst.Width, dst.Height, src.MinX, src.MinY, src.Width, src.Height);
	}

	/// <summary>
	///     Draws a texture.
	/// </summary>
	/// <param name="tex">Texture to draw.</param>
	/// <param name="dst">Draw destination.</param>
	public void DrawTexture(FragileTexture? tex, in Box2 dst) {
		if (tex == null) {
			return;
		}
		DrawTexture(new TexturePart(tex), dst);
	}

	/// <summary>
	///     Draws a texture.
	/// </summary>
	/// <param name="tex">Texture to draw.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	public void DrawTexture(FragileTexture? tex, float x, float y, float w, float h) {
		if (tex == null) {
			return;
		}
		DrawTexture(tex, x, y, w, h, 0.0F, 0.0F, tex.Width, tex.Height);
	}

	/// <summary>
	///     Draws a texture part.
	/// </summary>
	/// <param name="texPart">Texture part to draw.</param>
	/// <param name="dst">Draw destination.</param>
	public void DrawTexture(TexturePart texPart, in Box2 dst) {
		DrawTexture(texPart.Src.Pin(), dst, texPart.Region);
	}

	/// <summary>
	///     Draws a texture part.
	/// </summary>
	/// <param name="texPart">Texture part to draw.</param>
	/// <param name="dst">Draw destination.</param>
	/// <param name="src">Texture part source.</param>
	public void DrawTexture(TexturePart texPart, in Box2 dst, in Box2 src) {
		DrawTexture(new TexturePart(texPart.Src.Pin(), src), dst);
	}

	/// <summary>
	///     Draws a texture part.
	/// </summary>
	/// <param name="texPart">Texture part to draw.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	public void DrawTexture(TexturePart texPart, float x, float y, float w, float h) {
		DrawTexture(texPart.Src, x, y, w, h, texPart.U, texPart.V, texPart.Width, texPart.Height);
	}

	/// <summary>
	///     Draws a texture part.
	/// </summary>
	/// <param name="texPart">Texture part to draw.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	/// <param name="u">Src u.</param>
	/// <param name="v">Src v.</param>
	/// <param name="uw">Src width.</param>
	/// <param name="vh">Src height.</param>
	public void DrawTexture(TexturePart texPart, float x, float y, float w, float h, float u, float v, float uw,
		float vh) {
		DrawTexture(texPart.Src, x, y, w, h, u + texPart.U, v + texPart.V, uw, vh);
	}

	/// <summary>
	///     Draws a drawable.
	/// </summary>
	/// <param name="t">Drawable object to draw.</param>
	/// <param name="dst">Draw destination.</param>
	public void Draw<T>(in T t, in Box2 dst) where T : Drawable {
		t.Draw(this, dst.MinX, dst.MinY, dst.Width, dst.Height, 0.0F, 0.0F, 0.0F, 0.0F);
	}

	/// <summary>
	///     Draws a drawable.
	/// </summary>
	/// <param name="t">Drawable object to draw.</param>
	/// <param name="dst">Draw destination.</param>
	/// <param name="src">Texture part source.</param>
	public void Draw<T>(in T t, in Box2 dst, in Box2 src) where T : Drawable {
		t.Draw(this, dst.MinX, dst.MinY, dst.Width, dst.Height, src.MinX, src.MinY, src.Width, src.Height);
	}

	/// <summary>
	///     Draws a drawable.
	/// </summary>
	/// <param name="t">Drawable object to draw.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	public void Draw<T>(in T t, float x, float y, float w, float h) where T : Drawable {
		t.Draw(this, x, y, w, h, 0.0F, 0.0F, 0.0F, 0.0F);
	}

	/// <summary>
	///     Draws a drawable.
	/// </summary>
	/// <param name="t">Drawable object to draw.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	/// <param name="u">Src u.</param>
	/// <param name="v">Src v.</param>
	/// <param name="uw">Src width.</param>
	/// <param name="vh">Src height.</param>
	public void Draw<T>(in T t, float x, float y, float w, float h, float u, float v, float uw, float vh)
		where T : Drawable {
		t.Draw(this, x, y, w, h, u, v, uw, vh);
	}

	/// <summary>
	///     Draws a rectangle.
	/// </summary>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	public void DrawRectangle(float x, float y, float w, float h) {
		assert(BrushPrimitive.ColorSprite);

		ByteBuffer vBuf = _target.VertexBuf;
		ByteBuffer iBuf = _target.IndexBuf;

		Transform.Top.Transform(x, y, Depth, out float x1, out float y1, out float d1);
		Transform.Top.Transform(x + w, y + h, Depth, out float x2, out float y2, out float d2);
		float dMid = (d1 + d2) * 0.5F;

		// 0
		vBuf.Write(new Vector3(x1, y1, d1));
		vBuf.Write(Color.AsHalves());
		// 1
		vBuf.Write(new Vector3(x2, y1, dMid));
		vBuf.Write(Color.AsHalves());
		// 2
		vBuf.Write(new Vector3(x2, y2, d2));
		vBuf.Write(Color.AsHalves());
		// 3
		vBuf.Write(new Vector3(x1, y2, dMid));
		vBuf.Write(Color.AsHalves());

		iBuf.Write((uint) _target.VertexCount + 0);
		iBuf.Write((uint) _target.VertexCount + 2);
		iBuf.Write((uint) _target.VertexCount + 1);
		iBuf.Write((uint) _target.VertexCount + 2);
		iBuf.Write((uint) _target.VertexCount + 0);
		iBuf.Write((uint) _target.VertexCount + 3);

		_target.Write(4, 6);
	}

	/// <summary>
	///     Draws a rectangle.
	/// </summary>
	/// <param name="dst">Drawing destination.</param>
	public void DrawRectangle(in Box2 dst) {
		DrawRectangle(dst.MinX, dst.MinY, dst.Width, dst.Height);
	}

	/// <summary>
	///     Draws a rectangle frame.
	/// </summary>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	public void DrawRectangleFrame(float x, float y, float w, float h) {
		DrawLine(x, y, x + w, y);
		DrawLine(x, y, x, y + h);
		DrawLine(x + w, y, x + w, y + h);
		DrawLine(x, y + h, x + w, y + h);
	}

	/// <summary>
	///     Draws a rectangle frame.
	/// </summary>
	/// <param name="dst">Drawing destination.</param>
	public void DrawRectangleFrame(in Box2 dst) {
		DrawRectangleFrame(dst.MinX, dst.MinY, dst.Width, dst.Height);
	}

	/// <summary>
	///     Draws a line.
	/// </summary>
	/// <param name="x1">From x.</param>
	/// <param name="y1">From y.</param>
	/// <param name="x2">To x.</param>
	/// <param name="y2">To y.</param>
	public void DrawLine(float x1, float y1, float x2, float y2) {
		assert(BrushPrimitive.ColorLine);

		ByteBuffer vBuf = _target.VertexBuf;

		Transform.Top.Transform(x1, y1, Depth, out float x1t, out float y1t, out float d1);
		Transform.Top.Transform(x2, y2, Depth, out float x2t, out float y2t, out float d2);

		vBuf.Write(new Vector3(x1t, y1t, d1));
		vBuf.Write(Color.AsHalves());

		vBuf.Write(new Vector3(x2t, y2t, d2));
		vBuf.Write(Color.AsHalves());

		_target.Write(2, 0);
	}

	/// <summary>
	///     Draws a line.
	/// </summary>
	/// <param name="from">From point.</param>
	/// <param name="to">To point.</param>
	public void DrawLine(in Vector2 from, in Vector2 to) {
		DrawLine(from.X, from.Y, to.X, to.Y);
	}

	/// <summary>
	///     Draws a point.
	/// </summary>
	/// <param name="x">Position x.</param>
	/// <param name="y">Position y.</param>
	public void DrawPoint(float x, float y) {
		assert(BrushPrimitive.ColorPoint);

		ByteBuffer vBuf = _target.VertexBuf;

		Transform.Top.Transform(x, y, Depth, out float xt, out float yt, out float d);

		vBuf.Write(new Vector3(xt, yt, d));
		vBuf.Write(Color.AsHalves());

		_target.Write(1, 0);
	}

	/// <summary>
	///     Draws a point.
	/// </summary>
	/// <param name="at">Point position.</param>
	public void DrawPoint(in Vector2 at) {
		DrawPoint(at.X, at.Y);
	}

	/// <summary>
	///     Draws a text blob.
	/// </summary>
	/// <param name="blob">The blob to draw.</param>
	/// <param name="x">Drawing offset x.</param>
	/// <param name="y">Drawing offset y.</param>
	/// <param name="alignment">Text alignment.</param>
	public void DrawText(TextBlob blob, float x, float y, Alignment? alignment = null) {
		alignment ??= Alignment.LeftUp;

		float w = blob.Width;
		float h = blob.Height;

		switch (alignment.Value.Horizontal) {
			case -1:
				// Do nothing.
				break;
			case 0:
				x -= w / 2.0F;
				break;
			case 1:
				x -= w;
				break;
		}

		switch (alignment.Value.Vertical) {
			case -1:
				// Do nothing.
				break;
			case 0:
				y -= h / 2.0F;
				break;
			case 1:
				y -= h;
				break;
		}

		for (int i = 0; i < blob.GlyphRunList.Count; i++) {
			ref GlyphInstance gi = ref CollectionsMarshal.AsSpan(blob.GlyphRunList)[i];
			if (gi.Visible) {
				DrawTexture(gi.Glyph.TexPart, gi.Bounds.Translate(x, y));
			}
		}
	}

	/// <summary>
	///     Draws a text blob.
	/// </summary>
	/// <param name="blob">The blob to draw.</param>
	/// <param name="pos">Drawing offset.</param>
	/// <param name="alignment">Text alignment.</param>
	public void DrawText(TextBlob blob, in Vector2 pos, in Alignment? alignment = null) {
		DrawText(blob, pos.X, pos.Y, alignment);
	}

	private void initGfxResources() {
		InternalResources.init();
		
		RenderTarget = RenderTarget.GetUltimate();
		Sampler = RenderSystem.Create<Sampler>(new SamplerDesc());

		
		
		_ubo = RenderSystem.Create<BufferObject>(
			new BufferObjectDesc {
				Frequency = BufferFrequency.Stream,
				Type = BufferType.Uniform,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		_encoder = RenderSystem.Create<Encoder>(
			new EncoderDesc {
				Usage = EncoderUsage.Render
			});
		/*
		 * Creation of pipes.
		 * Builtin pipes are as follows:
		 * 0 - colored
		 * 1 - textured
		 */
		_pipes[0] = InternalResources.p4c!;
		_pipes[1] = InternalResources.p4t!;
		_sets[0] = RenderSystem.Create<ResourceSet>(InternalResources.rl4c!);
		_sets[1] = RenderSystem.Create<ResourceSet>(InternalResources.rl4t!);
	}

	private void assert(BrushPrimitive primitive) {
		if (_state._primitive == primitive) {
			return;
		}

		Flush();

		// Apply states based on primitive.
		if (primitive == BrushPrimitive.TextureSprite) {
			_state._pipe = _pipes[1];
			_state._set = _sets[1];
		} else {
			_state._pipe = _pipes[0];
			_state._set = _sets[0];
		}

		_state._tex = null;
		_state._primitive = primitive;
	}

	private void assert(Texture? tex) {
		if (_state._tex == tex) {
			return;
		}

		Flush();

		_state._tex = tex;
		if (tex != null) {
			_invWidth = 1.0F / tex.Width;
			_invHeight = 1.0F / tex.Height;
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		GC.SuppressFinalize(this);
		
		_sampler.Dispose();
		_encoder.Dispose();
	}
}

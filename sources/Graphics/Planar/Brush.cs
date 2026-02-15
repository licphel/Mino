#region
using System.Runtime.InteropServices;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Mino.Graphics.Text;
using Mino.Mathematics;
using Mino.Nio;
#endregion

namespace Mino.Graphics.Planar;

/// <summary>
///     2D batched brush,
///     provides integrated 2D rendering solution.
/// </summary>
public unsafe class Brush : IDisposable {
	private const string VertShaderTex = """
										   #version 330 core

										   layout(location = 0) in vec3 i_position;
										   layout(location = 1) in vec4 i_color;
										   layout(location = 2) in vec2 i_texCoord;

										   out vec4 o_color;
										   out vec2 o_texCoord;

										   layout(std140) uniform u_transform {
										       mat4 u_viewProjection;
										   };

										   void main(){
										       o_color = i_color;
										       o_texCoord = i_texCoord;

										       gl_Position =  u_viewProjection * vec4(i_position, 1.0);
										   }
										   """;
	private const string FragShaderTex = """
										   #version 330 core

										   in vec4 o_color;
										   in vec2 o_texCoord;

										   uniform sampler2D u_texture;

										   void main() {
										       vec4 col = texture(u_texture, o_texCoord);
										       gl_FragColor = o_color * col;
										   }
										   """;
	private const string VertShaderCol = """
										   #version 330 core

										   layout(location = 0) in vec3 i_position;
										   layout(location = 1) in vec4 i_color;

										   out vec4 o_color;

										   layout(std140) uniform u_transform {
										       mat4 u_viewProjection;
										   };

										   void main(){
										       o_color = i_color;

										       gl_Position =  u_viewProjection * vec4(i_position, 1.0);
										   }
										   """;
	private const string FragShaderCol = """
										   #version 330 core

										   in vec4 o_color;

										   void main() {
										       gl_FragColor = o_color;
										   }
										   """;

	/*
	 * STATE SWITCHING INFO
	 */
	private RenderPipe? _ast_Pipe = null;
	private BrushPrimitive? _ast_Primitive = null;
	private ResourceSet? _ast_Set = null;

	private Texture? _ast_Tex = null;
	private float _invWidth = 0;
	private float _invHeight = 0;

	private Color[] _color4 = [
		Color.PureWhite,
		Color.PureWhite,
		Color.PureWhite,
		Color.PureWhite
	];
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
	private Swapchain _swapchain = null!;
	private BrushCache _target;
	private BufferObject _vbo = null!;
	private Encoder _encoder = null!;
	private BufferObject _ibo = null!;
	private BufferObject _ubo = null!;
	/*
	 * Other variables
	 */
	private int _vertCnt = 0;
	private int _indCnt = 0;
	private bool _disposed;

	public Brush(BrushCache? target = null) {
		_target = target ?? new BrushCache.Self();
		initGfxResources();
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
			_swapchain = new Swapchain(value);
		}
	}

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
		_swapchain.Acquire(desc);
	}

	/// <summary>
	///     Ends a render pass.
	///     This operation will flush the brush.
	/// </summary>
	public void End() {
		Flush();
		_swapchain.Present();
	}

	/// <summary>
	///     Flushes the brush, clears all buffers and submits the draw.
	/// </summary>
	public void Flush() {
		if (_ast_Set == null || _ast_Pipe == null || _ast_Primitive == null) {
			return; // Unreachable...?
		}
		if (_vertCnt <= 0) {
			return;
		}

		ByteBuffer vBuf = _target.VertexBuf;
		ByteBuffer iBuf = _target.IndexBuf;

		_encoder.SetRenderPipe(_ast_Pipe);

		_vbo.Submit<byte>(vBuf.AsSpan());
		_encoder.SetBuffer(_vbo);

		_ast_Set.BindUniform(0, _ubo, sizeof(Matrix4x4));
		_encoder.SetResource(0, _ast_Set);

		switch (_ast_Primitive) {
			case BrushPrimitive.TextureSprite:
				_ast_Set.BindTexture(1, _ast_Tex!, _sampler);
				_encoder.SetTopology(Topology.Triangle);
				_encoder.SetBuffer(_ibo);
				_ibo.Submit<byte>(iBuf.AsSpan());
				_encoder.DrawIndexed(_indCnt, 0);
				break;
			case BrushPrimitive.ColorSprite:
				_encoder.SetTopology(Topology.Triangle);
				_encoder.SetBuffer(_ibo);
				_ibo.Submit<byte>(iBuf.AsSpan());
				_encoder.DrawIndexed(_indCnt, 0);
				break;
			case BrushPrimitive.ColorLine:
				_encoder.SetTopology(Topology.Line);
				_encoder.Draw(_vertCnt, 0);
				break;
			case BrushPrimitive.ColorPoint:
				_encoder.SetTopology(Topology.Point);
				_encoder.Draw(_vertCnt, 0);
				break;
			default:
				throw new Error("unreachable");
		}

		_encoder.QueuedExecute();
		_encoder.Reset();

		vBuf.Clear();
		iBuf.Clear();
		_vertCnt = 0;
		_indCnt = 0;
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
		_encoder.SetViewport((int) box.MinX, (int) box.MinY, (int) box.Width, (int) box.Height);
	}

	/// <summary>
	///     Sets scissor test.
	///     This operation will flush the brush.
	/// </summary>
	/// <param name="desc">Scissor test desc.</param>
	public void SetScissor(in ScissorDesc desc) {
		Flush();
		_encoder.SetScissor(desc);
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
	public void DrawTexture(Texture? tex, float x, float y, float w, float h, float u, float v, float uw, float vh) {
		if (tex == null) {
			return;
		}
		assert(BrushPrimitive.TextureSprite);
		assert(tex);

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

		float x1 = x;
		float y1 = y;
		float x2 = x + w;
		float y2 = y + h;

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
		vBuf.Write(new Vector3(x1, y1, Depth));
		vBuf.Write(_color4[0].AsHalves());
		vBuf.Write(new Vector2(u, v));
		// 1
		vBuf.Write(new Vector3(x2, y1, Depth));
		vBuf.Write(_color4[1].AsHalves());
		vBuf.Write(new Vector2(u2, v));
		// 2
		vBuf.Write(new Vector3(x2, y2, Depth));
		vBuf.Write(_color4[2].AsHalves());
		vBuf.Write(new Vector2(u2, v2));
		// 3
		vBuf.Write(new Vector3(x1, y2, Depth));
		vBuf.Write(_color4[3].AsHalves());
		vBuf.Write(new Vector2(u, v2));

		iBuf.Write(_vertCnt + 0);
		iBuf.Write(_vertCnt + 2);
		iBuf.Write(_vertCnt + 1);
		iBuf.Write(_vertCnt + 2);
		iBuf.Write(_vertCnt + 0);
		iBuf.Write(_vertCnt + 3);

		_vertCnt += 4;
		_indCnt += 6;
	}

	/// <summary>
	///     Draws a texture part.
	/// </summary>
	/// <param name="tex">Texture to draw.</param>
	/// <param name="dst">Draw destination.</param>
	/// <param name="src">Texture source.</param>
	public void DrawTexture(Texture? tex, in Box2 dst, in Box2 src) {
		DrawTexture(tex, dst.MinX, dst.MinY, dst.Width, dst.Height, src.MinX, src.MinY, src.Width, src.Height);
	}

	/// <summary>
	///     Draws a texture.
	/// </summary>
	/// <param name="tex">Texture to draw.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	public void DrawTexture(Texture? tex, float x, float y, float w, float h) {
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
		DrawTexture(texPart.Src, dst, texPart.Region);
	}

	/// <summary>
	///     Draws a texture part.
	/// </summary>
	/// <param name="texPart">Texture part to draw.</param>
	/// <param name="dst">Draw destination.</param>
	/// <param name="src">Texture part source.</param>
	public void DrawTexture(TexturePart texPart, in Box2 dst, in Box2 src) {
		DrawTexture(new TexturePart(texPart.Src, src), dst);
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
	public void DrawTexture(TexturePart texPart, float x, float y, float w, float h, float u, float v, float uw, float vh) {
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
	public void Draw<T>(in T t, float x, float y, float w, float h, float u, float v, float uw, float vh) where T : Drawable {
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

		float x1 = x;
		float y1 = y;
		float x2 = x + w;
		float y2 = y + h;

		// 0
		vBuf.Write(new Vector3(x1, y1, Depth));
		vBuf.Write(_color4[0].AsHalves());
		// 1
		vBuf.Write(new Vector3(x2, y1, Depth));
		vBuf.Write(_color4[1].AsHalves());
		// 2
		vBuf.Write(new Vector3(x2, y2, Depth));
		vBuf.Write(_color4[2].AsHalves());
		// 3
		vBuf.Write(new Vector3(x1, y2, Depth));
		vBuf.Write(_color4[3].AsHalves());

		iBuf.Write(_vertCnt + 0);
		iBuf.Write(_vertCnt + 2);
		iBuf.Write(_vertCnt + 1);
		iBuf.Write(_vertCnt + 2);
		iBuf.Write(_vertCnt + 0);
		iBuf.Write(_vertCnt + 3);

		_vertCnt += 4;
		_indCnt += 6;
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

		vBuf.Write(new Vector3(x1, y1, Depth));
		vBuf.Write(_color4[0].AsHalves());

		vBuf.Write(new Vector3(x2, y2, Depth));
		vBuf.Write(_color4[1].AsHalves());

		_vertCnt += 4;
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

		vBuf.Write(new Vector3(x, y, Depth));
		vBuf.Write(_color4[0].AsHalves());

		_vertCnt += 4;
	}

	/// <summary>
	///     Draws a point.
	/// </summary>
	/// <param name="at">Point position.</param>
	public void DrawPoint(in Vector2 at) {
		DrawPoint(at.X, at.Y);
	}

	/// <summary>
	///		Draws a text blob.
	/// </summary>
	/// <param name="blob">The blob to draw.</param>
	/// <param name="x">Drawing offset x.</param>
	/// <param name="y">Drawing offset y.</param>
	/// <param name="alignment">Text alignment.</param>
	public void DrawText(TextBlob blob, float x, float y, Alignment? alignment = null) {
		alignment??= Alignment.LeftUp;
		
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
			DrawTexture(gi.Glyph.TexPart, gi.Bounds.Translate(x, y));
		}
	}

	/// <summary>
	///		Draws a text blob.
	/// </summary>
	/// <param name="blob">The blob to draw.</param>
	/// <param name="pos">Drawing offset.</param>
	/// <param name="alignment">Text alignment.</param>
	public void DrawText(TextBlob blob, in Vector2 pos, in Alignment? alignment = null) {
		DrawText(blob, pos.X, pos.Y, alignment);
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_swapchain.Dispose();
		_vbo.Dispose();
		_ibo.Dispose();
		_sampler.Dispose();
		_encoder.Dispose();
		GC.SuppressFinalize(this);
	}

	private void initGfxResources() {
		RenderTarget = RenderTarget.GetUltimate();
		Sampler = new Sampler(new SamplerDesc());

		_vbo = new BufferObject(
			new BufferDesc {
				Frequency = BufferFrequency.Stream,
				Type = BufferType.Vertex,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		_ibo = new BufferObject(
			new BufferDesc {
				Frequency = BufferFrequency.Stream,
				Type = BufferType.Index,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		_ubo = new BufferObject(
			new BufferDesc {
				Frequency = BufferFrequency.Stream,
				Type = BufferType.Uniform,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		_encoder = new Encoder(
			new EncoderDesc {
				Usage = EncoderUsage.Render
			});
		/*
		 * Creation of pipes.
		 * Builtin pipes are as follows:
		 * 0 - colored
		 * 1 - textured
		 */
		ShaderProgram program_0 = ShaderProgram.FragVert(VertShaderCol, FragShaderCol);
		ShaderProgram program_1 = ShaderProgram.FragVert(VertShaderTex, FragShaderTex);
		ResourceSetLayout layout_0 = ResourceSetLayout.Bake(
			new ResourceSetLayout.Slot {
				Count = 1,
				Name = "u_transform",
				Stages = ShaderType.Vertex,
				Type = ResourceType.UniformBuffer
			});
		ResourceSetLayout layout_1 = ResourceSetLayout.Bake(
			new ResourceSetLayout.Slot {
				Count = 1,
				Name = "u_transform",
				Stages = ShaderType.Vertex,
				Type = ResourceType.UniformBuffer
			}, new ResourceSetLayout.Slot {
				Count = 1,
				Name = "u_texture",
				Stages = ShaderType.Fragment,
				Type = ResourceType.Texture
			});
		// Colored pipe.
		_pipes[0] = new RenderPipe(
			new RenderPipeDesc {
				Blend = BlendDesc.AlphaMix,
				Depth = DepthDesc.Leq,
				Rasterization = RasterizationDesc.Default,
				ResourceLayouts = [layout_0],
				ShaderProgram = program_0,
				Usage = RenderPipeUsage.Render,
				VertexLayout = VertexLayout.Bake(
					new VertexLayout.Attr {
						Components = 3,
						Normalized = false,
						Type = VertexAttributeType.Float32
					}, new VertexLayout.Attr {
						// Half color4
						Components = 4,
						Normalized = false,
						Type = VertexAttributeType.Float16
					})
			});
		// Textured pipe.
		_pipes[1] = new RenderPipe(
			new RenderPipeDesc {
				Blend = BlendDesc.AlphaMix,
				Depth = DepthDesc.Leq,
				Rasterization = RasterizationDesc.Default,
				ResourceLayouts = [layout_1],
				ShaderProgram = program_1,
				Usage = RenderPipeUsage.Render,
				VertexLayout = VertexLayout.Bake(
					new VertexLayout.Attr {
						Components = 3,
						Normalized = false,
						Type = VertexAttributeType.Float32
					}, new VertexLayout.Attr {
						// Half color4
						Components = 4,
						Normalized = false,
						Type = VertexAttributeType.Float16
					}, new VertexLayout.Attr {
						Components = 2,
						Normalized = false,
						Type = VertexAttributeType.Float32
					})
			});
		_sets[0] = new ResourceSet(layout_0);
		_sets[1] = new ResourceSet(layout_1);
	}

	private void assert(BrushPrimitive primitive) {
		if (_ast_Primitive == primitive) {
			return;
		}

		Flush();

		// Apply states based on primitive.
		if (primitive == BrushPrimitive.TextureSprite) {
			_ast_Pipe = _pipes[1];
			_ast_Set = _sets[1];
		} else {
			_ast_Pipe = _pipes[0];
			_ast_Set = _sets[0];
		}

		_ast_Primitive = primitive;
	}

	private void assert(Texture? tex) {
		if (_ast_Tex == tex) {
			return;
		}

		Flush();

		_ast_Tex = tex;
		if (tex != null) {
			_invWidth = 1.0F / tex.Width;
			_invHeight = 1.0F / tex.Height;
		}
	}
}

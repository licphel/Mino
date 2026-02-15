#region
using Mino.Algorithm.Noise;
using Mino.Algorithm.Random;
using Mino.Framework;
using Mino.Framework.XPlatform;
using Mino.Graphics;
using Mino.Graphics.Desktop;
using Mino.Graphics.Input;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;
using Mino.Mathematics.Spatial;
using Mino.Nio;
#endregion

namespace Mino;

internal static class NoiseVisualization {
	/*
	 *	Launch this demo using modern IDE function.
	 *	It shows a rotating 3D noise-textured rectangle.
	 */
	public static void Launch() {
		const string VERT_SHADER = """
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
		const string FRAG_SHADER = """
								   #version 330 core

								   in vec4 o_color;
								   in vec2 o_texCoord;

								   uniform sampler2D u_texture;

								   void main() {
								       vec4 col = texture(u_texture, o_texCoord);
								       gl_FragColor = o_color * col;
								   }
								   """;

		// set logger info and output locations
		Logger.Global.EnableDebug();
		Logger.Global.EnableNoexcept();
		Logger.Global.OutputTo(new Url("console://out"));

		// create window with debugger opened
		Window window = Service.GetBest<Window>("GLFW");
		window.Init(
			new WindowHints {
				AutoIconify = false,
				DebugContext = true
			});

		// game main loop controller, the executor
		Executor executor = new ExecutorSync();

		RenderSystem.LoadBackend(window, Service.GetBest<RenderBackend>("OpenGL"));

		ByteBuffer vertex = new ByteBuffer();
		vertex.Endianness = Endianness.Native;

		ByteBuffer index = new ByteBuffer();
		index.Endianness = Endianness.Native;

		Sampler sampler = new Sampler(new SamplerDesc());

		const int gridSize = 1024;
		const float worldSize = 50.0F;
		const float heightScale = 15.0F;

		Image heightmap = genHeightmap(gridSize, gridSize, 8.0);
		Image colorMap = genColorMap(heightmap);
		Texture colorTexture = new Texture(TextureDesc.CreateByImage(colorMap));
		genGridMesh(heightmap, vertex, index, gridSize, worldSize);

		// vbo and ebo
		BufferObject vbo = new BufferObject(
			new BufferDesc {
				Frequency = BufferFrequency.Static,
				Type = BufferType.Vertex,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		vbo.Submit<byte>(vertex.AsSpan());

		BufferObject ebo = new BufferObject(
			new BufferDesc {
				Frequency = BufferFrequency.Static,
				Type = BufferType.Index,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		ebo.Submit<byte>(index.AsSpan());

		// shader compilation
		ShaderModule vertModule = new ShaderModule(
			new ShaderModuleDesc {
				Type = ShaderType.Vertex,
				Code = VERT_SHADER
			});
		ShaderModule fragModule = new ShaderModule(
			new ShaderModuleDesc {
				Type = ShaderType.Fragment,
				Code = FRAG_SHADER
			});

		ShaderProgram shaderProgram = new ShaderProgram(
			new ShaderProgramDesc {
				Modules = [vertModule, fragModule]
			});

		// resource layout
		ResourceSetLayout resLayout = ResourceSetLayout.Bake(
			new ResourceSetLayout.Slot {
				Count = 1,
				Name = "u_texture",
				Stages = ShaderType.Fragment,
				Type = ResourceType.Texture
			}, new ResourceSetLayout.Slot {
				Count = 1,
				Name = "u_transform",
				Stages = ShaderType.Vertex,
				Type = ResourceType.UniformBuffer
			});

		// pipe pack
		RenderPipe pipe = new RenderPipe(
			new RenderPipeDesc {
				Blend = BlendDesc.AlphaMix,
				Depth = DepthDesc.Disabled,
				Rasterization = RasterizationDesc.NotCull,
				ResourceLayouts = [resLayout],
				VertexLayout = VertexLayout.Bake(
					new VertexLayout.Attr {
						Components = 3,
						Normalized = false,
						Type = VertexAttributeType.Float32
					}, new VertexLayout.Attr {
						Components = 4,
						Normalized = false,
						Type = VertexAttributeType.Float16
					}, new VertexLayout.Attr {
						Components = 2,
						Normalized = false,
						Type = VertexAttributeType.Float32
					}),
				ShaderProgram = shaderProgram,
				Usage = RenderPipeUsage.Render
			});

		// uniform buffer
		BufferObject uniform = new BufferObject(
			new BufferDesc {
				Frequency = BufferFrequency.Stream,
				Type = BufferType.Uniform,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});

		// cmd buffer
		Encoder encoder = new Encoder(
			new EncoderDesc {
				IsExtended = false,
				Usage = EncoderUsage.Render
			});

		// resource sets
		ResourceSet resourceSet = new ResourceSet(resLayout);
		resourceSet.BindTexture(0, colorTexture, sampler);
		resourceSet.BindUniform(1, uniform, 64);

		Swapchain swapchain = new Swapchain(RenderTarget.GetUltimate());

		float rotation = 0;
		float flyY = 30;
		float distX = 70;

		executor.OnTick += step => {
			RenderSystem.Update(step);
			window.Title = $"Demo | FPS: {executor.Fps}";

			if (KeyListener.Get(KeyCode.D).Hold) {
				rotation += (float) executor.Delta;
			}
			if (KeyListener.Get(KeyCode.A).Hold) {
				rotation -= (float) executor.Delta;
			}
			if (KeyListener.Get(KeyCode.Space).Hold) {
				flyY += (float) executor.Delta * 25;
			}
			if (KeyListener.Get(KeyCode.LeftShift).Hold) {
				flyY -= (float) executor.Delta * 25;
			}
			if (KeyListener.Get(KeyCode.S).Hold) {
				distX += (float) executor.Delta * 25;
			}
			if (KeyListener.Get(KeyCode.W).Hold) {
				distX -= (float) executor.Delta * 25;
			}
			if (KeyListener.Get(KeyCode.Enter).Press) {
				heightmap = genHeightmap(gridSize, gridSize, 8.0);
				colorMap = genColorMap(heightmap);
				colorTexture.Submit(TextureSubmission.CreateByImage(colorMap));
				genGridMesh(heightmap, vertex, index, gridSize, worldSize);
			}
		};

		executor.OnRender += () => {
			// camera orbit
			CameraPerspective camera = new CameraPerspective();
			camera.SetPerspective(MathF.PI / 3.0F, 16F / 9F);
			camera.SetClippingPlanes(0.1F, 1000.0F);
			camera.Up = Vector3.UnitY;
			camera.Target = new Vector3(0, heightScale * 0.5F, 0);
			camera.Position = new Vector3(MathF.Sin(rotation) * distX, flyY, MathF.Cos(rotation) * distX);

			// upload uniform buffer
			uniform.Submit([camera.ViewProjectionMatrix]);

			swapchain.Acquire();
			encoder.Reset();
			encoder.SetRenderPipe(pipe);
			encoder.SetBuffer(vbo);
			encoder.SetBuffer(ebo);
			encoder.SetResource(0, resourceSet);
			encoder.SetTopology(Topology.Triangle);
			encoder.SetViewport(0, 0, (int) window.Size.X, (int) window.Size.Y);
			encoder.DrawIndexed(index.ReadableBytes / 4, 0);
			encoder.QueuedExecute();
			swapchain.Present();
		};

		window.Vsync = false;
		executor.Start(window, 60);
	}

	private static Image genHeightmap(int width, int height, double scale) {
		Image img = Image.Create(width, height);

		NoiseGenerator primNoise = new NoiseGeneratorPerlin(new RandomGeneratorXoroshiro128());
		NoiseGenerator noise = new NoiseGeneratorOctave(primNoise, 2);

		for (int i = 0; i < img.Width; i++) {
			for (int j = 0; j < img.Height; j++) {
				double nx = i / (double) width * scale;
				double ny = j / (double) height * scale;
				img[i][j] = new Color((float) noise.Generate(nx, ny, 0.0F), 0.0F, 0.0F);
			}
		}

		return img;
	}

	private static Image genColorMap(Image heightmap) {
		Image img = Image.Create(heightmap.Width, heightmap.Height);

		for (int i = 0; i < img.Width; i++) {
			for (int j = 0; j < img.Height; j++) {
				float h = heightmap[i][j].Red;
				img[i][j] = Color.HsvToRgb(h, 0.75F, 0.8F);
			}
		}

		return img;
	}

	private static void genGridMesh(Image heightMap, ByteBuffer vertex, ByteBuffer index, int gridSize,
		float worldSize) {
		vertex.Clear();
		index.Clear();

		float cellSize = worldSize / (gridSize - 1);
		float halfSize = worldSize * 0.5F;

		// Generate vertices
		for (int j = 0; j < gridSize; j++) {
			float z = j * cellSize - halfSize;
			float v = 1.0F - j / (float) (gridSize - 1);

			for (int i = 0; i < gridSize; i++) {
				float x = i * cellSize - halfSize;
				float u = i / (float) (gridSize - 1);

				// Position
				vertex.Write(new Vector3(x, heightMap[i][j].Red * gridSize / 128, z));
				// Color (white, modulated by texture)
				vertex.Write(new Color(1.0F, 1.0F, 1.0F, 0.5F).AsHalves());
				// UV
				vertex.Write(new Vector2(u, v));
			}
		}

		// Generate indices (two triangles per grid cell)
		for (int j = 0; j < gridSize - 1; j++) {
			for (int i = 0; i < gridSize - 1; i++) {
				int topLeft = j * gridSize + i;
				int topRight = topLeft + 1;
				int bottomLeft = (j + 1) * gridSize + i;
				int bottomRight = bottomLeft + 1;

				// First triangle (top-left, bottom-left, top-right)
				index.Write((uint) topLeft);
				index.Write((uint) bottomLeft);
				index.Write((uint) topRight);

				// Second triangle (top-right, bottom-left, bottom-right)
				index.Write((uint) topRight);
				index.Write((uint) bottomLeft);
				index.Write((uint) bottomRight);
			}
		}
	}
}

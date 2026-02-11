using Mino.Algorithm.Random;
using Mino.Framework;
using Mino.Graphics;
using Mino.Graphics.Desktop;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;
using Mino.Mathematics.Spatial;
using Mino.Nio;
using Buffer = Mino.Graphics.Buffer;

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
		// set output location.
		// actually it can have multiple locations, but for our demo, one is enough.
		Logger.Global.OutputTo(new Url("console://out"));
		
		// create window with debugger opened
		Window window = Backend.Find<Window>("GLFW");
		window.Init(
			new WindowHints {
				DebugContext = true
			});
		
		// game main loop controller, the executor
		Executor executor = new ExecutorSync();

		RenderSystem.LoadBackend(window, Backend.Find<RenderBackend>("OpenGL"));
	
		ByteBuffer vertex = new ByteBuffer();
		// set to native endianness
		vertex.Endianness = Endianness.Native;

		ByteBuffer index = new ByteBuffer();
		index.Endianness = Endianness.Native;

		Sampler sampler = new Sampler(new SamplerDesc());

		TextureAtlas atlas = new TextureAtlas();

		// visualize noise images
		// and atlas
		List<TexturePart> parts = new List<TexturePart>();
		atlas.Init();
		for (int i = 25; i >= 3; i--) {
			for (int j = 0; j < 5; j++) {
				parts.Add(atlas.Accept(genNoiseTex(i)));
			}
		}
		atlas.EndAccept();
		
		// create texture by image
		Texture texture = parts[0].Src;
		
		// vbo and ebo
		Buffer vbo = new Buffer(
			new BufferDesc {
				Frequency = BufferFrequency.Stream,
				Type = BufferType.Vertex,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		vbo.Submit<byte>(vertex.AsSpan());
		Buffer ebo = new Buffer(
			new BufferDesc {
				Frequency = BufferFrequency.Stream,
				Type = BufferType.Index,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			});
		ebo.Submit<byte>(index.AsSpan());

		// shader compilation
		ShaderModule m1 = new ShaderModule(
			new ShaderModuleDesc {
				Type = ShaderType.Vertex,
				Code = VERT_SHADER
			});
		ShaderModule m2 = new ShaderModule(
			new ShaderModuleDesc {
				Type = ShaderType.Fragment,
				Code = FRAG_SHADER
			});
		ShaderProgram shaderProgram = new ShaderProgram(
			new ShaderProgramDesc {
				Modules = [m1, m2]
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

		// pipeline pack
		Pipeline pipeline = new Pipeline(
			new PipelineDesc {
				Blend = BlendDesc.AlphaMix,
				ResourceLayouts = [
					resLayout
				],
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
				Type = PipelineType.Render
			});

		// uniform buffer
		Buffer uniform = new Buffer(
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
		
		// reusable resource set
		ResourceSet resourceSet = new ResourceSet(resLayout);
		resourceSet.BindTexture(0, texture, sampler);
		resourceSet.BindUniform(1, uniform, 64);

		// bottom left
		vertex.Write(new Vector3(-20, 0, -20));
		vertex.Write(Color.PureWhite.AsHalves());
		vertex.Write(new Vector2(0, 1));
		// bottom right
		vertex.Write(new Vector3(-20, 0, 20));
		vertex.Write(Color.PureWhite.AsHalves());
		vertex.Write(new Vector2(1, 1));
		// top right
		vertex.Write(new Vector3(20, 0, 20));
		vertex.Write(Color.PureWhite.AsHalves());
		vertex.Write(new Vector2(1, 0));
		// top left
		vertex.Write(new Vector3(20, 0, -20));
		vertex.Write(Color.PureWhite.AsHalves());
		vertex.Write(new Vector2(0, 0));
		vbo.Submit<byte>(vertex.AsSpan());

		// indices
		index.Write(0U);
		index.Write(1U);
		index.Write(3U);
		index.Write(1U);
		index.Write(2U);
		index.Write(3U);
		ebo.Submit<byte>(index.AsSpan());

		Swapchain swapchain = new Swapchain(
			new RenderPassDesc {
				ClearColor = new Color(0.72F, 0.95F, 0.98F)
			}, RenderTarget.GetUltimate());

		executor.OnTick += step => {
			RenderSystem.Update(step);
			window.Title = $"Demo | FPS: {executor.Fps}";
		};
		
		executor.OnRender += () => {
			// do camera transform
			float dt = (float) executor.Timestamp.TotalSeconds / 3;
			
			CameraPerspective camera = new CameraPerspective();
			camera.SetPerspective(MathF.PI / 3.0F, 16F / 9F);
			camera.SetClippingPlanes(0.1F, 1000.0F);
			camera.Up = Vector3.UnitY;
			camera.Target = new Vector3(0, 0, 0);
			camera.Position = new Vector3(MathF.Cos(dt) * 15, 40, MathF.Sin(dt) * 15);
			
			// upload uniform buffer
			uniform.Submit([camera.ViewProjectionMatrix]);

			swapchain.Acquire();
			encoder.Reset();
			encoder.SetPipeline(pipeline);
			encoder.SetBuffer(vbo);
			encoder.SetBuffer(ebo);
			encoder.SetResource(0, resourceSet);
			encoder.SetTopology(Topology.Triangle);
			encoder.SetViewport(0, 0, (int) window.Size.X, (int) window.Size.Y);
			encoder.DrawIndexed(ebo.LastBound / 4, 0);
			encoder.QueuedExecute();
			swapchain.Present();
		};

		window.Vsync = false;
		executor.Start(window, 60);
	}

	private static Image genNoiseTex(int size) {
		Image img = Image.CreateEmpty(size, size);
		RandomNoise noise = new RandomNoiseVoronoi(new RandomGeneratorXoroshiro128());

		// Gen image data.
		for (int i = 0; i < img.Width; i++) {
			float px = (float) i / img.Width;
			for (int j = 0; j < img.Height; j++) {
				float py = (float) j / img.Height;
				img[i][j] = Color.HsvToRgb((float) noise.Generate(px * 5, py * 5), 0.5F, 0.8F);
			}
		}

		return img;
	}
}

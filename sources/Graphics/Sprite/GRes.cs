using Mino.Graphics.Desc;
using Mino.Graphics.Enum;

namespace Mino.Graphics.Sprite;

internal static class GRes {
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



	internal static ShaderProgram? s4c, s4t;
	internal static ResourceSetLayout? rl4c, rl4t;
	internal static VertexLayout? vl4c, vl4t;
	internal static RenderPipe? p4c, p4t;
	private static bool _init;

	internal static void init() {
		if (_init) {
			return;
		}
		_init = true;
		
		s4c = ShaderProgram.CreateRender(VertShaderCol, FragShaderCol); 
		s4t = ShaderProgram.CreateRender(VertShaderTex, FragShaderTex);
		
		rl4c = ResourceSetLayout.Bake(
			new ResourceSetLayout.Slot {
				Count = 1,
				Name = "u_transform",
				Stages = ShaderType.Vertex,
				Type = ResourceType.UniformBuffer
			});
		rl4t = ResourceSetLayout.Bake(
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

		vl4c = VertexLayout.Bake(
			new VertexLayout.Attr {
				Components = 3,
				Normalized = false,
				Type = VertexAttributeType.Float32
			}, new VertexLayout.Attr {
				// Half color4
				Components = 4,
				Normalized = false,
				Type = VertexAttributeType.Float16
			});
		vl4t = VertexLayout.Bake(
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
			});
		
		 p4c = RenderSystem.Create<RenderPipe>(
			new RenderPipeDesc {
				Blend = BlendDesc.AlphaMix,
				Depth = DepthDesc.Leq,
				Rasterization = RasterizationDesc.Default,
				ResourceLayouts = [rl4c],
				ShaderProgram = s4c,
				Usage = RenderPipeUsage.Render,
				VertexLayout = vl4c
			});
		 p4t = RenderSystem.Create<RenderPipe>(
			 new RenderPipeDesc {
				 Blend = BlendDesc.AlphaMix,
				 Depth = DepthDesc.Leq,
				 Rasterization = RasterizationDesc.Default,
				 ResourceLayouts = [rl4t],
				 ShaderProgram = s4t,
				 Usage = RenderPipeUsage.Render,
				 VertexLayout = vl4t
			 });
	}	
}

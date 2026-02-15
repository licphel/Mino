#region
using Mino.Graphics.Desktop;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
#endregion

namespace Mino.Graphics.RHI;

/// <summary>
///     Low-level graphics backend interface for cross-platform hardware-accelerated rendering.
/// </summary>
public interface RenderBackend : IDisposable {
	void Init(Window window);
	void PollEvents();

	// Frame
	void FrameBegin();
	void FrameEnd();
	uint GetUltimateRenderTarget();

	// Buffer
	uint BufferGen();
	void BufferDelete(uint buffer);
	void BufferAlloc<T>(uint buffer, in BufferDesc desc, ReadOnlySpan<T> data, int capacity) where T : unmanaged;
	void BufferSubmit<T>(uint buffer, ReadOnlySpan<T> data, int offset) where T : unmanaged;

	// Texture
	uint TextureGen();
	void TextureDelete(uint texture);
	void TextureData(uint texture, in TextureDesc desc);
	void TextureSubmit(uint texture, in TextureSubmission submission);
	void TextureBlit(
		uint src, int srcX, int srcY, int srcW, int srcH,
		uint dst, int dstX, int dstY, int dstW, int dstH,
		TextureFilter filter
	);

	// Sampler
	uint SamplerGen();
	void SamplerDelete(uint sampler);
	void SamplerData(uint sampler, in SamplerDesc desc);

	// Shader module
	uint ShaderModuleGen();
	void ShaderModuleDelete(uint module);
	void ShaderModuleCompile(uint module, in ShaderModuleDesc desc);

	// Shader program
	uint ShaderProgramGen();
	void ShaderProgramDelete(uint program);
	void ShaderProgramLink(uint program, in ShaderProgramDesc desc);

	// Shader uniform
	uint UniformGen(uint program, string name);
	void UniformData<T>(uint program, uint uniform, in T data) where T : unmanaged;
	void UniformData<T>(uint program, uint uniform, ReadOnlySpan<T> data) where T : unmanaged;

	// Render target
	uint RenderTargetGen();
	void RenderTargetDelete(uint renderTarget);
	void RenderTargetData(uint renderTarget, in RenderTargetDesc desc);
	void RenderTargetBlit(
		uint src, int srcX, int srcY, int srcW, int srcH,
		uint dst, int dstX, int dstY, int dstW, int dstH,
		TextureFilter filter
	);
	void RenderPassBegin(uint renderTarget, in RenderPassDesc desc);
	void RenderPassEnd();

	// RenderPipe
	uint RenderPipeGen();
	void RenderPipeCompile(uint pipe, in RenderPipeDesc desc);
	void RenderPipeDelete(uint pipe);

	// Resource set
	uint ResourceSetGen();
	void ResourceSetDelete(uint set);
	void ResourceSetLayout(uint set, in ResourceSetLayout layout);
	void ResourceSetBindBuffer(uint set, int slot, ResourceType type, uint buffer, int offset, int size);
	void ResourceSetBindTexture(uint set, int slot, uint texture, uint sampler);

	// Encoder
	uint EncoderGen();
	void EncoderDelete(uint encoder);
	void EncoderReset(uint encoder);
	void EncoderCompile(uint encoder, in EncoderDesc desc);
	void EncoderQueuedExecute(uint encoder);
	void EncoderTopology(uint encoder, Topology topology);
	void EncoderBuffer(uint encoder, BufferType type, uint buffer);
	void EncoderResourceSet(uint encoder, int slot, uint set);
	void EncoderDraw(uint encoder, int vertexCount, int firstVertex);
	void EncoderDrawIndexed(uint encoder, int indexCount, int firstIndex);
	void EncoderDispatch(uint encoder, uint x, uint y, uint z);
	void EncoderViewport(uint encoder, int x, int y, int width, int height);
	void EncoderScissor(uint encoder, in ScissorDesc desc);
	void EncoderRenderPipe(uint encoder, uint pipe);
}

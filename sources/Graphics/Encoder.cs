#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
#endregion

namespace Mino.Graphics;

/// <summary>
///     A gpu command encoder.
/// </summary>
public interface Encoder : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The encoder desc.
	/// </summary>
	EncoderDesc Desc { get; }

	/// <summary>
	///     Clears all commands for reuse.
	/// </summary>
	void Reset();

	/// <summary>
	///     Executes the encoder on proper timing.
	/// </summary>
	void QueuedExecute();

	/// <summary>
	///     Encodes a set topology command.
	/// </summary>
	/// <param name="topology">Target primitive topology.</param>
	void SetTopology(Topology topology);

	/// <summary>
	///     Encodes a bind buffer command.
	/// </summary>
	/// <param name="buffer">Buffer like index buffer, vertex buffer, etc.</param>
	void SetBuffer(BufferObject buffer);

	/// <summary>
	///     Encodes a set pipe command.
	/// </summary>
	/// <param name="pipe">Render pipe to bind.</param>
	void SetRenderPipe(RenderPipe pipe);

	/// <summary>
	///     Encodes a set viewport command.
	/// </summary>
	/// <param name="x">Viewport x.</param>
	/// <param name="y">Viewport y.</param>
	/// <param name="width">Viewport width.</param>
	/// <param name="height">Viewport height.</param>
	void SetViewport(int x, int y, int width, int height);

	/// <summary>
	///     Encodes a set scissor command.
	/// </summary>
	/// <param name="desc">Scissor state.</param>
	void SetScissor(in ScissorDesc desc);

	/// <summary>
	///     Encodes a set resource command.
	/// </summary>
	/// <param name="slot">Resource set slot.</param>
	/// <param name="set">Bound resource set.</param>
	void SetResource(int slot, ResourceSet set);

	/// <summary>
	///     Encodes a draw arrays command.
	/// </summary>
	/// <param name="vertexCount">Vertex count.</param>
	/// <param name="firstVertex">First vertex.</param>
	void Draw(int vertexCount, int firstVertex);

	/// <summary>
	///     Encodes a draw indexed command.
	/// </summary>
	/// <param name="indexCount">Index count.</param>
	/// <param name="firstIndex">First index.</param>
	void DrawIndexed(int indexCount, int firstIndex);

	/// <summary>
	///     Encodes a compute dispatch command.
	/// </summary>
	/// <param name="x">Work groups x.</param>
	/// <param name="y">Work groups y.</param>
	/// <param name="z">Work groups z.</param>
	void Dispatch(uint x, uint y, uint z);
}

using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics;

/// <summary>
///     A gpu command encoder.
/// </summary>
public class Encoder : IDisposable {
	private RenderBackend _backend;
	private int _cmdCnt = 0;
	private bool _disposed;
	private uint _handle;

	public Encoder(in EncoderDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.EncoderGen();
		// Compile the encoder.
		_backend.EncoderCompile(_handle, desc);
	}

	/// <summary>
	///     The encoder desc.
	/// </summary>
	public EncoderDesc Desc { get; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.EncoderDelete(_handle);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Clears all commands for reuse.
	/// </summary>
	public void Reset() {
		if (_cmdCnt == 0) {
			// Perf check.
			return;
		}
		_cmdCnt = 0;
		_backend.EncoderReset(_handle);
	}

	/// <summary>
	///     Executes the encoder on proper timing.
	/// </summary>
	public void QueuedExecute() {
		_cmdCnt++;
		_backend.EncoderQueuedExecute(_handle);
	}

	/// <summary>
	///     Encodes a set topology command.
	/// </summary>
	/// <param name="topology">Target primitive topology.</param>
	public void SetTopology(Topology topology) {
		_cmdCnt++;
		_backend.EncoderTopology(_handle, topology);
	}

	/// <summary>
	///     Encodes a bind buffer command.
	/// </summary>
	/// <param name="buffer">Buffer like index buffer, vertex buffer, etc.</param>
	/// <exception cref="Error">Thrown if buffer type is invalid.</exception>
	public void SetBuffer(Buffer buffer) {
		if (buffer.Desc.Type == BufferType.Uniform) {
			throw new Error("unexpected uniform buffer");
		}
		_cmdCnt++;
		_backend.EncoderBuffer(_handle, buffer.Desc.Type, buffer);
	}

	/// <summary>
	///     Encodes a set pipeline command.
	/// </summary>
	/// <param name="pipeline">Pipeline to bind.</param>
	/// <exception cref="Error">Thrown if encoder is not compatible with the given pipeline.</exception>
	public void SetPipeline(Pipeline pipeline) {
		EncoderUsage uA = Desc.Usage;
		PipelineType uB = pipeline.Desc.Type;

		if ((int) uA != (int) uB) {
			throw new Error("different encoder-pipeline usage");
		}
		_cmdCnt++;
		_backend.EncoderPipeline(_handle, pipeline);
	}

	/// <summary>
	///     Encodes a set viewport command.
	/// </summary>
	/// <param name="x">Viewport x.</param>
	/// <param name="y">Viewport y.</param>
	/// <param name="width">Viewport width.</param>
	/// <param name="height">Viewport height.</param>
	/// <exception cref="Error">Thrown if width or height is negative.</exception>
	public void SetViewport(int x, int y, int width, int height) {
		if (width < 0 || height < 0) {
			throw new Error("invalid viewport");
		}
		_cmdCnt++;
		_backend.EncoderViewport(_handle, x, y, width, height);
	}

	/// <summary>
	///     Encodes a set scissor command.
	/// </summary>
	/// <param name="desc">Scissor state.</param>
	/// <exception cref="Error">Thrown if scissor is invalid.</exception>
	public void SetScissor(in ScissorDesc desc) {
		if (desc.Width < 0 || desc.Height < 0) {
			throw new Error("invalid scissor");
		}
		_cmdCnt++;
		_backend.EncoderScissor(_handle, desc);
	}

	/// <summary>
	///     Encodes a set resource command.
	/// </summary>
	/// <param name="slot">Resource set slot.</param>
	/// <param name="set">Bound resource set.</param>
	public void SetResource(int slot, ResourceSet set) {
		// We do not check slot because bound pipeline is unknown.
		_cmdCnt++;
		_backend.EncoderResourceSet(_handle, slot, set);
	}

	/// <summary>
	///     Encodes a draw arrays command.
	/// </summary>
	/// <param name="vertexCount">Vertex count.</param>
	/// <param name="firstVertex">First vertex.</param>
	public void Draw(int vertexCount, int firstVertex) {
		_cmdCnt++;
		_backend.EncoderDraw(_handle, vertexCount, firstVertex);
	}

	/// <summary>
	///     Encodes a draw indexed command.
	/// </summary>
	/// <param name="indexCount">Index count.</param>
	/// <param name="firstIndex">First index.</param>
	public void DrawIndexed(int indexCount, int firstIndex) {
		_cmdCnt++;
		_backend.EncoderDrawIndexed(_handle, indexCount, firstIndex);
	}

	/// <summary>
	///     Encodes a compute dispatch command.
	/// </summary>
	/// <param name="x">Work groups x.</param>
	/// <param name="y">Work groups y.</param>
	/// <param name="z">Work groups z.</param>
	public void Dispatch(uint x, uint y, uint z) {
		_cmdCnt++;
		_backend.EncoderDispatch(_handle, x, y, z);
	}

	// Finalizer in case.
	~Encoder() {
		Dispose();
	}

	// Implicit cast to native handle.
	public static implicit operator uint(Encoder obj) {
		return obj._handle;
	}
}

using System.Collections;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Mino.Nio;
using Mino.Utility;

namespace Mino.Graphics.Sprite;

/// <summary>
///		Flushable mesh working with brush.
/// </summary>
public sealed class MultiMesh : IEnumerable<MultiMesh.Node>, IDisposable {
	/// <summary>
	///		Ultimate mesh will be automatedly cleaned.
	/// </summary>
	public static readonly MultiMesh Ultimate = new MultiMesh();
	
	/// <summary>
	///		Each node of a multi mesh works under a set of render states.
	/// </summary>
	public sealed class Node {
		// Not so big initial cap so that it won't lag.
		public const int InitialCap = 1024;
		
		public readonly ByteBuffer VertexBuf = new ByteBuffer().With(Endianness.Native);
		public readonly ByteBuffer IndexBuf = new ByteBuffer().With(Endianness.Native);
		public readonly BufferObject Vbo;
		public readonly BufferObject Ibo;
		public bool Dirty = false;
		public int VertexCount = 0;
		public int IndexCount = 0;
		public BrushState RecordedState;

		public Node() {
			BufferObjectDesc desc = new BufferObjectDesc {
				Frequency = BufferFrequency.Stream,
				Usage = BufferUsage.GpuRead | BufferUsage.CpuWrite
			};

			Vbo = RenderSystem.Create<BufferObject>(desc with {
				Type = BufferType.Vertex
			});
			Vbo.Allocate<byte>(InitialCap, null);

			Ibo = RenderSystem.Create<BufferObject>(desc with {
				Type = BufferType.Index
			});
			Ibo.Allocate<byte>(InitialCap, null);
		}
		
		/// <summary>
		///		Writes vertex and index counts.
		/// </summary>
		/// <param name="vertex">Vertex count addition.</param>
		/// <param name="index">Index count addition.</param>
		public void Write(int vertex, int index) {
			VertexCount += vertex;
			IndexCount += index;
			Dirty = true;
		}

		/// <summary>
		///		Resets the node to empty.
		/// </summary>
		public void Reset() {
			VertexCount = 0;
			IndexCount = 0;
			VertexBuf.Clear();
			IndexBuf.Clear();
			Dirty = false;
		}
	}

	private List<Node> _nodes = new List<Node>();
	private int _curNode = 0;
	private Brush? _brush;
	private bool _disposed;

	public MultiMesh() {
		// Initial node.
		_nodes.Add(new Node());
	}

	/// <summary>
	///		Whether the mesh is ultimate, that's to say, it will not keep its content.  
	/// </summary>
	public bool IsUltimate {
		get => this == Ultimate;
	}
	
	/// <summary>
	///		Acquires a node.
	/// </summary>
	/// <returns>The next node.</returns>
	public Node Acquire() {
		if (_curNode < _nodes.Count) {
			return _nodes[_curNode++];
		}

		// Use overallocated empty node first.
		if (_nodes[_curNode - 1].VertexCount == 0) {
			return _nodes[_curNode - 1];
		}
		
		Node node = new Node();
		_nodes.Add(node);
		return _nodes[_curNode++];
	}
	
	/// <summary>
	///		Begins a mesh record.
	/// </summary>
	/// <returns>Mesh brush.</returns>
	public Brush Begin(in RenderPassDesc? desc = null) {
		_curNode = 0;
		
		Brush brush = _brush ??= new Brush(this);
		brush.NextNode();
		brush.Begin(desc);
		return brush;
	}

	/// <summary>
	///		Ends a mesh record.
	/// </summary>
	/// <exception cref="Crash">Thrown if brush is null.</exception>
	public void End() {
		if (_brush == null) {
			throw new Crash("Brush is not initialized");
		}
		
		_brush.End();
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_brush?.Dispose();

		foreach (Node node in _nodes) {
			node.Vbo.Dispose();
			node.Ibo.Dispose();
		}
	}
	
	public IEnumerator<Node> GetEnumerator() {
		return _nodes.GetEnumerator();
	}
	
	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}

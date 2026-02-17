#region
using Mino.Graphics.Hardware.Enum;
using Silk.NET.OpenGL;
using PolygonMode = Mino.Graphics.Hardware.Enum.PolygonMode;
using ShaderType = Mino.Graphics.Hardware.Enum.ShaderType;
#endregion

namespace Mino.Native.OpenGL;

public class GLEnumC {
	public static GLEnum Cast(BufferType type) {
		return type switch {
			BufferType.Vertex => GLEnum.ArrayBuffer,
			BufferType.Index => GLEnum.ElementArrayBuffer,
			BufferType.Uniform => GLEnum.UniformBuffer,
			_ => throw new Error("invalid arg: " + nameof(type))
		};
	}

	public static GLEnum Cast(BufferUsage usage, BufferFrequency freq) {
		if (freq == BufferFrequency.Static) {
			if ((usage & BufferUsage.GpuWrite) != 0) {
				return GLEnum.StaticCopy;
			}
			if ((usage & BufferUsage.CpuWrite) != 0) {
				return GLEnum.StaticDraw;
			}
			return GLEnum.StaticRead;
		}
		if (freq == BufferFrequency.Dynamic) {
			if ((usage & BufferUsage.GpuWrite) != 0) {
				return GLEnum.DynamicCopy;
			}
			if ((usage & BufferUsage.CpuWrite) != 0) {
				return GLEnum.DynamicDraw;
			}
			return GLEnum.DynamicRead;
		}
		if (freq == BufferFrequency.Stream) {
			if ((usage & BufferUsage.GpuWrite) != 0) {
				return GLEnum.StreamCopy;
			}
			if ((usage & BufferUsage.CpuWrite) != 0) {
				return GLEnum.StreamDraw;
			}
			return GLEnum.StreamRead;
		}
		throw new Error("invalid arg: " + nameof(usage));
	}

	public static GLEnum Cast(VertexAttributeType type) {
		return type switch {
			VertexAttributeType.uint8 => GLEnum.UnsignedByte,
			VertexAttributeType.uint16 => GLEnum.UnsignedShort,
			VertexAttributeType.uint32 => GLEnum.UnsignedInt,
			VertexAttributeType.Int8 => GLEnum.Byte,
			VertexAttributeType.Int16 => GLEnum.Short,
			VertexAttributeType.Int32 => GLEnum.Int,
			VertexAttributeType.Float16 => GLEnum.HalfFloat,
			VertexAttributeType.Float32 => GLEnum.Float,
			_ => throw new Error("invalid arg: " + nameof(type))
		};
	}

	public static GLEnum Cast(BlendFactor factor) {
		return factor switch {
			BlendFactor.Zero => GLEnum.Zero,
			BlendFactor.One => GLEnum.One,
			BlendFactor.SrcColor => GLEnum.SrcColor,
			BlendFactor.OneMinusSrcColor => GLEnum.OneMinusSrcColor,
			BlendFactor.DstColor => GLEnum.DstColor,
			BlendFactor.OneMinusDstColor => GLEnum.OneMinusDstColor,
			BlendFactor.SrcAlpha => GLEnum.SrcAlpha,
			BlendFactor.OneMinusSrcAlpha => GLEnum.OneMinusSrcAlpha,
			BlendFactor.DstAlpha => GLEnum.DstAlpha,
			BlendFactor.OneMinusDstAlpha => GLEnum.OneMinusDstAlpha,
			BlendFactor.ConstantColor => GLEnum.ConstantColor,
			BlendFactor.OneMinusConstantColor => GLEnum.OneMinusConstantColor,
			BlendFactor.ConstantAlpha => GLEnum.ConstantAlpha,
			BlendFactor.OneMinusConstantAlpha => GLEnum.OneMinusConstantAlpha,
			BlendFactor.SrcAlphaSaturate => GLEnum.SrcAlphaSaturate,
			_ => throw new Error("invalid arg: " + nameof(factor))
		};
	}

	public static GLEnum Cast(BlendFunc func) {
		return func switch {
			BlendFunc.Add => GLEnum.FuncAdd,
			BlendFunc.Subtract => GLEnum.FuncSubtract,
			BlendFunc.ReverseSubtract => GLEnum.FuncReverseSubtract,
			BlendFunc.Min => GLEnum.Min,
			BlendFunc.Max => GLEnum.Max,
			_ => throw new Error("invalid arg: " + nameof(func))
		};
	}

	public static GLEnum Cast(CompareOp cd) {
		return cd switch {
			CompareOp.Always => GLEnum.Always,
			CompareOp.Never => GLEnum.Never,
			CompareOp.Equal => GLEnum.Equal,
			CompareOp.NotEqual => GLEnum.Notequal,
			CompareOp.Greater => GLEnum.Greater,
			CompareOp.GreaterOrEqual => GLEnum.Gequal,
			CompareOp.Less => GLEnum.Less,
			CompareOp.LessOrEqual => GLEnum.Lequal,
			_ => throw new Error("invalid arg: " + nameof(cd))
		};
	}

	public static GLEnum Cast(StencilFunc op) {
		return op switch {
			StencilFunc.Keep => GLEnum.Keep,
			StencilFunc.Replace => GLEnum.Replace,
			StencilFunc.Zero => GLEnum.Zero,
			StencilFunc.Incr => GLEnum.Incr,
			StencilFunc.Decr => GLEnum.Decr,
			StencilFunc.IncrWrap => GLEnum.IncrWrap,
			StencilFunc.DecrWrap => GLEnum.DecrWrap,
			StencilFunc.Invert => GLEnum.Invert,
			_ => throw new Error("invalid arg: " + nameof(op))
		};
	}

	public static (GLEnum, GLEnum, GLEnum) Cast(TextureFormat format) {
		return format switch {
			// 8-bit unsigned integer formats
			TextureFormat.Red8 => (GLEnum.R8, GLEnum.Red, GLEnum.UnsignedByte),
			TextureFormat.RedGreen8 => (GLEnum.RG8, GLEnum.RG, GLEnum.UnsignedByte),
			TextureFormat.RedGreenBlue8 => (GLEnum.Rgb8, GLEnum.Rgb, GLEnum.UnsignedByte),
			TextureFormat.RedGreenBlueAlpha8 => (GLEnum.Rgba8, GLEnum.Rgba, GLEnum.UnsignedByte),

			// 16-bit floating point formats
			TextureFormat.Red16F => (GLEnum.R16f, GLEnum.Red, GLEnum.HalfFloat),
			TextureFormat.RedGreen16F => (GLEnum.RG16f, GLEnum.RG, GLEnum.HalfFloat),
			TextureFormat.RedGreenBlue16F => (GLEnum.Rgb16f, GLEnum.Rgb, GLEnum.HalfFloat),
			TextureFormat.RedGreenBlueAlpha16F => (GLEnum.Rgba16f, GLEnum.Rgba, GLEnum.HalfFloat),

			// 32-bit floating point formats
			TextureFormat.Red32F => (GLEnum.R32f, GLEnum.Red, GLEnum.Float),
			TextureFormat.RedGreen32F => (GLEnum.RG32f, GLEnum.RG, GLEnum.Float),
			TextureFormat.RedGreenBlue32F => (GLEnum.Rgb32f, GLEnum.Rgb, GLEnum.Float),
			TextureFormat.RedGreenBlueAlpha32F => (GLEnum.Rgba32f, GLEnum.Rgba, GLEnum.Float),

			// Depth and depth-stencil formats
			TextureFormat.Depth16 => (GLEnum.DepthComponent16, GLEnum.DepthComponent, GLEnum.UnsignedShort),
			TextureFormat.Depth24 => (GLEnum.DepthComponent24, GLEnum.DepthComponent, GLEnum.UnsignedInt),
			TextureFormat.Depth32F => (GLEnum.DepthComponent32f, GLEnum.DepthComponent, GLEnum.Float),
			TextureFormat.Depth24Stencil8 => (GLEnum.Depth24Stencil8, GLEnum.DepthStencil, GLEnum.UnsignedInt248),

			_ => throw new Error("invalid arg: " + nameof(format))
		};
	}

	public static GLEnum Cast(TextureType type) {
		return type switch {
			TextureType.Texture1D => GLEnum.Texture1D,
			TextureType.Texture2D => GLEnum.Texture2D,
			TextureType.Texture3D => GLEnum.Texture3D,
			_ => throw new Error("invalid arg: " + nameof(type))
		};
	}

	public static int Cast(TextureWrap wrap) {
		return wrap switch {
			TextureWrap.Repeat => (int) GLEnum.Repeat,
			TextureWrap.MirroredRepeat => (int) GLEnum.MirroredRepeat,
			TextureWrap.ClampToEdge => (int) GLEnum.ClampToEdge,
			TextureWrap.ClampToBorder => (int) GLEnum.ClampToBorder,
			_ => throw new Error("invalid arg: " + nameof(wrap))
		};
	}

	public static int Cast(TextureFilter filter) {
		return filter switch {
			TextureFilter.Linear => (int) GLEnum.Linear,
			TextureFilter.Nearest => (int) GLEnum.Nearest,
			TextureFilter.LinearMipmapLinear => (int) GLEnum.LinearMipmapLinear,
			TextureFilter.LinearMipmapNearest => (int) GLEnum.LinearMipmapNearest,
			TextureFilter.NearestMipmapLinear => (int) GLEnum.NearestMipmapLinear,
			TextureFilter.NearestMipmapNearest => (int) GLEnum.NearestMipmapNearest,
			_ => throw new Error("invalid arg: " + nameof(filter))
		};
	}

	public static GLEnum Cast(ShaderType type) {
		return type switch {
			ShaderType.Vertex => GLEnum.VertexShader,
			ShaderType.Fragment => GLEnum.FragmentShader,
			ShaderType.Geometry => GLEnum.GeometryShader,
			ShaderType.Compute => GLEnum.ComputeShader,
			_ => throw new Error("invalid arg: " + nameof(type))
		};
	}

	public static GLEnum Cast(Topology mode) {
		return mode switch {
			Topology.Triangle => GLEnum.Triangles,
			Topology.TriangleFan => GLEnum.TriangleFan,
			Topology.TriangleStrip => GLEnum.TriangleStrip,
			Topology.Line => GLEnum.Lines,
			Topology.LineStrip => GLEnum.LineStrip,
			Topology.LineLoop => GLEnum.LineLoop,
			Topology.Point => GLEnum.Points,
			_ => throw new Error("invalid arg: " + nameof(mode))
		};
	}

	public static GLEnum Cast(PolygonMode mode) {
		return mode switch {
			PolygonMode.Fill => GLEnum.Fill,
			PolygonMode.Line => GLEnum.Line,
			PolygonMode.Point => GLEnum.Point,
			_ => throw new Error("invalid arg: " + nameof(mode))
		};
	}

	public static GLEnum Cast(CullMode mode) {
		return mode switch {
			CullMode.None => GLEnum.None,
			CullMode.Back => GLEnum.Back,
			CullMode.Front => GLEnum.Front,
			CullMode.FrontAndBack => GLEnum.FrontAndBack,
			_ => throw new Error("invalid arg: " + nameof(mode))
		};
	}
}

using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a texture sampler.
/// </summary>
public record struct SamplerDesc {
	public float AnisotropyLevel;
	public float LodBias;
	public TextureFilter MagFilter;
	public float MaxLod;
	public TextureFilter MinFilter;
	public float MinLod;
	public int MipmapLevel;
	public int SampleCount;
	public Color4f WrapBorderColor;
	public TextureWrap WrapX;
	public TextureWrap WrapY;
	public TextureWrap WrapZ;

	public SamplerDesc() {
		AnisotropyLevel = 1.0F;
		LodBias = 0.0F;
		MagFilter = TextureFilter.Nearest;
		MaxLod = 0.0F;
		MinFilter = TextureFilter.Nearest;
		MinLod = 0.0F;
		MipmapLevel = 0;
		SampleCount = 1;
		WrapBorderColor = Color4f.Empty;
		WrapX = TextureWrap.Repeat;
		WrapY = TextureWrap.Repeat;
		WrapZ = TextureWrap.Repeat;
	}
}

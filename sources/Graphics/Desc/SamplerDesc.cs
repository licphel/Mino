#region
using Mino.Graphics.Enum;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Desc;

/// <summary>
///     Describes a texture sampler.
/// </summary>
public record struct SamplerDesc {
	public TextureFilter MagFilter;
	public TextureFilter MinFilter;
	public int MipmapLevel;
	public int SampleCount;
	public TextureWrap WrapX;
	public TextureWrap WrapY;
	public TextureWrap WrapZ;
	public Color WrapBorderColor;
	public float LodBias;
	public float MinLod;
	public float MaxLod;
	public float AnisotropyLevel;

	public SamplerDesc() {
		AnisotropyLevel = 1.0F;
		LodBias = 0.0F;
		MagFilter = TextureFilter.Nearest;
		MaxLod = 0.0F;
		MinFilter = TextureFilter.Nearest;
		MinLod = 0.0F;
		MipmapLevel = 0;
		SampleCount = 1;
		WrapBorderColor = Color.Empty;
		WrapX = TextureWrap.Repeat;
		WrapY = TextureWrap.Repeat;
		WrapZ = TextureWrap.Repeat;
	}
}

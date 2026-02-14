using System.Runtime.InteropServices;
using FreeTypeSharp;
using Mino.Nio;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace Mino.Graphics.Text;

/// <summary>
///     Freetype based font.
/// </summary>
public unsafe class Font : IDisposable {
	public const string NUMBER = "1234567890";
	public const string ALPHABETIC = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	public const string ASCII = "!@#$%^&*()_+-=[]{}|\\;':\"<>,./?~`" + ALPHABETIC + NUMBER;

	/*
	 * Basic line height.
	 * To render, for example 32px text, we can calculate scaling based on this.
	 */
	public const float BASIC_LH = 16.0F;
	
	private FT_FaceRec_* _ftFace;
	private FT_LibraryRec_* _ftLib;
	private TextureAtlas _atlas = new TextureAtlas();
	private Dictionary<char, Glyph> _glyphs = new Dictionary<char, Glyph>();
	private uint _resolution;
	private bool _disposed;
	
	// Forbit everyone directly new a font.
	private Font() {
	}

	/// <summary>
	///     Font information.
	/// </summary>
	public FontInfo Info { get; set; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		FT_Done_Face(_ftFace);
		FT_Done_Library(_ftLib);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Gets the glyph data of the given character.
	/// </summary>
	/// <param name="ch">The character.</param>
	/// <returns>A glyph data.</returns>
	public Glyph GetGlyph(char ch) {
		if (_glyphs.TryGetValue(ch, out Glyph glyph)) {
			return glyph;
		}

		uint idx = FT_Get_Char_Index(_ftFace, ch);
		FT_Set_Pixel_Sizes(_ftFace, 0, _resolution);
		FT_Load_Glyph(_ftFace, idx, FT_LOAD_DEFAULT);
		FT_Render_Glyph(_ftFace->glyph, FT_RENDER_MODE_NORMAL);
		FT_Bitmap_ m0 = _ftFace->glyph->bitmap;

		int len = (int) (m0.width * m0.rows * 4);
		byte[] imgData = new byte[len];
		for (int i = 0; i < len; i += 4) {
			byte grey = m0.buffer[i / 4];
			imgData[i + 0] = 255;
			imgData[i + 1] = 255;
			imgData[i + 2] = 255;
			imgData[i + 3] = grey;
		}

		FT_GlyphSlotRec_* ftGlyph = _ftFace->glyph;
		FT_Glyph_Metrics_ metrics = ftGlyph->metrics;

		TexturePart texture =
			_atlas.Accept(Image.Create((int) ftGlyph->bitmap.width, (int) ftGlyph->bitmap.rows, imgData));

		float scale = 1.0F / (_resolution / BASIC_LH * 64.0F);

		// Creates glyph data.
		return _glyphs[ch] = new Glyph(
			texture,
			metrics.width * scale,
			metrics.height * scale,
			ftGlyph->advance.x * scale,
			metrics.horiBearingX * scale,
			metrics.horiBearingY * scale
		);
	}

	/// <summary>
	///     Checks if the font support the given character.
	/// </summary>
	/// <param name="ch">character to check.</param>
	/// <returns>True if supported, otherwise false.</returns>
	public bool Has(char ch) {
		uint idx = FT_Get_Char_Index(_ftFace, ch);
		return idx != 0;
	}

	/// <summary>
	///     Bakes a text blob for rendering.
	/// </summary>
	/// <param name="text">Target text.</param>
	/// <param name="maxWidth">Max render width.</param>
	/// <param name="nextLine">Next line strategy.</param>
	/// <param name="lineH">Line height.</param>
	/// <returns>A baked text blob.</returns>
	public TextBlob Bake(string text, float maxWidth = int.MaxValue, TextNextLine nextLine = TextNextLine.Latin, float lineH = BASIC_LH) {
		return new TextBlob(text, this, lineH, maxWidth, nextLine);
	}

	private void init() {
		_atlas.Init();

		float scale = 1.0F / (_resolution / BASIC_LH * 64.0F);
		// Init info.
		Info = new FontInfo(
			_ftFace->ascender * scale,
			_ftFace->descender * scale,
			(_ftFace->ascender - _ftFace->descender) * scale,
			BASIC_LH
		);
	}

	/// <summary>
	///     Loads a font.
	/// </summary>
	/// <param name="url">Font path locally.</param>
	/// <param name="quality">Quality of font rendering.</param>
	/// <returns>A new font.</returns>
	public static Font Load(Url url, FontQuality quality = FontQuality.Medium) {
		FT_LibraryRec_* lib;
		FT_FaceRec_* face;
		FT_Init_FreeType(&lib);
		FT_New_Face(lib, (byte*) Marshal.StringToHGlobalAnsi(url.ToFilePath()), 0, &face);
		FT_Select_Charmap(face, FT_Encoding_.FT_ENCODING_UNICODE);

		// Handle resolutions.
		uint res = quality switch {
			FontQuality.Low => 32U,
			FontQuality.Medium => 64U,
			FontQuality.High => 128U,
			_ => throw new Error("invalid arg: " + nameof(quality))
		};
		Font font = new Font { _ftLib = lib, _ftFace = face, _resolution = res };
		font.init();
		return font;
	}
}

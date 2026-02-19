#region
using System.Runtime.InteropServices;
using FreeTypeSharp;
using Mino.Nio;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;
#endregion

namespace Mino.Graphics.Text;

/// <summary>
///     Freetype based font.
/// </summary>
public unsafe class Font : IDisposable {
	public const string Number = "1234567890";
	public const string Alphabetic = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	public const string Ascii = "!@#$%^&*()_+-=[]{}|\\;':\"<>,./?~`" + Alphabetic + Number;

	/*
	 * Basic line height.
	 * To render, for example 32px text, we can calculate scaling based on this.
	 */
	public const float BasicLineHeight = 16.0F;

	private FT_FaceRec_* _ftFace;
	private FT_LibraryRec_* _ftLib;
	private TextureAtlas _atlas = new TextureAtlas();
	private Dictionary<uint, Glyph> _glyphs = new Dictionary<uint, Glyph>();
	private uint _resolution;
	private bool _disposed;
	private bool _pixel;

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
	/// <param name="style">Style of the font.</param>
	/// <returns>A glyph data.</returns>
	public Glyph GetGlyph(char ch, FontStyle style) {
		uint key = (uint) style << 16 | ch;
		
		if (_glyphs.TryGetValue(key, out Glyph glyph)) {
			return glyph;
		}

		uint idx = FT_Get_Char_Index(_ftFace, ch);
		FT_Load_Glyph(_ftFace, idx, _pixel ? (FT_LOAD) FT_LOAD_TARGET_MONO : FT_LOAD_DEFAULT);
		
		if ((style & FontStyle.Bold) != 0) {
			FT_GlyphSlot_Embolden(_ftFace->glyph);
			_ftFace->glyph->advance.x = (int) (_ftFace->glyph->advance.x * 1.1F);
		}
		if ((style & FontStyle.Italic) != 0) {
			FT_GlyphSlot_Oblique(_ftFace->glyph);
		}
		
		FT_Render_Glyph(_ftFace->glyph, _pixel ? FT_RENDER_MODE_MONO : FT_RENDER_MODE_NORMAL);
		FT_Bitmap_ m0 = _ftFace->glyph->bitmap;
		
		int width = (int)m0.width;
		int height = (int)m0.rows;
		int datL = width * height * 4;
		byte[] dat = new byte[datL];
		
		if (_pixel) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int bytePos = (y * ((width + 7) / 8)) + (x / 8);
					int bitPos = 7 - x % 8;
            
					byte bits = m0.buffer[bytePos];
					byte pixel = (byte) (bits >> bitPos & 1);
            
					int datPos = (y * width + x) * 4;
					dat[datPos + 0] = 255;
					dat[datPos + 1] = 255;
					dat[datPos + 2] = 255;
					dat[datPos + 3] = pixel == 1 ? (byte)255 : (byte)0;
				}
			}
		} else
		{
			for (int i = 0; i < datL; i += 4) {
				byte grey = m0.buffer[i / 4];
				dat[i + 0] = 255;
				dat[i + 1] = 255;
				dat[i + 2] = 255;
				dat[i + 3] = grey;
			}
		}

		FT_GlyphSlotRec_* ftGlyph = _ftFace->glyph;
		FT_Glyph_Metrics_ metrics = ftGlyph->metrics;

		TexturePart texture =
			_atlas.Accept(Image.Create((int) ftGlyph->bitmap.width, (int) ftGlyph->bitmap.rows, dat));

		float scale = 1.0F / (_resolution / BasicLineHeight * 64.0F);
		
		// Creates glyph data.
		return _glyphs[key] = new Glyph(
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
	/// <param name="lineH">Line height.</param>
	/// <param name="style">Font style.</param>
	/// <returns>A baked text blob.</returns>
	public TextBlob Bake(string text, float maxWidth = int.MaxValue, float lineH = BasicLineHeight, FontStyle style = FontStyle.Regular) {
		return new TextBlob(this, text, maxWidth, lineH, style);
	}

	private void init() {
		_atlas.Init();
		SetResolution((int) BasicLineHeight);
	}

	/// <summary>
	///		Sets the font to pixel mode.
	/// </summary>
	public void SetPixel() {
		_pixel = true;
	}

	/// <summary>
	///		Sets font source resolution.
	/// </summary>
	/// <param name="res">Font source resolution.</param>
	public void SetResolution(int res) {
		_resolution = (uint) res;
		FT_Set_Pixel_Sizes(_ftFace, 0, (uint) res);
		
		float scale = 1.0F / (_resolution / BasicLineHeight * 64.0F);
		// Init info.
		
		Info = new FontInfo(
			_ftFace->ascender * scale,
			_ftFace->descender * scale,
			_ftFace->size->metrics.height * scale,
			_ftFace->underline_position * scale,
			_ftFace->underline_thickness * scale,
			_ftFace->ascender * scale / 2.0F,
			_ftFace->underline_thickness * scale
		);
	}
	
	/// <summary>
	///     Loads a font.
	/// </summary>
	/// <param name="url">Font path locally.</param>
	/// <returns>A new font.</returns>
	public static Font Load(Url url) {
		FT_LibraryRec_* lib;
		FT_FaceRec_* face;
		FT_Init_FreeType(&lib);
		FT_New_Face(lib, (byte*) Marshal.StringToHGlobalAnsi(url.ToFilePath()), 0, &face);
		FT_Select_Charmap(face, FT_Encoding_.FT_ENCODING_UNICODE);
		
		Font font = new Font {
			_ftLib = lib, 
			_ftFace = face
		};
		font.init();
		return font;
	}
}

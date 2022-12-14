using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using OpenTK;
using Stareater.GLData.SpriteShader;

namespace Stareater.GLData
{
	class TextRenderUtil
	{
		#region Singleton
		static TextRenderUtil instance = null;

		public static TextRenderUtil Get
		{
			get
			{
				if (instance == null)
					instance = new TextRenderUtil();
				return instance;
			}
		}
		#endregion

		const float SdfFontSize = 35;
		const int SdfPadding = 4;

		public const float AntialiasPixelSize = SdfFontSize / SdfPadding / 2;

		const int Width = 512;
		const int Height = 512;
		const int Spacing = 2;

		const float SpaceUnitWidth = 0.25f;
		const float AntialiasWidth = 1 / SdfFontSize;
		
		private ColorMap textureData;

		private readonly Dictionary<char, CharTextureInfo> characterInfos = new Dictionary<char, CharTextureInfo>();
		private readonly Font font;
		private readonly AtlasBuilder textureBuilder = new AtlasBuilder(Spacing, new Size(Width, Height));
		private readonly float lineScale;

		private TextRenderUtil()
		{
			this.font = new Font("Arial", SdfFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
			var fontFamily = this.font.FontFamily;
			this.lineScale = fontFamily.GetLineSpacing(this.font.Style) / (float) fontFamily.GetEmHeight(this.font.Style);
		}

		public int TextureId { get; private set; }

		
		public void Initialize()
		{
			if (this.textureData == null)
				this.textureData = new ColorMap(Width, Height, Color.FromArgb(0, 255, 255, 255));
			
			if (this.TextureId == 0)
				this.TextureId = TextureUtils.CreateTexture(this.textureData);
		}

		public Vector2 SizeOf(string text)
		{
			if (string.IsNullOrEmpty(text))
				return new Vector2();

			this.prepare(text);
			return this.measureSize(text);
		}

		public Dictionary<float, IEnumerable<float>> BufferText(string text, float adjustment, float alignment, float z0, float zRange)
		{
			this.prepare(text);
			float textWidth = this.measureSize(text).X;

			float charOffsetX = textWidth * adjustment;
			float charOffsetY = 0;
			var layers = new List<float>[4];
			int row = 0;
			int colunm = 0;
			for (int i = 0; i < layers.Length; i++)
				layers[i] = new List<float>();

			foreach (var line in text.Split('\n'))
			{
				var lineWidth = line.Sum(c => 
					c == ' ' ? SpaceUnitWidth : 
					!char.IsWhiteSpace(c) ? this.characterInfos[c].Width : 
					0
				);
				charOffsetX = textWidth * adjustment + (textWidth - lineWidth) * alignment;

				foreach (char c in line)
					if (!char.IsWhiteSpace(c))
					{
						var charInfo = this.characterInfos[c];
						var layer = 2 * (row % 2) + colunm % 2;

						for (int v = 0; v < 6; v++)
							layers[layer].AddRange(SpriteHelpers.TexturedVertex(
								charInfo.VertexCoords[v].X + charOffsetX,
								charInfo.VertexCoords[v].Y + charOffsetY,
								charInfo.TextureCoords[v].X, charInfo.TextureCoords[v].Y));

						charOffsetX += charInfo.Width;
						colunm++;
					}
					else if (c == ' ')
						charOffsetX += SpaceUnitWidth;

				charOffsetY -= this.lineScale;
				row++;
				colunm = 0;

			}

			return Enumerable.Range(0, layers.Length).
				ToDictionary(i => z0 - i * zRange / 4, i => (IEnumerable<float>)layers[i]);
		}

		private Vector2 measureSize(string text)
		{
			var textWidth = 0f;
			var textHeight = 0f;
			var lineY = 0f;

			foreach (var line in text.Split('\n'))
			{
				var lineWidth = 0f;
				var lineHeight = this.lineScale;

				foreach (char c in line)
				{
					if (!char.IsWhiteSpace(c))
					{
						lineWidth += this.characterInfos[c].Width;
						lineHeight = Math.Max(lineHeight, this.characterInfos[c].Height);
					}
					else if (c == ' ')
						lineWidth += SpaceUnitWidth;
					else if (c != '\r')
						throw new ArgumentException("Unsupported whitespace character, character code: " + (int)c);
				}

				textWidth = Math.Max(textWidth, lineWidth);
				textHeight = lineY + lineHeight;
				lineY += lineScale;
			}

			return new Vector2(textWidth + 2 * AntialiasWidth, textHeight + 2 * AntialiasWidth);
		}

		private void prepare(string text)
		{
			this.Initialize();

			var missinCharacters = new HashSet<char>();
			foreach (char c in text)
				if (!this.characterInfos.ContainsKey(c) && !char.IsWhiteSpace(c))
					missinCharacters.Add(c);

			if (missinCharacters.Count == 0)
				return;

			foreach (char c in missinCharacters)
			{
				var path = new GraphicsPath();
				path.AddString(c.ToString(), font.FontFamily, (int)font.Style, font.Size, new Point(0, 0), StringFormat.GenericTypographic);
				path.Flatten();

				var contures = getContures(path).ToList();
				var minX = (int)Math.Floor(path.PathPoints.Min(p => p.X));
				var maxX = (int)Math.Ceiling(path.PathPoints.Max(p => p.X));
				var minY = (int)Math.Floor(path.PathPoints.Min(p => p.Y));
				var maxY = (int)Math.Ceiling(path.PathPoints.Max(p => p.Y));
				path.Dispose();

				var width = maxX - minX;
				var height = maxY - minY;
				var rect = this.textureBuilder.Add(new Size(width + SdfPadding * 2, height + SdfPadding * 2));

				var distField = new double[rect.Size.Height, rect.Size.Width];

				for (int y = 0; y < rect.Size.Height; y++)
					for (int x = 0; x < rect.Size.Width; x++)
					{
						var fromP = new Vector2(x + minX - SdfPadding, y + minY - SdfPadding);
						var minDist = Math.Min(contures.Min(shape => shape.Distance(fromP)), SdfPadding);
						if (contures.Sum(shape => shape.RayHits(fromP)) % 2 != 0)
							minDist *= -1;

						distField[y, x] = -minDist / SdfPadding / 2 + 0.5;
					}

				for (int y = 0; y < rect.Size.Height; y++)
					for (int x = 0; x < rect.Size.Width; x++)
						this.textureData[rect.X + x, rect.Y + y] = Color.FromArgb((int)(distField[y, x] * 255), 255, 255, 255);

				this.characterInfos[c] = new CharTextureInfo(
					rect,
					this.textureData.Width, this.textureData.Height,
					rect.Size.Width / font.Size,
					rect.Size.Height / font.Size,
					(minX + maxX) / font.Size / 2,
					-(minY + maxY) / font.Size / 2,
					maxX / font.Size,
					maxY / font.Size
				);
			}

			TextureUtils.UpdateTexture(this.TextureId, this.textureData);
		}

		private static IEnumerable<GlyphContour> getContures(GraphicsPath path)
		{
			var pathPoints = path.PathPoints;
			var pathTypes = path.PathTypes.Select(x => x & (byte)PathPointType.PathTypeMask).ToList();
			var contoureRanges = new List<KeyValuePair<int, int>>();
			var start = 0;
			var count = 0;

			for (int i = 0; i < pathPoints.Length; i++)
			{
				if (pathTypes[i] == (byte)PathPointType.Start)
				{
					contoureRanges.Add(new KeyValuePair<int, int>(start, count));
					start = i;
					count = 0;
				}

				count++;
			}
			contoureRanges.Add(new KeyValuePair<int, int>(start, count));

			foreach (var group in contoureRanges.Where(x => x.Value > 1))
			{
				var strokes = new List<PointF[]>();
				for (int i = 1; i < group.Value; i++)
					strokes.Add(new[] { pathPoints[group.Key + i - 1], pathPoints[group.Key + i] });

				strokes.Add(new[] { pathPoints[group.Key + group.Value - 1], pathPoints[group.Key] });
				yield return new GlyphContour(strokes);
			}
		}
	}
}

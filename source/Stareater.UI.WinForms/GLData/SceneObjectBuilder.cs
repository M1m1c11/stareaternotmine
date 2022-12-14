using OpenTK;
using Stareater.GraphicsEngine;
using System.Collections.Generic;
using System;
using Stareater.GLData.SpriteShader;
using System.Drawing;
using Stareater.GLData.OrbitShader;
using Stareater.GLData.SdfShader;
using System.Linq;

namespace Stareater.GLData
{
	class SceneObjectBuilder
	{
		private readonly List<PolygonData> polygons = new List<PolygonData>();
		private readonly PhysicalData physicalShape = null;
		private readonly object data = null;

		private PolygonType currentPolygonType = PolygonType.None;
		private Dictionary<float, List<float>> vertexData = new Dictionary<float, List<float>>();

		#region Sprite and common SDF data
		private int textureId;
		private Color color;
		private Matrix4 localTransform = Matrix4.Identity;
		private ClipWindow clipArea = null;
		#endregion

		#region SDF specific data
		private float? pixelSize = null;
		private float? smoothDist = null;
		#endregion

		#region Orbit data
		private float minRadius;
		private float maxRadius;
		private TextureInfo sprite;
		#endregion

		public SceneObjectBuilder()
		{ }

		public SceneObjectBuilder(object data, Vector2 center, float size)
		{
			this.data = data;
			this.physicalShape = new PhysicalData(center, new Vector2(size));
		}

		public SceneObject Build()
		{
			this.applyPolygonData();
			return new SceneObject(this.polygons, this.physicalShape, this.data);
		}

		public SceneObject Build(Func<IList<PolygonData>, IAnimator> animatorGenerator)
		{
			this.applyPolygonData();
			return new SceneObject(
				this.polygons, 
				this.physicalShape,
				this.data,
				animatorGenerator(this.polygons));
		}

		#region Builder methods
		public SceneObjectBuilder StartSprite(float z, int textureId, Color color)
		{
			this.applyPolygonData();
			
			this.currentPolygonType = PolygonType.Sprite;
			this.vertexData[z] = new List<float>();
			this.textureId = textureId;
			this.color = color;

			return this;
		}

		public SceneObjectBuilder StartSimpleSprite(float z, TextureInfo sprite, Color color)
		{
			this.applyPolygonData();

			this.currentPolygonType = PolygonType.Sprite;
			this.vertexData[z] = new List<float>(SpriteHelpers.UnitRect(sprite));
			this.textureId = sprite.Id;
			this.color = color;

			return this;
		}

		public SceneObjectBuilder StartOrbit(float z, float minRadius, float maxRadius, TextureInfo sprite, Color color)
		{
			this.applyPolygonData();

			this.currentPolygonType = PolygonType.Orbit;
			this.vertexData[z] = new List<float>();
			this.minRadius = minRadius;
			this.maxRadius = maxRadius;
			this.sprite = sprite;
			this.color = color;

			return this;
		}

		public SceneObjectBuilder StartText(string text, float adjustment, float alignment, float z0, float zRange, Color color)
		{
			this.applyPolygonData();

			this.currentPolygonType = PolygonType.Sdf;
			this.smoothDist = this.pixelSize;
			foreach (var layer in TextRenderUtil.Get.BufferText(text, adjustment, alignment, z0, zRange))
				this.vertexData[layer.Key] = new List<float>(layer.Value);

			this.textureId = TextRenderUtil.Get.TextureId;
			this.color = color;

			return this;
		}

		public SceneObjectBuilder PixelSize(float size)
		{
			this.pixelSize = size * TextRenderUtil.AntialiasPixelSize;

			return this;
		}

		public SceneObjectBuilder Clip(ClipWindow clipArea)
		{
			if (!clipArea.IsEmpty)
				this.clipArea = clipArea;

			return this;
		}

		public SceneObjectBuilder AddVertices(IEnumerable<float> vertexData)
		{
			if (this.vertexData.Count != 1)
				throw new InvalidOperationException("Operation requires exactly one working layer");
			this.assertStarted();

			this.vertexData.First().Value.AddRange(vertexData);

			return this;
		}

		public SceneObjectBuilder AddVertices(IEnumerable<Vector2> vertices)
		{
			return this.AddVertices(vertices.SelectMany(v => new[] { v.X, v.Y }));
		}

		public SceneObjectBuilder Scale(float scale)
		{
			return this.Scale(scale, scale);
		}

		public SceneObjectBuilder Scale(Vector2 scale)
		{
			return this.Scale(scale.X, scale.Y);
		}

		public SceneObjectBuilder Scale(float scaleX, float scaleY)
		{
			this.assertStarted();

			this.localTransform *= Matrix4.CreateScale(scaleX, scaleY, 1);

			if (this.smoothDist.HasValue)
				this.smoothDist /= scaleY;

			return this;
		}

		public SceneObjectBuilder Translate(double x, double y)
		{
			this.assertStarted();

			this.localTransform *= Matrix4.CreateTranslation((float)x, (float)y, 0);

			return this;
		}

		public SceneObjectBuilder Translate(Vector2 offset)
		{
			this.assertStarted();

			this.localTransform *= Matrix4.CreateTranslation(offset.X, offset.Y, 0);

			return this;
		}

		public SceneObjectBuilder Transform(Matrix4 transform)
		{
			this.assertStarted();

			this.localTransform *= transform;

			return this;
		}
		#endregion

		private void applyPolygonData()
		{
			if (this.currentPolygonType == PolygonType.None)
				return;

			IShaderData shaderData;

			switch (this.currentPolygonType)
			{
				case PolygonType.Sprite:
					shaderData = new SpriteData(this.localTransform, this.textureId, this.color, this.clipArea);
					break;
				case PolygonType.Orbit:
					shaderData = new OrbitData(this.minRadius, this.maxRadius, this.color, this.localTransform, this.sprite);
					break;
				case PolygonType.Sdf:
					if (!this.smoothDist.HasValue)
						throw new InvalidOperationException("Pixel size not defined");
					shaderData = new SdfData(this.localTransform, this.textureId, this.color, this.smoothDist.Value, this.clipArea);
					break;
				default:
					throw new NotImplementedException(this.currentPolygonType.ToString());
			}

			foreach(var layer in this.vertexData)
				this.polygons.Add(new PolygonData(layer.Key, shaderData, layer.Value));

			//clean up
			this.vertexData = new Dictionary<float, List<float>>();
			this.localTransform = Matrix4.Identity;
			this.smoothDist = null;
		}

		private void assertStarted()
		{
			if (this.currentPolygonType == PolygonType.None)
				throw new InvalidOperationException("Polygon data not started");
		}

		private enum PolygonType
		{
			None,
			Orbit,
			Sprite,
			Sdf
		}
	}
}

﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Stareater.Controllers;
using Stareater.Galaxy;

namespace Stareater.GLRenderers
{
	public class SystemRenderer : IRenderer
	{
		private const double DefaultViewSize = 1;
		
		private const float FarZ = -1;
		private const float StarColorZ = -0.8f;
		private const float PlanetZ = -0.8f;
		private const float OrbitZ = -0.9f;
		
		private const float PanClickTolerance = 0.01f;
		
		private const float BodiesY = 0.2f;
		private const float OrbitStep = 0.3f;
		private const float OrbitOffset = 0.5f;
		private const float OrbitWidth = 0.005f;
		
		private const float StarScale = 0.5f;
		private const float PlanetScale = 0.15f;
		
		private StarSystemController controller;
		private Control eventDispatcher;
		private Action systemClosedHandler;
		
		private bool resetProjection = true;
		private Matrix4 invProjection;
		
		private Vector4? lastMousePosition = null;
		private float panAbsPath = 0;
		private float originOffset;
		private float minOffset = -StarScale / 2;
		private float maxOffset;
		
		public SystemRenderer(Action systemClosedHandler)
		{
			this.systemClosedHandler = systemClosedHandler;
		}
		
		public void Draw(double deltaTime)
		{
			if (resetProjection) {
				double aspect = eventDispatcher.Width / (double)eventDispatcher.Height;
				double semiRadius = 0.5 * DefaultViewSize;

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(
					-aspect * semiRadius + originOffset, aspect * semiRadius + originOffset,
					-semiRadius, semiRadius, 
					0, -FarZ);

				GL.GetFloat(GetPName.ProjectionMatrix, out invProjection);
				invProjection.Invert();
				GL.MatrixMode(MatrixMode.Modelview);
				resetProjection = false;
			}
			
			
			GL.PushMatrix();
			GL.Translate(0, BodiesY, 0);
			
			GL.Color4(controller.Star.Color);
			GL.PushMatrix();
			GL.Scale(StarScale, StarScale, StarScale);

			TextureUtils.Get.DrawSprite(GalaxyTextures.Get.SystemStar, StarColorZ);
		
			GL.PopMatrix();
			
			foreach(Planet planet in controller.Planets)
			{ 
				float orbitR = planet.Position * OrbitStep + OrbitOffset;
				float orbitMin = orbitR - OrbitWidth;
				float orbitMax = orbitR + OrbitWidth;
				
				GL.Disable(EnableCap.Texture2D);
				GL.Color4(Color.Gray);
				GL.Begin(BeginMode.Quads);
				for(int i = 0; i < 100; i++)
				{
					float angle0 = (float)Math.PI * i / 50f;
					float angle1 = (float)Math.PI * (i +1) / 50f;
					
					GL.Vertex3(orbitMin * (float)Math.Cos(angle0), orbitMin * (float)Math.Sin(angle0), OrbitZ);
					GL.Vertex3(orbitMax * (float)Math.Cos(angle0), orbitMax * (float)Math.Sin(angle0), OrbitZ);
					GL.Vertex3(orbitMax * (float)Math.Cos(angle1), orbitMax * (float)Math.Sin(angle1), OrbitZ);
					GL.Vertex3(orbitMin * (float)Math.Cos(angle1), orbitMin * (float)Math.Sin(angle1), OrbitZ);
				}
				GL.End();
				
				GL.Color4(Color.Blue);
				GL.Enable(EnableCap.Texture2D);
				GL.PushMatrix();
				GL.Translate(orbitR, 0, 0);
				GL.Scale(PlanetScale, PlanetScale, PlanetScale);
	
				TextureUtils.Get.DrawSprite(GalaxyTextures.Get.Planet, StarColorZ);
			
				GL.PopMatrix();
			}
			
			GL.PopMatrix();
		}
		
		public void Load()
		{
			//no op
		}
		
		public void Unload()
		{
			//no op
		}
		
		public void AttachToCanvas(Control eventDispatcher)
		{
			this.eventDispatcher = eventDispatcher;
			
			eventDispatcher.MouseMove += mousePan;
			eventDispatcher.MouseClick += mouseClick;
		}
		
		public void DetachFromCanvas()
		{
			eventDispatcher.MouseMove -= mousePan;
			eventDispatcher.MouseClick -= mouseClick;
			
			this.eventDispatcher = null;
		}
		
		public void SetStarSystem(StarSystemController controller)
		{
			this.controller = controller;
			
			this.resetProjection = true;
			this.originOffset = 0.5f; //TODO: Get most populated planet
			this.maxOffset = controller.Planets.Count() * OrbitStep + OrbitOffset + PlanetScale / 2;
		}
		
		public void ResetProjection()
		{
			resetProjection = true;
		}
		
		private Vector4 mouseToView(int x, int y)
		{
			return new Vector4(
				2 * x / (float)eventDispatcher.Width - 1,
				1 - 2 * y / (float)eventDispatcher.Height, 
				0, 1
			);
		}
		
		private void limitPan()
		{
			if (originOffset < minOffset) 
				originOffset = minOffset;
			if (originOffset > maxOffset) 
				originOffset = maxOffset;
		}
		
		private void mouseClick(object sender, MouseEventArgs e)
		{
			if (panAbsPath > PanClickTolerance)
				return;
			
			this.systemClosedHandler();
		}
		
		private void mousePan(object sender, MouseEventArgs e)
		{
			Vector4 currentPosition = mouseToView(e.X, e.Y);

			if (!lastMousePosition.HasValue)
				lastMousePosition = currentPosition;

			if (!e.Button.HasFlag(MouseButtons.Left)) {
				lastMousePosition = currentPosition;
				panAbsPath = 0;
				return;
			}
			
			panAbsPath += (currentPosition - lastMousePosition.Value).Length;

			originOffset -= (Vector4.Transform(currentPosition, invProjection) -
				Vector4.Transform(lastMousePosition.Value, invProjection)
				).X;

			limitPan();
			
			lastMousePosition = currentPosition;
			resetProjection = true;
			eventDispatcher.Refresh();
		}
		
		public void Dispose()
		{
			if (eventDispatcher != null) {
				DetachFromCanvas();
				Unload();
			}
		}
	}
}

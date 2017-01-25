﻿using OpenTK;

namespace Stareater.GraphicsEngine
{
	interface IDrawable
	{
		void Draw(Matrix4 view);
	}
}
using System;

namespace Stareater.Galaxy.Builders
{
	public interface IStarPositioner : IMapBuilderPiece
	{
		string Name { get; }
		string Description { get; }
		StarPositions Generate(Random rng, int playerCount);
	}
}

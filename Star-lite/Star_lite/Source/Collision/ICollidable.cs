using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Starlite.Map;

namespace Starlite.Entities
{
	public interface ICollidable
	{
		Vector2 GetWorldPosition();

		// Used by the Scene SOLELY to move an entity outside of an impassible tile if needed.
		void Depentrate(Vector2 displacement);

		Rectangle GetBoundingBox(Vector2 position = new Vector2());

		Rectangle[] GetHitboxes();
	}
}

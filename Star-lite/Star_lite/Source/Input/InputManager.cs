using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Starlite.Input
{
	public static class InputManager
	{
		private static KeyboardState previousKeyboardState;
		private static MouseState previousMouseState;

		/// <summary>
		/// THIS SHOULD BE THE LAST CALL IN THE GAME'S UPDATE FUNCTION!
		/// </summary>
		public static void SwapStates()
		{
			previousKeyboardState = Keyboard.GetState();
			previousMouseState = Mouse.GetState();
		}

		public static float GetHorizontalAxis()
		{
			var result = 0.0f;
			var state = Keyboard.GetState();

			if (state.IsKeyDown(Keys.A))
				result = -1.0f;
			else if (state.IsKeyDown(Keys.D))
				result = 1.0f;

			return result;
		}

		public static float GetVerticalAxis()
		{
			var result = 0.0f;
			var state = Keyboard.GetState();

			if (state.IsKeyDown(Keys.W))
				result = 1.0f;
			else if (state.IsKeyDown(Keys.S))
				result = -1.0f;

			return result;
		}

		public static bool IsKeyPressed(Keys key)
		{
			var state = Keyboard.GetState();
			return state.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
		}

		public static bool IsKeyPressedContinuously(Keys key)
		{
			var state = Keyboard.GetState();
			return state.IsKeyDown(key);
		}

		public static bool IsKeyReleased(Keys key)
		{
			var state = Keyboard.GetState();
			return state.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key);
		}

		public static Vector2 GetMousePosition()
		{
			var state = Mouse.GetState();
			return new Vector2(state.X, state.Y);
		}

		public static bool LMBPressed()
		{
			var state = Mouse.GetState();
			return state.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
		}

		public static bool LMBHeldDown()
		{
			var state = Mouse.GetState();
			return state.LeftButton == ButtonState.Pressed;
		}

		public static bool LMBReleased()
		{
			var state = Mouse.GetState();
			return state.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
		}

		public static bool RMBPressed()
		{
			var state = Mouse.GetState();
			return state.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
		}

		public static bool RMBReleased()
		{
			var state = Mouse.GetState();
			return state.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed;
		}
	}

}

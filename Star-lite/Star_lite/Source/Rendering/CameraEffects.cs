using Microsoft.Xna.Framework;
using Starlite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Starlite.Rendering
{
    public class CameraEffects
    {
        private class CameraShake
        {
            public float Strength;
            public float Duration;
            public float PassedTime;
        }

        private Random random = new Random();
        private Scene scene;

        private List<CameraShake> cameraShakes = new List<CameraShake>();

        public CameraEffects(Scene scene)
        {
            this.scene = scene;
        }

        // Must be called in order to function
        public void Update(float deltaTime)
        {
            var shakesToDelete = new List<CameraShake>();

            for (int i = 0; i < cameraShakes.Count; i++)
            {
                CameraShake cameraShake = this.cameraShakes[i];
                if (cameraShake.PassedTime <= cameraShake.Duration)
                {
                    var x = ((float)random.NextDouble() - 0.5f) * cameraShake.Strength;
                    var y = ((float)random.NextDouble() - 0.5f) * cameraShake.Strength;

                    this.scene.effectsOffset = new Vector2(x, y);

                    cameraShake.PassedTime += deltaTime;
                }
                else
                {
                    shakesToDelete.Add(cameraShake);
                }
            }

            foreach (var shake in shakesToDelete)
                this.cameraShakes.Remove(shake);

            if (this.cameraShakes.Count == 0)
                scene.effectsOffset = Vector2.Zero;
        }

        public void PerformCameraShake(float strength, float duration)
        {
            this.cameraShakes.Add(new CameraShake
            {
                Strength = strength,
                Duration = duration
            });
        }
    }
}

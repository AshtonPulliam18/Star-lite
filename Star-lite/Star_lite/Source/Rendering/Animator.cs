using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Starlite.Rendering
{
    public class Animator
    {

        public Dictionary<String, Rectangle[]> Animations;
        public Dictionary<String, int> AnimationCooldowns;

        private String currentAni;
        private int currentCool, currentFrame;
        public Animator(String[] actions, Rectangle[][] frames, int[] cooldowns)
        {
            Animations = new Dictionary<String, Rectangle[]>();
            AnimationCooldowns = new Dictionary<String, int>();
            for (int i = 0; i < actions.Length; i++) {
                Animations.Add(actions[i], frames[i]);
                AnimationCooldowns.Add(actions[i], cooldowns[i]);
            }
        }

        public Rectangle UpdateFrame(String animation)
        {
            if (animation.Equals(currentAni))
            {
                if (currentCool == 0)
                {
                    currentCool = AnimationCooldowns[currentAni];
                    currentFrame++;
                    if (currentFrame > Animations[currentAni].Length - 1)
                        currentFrame = 0;
                }
                else
                {
                    currentCool--;
                }
            }
            else
            {
                currentAni = animation;
                currentFrame = 0;
                currentCool = AnimationCooldowns[animation];
            }

            return Animations[currentAni][currentFrame];
        }

        public Rectangle UpdateFrameSingle(String animation)
        {
            if (animation.Equals(currentAni))
            {
                if (currentCool == 0)
                {
                    currentCool = AnimationCooldowns[currentAni];
                    currentFrame++;
                    if (currentFrame > Animations[currentAni].Length - 1)
                    {
                        currentFrame = Animations[currentAni].Length - 1;
                        return Rectangle.Empty;
                    }
                }
                else
                {
                    currentCool--;
                }
            }
            else
            {
                currentAni = animation;
                currentFrame = 0;
                currentCool = AnimationCooldowns[animation];
            }

            return Animations[currentAni][currentFrame];
        }
    }
}

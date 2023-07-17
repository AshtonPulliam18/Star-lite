using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Starlite.Rendering
{
    public class SfxManager
    {
        public Dictionary<String, SoundEffect> Sounds;
        public Dictionary<String, float> SoundVolumes;


        private SoundEffectInstance currentEffect;
        private String currentSound;
        private int currentTimer;
        private bool fadingOut, fadingIn;
        public SfxManager(String[] sounds, SoundEffect[] effects, float[] volumes)
        {
            Sounds = new Dictionary<string, SoundEffect>();
            SoundVolumes = new Dictionary<string, float>();

            for (int i = 0; i < sounds.Length; i++)
            {
                Sounds.Add(sounds[i], effects[i]);
                SoundVolumes.Add(sounds[i], volumes[i]);
                currentTimer = -1;
            }
        }

        public bool UpdateSfx(String sound, bool fadeIn, bool fadeOut)
        {
            if (sound == "none" || !sound.Equals(currentSound))
            {
                if (fadingOut)
                    FadeOut();
                else
                {
                    currentEffect?.Stop();
                    currentTimer = -1;
                    fadingIn = fadeIn;
                    fadingOut = fadeOut;
                    currentSound = sound;
                }
            }
            else
            {
                if (currentTimer >= GetDuration(sound) || currentTimer == -1)
                {
                    currentTimer = 0;
                    currentEffect = Sounds[sound].CreateInstance();
                    currentEffect.Volume = !fadingIn ? SoundVolumes[sound] : 0;
                    currentEffect.Play();
                }
                else
                    currentTimer++;
                if (fadingIn)
                    FadeIn(sound);
                else if (currentTimer >= GetDuration(sound) * .75 && fadingOut)
                    FadeOut();
            }
            return currentTimer >= GetDuration(sound);
        }

        public void UpdateSfxSingle(String sound, float pitch)
        {
            currentEffect = Sounds[sound].CreateInstance();
            currentEffect.Volume = SoundVolumes[sound];
            currentEffect.Pitch = pitch;
            currentEffect.Play();
        }

        private void FadeIn(String sound)
        {
            if (currentEffect?.Volume < SoundVolumes[sound])
                currentEffect.Volume += .001f;
            else
                fadingIn = false;
        }

        private void FadeOut()
        {
            if (currentEffect?.Volume > .005)
                currentEffect.Volume -= .001f;
            else
                fadingOut = false;
        }
        private int GetDuration(String sound)
        {
            if (sound.Equals("none"))
                return 0;
            return (int)Sounds[sound].Duration.TotalSeconds * 60;
        }




    }
}

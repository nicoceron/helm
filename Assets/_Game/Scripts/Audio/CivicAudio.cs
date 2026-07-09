using System;
using UnityEngine;

namespace Lionrise
{
    /// <summary>Small original procedural audio palette for the asset-free prototype.</summary>
    public sealed class CivicAudio : MonoBehaviour
    {
        private AudioSource sfx;
        private AudioSource music;
        private AudioClip swipeLeft;
        private AudioClip swipeRight;
        private AudioClip success;
        private AudioClip failure;

        private void Awake()
        {
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.volume = .32f;
            music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.loop = true;
            music.volume = .085f;
            swipeLeft = Tone("Swipe Left", 180, 95, .19f, .32f);
            swipeRight = Tone("Swipe Right", 150, 290, .19f, .32f);
            success = Tone("Audit Recognition", 220, 560, .7f, .24f);
            failure = Tone("Timeline Closed", 190, 62, .75f, .22f);
            music.clip = Ambient();
            music.Play();
        }

        public void Commit(ChoiceSide side, bool haptics)
        {
            sfx.PlayOneShot(side == ChoiceSide.Left ? swipeLeft : swipeRight);
#if UNITY_IOS || UNITY_ANDROID
            if (haptics) Handheld.Vibrate();
#endif
        }

        public void Ending(bool victory) => sfx.PlayOneShot(victory ? success : failure);

        private static AudioClip Tone(string name, float startHz, float endHz, float seconds, float amplitude)
        {
            const int rate = 22050;
            var count = Mathf.CeilToInt(rate * seconds);
            var samples = new float[count];
            var phase = 0f;
            for (var i = 0; i < count; i++)
            {
                var t = i / (float)count;
                var frequency = Mathf.Lerp(startHz, endHz, t * t);
                phase += Mathf.PI * 2f * frequency / rate;
                var envelope = Mathf.Sin(Mathf.PI * t) * Mathf.Exp(-1.8f * t);
                samples[i] = Mathf.Sin(phase) * envelope * amplitude;
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip Ambient()
        {
            const int rate = 22050;
            const int seconds = 12;
            var samples = new float[rate * seconds];
            var random = new System.Random(1965);
            for (var i = 0; i < samples.Length; i++)
            {
                var time = i / (float)rate;
                var pulse = Mathf.Sin(time * Mathf.PI * 2f * 55f) * .18f + Mathf.Sin(time * Mathf.PI * 2f * 82.5f) * .08f;
                var breath = .55f + .45f * Mathf.Sin(time * Mathf.PI * 2f / seconds);
                var noise = ((float)random.NextDouble() * 2f - 1f) * .012f;
                samples[i] = pulse * breath + noise;
            }
            var clip = AudioClip.Create("Vacuum State Procedural Loop", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

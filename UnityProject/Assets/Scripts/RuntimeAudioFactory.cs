using UnityEngine;

public static class RuntimeAudioFactory
{
    private static GameObject audioRoot;
    private static AudioSource ambienceSource;

    private static AudioClip engineLoopClip;
    private static AudioClip playerShotClip;
    private static AudioClip enemyShotClip;
    private static AudioClip impactClip;
    private static AudioClip explosionClip;
    private static AudioClip ambienceClip;

    public static void EnsureBattleAmbience()
    {
        EnsureRoot();
        if (ambienceSource == null)
        {
            ambienceSource = audioRoot.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = true;
            ambienceSource.loop = true;
            ambienceSource.spatialBlend = 0f;
            ambienceSource.volume = 0.16f;
            ambienceSource.clip = GetAmbienceClip();
        }

        if (!ambienceSource.isPlaying)
        {
            ambienceSource.Play();
        }
    }

    public static AudioClip GetEngineLoopClip()
    {
        if (engineLoopClip == null)
        {
            engineLoopClip = BuildClip("EngineLoop", 2.2f, 22050, SampleEngineLoop);
        }

        return engineLoopClip;
    }

    public static AudioClip GetPlayerShotClip()
    {
        if (playerShotClip == null)
        {
            playerShotClip = BuildClip("PlayerShot", 0.33f, 22050, SamplePlayerShot);
        }

        return playerShotClip;
    }

    public static AudioClip GetEnemyShotClip()
    {
        if (enemyShotClip == null)
        {
            enemyShotClip = BuildClip("EnemyShot", 0.28f, 22050, SampleEnemyShot);
        }

        return enemyShotClip;
    }

    public static AudioClip GetImpactClip()
    {
        if (impactClip == null)
        {
            impactClip = BuildClip("Impact", 0.24f, 22050, SampleImpact);
        }

        return impactClip;
    }

    public static AudioClip GetExplosionClip()
    {
        if (explosionClip == null)
        {
            explosionClip = BuildClip("Explosion", 0.86f, 22050, SampleExplosion);
        }

        return explosionClip;
    }

    public static void PlayOneShotAt(
        Vector3 position,
        AudioClip clip,
        float volume,
        float spatialBlend = 1f,
        float minPitch = 1f,
        float maxPitch = 1f)
    {
        if (clip == null)
        {
            return;
        }

        EnsureRoot();
        var go = new GameObject("SFX_OneShot");
        go.transform.SetParent(audioRoot.transform, false);
        go.transform.position = position;

        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.spatialBlend = Mathf.Clamp01(spatialBlend);
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 3f;
        source.maxDistance = 46f;
        source.pitch = Random.Range(minPitch, maxPitch);
        source.Play();

        float cleanupDelay = clip.length / Mathf.Max(0.1f, source.pitch) + 0.15f;
        Object.Destroy(go, cleanupDelay);
    }

    private static void EnsureRoot()
    {
        if (audioRoot != null)
        {
            return;
        }

        audioRoot = GameObject.Find("RuntimeAudioRoot");
        if (audioRoot == null)
        {
            audioRoot = new GameObject("RuntimeAudioRoot");
        }
    }

    private static AudioClip GetAmbienceClip()
    {
        if (ambienceClip == null)
        {
            ambienceClip = BuildClip("BattleAmbience", 4.8f, 22050, SampleAmbience);
        }

        return ambienceClip;
    }

    private static AudioClip BuildClip(string name, float lengthSeconds, int sampleRate, System.Func<float, float> sampler)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(lengthSeconds * sampleRate));
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            data[i] = Mathf.Clamp(sampler(t), -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static float SampleEngineLoop(float t)
    {
        float wobble = Mathf.Sin(t * 2f * Mathf.PI * 0.72f) * 8f;
        float baseFreq = 52f + wobble;
        float harmonics =
            Mathf.Sin(t * 2f * Mathf.PI * baseFreq) * 0.33f +
            Mathf.Sin(t * 2f * Mathf.PI * baseFreq * 2f) * 0.11f +
            Mathf.Sin(t * 2f * Mathf.PI * 18f) * 0.08f;
        float grit = (Mathf.PerlinNoise(t * 31f, 0.37f) - 0.5f) * 0.18f;
        return (harmonics + grit) * 0.75f;
    }

    private static float SamplePlayerShot(float t)
    {
        float envelope = Mathf.Exp(-13f * t);
        float lowBoom = Mathf.Sin(t * 2f * Mathf.PI * 92f) * 0.72f;
        float highSnap = Mathf.Sin(t * 2f * Mathf.PI * 480f) * Mathf.Exp(-30f * t) * 0.3f;
        float noise = (Mathf.PerlinNoise(t * 120f, 0.5f) - 0.5f) * 0.75f * Mathf.Exp(-16f * t);
        return (lowBoom + highSnap + noise) * envelope;
    }

    private static float SampleEnemyShot(float t)
    {
        float envelope = Mathf.Exp(-15f * t);
        float body = Mathf.Sin(t * 2f * Mathf.PI * 118f) * 0.58f;
        float snap = Mathf.Sin(t * 2f * Mathf.PI * 360f) * Mathf.Exp(-26f * t) * 0.23f;
        float noise = (Mathf.PerlinNoise(t * 96f, 0.8f) - 0.5f) * 0.58f * Mathf.Exp(-19f * t);
        return (body + snap + noise) * envelope;
    }

    private static float SampleImpact(float t)
    {
        float envelope = Mathf.Exp(-20f * t);
        float thud = Mathf.Sin(t * 2f * Mathf.PI * 138f) * 0.55f;
        float crack = (Mathf.PerlinNoise(t * 160f, 0.2f) - 0.5f) * 0.92f;
        return (thud + crack) * envelope;
    }

    private static float SampleExplosion(float t)
    {
        float attack = Mathf.Clamp01(t * 22f);
        float decay = Mathf.Exp(-4.5f * t);
        float envelope = attack * decay;

        float rumble = Mathf.Sin(t * 2f * Mathf.PI * 46f) * 0.44f;
        float roar = (Mathf.PerlinNoise(t * 38f, 0.76f) - 0.5f) * 1.35f;
        float tail = Mathf.Sin(t * 2f * Mathf.PI * 14f) * 0.2f;
        return (rumble + roar + tail) * envelope;
    }

    private static float SampleAmbience(float t)
    {
        float wind = Mathf.Sin(t * 2f * Mathf.PI * 0.24f) * 0.21f + Mathf.Sin(t * 2f * Mathf.PI * 0.11f) * 0.14f;
        float noise = (Mathf.PerlinNoise(t * 6.5f, 0.93f) - 0.5f) * 0.38f;
        return (wind + noise) * 0.45f;
    }
}

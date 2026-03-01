using System.Collections.Generic;
using UnityEngine;

public static class RuntimeRoadFactory
{
    private const float FieldHalfSize = 28f;
    private static readonly List<SurfaceZone> surfaceZones = new List<SurfaceZone>(64);

    public struct SurfaceProfile
    {
        public float Traction;
        public float SpeedFactor;
        public float TurnFactor;

        public SurfaceProfile(float traction, float speedFactor, float turnFactor)
        {
            Traction = traction;
            SpeedFactor = speedFactor;
            TurnFactor = turnFactor;
        }
    }

    private struct SurfaceZone
    {
        public Vector2 Center;
        public Vector2 HalfSize;
        public float Traction;
        public float SpeedFactor;
        public float TurnFactor;
    }

    public static void BuildIfMissing()
    {
        GameObject existing = GameObject.Find("RuntimeBattlefield");
        if (existing != null)
        {
            EnsureAtmosphere();
            EnsureSurfaceZonesFromExisting(existing.transform);
            return;
        }

        surfaceZones.Clear();
        var root = new GameObject("RuntimeBattlefield");

        Material grassMat = RuntimeMaterialFactory.Create(new Color(0.26f, 0.42f, 0.24f, 1f));
        Material dirtMat = RuntimeMaterialFactory.Create(new Color(0.45f, 0.39f, 0.28f, 1f));
        Material rockMat = RuntimeMaterialFactory.Create(new Color(0.38f, 0.39f, 0.37f, 1f));
        Material trunkMat = RuntimeMaterialFactory.Create(new Color(0.36f, 0.23f, 0.15f, 1f));
        Material leafMat = RuntimeMaterialFactory.Create(new Color(0.2f, 0.36f, 0.18f, 1f));
        Material boundaryMat = RuntimeMaterialFactory.Create(new Color(0.5f, 0.44f, 0.34f, 1f));

        CreateFieldGround(root.transform, grassMat, dirtMat);
        CreateFieldBoundary(root.transform, boundaryMat);
        CreateFieldCover(root.transform, rockMat, trunkMat, leafMat, dirtMat);
        CreateFieldMounds(root.transform, dirtMat);
        EnsureDirectionalLight(root.transform);
        EnsureAtmosphere();
    }

    private static void CreateFieldGround(Transform parent, Material grassMat, Material dirtMat)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(parent, false);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(5.8f, 1f, 5.8f);
        ground.GetComponent<Renderer>().material = grassMat;

        Collider groundCollider = ground.GetComponent<Collider>();
        if (groundCollider != null)
        {
            Object.Destroy(groundCollider);
        }

        for (int i = 0; i < 22; i++)
        {
            Vector3 p = new Vector3(
                Random.Range(-FieldHalfSize + 2f, FieldHalfSize - 2f),
                0.015f,
                Random.Range(-FieldHalfSize + 2f, FieldHalfSize - 2f));
            Vector3 s = new Vector3(Random.Range(2.1f, 4.5f), 0.03f, Random.Range(1.8f, 3.9f));
            CreateBox(parent, "DirtPatch", p, s, dirtMat);
            AddSurfaceZone(p, s, 0.72f, 0.84f, 0.82f);
        }
    }

    private static void CreateFieldBoundary(Transform parent, Material boundaryMat)
    {
        float wallY = 0.42f;
        float wallHeight = 0.85f;
        float wallThickness = 0.5f;
        float wallLength = FieldHalfSize * 2f + wallThickness;

        CreateBox(parent, "BoundaryNorth", new Vector3(0f, wallY, FieldHalfSize + wallThickness * 0.5f), new Vector3(wallLength, wallHeight, wallThickness), boundaryMat);
        CreateBox(parent, "BoundarySouth", new Vector3(0f, wallY, -FieldHalfSize - wallThickness * 0.5f), new Vector3(wallLength, wallHeight, wallThickness), boundaryMat);
        CreateBox(parent, "BoundaryWest", new Vector3(-FieldHalfSize - wallThickness * 0.5f, wallY, 0f), new Vector3(wallThickness, wallHeight, wallLength), boundaryMat);
        CreateBox(parent, "BoundaryEast", new Vector3(FieldHalfSize + wallThickness * 0.5f, wallY, 0f), new Vector3(wallThickness, wallHeight, wallLength), boundaryMat);
    }

    private static void CreateFieldCover(Transform parent, Material rockMat, Material trunkMat, Material leafMat, Material dirtMat)
    {
        for (int i = 0; i < 14; i++)
        {
            float x = Random.Range(-FieldHalfSize + 5f, FieldHalfSize - 5f);
            float z = Random.Range(-FieldHalfSize + 5f, FieldHalfSize - 5f);

            if ((new Vector3(x, 0f, z)).sqrMagnitude < 70f)
            {
                continue;
            }

            if (Random.value > 0.4f)
            {
                CreateRockCluster(parent, new Vector3(x, 0f, z), rockMat, dirtMat);
            }
            else
            {
                CreateTree(parent, new Vector3(x, 0f, z), trunkMat, leafMat);
            }
        }
    }

    private static void CreateFieldMounds(Transform parent, Material dirtMat)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 p = new Vector3(
                Random.Range(-FieldHalfSize + 5f, FieldHalfSize - 5f),
                0.2f,
                Random.Range(-FieldHalfSize + 5f, FieldHalfSize - 5f));

            if (p.sqrMagnitude < 60f)
            {
                continue;
            }

            var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mound.name = "GroundMound";
            mound.transform.SetParent(parent, false);
            mound.transform.localPosition = p;
            mound.transform.localScale = new Vector3(Random.Range(3f, 6f), Random.Range(0.7f, 1.4f), Random.Range(2.4f, 5.5f));
            mound.GetComponent<Renderer>().material = dirtMat;

            AddSurfaceZone(mound.transform.localPosition, mound.transform.localScale, 0.55f, 0.7f, 0.64f);

            Collider col = mound.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }
        }
    }

    private static void CreateRockCluster(Transform parent, Vector3 center, Material rockMat, Material dirtMat)
    {
        float baseSize = Random.Range(0.8f, 1.6f);
        CreateBox(parent, "RockBase", center + new Vector3(0f, 0.07f, 0f), new Vector3(baseSize * 1.5f, 0.14f, baseSize * 1.4f), dirtMat);

        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.7f, 0.7f), 0.25f + i * 0.08f, Random.Range(-0.7f, 0.7f));
            Vector3 scale = new Vector3(Random.Range(0.55f, 1.2f), Random.Range(0.35f, 0.75f), Random.Range(0.55f, 1.2f));
            CreateBox(parent, "Rock", center + offset, scale, rockMat);
        }
    }

    private static void CreateTree(Transform parent, Vector3 center, Material trunkMat, Material leafMat)
    {
        CreateBox(parent, "TreeTrunk", center + new Vector3(0f, 0.7f, 0f), new Vector3(0.35f, 1.4f, 0.35f), trunkMat);
        CreateBox(parent, "TreeLeaves", center + new Vector3(0f, 1.7f, 0f), new Vector3(1.8f, 1.8f, 1.8f), leafMat);
    }

    private static void EnsureDirectionalLight(Transform parent)
    {
        if (GameObject.Find("Directional Light") != null)
        {
            return;
        }

        var lightGo = new GameObject("Directional Light");
        lightGo.transform.SetParent(parent, false);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.98f, 0.94f, 1f);
        lightGo.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
    }

    private static void EnsureAtmosphere()
    {
        RenderSettings.ambientLight = new Color(0.63f, 0.67f, 0.7f, 1f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.68f, 0.77f, 0.84f, 1f);
        RenderSettings.fogDensity = 0.0085f;
    }

    private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material = material;

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            Object.Destroy(col);
        }

        return go;
    }

    public static SurfaceProfile SampleSurfaceProfile(Vector3 worldPosition)
    {
        if (surfaceZones.Count == 0)
        {
            return new SurfaceProfile(1f, 1f, 1f);
        }

        Vector2 point = new Vector2(worldPosition.x, worldPosition.z);
        SurfaceProfile profile = new SurfaceProfile(1f, 1f, 1f);

        for (int i = 0; i < surfaceZones.Count; i++)
        {
            SurfaceZone zone = surfaceZones[i];
            float wx = 1f - Mathf.Clamp01(Mathf.Abs(point.x - zone.Center.x) / Mathf.Max(0.1f, zone.HalfSize.x));
            float wz = 1f - Mathf.Clamp01(Mathf.Abs(point.y - zone.Center.y) / Mathf.Max(0.1f, zone.HalfSize.y));
            float influence = wx * wz;
            if (influence <= 0.001f)
            {
                continue;
            }

            // Smooth blend to avoid harsh transition between surfaces.
            influence = influence * influence * (3f - 2f * influence);

            float traction = Mathf.Lerp(1f, zone.Traction, influence);
            float speedFactor = Mathf.Lerp(1f, zone.SpeedFactor, influence);
            float turnFactor = Mathf.Lerp(1f, zone.TurnFactor, influence);

            profile.Traction = Mathf.Min(profile.Traction, traction);
            profile.SpeedFactor = Mathf.Min(profile.SpeedFactor, speedFactor);
            profile.TurnFactor = Mathf.Min(profile.TurnFactor, turnFactor);
        }

        float roughness = Mathf.PerlinNoise((point.x + 113f) * 0.07f, (point.y + 71f) * 0.07f);
        float roughPenalty = Mathf.Lerp(1f, 0.86f, Mathf.Clamp01((roughness - 0.6f) * 2.4f));
        profile.Traction *= roughPenalty;
        profile.TurnFactor *= Mathf.Lerp(1f, 0.9f, 1f - roughPenalty);

        profile.Traction = Mathf.Clamp(profile.Traction, 0.42f, 1f);
        profile.SpeedFactor = Mathf.Clamp(profile.SpeedFactor, 0.62f, 1f);
        profile.TurnFactor = Mathf.Clamp(profile.TurnFactor, 0.55f, 1f);
        return profile;
    }

    private static void AddSurfaceZone(Vector3 center, Vector3 size, float traction, float speedFactor, float turnFactor)
    {
        surfaceZones.Add(new SurfaceZone
        {
            Center = new Vector2(center.x, center.z),
            HalfSize = new Vector2(Mathf.Abs(size.x) * 0.5f, Mathf.Abs(size.z) * 0.5f),
            Traction = traction,
            SpeedFactor = speedFactor,
            TurnFactor = turnFactor
        });
    }

    private static void EnsureSurfaceZonesFromExisting(Transform root)
    {
        if (surfaceZones.Count > 0 || root == null)
        {
            return;
        }

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (t.name == "DirtPatch")
            {
                AddSurfaceZone(t.position, t.localScale, 0.72f, 0.84f, 0.82f);
            }
            else if (t.name == "GroundMound")
            {
                AddSurfaceZone(t.position, t.localScale, 0.55f, 0.7f, 0.64f);
            }
        }
    }
}

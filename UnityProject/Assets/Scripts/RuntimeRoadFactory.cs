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

        Material grassMat = RuntimeMaterialFactory.Create(new Color(0.26f, 0.42f, 0.24f, 1f), RuntimeMaterialFactory.MaterialPreset.Grass);
        Material dirtMat = RuntimeMaterialFactory.Create(new Color(0.45f, 0.39f, 0.28f, 1f), RuntimeMaterialFactory.MaterialPreset.Dirt);
        Material rockMat = RuntimeMaterialFactory.Create(new Color(0.38f, 0.39f, 0.37f, 1f), RuntimeMaterialFactory.MaterialPreset.Rock);
        Material trunkMat = RuntimeMaterialFactory.Create(new Color(0.36f, 0.23f, 0.15f, 1f), RuntimeMaterialFactory.MaterialPreset.Bark);
        Material leafMat = RuntimeMaterialFactory.Create(new Color(0.2f, 0.36f, 0.18f, 1f), RuntimeMaterialFactory.MaterialPreset.Leaf);
        Material boundaryMat = RuntimeMaterialFactory.Create(new Color(0.5f, 0.44f, 0.34f, 1f), RuntimeMaterialFactory.MaterialPreset.Boundary);

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
        for (int i = 0; i < 30; i++)
        {
            float x = Random.Range(-FieldHalfSize + 5f, FieldHalfSize - 5f);
            float z = Random.Range(-FieldHalfSize + 5f, FieldHalfSize - 5f);

            if ((new Vector3(x, 0f, z)).sqrMagnitude < 70f)
            {
                continue;
            }

            float roll = Random.value;
            if (roll > 0.35f)
            {
                CreateRockCluster(parent, new Vector3(x, 0f, z), rockMat, dirtMat);
            }
            else if (roll > 0.1f)
            {
                CreateTree(parent, new Vector3(x, 0f, z), trunkMat, leafMat);
            }
            else
            {
                CreateBush(parent, new Vector3(x, 0f, z), leafMat, dirtMat);
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
        float baseSize = Random.Range(1.1f, 2f);
        CreateBox(parent, "RockBase", center + new Vector3(0f, 0.06f, 0f), new Vector3(baseSize * 1.75f, 0.12f, baseSize * 1.5f), dirtMat);
        AddSurfaceZone(center + new Vector3(0f, 0.02f, 0f), new Vector3(baseSize * 1.75f, 0.1f, baseSize * 1.5f), 0.62f, 0.78f, 0.74f);

        int rockCount = Random.Range(4, 7);
        for (int i = 0; i < rockCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.95f, 0.95f), Random.Range(0.18f, 0.55f), Random.Range(-0.95f, 0.95f));
            Vector3 scale = new Vector3(Random.Range(0.45f, 1.25f), Random.Range(0.28f, 0.82f), Random.Range(0.45f, 1.25f));
            PrimitiveType type = Random.value > 0.65f ? PrimitiveType.Capsule : PrimitiveType.Cube;
            GameObject rock = CreatePart(parent, "Rock", type, center + offset, scale, rockMat);
            rock.transform.rotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
        }

        int pebbleCount = Random.Range(5, 10);
        for (int i = 0; i < pebbleCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1.35f, 1.35f), 0.11f, Random.Range(-1.35f, 1.35f));
            Vector3 scale = Vector3.one * Random.Range(0.1f, 0.24f);
            CreatePart(parent, "Pebble", PrimitiveType.Sphere, center + offset, scale, rockMat);
        }
    }

    private static void CreateTree(Transform parent, Vector3 center, Material trunkMat, Material leafMat)
    {
        float trunkHeight = Random.Range(1.5f, 2.3f);
        float trunkWidth = Random.Range(0.28f, 0.42f);
        GameObject trunk = CreatePart(
            parent,
            "TreeTrunk",
            PrimitiveType.Cylinder,
            center + new Vector3(0f, trunkHeight * 0.5f, 0f),
            new Vector3(trunkWidth, trunkHeight * 0.5f, trunkWidth),
            trunkMat);
        trunk.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-2f, 2f));

        float branchY = trunkHeight * 0.7f;
        for (int i = 0; i < 3; i++)
        {
            float yaw = 120f * i + Random.Range(-20f, 20f);
            Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 branchPos = center + new Vector3(0f, branchY + Random.Range(-0.16f, 0.16f), 0f) + dir * Random.Range(0.28f, 0.42f);
            GameObject branch = CreatePart(
                parent,
                "TreeBranch",
                PrimitiveType.Cylinder,
                branchPos,
                new Vector3(trunkWidth * 0.45f, Random.Range(0.22f, 0.32f), trunkWidth * 0.45f),
                trunkMat);
            branch.transform.rotation = Quaternion.Euler(Random.Range(-20f, -8f), yaw, Random.Range(-18f, 18f));
        }

        float canopyBaseY = trunkHeight + 0.42f;
        for (int i = 0; i < 3; i++)
        {
            float size = Random.Range(1.2f, 1.9f) * (1f - i * 0.14f);
            Vector3 offset = new Vector3(Random.Range(-0.22f, 0.22f), i * 0.44f, Random.Range(-0.22f, 0.22f));
            CreatePart(
                parent,
                "TreeLeaves",
                PrimitiveType.Sphere,
                center + new Vector3(0f, canopyBaseY, 0f) + offset,
                new Vector3(size, size * Random.Range(0.85f, 1.08f), size),
                leafMat);
        }
    }

    private static void CreateBush(Transform parent, Vector3 center, Material leafMat, Material dirtMat)
    {
        float radius = Random.Range(0.95f, 1.5f);
        CreatePart(parent, "BushBase", PrimitiveType.Cylinder, center + new Vector3(0f, 0.06f, 0f), new Vector3(radius * 0.55f, 0.06f, radius * 0.55f), dirtMat);

        int chunks = Random.Range(4, 7);
        for (int i = 0; i < chunks; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.48f, 0.48f), Random.Range(0.28f, 0.58f), Random.Range(-0.48f, 0.48f));
            float size = Random.Range(0.6f, 1f);
            CreatePart(parent, "BushLeaf", PrimitiveType.Sphere, center + offset, new Vector3(size, size * 0.78f, size), leafMat);
        }
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
        return CreatePart(parent, name, PrimitiveType.Cube, position, scale, material);
    }

    private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Material material)
    {
        var go = GameObject.CreatePrimitive(primitiveType);
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

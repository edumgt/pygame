using UnityEngine;

public static class RuntimeRoadFactory
{
    private const float ScrollSpeed = 12f;
    private const float LoopMinZ = -32f;
    private const float LoopMaxZ = 32f;

    public static void BuildIfMissing()
    {
        if (GameObject.Find("RuntimeEnvironment") != null)
        {
            return;
        }

        var root = new GameObject("RuntimeEnvironment");

        Material roadMat = RuntimeMaterialFactory.Create(new Color(0.14f, 0.14f, 0.16f, 1f));
        Material shoulderMat = RuntimeMaterialFactory.Create(new Color(0.22f, 0.2f, 0.18f, 1f));
        Material lineMat = RuntimeMaterialFactory.Create(new Color(0.95f, 0.9f, 0.58f, 1f));
        Material railMat = RuntimeMaterialFactory.Create(new Color(0.65f, 0.68f, 0.72f, 1f));
        Material poleMat = RuntimeMaterialFactory.Create(new Color(0.18f, 0.18f, 0.2f, 1f));
        Material lampMat = RuntimeMaterialFactory.Create(new Color(1f, 0.95f, 0.72f, 1f));

        CreateRoad(root.transform, roadMat, shoulderMat, lineMat);
        CreateGuardRails(root.transform, railMat);
        CreateStreetLights(root.transform, poleMat, lampMat);
        CreateRoadsideBlocks(root.transform);
        EnsureDirectionalLight(root.transform);
    }

    private static void CreateRoad(Transform parent, Material roadMat, Material shoulderMat, Material lineMat)
    {
        var road = GameObject.CreatePrimitive(PrimitiveType.Plane);
        road.name = "Road";
        road.transform.SetParent(parent, false);
        road.transform.position = new Vector3(0f, 0f, 6f);
        road.transform.localScale = new Vector3(0.8f, 1f, 3.2f);
        road.GetComponent<Renderer>().material = roadMat;
        AddLoopMover(road, ScrollSpeed, LoopMinZ, LoopMaxZ);

        AddLoopMover(CreateBox(parent, "ShoulderLeft", new Vector3(-5.1f, 0.02f, 6f), new Vector3(2f, 0.04f, 65f), shoulderMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
        AddLoopMover(CreateBox(parent, "ShoulderRight", new Vector3(5.1f, 0.02f, 6f), new Vector3(2f, 0.04f, 65f), shoulderMat), ScrollSpeed, LoopMinZ, LoopMaxZ);

        AddLoopMover(CreateBox(parent, "EdgeLineLeft", new Vector3(-4.1f, 0.03f, 6f), new Vector3(0.1f, 0.02f, 65f), lineMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
        AddLoopMover(CreateBox(parent, "EdgeLineRight", new Vector3(4.1f, 0.03f, 6f), new Vector3(0.1f, 0.02f, 65f), lineMat), ScrollSpeed, LoopMinZ, LoopMaxZ);

        for (float z = -22f; z <= 34f; z += 3.8f)
        {
            AddLoopMover(CreateBox(parent, "LaneDash", new Vector3(0f, 0.03f, z), new Vector3(0.16f, 0.02f, 2.2f), lineMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
        }
    }

    private static void CreateGuardRails(Transform parent, Material railMat)
    {
        AddLoopMover(CreateBox(parent, "RailLeft", new Vector3(-6f, 0.55f, 6f), new Vector3(0.24f, 0.18f, 65f), railMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
        AddLoopMover(CreateBox(parent, "RailRight", new Vector3(6f, 0.55f, 6f), new Vector3(0.24f, 0.18f, 65f), railMat), ScrollSpeed, LoopMinZ, LoopMaxZ);

        for (float z = -24f; z <= 36f; z += 4f)
        {
            AddLoopMover(CreateBox(parent, "RailPostL", new Vector3(-6f, 0.28f, z), new Vector3(0.12f, 0.5f, 0.12f), railMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
            AddLoopMover(CreateBox(parent, "RailPostR", new Vector3(6f, 0.28f, z), new Vector3(0.12f, 0.5f, 0.12f), railMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
        }
    }

    private static void CreateStreetLights(Transform parent, Material poleMat, Material lampMat)
    {
        for (float z = -18f; z <= 30f; z += 8f)
        {
            CreateLamp(parent, new Vector3(-7.4f, 0f, z), poleMat, lampMat);
            CreateLamp(parent, new Vector3(7.4f, 0f, z + 3.5f), poleMat, lampMat);
        }
    }

    private static void CreateLamp(Transform parent, Vector3 basePos, Material poleMat, Material lampMat)
    {
        var lampRoot = new GameObject("StreetLamp");
        lampRoot.transform.SetParent(parent, false);
        lampRoot.transform.position = basePos;

        CreateBox(lampRoot.transform, "LampPole", new Vector3(0f, 2.1f, 0f), new Vector3(0.12f, 4.2f, 0.12f), poleMat);
        CreateBox(lampRoot.transform, "LampArm", new Vector3(0.35f, 4.15f, 0f), new Vector3(0.7f, 0.08f, 0.08f), poleMat);
        CreateBox(lampRoot.transform, "LampHead", new Vector3(0.68f, 4.05f, 0f), new Vector3(0.18f, 0.1f, 0.14f), lampMat);

        var lightGo = new GameObject("LampLight");
        lightGo.transform.SetParent(lampRoot.transform, false);
        lightGo.transform.localPosition = new Vector3(0.68f, 3.95f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 10f;
        light.intensity = 0.35f;
        light.color = new Color(1f, 0.95f, 0.78f, 1f);

        AddLoopMover(lampRoot, ScrollSpeed, LoopMinZ, LoopMaxZ);
    }

    private static void CreateRoadsideBlocks(Transform parent)
    {
        Color[] palette =
        {
            new Color(0.24f, 0.26f, 0.3f, 1f),
            new Color(0.3f, 0.33f, 0.37f, 1f),
            new Color(0.34f, 0.36f, 0.41f, 1f),
            new Color(0.2f, 0.22f, 0.26f, 1f),
        };

        int i = 0;
        for (float z = -24f; z <= 36f; z += 5f)
        {
            float leftHeight = 2.5f + (i % 4) * 0.8f;
            float rightHeight = 2.8f + ((i + 2) % 4) * 0.9f;
            Material leftMat = RuntimeMaterialFactory.Create(palette[i % palette.Length]);
            Material rightMat = RuntimeMaterialFactory.Create(palette[(i + 1) % palette.Length]);

            AddLoopMover(CreateBox(parent, "BlockL", new Vector3(-10.5f, leftHeight * 0.5f, z), new Vector3(3f, leftHeight, 3.5f), leftMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
            AddLoopMover(CreateBox(parent, "BlockR", new Vector3(10.5f, rightHeight * 0.5f, z + 1.6f), new Vector3(3f, rightHeight, 3.5f), rightMat), ScrollSpeed, LoopMinZ, LoopMaxZ);
            i++;
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
        light.intensity = 1.15f;
        light.color = new Color(1f, 0.98f, 0.94f, 1f);
        lightGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
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

    private static void AddLoopMover(GameObject go, float speed, float minZ, float maxZ)
    {
        var mover = go.AddComponent<RuntimeLoopMover>();
        mover.Configure(speed, minZ, maxZ);
    }
}

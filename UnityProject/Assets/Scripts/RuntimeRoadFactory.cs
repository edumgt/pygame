using UnityEngine;

public static class RuntimeRoadFactory
{
    public static void BuildIfMissing()
    {
        if (GameObject.Find("RuntimeBattlefield") != null)
        {
            return;
        }

        var root = new GameObject("RuntimeBattlefield");

        Material groundMat = RuntimeMaterialFactory.Create(new Color(0.33f, 0.4f, 0.29f, 1f));
        Material laneMat = RuntimeMaterialFactory.Create(new Color(0.9f, 0.86f, 0.5f, 1f));
        Material wallMat = RuntimeMaterialFactory.Create(new Color(0.46f, 0.42f, 0.35f, 1f));
        Material bunkerMat = RuntimeMaterialFactory.Create(new Color(0.22f, 0.24f, 0.26f, 1f));
        Material markerMat = RuntimeMaterialFactory.Create(new Color(0.86f, 0.26f, 0.22f, 1f));

        CreateGround(root.transform, groundMat, laneMat);
        CreateBoundary(root.transform, wallMat, bunkerMat);
        CreateTargetMarkers(root.transform, markerMat);
        EnsureDirectionalLight(root.transform);
    }

    private static void CreateGround(Transform parent, Material groundMat, Material laneMat)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(parent, false);
        ground.transform.localPosition = new Vector3(0f, 0f, 6f);
        ground.transform.localScale = new Vector3(1.2f, 1f, 3.6f);
        ground.GetComponent<Renderer>().material = groundMat;

        Collider groundCollider = ground.GetComponent<Collider>();
        if (groundCollider != null)
        {
            Object.Destroy(groundCollider);
        }

        CreateBox(parent, "LaneCenter", new Vector3(0f, 0.02f, 6f), new Vector3(0.12f, 0.04f, 65f), laneMat);

        for (float z = -18f; z <= 28f; z += 4f)
        {
            CreateBox(parent, "LaneDashL", new Vector3(-2.2f, 0.03f, z), new Vector3(0.12f, 0.02f, 2f), laneMat);
            CreateBox(parent, "LaneDashR", new Vector3(2.2f, 0.03f, z), new Vector3(0.12f, 0.02f, 2f), laneMat);
        }
    }

    private static void CreateBoundary(Transform parent, Material wallMat, Material bunkerMat)
    {
        CreateBox(parent, "WallLeft", new Vector3(-6.8f, 0.75f, 6f), new Vector3(0.5f, 1.5f, 66f), wallMat);
        CreateBox(parent, "WallRight", new Vector3(6.8f, 0.75f, 6f), new Vector3(0.5f, 1.5f, 66f), wallMat);
        CreateBox(parent, "FrontWall", new Vector3(0f, 0.8f, 31f), new Vector3(14f, 1.6f, 0.8f), wallMat);
        CreateBox(parent, "BackBunker", new Vector3(0f, 0.8f, -10.5f), new Vector3(14f, 1.6f, 0.8f), bunkerMat);

        for (float x = -4.8f; x <= 4.8f; x += 2.4f)
        {
            CreateBox(parent, "Cover", new Vector3(x, 0.6f, -6f), new Vector3(1f, 1.2f, 0.55f), bunkerMat);
        }
    }

    private static void CreateTargetMarkers(Transform parent, Material markerMat)
    {
        for (float z = -4f; z <= 24f; z += 4f)
        {
            CreateBox(parent, "MarkerL", new Vector3(-5.45f, 0.95f, z), new Vector3(0.2f, 1.9f, 0.2f), markerMat);
            CreateBox(parent, "MarkerR", new Vector3(5.45f, 0.95f, z), new Vector3(0.2f, 1.9f, 0.2f), markerMat);
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
}

using UnityEngine;

public static class RuntimeCarFactory
{
    public static GameObject CreateSportsCar(Vector3 position)
    {
        return CreateTank(position);
    }

    public static GameObject CreateTank(Vector3 position)
    {
        var root = new GameObject("Tank");
        root.transform.position = position;

        var collider = root.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(0f, 0.65f, 0.1f);
        collider.size = new Vector3(1.9f, 1.15f, 2.55f);

        var rb = root.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Material hullMat = RuntimeMaterialFactory.Create(new Color(0.24f, 0.36f, 0.22f, 1f));
        Material turretMat = RuntimeMaterialFactory.Create(new Color(0.2f, 0.31f, 0.18f, 1f));
        Material trackMat = RuntimeMaterialFactory.Create(new Color(0.1f, 0.1f, 0.11f, 1f));
        Material detailMat = RuntimeMaterialFactory.Create(new Color(0.56f, 0.62f, 0.38f, 1f));
        Material zoneMat = RuntimeMaterialFactory.Create(new Color(0.96f, 0.86f, 0.18f, 1f));

        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), new Vector3(1.55f, 0.55f, 2.2f), hullMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(-0.92f, 0.26f, 0f), new Vector3(0.28f, 0.25f, 2.28f), trackMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0.92f, 0.26f, 0f), new Vector3(0.28f, 0.25f, 2.28f), trackMat);
        CreatePart(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.82f, -0.1f), new Vector3(0.52f, 0.19f, 0.52f), turretMat);

        GameObject barrel = CreatePart(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.86f, 0.92f), new Vector3(0.12f, 0.72f, 0.12f), detailMat);
        barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.68f, -0.84f), new Vector3(0.48f, 0.16f, 0.42f), detailMat);

        var spawnPoint = new GameObject("MissileSpawnPoint");
        spawnPoint.transform.SetParent(root.transform, false);
        spawnPoint.transform.localPosition = new Vector3(0f, 0.92f, 1.82f);

        var aimZone = new GameObject("AimZone");
        aimZone.transform.SetParent(root.transform, false);
        aimZone.transform.localPosition = new Vector3(0f, 0.95f, 8.6f);
        var zoneCollider = aimZone.AddComponent<SphereCollider>();
        zoneCollider.isTrigger = true;
        zoneCollider.radius = 1.6f;
        aimZone.AddComponent<TankAimZone>();

        GameObject zoneVisual = CreatePart(aimZone.transform, PrimitiveType.Cylinder, new Vector3(0f, -0.85f, 0f), new Vector3(1.6f, 0.03f, 1.6f), zoneMat);
        zoneVisual.name = "AimZoneVisual";
        zoneVisual.transform.localRotation = Quaternion.identity;

        return root;
    }

    private static GameObject CreatePart(Transform parent, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var part = GameObject.CreatePrimitive(primitiveType);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().material = material;

        Collider col = part.GetComponent<Collider>();
        if (col != null)
        {
            Object.Destroy(col);
        }

        return part;
    }
}

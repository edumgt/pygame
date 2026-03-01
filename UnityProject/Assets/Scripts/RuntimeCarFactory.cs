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
        collider.isTrigger = false;
        collider.center = new Vector3(0f, 0.7f, 0f);
        collider.size = new Vector3(1.9f, 1.2f, 2.6f);

        var rb = root.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Material hullMat = RuntimeMaterialFactory.Create(new Color(0.25f, 0.38f, 0.22f, 1f));
        Material turretMat = RuntimeMaterialFactory.Create(new Color(0.22f, 0.33f, 0.2f, 1f));
        Material trackMat = RuntimeMaterialFactory.Create(new Color(0.1f, 0.1f, 0.12f, 1f));
        Material detailMat = RuntimeMaterialFactory.Create(new Color(0.58f, 0.64f, 0.4f, 1f));

        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.4f, 0f), new Vector3(1.55f, 0.55f, 2.2f), hullMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(-0.92f, 0.26f, 0f), new Vector3(0.28f, 0.24f, 2.28f), trackMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0.92f, 0.26f, 0f), new Vector3(0.28f, 0.24f, 2.28f), trackMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.63f, -0.84f), new Vector3(0.5f, 0.18f, 0.42f), detailMat);

        var turretPivot = new GameObject("TurretPivot");
        turretPivot.transform.SetParent(root.transform, false);
        turretPivot.transform.localPosition = new Vector3(0f, 0.86f, -0.06f);

        CreatePart(turretPivot.transform, PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(0.52f, 0.19f, 0.52f), turretMat);
        GameObject barrel = CreatePart(turretPivot.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0.94f), new Vector3(0.12f, 0.72f, 0.12f), detailMat);
        barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        var muzzlePoint = new GameObject("MuzzlePoint");
        muzzlePoint.transform.SetParent(turretPivot.transform, false);
        muzzlePoint.transform.localPosition = new Vector3(0f, 0.02f, 1.72f);

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

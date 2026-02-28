using UnityEngine;

public static class RuntimeCarFactory
{
    public static GameObject CreateSportsCar(Vector3 position)
    {
        var root = new GameObject("Player");
        root.transform.position = position;

        var collider = root.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(0f, 0.55f, 0.1f);
        collider.size = new Vector3(1.25f, 0.65f, 2.5f);

        var rb = root.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Material bodyMat = RuntimeMaterialFactory.Create(new Color(0.18f, 0.82f, 1f, 1f));
        Material glassMat = RuntimeMaterialFactory.Create(new Color(0.14f, 0.16f, 0.2f, 1f));
        Material trimMat = RuntimeMaterialFactory.Create(new Color(0.07f, 0.07f, 0.08f, 1f));
        Material lightMat = RuntimeMaterialFactory.Create(new Color(1f, 0.94f, 0.7f, 1f));

        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(1.15f, 0.45f, 2.25f), bodyMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.1f), new Vector3(0.95f, 0.32f, 1.15f), bodyMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.84f, -0.25f), new Vector3(0.82f, 0.2f, 0.75f), glassMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.62f, 0.95f), new Vector3(1.05f, 0.2f, 0.22f), trimMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.58f, -1.08f), new Vector3(1.05f, 0.12f, 0.18f), trimMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(-0.32f, 0.62f, -1.12f), new Vector3(0.15f, 0.08f, 0.04f), lightMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0.32f, 0.62f, -1.12f), new Vector3(0.15f, 0.08f, 0.04f), lightMat);
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(-0.32f, 0.55f, 1.12f), new Vector3(0.14f, 0.08f, 0.04f), RuntimeMaterialFactory.Create(new Color(1f, 0.2f, 0.2f, 1f)));
        CreatePart(root.transform, PrimitiveType.Cube, new Vector3(0.32f, 0.55f, 1.12f), new Vector3(0.14f, 0.08f, 0.04f), RuntimeMaterialFactory.Create(new Color(1f, 0.2f, 0.2f, 1f)));

        CreateWheel(root.transform, new Vector3(-0.5f, 0.24f, -0.78f), trimMat);
        CreateWheel(root.transform, new Vector3(0.5f, 0.24f, -0.78f), trimMat);
        CreateWheel(root.transform, new Vector3(-0.5f, 0.24f, 0.78f), trimMat);
        CreateWheel(root.transform, new Vector3(0.5f, 0.24f, 0.78f), trimMat);

        return root;
    }

    private static void CreateWheel(Transform parent, Vector3 localPosition, Material material)
    {
        GameObject wheel = CreatePart(parent, PrimitiveType.Cylinder, localPosition, new Vector3(0.34f, 0.12f, 0.34f), material);
        wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
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

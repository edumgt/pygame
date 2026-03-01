using UnityEngine;

public class TankVFXController : MonoBehaviour
{
    [Header("Track Anchors")]
    [SerializeField] private float trackHalfWidth = 0.92f;
    [SerializeField] private float trackRearOffset = -0.82f;
    [SerializeField] private float trackHeight = 0.07f;

    [Header("Dust")]
    [SerializeField] private float dustStartSpeed = 1.2f;
    [SerializeField] private float maxDustRate = 85f;
    [SerializeField] private float maxDustLifetime = 1.35f;
    [SerializeField] private float dustStartSize = 0.62f;

    [Header("Track Marks")]
    [SerializeField] private float markMinDistance = 0.28f;
    [SerializeField] private int maxPointsPerTrail = 220;
    [SerializeField] private float markWidth = 0.23f;

    private ParticleSystem leftDust;
    private ParticleSystem rightDust;
    private Transform leftAnchor;
    private Transform rightAnchor;

    private LineRenderer leftTrail;
    private LineRenderer rightTrail;
    private Vector3 lastLeftMark;
    private Vector3 lastRightMark;
    private bool hasLastLeftMark;
    private bool hasLastRightMark;

    private Vector3 previousPosition;
    private Vector3 velocity;
    private Quaternion previousRotation;
    private bool hasPreviousFrame;
    private float smoothedSlip;

    public void ResetEffects()
    {
        if (leftDust != null)
        {
            leftDust.Clear();
            leftDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (rightDust != null)
        {
            rightDust.Clear();
            rightDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ResetTrail(ref leftTrail, ref hasLastLeftMark);
        ResetTrail(ref rightTrail, ref hasLastRightMark);

        hasPreviousFrame = false;
        velocity = Vector3.zero;
        smoothedSlip = 0f;
    }

    private void Awake()
    {
        EnsureRig();
    }

    private void LateUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            SetDustRate(leftDust, 0f);
            SetDustRate(rightDust, 0f);
            return;
        }

        if (!EnsureRig())
        {
            return;
        }

        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        if (hasPreviousFrame)
        {
            velocity = (currentPosition - previousPosition) / dt;
        }
        else
        {
            velocity = Vector3.zero;
        }

        float speed = velocity.magnitude;
        float yawRate = 0f;
        if (hasPreviousFrame)
        {
            Quaternion delta = Quaternion.Inverse(previousRotation) * currentRotation;
            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f)
            {
                angle -= 360f;
            }

            float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.up));
            yawRate = sign * angle / dt;
        }

        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        RuntimeRoadFactory.SurfaceProfile surface = RuntimeRoadFactory.SampleSurfaceProfile(transform.position);

        float lateralSlip = Mathf.Abs(localVelocity.x);
        float turnSlip = Mathf.Abs(yawRate) * 0.018f;
        float tractionLoss = 1f - surface.Traction;
        float rawSlip = Mathf.Clamp01(lateralSlip * 0.14f + turnSlip + tractionLoss * 1.25f);
        float smoothT = 1f - Mathf.Exp(-9f * dt);
        smoothedSlip = Mathf.Lerp(smoothedSlip, rawSlip, smoothT);

        float speedFactor = Mathf.InverseLerp(dustStartSpeed, maxForwardVisualSpeed(surface), speed);
        float dustRate = maxDustRate * smoothedSlip * speedFactor;
        SetDustRate(leftDust, dustRate);
        SetDustRate(rightDust, dustRate);

        if (speed > 0.65f && (smoothedSlip > 0.11f || surface.Traction < 0.92f))
        {
            UpdateTrackTrail(leftTrail, leftAnchor.position, ref lastLeftMark, ref hasLastLeftMark, surface, smoothedSlip);
            UpdateTrackTrail(rightTrail, rightAnchor.position, ref lastRightMark, ref hasLastRightMark, surface, smoothedSlip);
        }

        previousPosition = currentPosition;
        previousRotation = currentRotation;
        hasPreviousFrame = true;
    }

    private bool EnsureRig()
    {
        if (leftAnchor == null || rightAnchor == null)
        {
            Transform existingLeft = transform.Find("TrackVFXLeft");
            Transform existingRight = transform.Find("TrackVFXRight");

            if (existingLeft == null)
            {
                var left = new GameObject("TrackVFXLeft");
                left.transform.SetParent(transform, false);
                left.transform.localPosition = new Vector3(-trackHalfWidth, trackHeight, trackRearOffset);
                existingLeft = left.transform;
            }

            if (existingRight == null)
            {
                var right = new GameObject("TrackVFXRight");
                right.transform.SetParent(transform, false);
                right.transform.localPosition = new Vector3(trackHalfWidth, trackHeight, trackRearOffset);
                existingRight = right.transform;
            }

            leftAnchor = existingLeft;
            rightAnchor = existingRight;
        }

        if (leftDust == null)
        {
            leftDust = CreateDustSystem("TrackDustLeft", leftAnchor);
        }

        if (rightDust == null)
        {
            rightDust = CreateDustSystem("TrackDustRight", rightAnchor);
        }

        if (leftTrail == null)
        {
            leftTrail = CreateTrail("TrackTrailLeft");
        }

        if (rightTrail == null)
        {
            rightTrail = CreateTrail("TrackTrailRight");
        }

        return leftAnchor != null && rightAnchor != null && leftDust != null && rightDust != null && leftTrail != null && rightTrail != null;
    }

    private ParticleSystem CreateDustSystem(string name, Transform parentAnchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parentAnchor, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = maxDustLifetime;
        main.startSpeed = 1.7f;
        main.startSize = dustStartSize;
        main.startColor = new Color(0.6f, 0.49f, 0.36f, 0.56f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        main.gravityModifier = 0f;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 24f;
        shape.radius = 0.15f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.y = new ParticleSystem.MinMaxCurve(0.34f);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.61f, 0.52f, 0.39f), 0f),
                new GradientColorKey(new Color(0.44f, 0.4f, 0.31f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.44f, 0f),
                new GradientAlphaKey(0.15f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = gradient;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateDustMaterial();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;

        return ps;
    }

    private LineRenderer CreateTrail(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform.parent != null ? transform.parent : transform, true);
        var line = go.AddComponent<LineRenderer>();
        line.material = CreateTrailMaterial();
        line.textureMode = LineTextureMode.Tile;
        line.alignment = LineAlignment.View;
        line.widthMultiplier = markWidth;
        line.numCapVertices = 0;
        line.numCornerVertices = 1;
        line.useWorldSpace = true;
        line.positionCount = 0;
        line.receiveShadows = false;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.startColor = new Color(0.11f, 0.1f, 0.1f, 0.26f);
        line.endColor = new Color(0.11f, 0.1f, 0.1f, 0.26f);
        return line;
    }

    private static void SetDustRate(ParticleSystem ps, float rate)
    {
        if (ps == null)
        {
            return;
        }

        var emission = ps.emission;
        emission.rateOverTime = rate;
    }

    private void UpdateTrackTrail(
        LineRenderer line,
        Vector3 anchorPosition,
        ref Vector3 lastMark,
        ref bool hasLastMark,
        RuntimeRoadFactory.SurfaceProfile surface,
        float slip)
    {
        if (line == null)
        {
            return;
        }

        Vector3 markPos = anchorPosition + Vector3.up * 0.015f;
        if (hasLastMark && Vector3.Distance(markPos, lastMark) < markMinDistance)
        {
            return;
        }

        if (line.positionCount >= maxPointsPerTrail)
        {
            for (int i = 1; i < line.positionCount; i++)
            {
                line.SetPosition(i - 1, line.GetPosition(i));
            }

            line.positionCount = maxPointsPerTrail - 1;
        }

        line.positionCount += 1;
        line.SetPosition(line.positionCount - 1, markPos);

        float alpha = Mathf.Lerp(0.09f, 0.42f, Mathf.Clamp01(slip * 0.75f + (1f - surface.Traction) * 1.15f));
        Color c = new Color(0.12f, 0.1f, 0.09f, alpha);
        line.startColor = c;
        line.endColor = c;

        lastMark = markPos;
        hasLastMark = true;
    }

    private static void ResetTrail(ref LineRenderer line, ref bool hasLastPoint)
    {
        if (line != null)
        {
            line.positionCount = 0;
        }

        hasLastPoint = false;
    }

    private float maxForwardVisualSpeed(RuntimeRoadFactory.SurfaceProfile surface)
    {
        return Mathf.Max(4f, 12f * surface.SpeedFactor);
    }

    private static Material CreateDustMaterial()
    {
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", new Color(0.61f, 0.52f, 0.39f, 0.58f));
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", new Color(0.61f, 0.52f, 0.39f, 0.58f));
        }

        return mat;
    }

    private static Material CreateTrailMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", new Color(0.12f, 0.1f, 0.09f, 0.3f));
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", new Color(0.12f, 0.1f, 0.09f, 0.3f));
        }

        return mat;
    }
}

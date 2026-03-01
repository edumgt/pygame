using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text fireText;
    [SerializeField] private TMP_Text lockText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image hullBarFill;
    [SerializeField] private Image hullBarFrame;
    [SerializeField] private Image hullBarDamageFlash;
    [SerializeField] private Image topBand;
    [SerializeField] private Image gameOverBackdrop;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultText;

    private const float DamageFlashDuration = 0.22f;

    private float targetHullNormalized = 1f;
    private float displayedHullNormalized = 1f;
    private float damageFlashTimer;
    private Color topBandBaseColor = new Color(0.07f, 0.12f, 0.16f, 0.82f);
    private Color hullFrameBaseColor = new Color(0.16f, 0.25f, 0.3f, 0.94f);

    private void Awake()
    {
        EnsureRuntimeHud();
        displayedHullNormalized = targetHullNormalized = 1f;
        UpdateHullVisual(true);
    }

    private void Update()
    {
        UpdateHullVisual(false);
    }

    public void UpdateCombat(
        int score,
        int highScore,
        int destroyed,
        int targetGoal,
        float hullHp,
        float hullMaxHp,
        int shots,
        int hits,
        bool shellReady,
        int wave,
        bool hasTargetLock)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }

        if (objectiveText != null)
        {
            objectiveText.text = $"Kills: {destroyed}/{targetGoal}";
        }

        float maxHp = Mathf.Max(1f, hullMaxHp);
        float clampedHull = Mathf.Clamp(hullHp, 0f, maxHp);
        float normalizedHull = clampedHull / maxHp;
        if (normalizedHull + 0.001f < targetHullNormalized)
        {
            damageFlashTimer = DamageFlashDuration;
        }

        targetHullNormalized = normalizedHull;

        if (livesText != null)
        {
            livesText.text = $"Hull: {Mathf.CeilToInt(clampedHull)}/{Mathf.CeilToInt(maxHp)}";
        }

        if (fireText != null)
        {
            fireText.text = $"Cannon: {(shellReady ? "READY" : "RELOADING")}";
            fireText.color = shellReady
                ? new Color(0.79f, 0.96f, 0.84f, 1f)
                : new Color(1f, 0.76f, 0.52f, 1f);
        }

        if (lockText != null)
        {
            lockText.text = $"Wave: {wave}  Lock: {(hasTargetLock ? "ON" : "OFF")}";
            lockText.color = hasTargetLock
                ? new Color(0.84f, 0.98f, 0.88f, 1f)
                : new Color(0.94f, 0.84f, 0.78f, 1f);
        }

        if (accuracyText != null)
        {
            float accuracy = shots > 0 ? (float)hits / shots * 100f : 0f;
            accuracyText.text = $"Accuracy: {accuracy:0}% ({hits}/{shots})";
        }
    }

    public void SetTransientMessage(string message)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageText.color = string.IsNullOrEmpty(message)
            ? new Color(0.92f, 0.95f, 0.98f, 0f)
            : new Color(0.92f, 0.95f, 0.98f, 1f);
    }

    public void ShowGameOver(bool show, bool victory)
    {
        if (gameOverBackdrop != null)
        {
            gameOverBackdrop.gameObject.SetActive(show);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }

        if (resultText != null)
        {
            resultText.text = show
                ? (victory ? "Mission Complete\nPress R to restart" : "Tank Destroyed\nPress R to restart")
                : string.Empty;
        }
    }

    private void UpdateHullVisual(bool immediate)
    {
        if (immediate)
        {
            displayedHullNormalized = targetHullNormalized;
        }
        else
        {
            float speed = Mathf.Lerp(1.4f, 4.6f, Mathf.Abs(displayedHullNormalized - targetHullNormalized));
            displayedHullNormalized = Mathf.MoveTowards(displayedHullNormalized, targetHullNormalized, speed * Time.unscaledDeltaTime);
        }

        if (damageFlashTimer > 0f)
        {
            damageFlashTimer = Mathf.Max(0f, damageFlashTimer - Time.unscaledDeltaTime);
        }

        if (hullBarFill != null)
        {
            hullBarFill.fillAmount = Mathf.Clamp01(displayedHullNormalized);
            hullBarFill.color = EvaluateHullColor(displayedHullNormalized);
        }

        if (hullBarDamageFlash != null)
        {
            Color flash = hullBarDamageFlash.color;
            flash.a = damageFlashTimer > 0f
                ? Mathf.Clamp01(damageFlashTimer / DamageFlashDuration) * 0.72f
                : 0f;
            hullBarDamageFlash.color = flash;
        }

        float danger = Mathf.InverseLerp(0.55f, 0.12f, targetHullNormalized);
        float pulse = (Mathf.Sin(Time.unscaledTime * 10f) * 0.5f + 0.5f) * danger;

        if (hullBarFrame != null)
        {
            hullBarFrame.color = Color.Lerp(hullFrameBaseColor, new Color(0.95f, 0.21f, 0.15f, 0.98f), pulse);
        }

        if (topBand != null)
        {
            topBand.color = Color.Lerp(topBandBaseColor, new Color(0.3f, 0.08f, 0.07f, 0.88f), danger * 0.75f);
        }

        if (livesText != null)
        {
            livesText.color = Color.Lerp(new Color(0.85f, 0.93f, 0.99f, 1f), new Color(1f, 0.42f, 0.35f, 1f), danger);
        }
    }

    private static Color EvaluateHullColor(float normalized)
    {
        Color low = new Color(0.94f, 0.22f, 0.17f, 0.97f);
        Color mid = new Color(0.96f, 0.7f, 0.2f, 0.97f);
        Color high = new Color(0.26f, 0.91f, 0.5f, 0.97f);

        if (normalized < 0.5f)
        {
            return Color.Lerp(low, mid, normalized * 2f);
        }

        return Color.Lerp(mid, high, (normalized - 0.5f) * 2f);
    }

    private void EnsureRuntimeHud()
    {
        bool hasHudRefs =
            scoreText != null &&
            highScoreText != null &&
            objectiveText != null &&
            livesText != null &&
            fireText != null &&
            lockText != null &&
            accuracyText != null &&
            messageText != null &&
            hullBarFill != null &&
            gameOverPanel != null &&
            resultText != null;
        if (hasHudRefs)
        {
            return;
        }

        BuildRuntimeHud();
    }

    private void BuildRuntimeHud()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            var canvasGo = new GameObject("CombatHUDCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        topBand = CreateImage(
            "TopBand",
            canvasRect,
            new Color(0.07f, 0.12f, 0.16f, 0.82f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -164f),
            Vector2.zero);
        topBandBaseColor = topBand.color;

        CreateImage(
            "TopBandAccent",
            topBand.rectTransform,
            new Color(0.2f, 0.52f, 0.65f, 0.82f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            Vector2.zero,
            new Vector2(0f, 4f));

        scoreText = CreateText(
            "ScoreText",
            topBand.rectTransform,
            "Score: 0",
            35,
            FontStyles.Bold,
            TextAlignmentOptions.TopLeft,
            new Color(0.97f, 0.99f, 1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -18f),
            new Vector2(420f, 44f));

        highScoreText = CreateText(
            "HighScoreText",
            topBand.rectTransform,
            "High Score: 0",
            26,
            FontStyles.Normal,
            TextAlignmentOptions.TopLeft,
            new Color(0.83f, 0.9f, 0.96f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(26f, -58f),
            new Vector2(420f, 34f));

        objectiveText = CreateText(
            "ObjectiveText",
            topBand.rectTransform,
            "Kills: 0/0",
            25,
            FontStyles.Bold,
            TextAlignmentOptions.TopLeft,
            new Color(0.95f, 0.91f, 0.78f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(26f, -92f),
            new Vector2(420f, 34f));

        RectTransform hullRoot = CreateRect(
            "HullGaugeRoot",
            topBand.rectTransform,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(-310f, -74f),
            new Vector2(310f, -10f));

        CreateText(
            "HullLabel",
            hullRoot,
            "HULL INTEGRITY",
            20,
            FontStyles.Bold,
            TextAlignmentOptions.Top,
            new Color(0.82f, 0.89f, 0.94f, 0.98f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -2f),
            new Vector2(0f, 24f));

        hullBarFrame = CreateImage(
            "HullBarFrame",
            hullRoot,
            new Color(0.16f, 0.25f, 0.3f, 0.94f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 36f));
        hullFrameBaseColor = hullBarFrame.color;

        CreateImage(
            "HullBarBackground",
            hullBarFrame.rectTransform,
            new Color(0.03f, 0.04f, 0.05f, 0.92f),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(6f, 6f),
            new Vector2(-6f, -6f));

        hullBarFill = CreateImage(
            "HullBarFill",
            hullBarFrame.rectTransform,
            new Color(0.26f, 0.91f, 0.5f, 0.97f),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(8f, 8f),
            new Vector2(-8f, -8f));
        hullBarFill.type = Image.Type.Filled;
        hullBarFill.fillMethod = Image.FillMethod.Horizontal;
        hullBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        hullBarFill.fillAmount = 1f;

        hullBarDamageFlash = CreateImage(
            "HullBarDamageFlash",
            hullBarFrame.rectTransform,
            new Color(1f, 0.88f, 0.86f, 0f),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(8f, 8f),
            new Vector2(-8f, -8f));

        livesText = CreateText(
            "HullText",
            topBand.rectTransform,
            "Hull: 100/100",
            24,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.85f, 0.93f, 0.99f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -126f),
            new Vector2(340f, 30f));

        fireText = CreateText(
            "FireText",
            topBand.rectTransform,
            "Cannon: READY",
            27,
            FontStyles.Bold,
            TextAlignmentOptions.TopRight,
            new Color(0.79f, 0.96f, 0.84f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-26f, -22f),
            new Vector2(460f, 38f));

        lockText = CreateText(
            "LockText",
            topBand.rectTransform,
            "Wave: 1  Lock: OFF",
            24,
            FontStyles.Normal,
            TextAlignmentOptions.TopRight,
            new Color(0.93f, 0.86f, 0.79f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-26f, -60f),
            new Vector2(460f, 34f));

        accuracyText = CreateText(
            "AccuracyText",
            topBand.rectTransform,
            "Accuracy: 0%",
            23,
            FontStyles.Normal,
            TextAlignmentOptions.TopRight,
            new Color(0.82f, 0.9f, 0.95f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-26f, -92f),
            new Vector2(460f, 32f));

        messageText = CreateText(
            "MessageText",
            canvasRect,
            string.Empty,
            36,
            FontStyles.Bold,
            TextAlignmentOptions.Top,
            new Color(0.92f, 0.95f, 0.98f, 0f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -176f),
            new Vector2(980f, 44f));

        CreateText(
            "ControlHintText",
            canvasRect,
            "Controls: W/S move, A/D steer, Q/E turret, Space fire, R restart",
            24,
            FontStyles.Normal,
            TextAlignmentOptions.Top,
            new Color(0.8f, 0.86f, 0.9f, 0.92f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -210f),
            new Vector2(1300f, 32f));

        gameOverBackdrop = CreateImage(
            "GameOverBackdrop",
            canvasRect,
            new Color(0f, 0f, 0f, 0.58f),
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);
        gameOverBackdrop.gameObject.SetActive(false);

        Image panel = CreateImage(
            "GameOverPanel",
            canvasRect,
            new Color(0.05f, 0.08f, 0.11f, 0.94f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(-420f, -130f),
            new Vector2(420f, 130f));
        gameOverPanel = panel.gameObject;
        gameOverPanel.SetActive(false);

        CreateImage(
            "GameOverPanelAccent",
            panel.rectTransform,
            new Color(0.28f, 0.68f, 0.83f, 0.88f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -5f),
            Vector2.zero);

        resultText = CreateText(
            "ResultText",
            panel.rectTransform,
            string.Empty,
            54,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.97f, 0.99f, 1f, 1f),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            Vector2.zero,
            Vector2.zero);

        ShowGameOver(false, false);
    }

    private static RectTransform CreateRect(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }

    private static Image CreateImage(
        string name,
        Transform parent,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string textValue,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }
}

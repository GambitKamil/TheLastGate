using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only: never moves the agent or changes collision geometry.
[DefaultExecutionOrder(-100)]
public class FinalGamePresentation : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Font turkishFont;

    [SerializeField] private CompanionAgent companion;

    [SerializeField] private BridgeController bridge;
    [SerializeField] private DialogueUI dialogue;
    [SerializeField] private SpriteRenderer leverRenderer;
    [SerializeField] private Sprite leverOff;
    [SerializeField] private Sprite leverOn;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] runFrames;

    private TMP_FontAsset runtimeFont;
    private TMP_Text objective;
    private TMP_Text controls;
    private TMP_Text stage;
    private GameObject controlPanel;
    private GameObject missionPanel;
    private GameObject chapterPanel;
    private GameObject closedBarrier;
    private Collider2D gateCollider;
    private bool hasMoved;
    private SpriteRenderer companionRenderer;
    private Rigidbody2D companionBody;
    private float animationTime;
    private readonly Color gold = new Color32(224, 184, 110, 255);
    private readonly Color cream = new Color32(236, 233, 215, 255);

    private void Awake()
    {
        if (canvas == null) return;
        if (turkishFont != null)
        {
            runtimeFont = TMP_FontAsset.CreateFontAsset(turkishFont);
            runtimeFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            runtimeFont.isMultiAtlasTexturesEnabled = true;
            runtimeFont.TryAddCharacters("ÇçĞğİıÖöŞşÜü…' →");
            foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
                if (text.GetComponentInParent<TMP_InputField>() == null) text.font = runtimeFont;
        }
        RectTransform card = Panel("MissionCard", new Vector2(0, 1),
            new Vector2(24, -24), new Vector2(405, 104));
        missionPanel = card.gameObject;
        Accent(card, new Vector2(0, 0), new Vector2(4, 104), gold);
        Label(card, "SON KAPI", 16, gold,
            new Vector2(20, -12), new Vector2(350, 22));
        objective = Label(card, "", 21, cream,
            new Vector2(20, -40), new Vector2(370, 50));

        RectTransform chapter = Panel("ChapterCard", new Vector2(1, 1),
            new Vector2(-24, -24), new Vector2(230, 55));
        chapterPanel = chapter.gameObject;
        stage = Label(chapter, "01 / ORMAN KARAKOLU", 15, gold,
            new Vector2(14, -16), new Vector2(204, 24));

        RectTransform footer = Panel("Controls", new Vector2(0.5f, 0),
            new Vector2(0, 14), new Vector2(600, 40));
        controlPanel = footer.gameObject;
        controls = Label(footer, "WASD  Hareket     C  Yoldaşa komut     E  Konuş", 17, cream,
            new Vector2(14, -9), new Vector2(572, 26));
        controls.alignment = TextAlignmentOptions.Center;

    }

    private void Start()
    {
        GameObject gate = GameObject.Find("Gate");
        if (gate != null) gateCollider = gate.GetComponent<Collider2D>();
        GameObject challenge = GameObject.Find("CompanionChallenge");
        if (challenge != null)
        {
            Transform barrier = challenge.transform.Find("ClosedExit_Portcullis");
            if (barrier != null) closedBarrier = barrier.gameObject;
        }
        if (companion == null) return;
        companionRenderer = companion.GetComponent<SpriteRenderer>();
        companionBody = companion.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

        bool opened = bridge != null && bridge.IsActivated();
        if (closedBarrier != null) closedBarrier.SetActive(!opened);
        if (companionBody != null && companionBody.linearVelocity.sqrMagnitude > 0.02f) hasMoved = true;
        if (leverRenderer != null)
        {
            leverRenderer.sprite = opened ? leverOn : leverOff;
            leverRenderer.color = opened ? new Color32(170, 241, 202, 255) : Color.white;
        }
        if (objective != null)
        {
            bool gateOpen = gateCollider != null && !gateCollider.enabled;
            objective.text = !PlayerController.CanMove
                ? "Yolculuk tamamlandı."
                : gateOpen ? "Son Kapı'dan geç."
                : opened ? "Muhafızı bul. Geçiş izni iste."
                : hasMoved
                    ? "Yoldaşın şaltere gidiyor…"
                    : "Yoldaşına yaklaş. C ile yardım iste.";
            stage.text = gateOpen ? "03 / SON KAPI" : opened ? "02 / KAPI MUHAFIZI" : "01 / ORMAN KARAKOLU";
        }
        if (missionPanel != null) missionPanel.SetActive(PlayerController.CanMove);
        if (chapterPanel != null) chapterPanel.SetActive(PlayerController.CanMove);
        bool inDialogue = dialogue != null && dialogue.IsDialogueOpen;
        if (controlPanel != null)
            controlPanel.SetActive(false);
        AnimateCompanion();
    }

    private void AnimateCompanion()
    {
        if (companionRenderer == null || companionBody == null) return;
        bool moving = companionBody.linearVelocity.sqrMagnitude > 0.02f;
        Sprite[] frames = moving ? runFrames : idleFrames;
        if (frames == null || frames.Length == 0) return;
        animationTime += Time.deltaTime * (moving ? 10f : 5f);
        companionRenderer.sprite = frames[(int)animationTime % frames.Length];
        if (Mathf.Abs(companionBody.linearVelocity.x) > 0.05f)
            companionRenderer.flipX = companionBody.linearVelocity.x < 0f;
    }

    private RectTransform Panel(string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.layer = canvas.gameObject.layer;
        obj.transform.SetParent(canvas.transform, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var image = obj.GetComponent<Image>();
        image.color = new Color32(13, 25, 30, 235);
        image.raycastTarget = false;
        var outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color32(121, 135, 113, 90);
        outline.effectDistance = new Vector2(1, -1);
        return rect;
    }

    private TMP_Text Label(RectTransform parent, string value, float size, Color color,
        Vector2 position, Vector2 dimensions)
    {
        var obj = new GameObject("Label", typeof(RectTransform));
        obj.layer = canvas.gameObject.layer;
        obj.transform.SetParent(parent, false);
        var text = obj.AddComponent<TextMeshProUGUI>();
        var rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        if (runtimeFont != null) text.font = runtimeFont;
        text.fontSize = size;
        text.color = color;
        text.text = value;
        text.raycastTarget = false;
        text.richText = false;
        return text;
    }

    private void Accent(RectTransform parent, Vector2 position, Vector2 size, Color color)
    {
        var obj = new GameObject("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.layer = canvas.gameObject.layer;
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        obj.GetComponent<Image>().color = color;
        obj.GetComponent<Image>().raycastTarget = false;
    }

    private void OnDestroy()
    {
        if (runtimeFont != null)
        {
            foreach (Texture2D atlas in runtimeFont.atlasTextures)
                if (atlas != null) Destroy(atlas);
            if (runtimeFont.material != null) Destroy(runtimeFont.material);
            Destroy(runtimeFont);
        }
    }
}

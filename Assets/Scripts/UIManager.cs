using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [Header("Global Events")]
    public UnityEvent onConnectedEvent;
    public UnityEvent onDisconnectedEvent;

    [Header("Connectors (drag the SeatbeltConnector you want to track)")]
    [SerializeField] private SeatbeltConnector topConnector;
    [SerializeField] private SeatbeltConnector midConnector;
    [SerializeField] private SeatbeltConnector botConnector;

    [Header("Vest Attacher (drag the LifeVestAttacher you want to track)")]
    [SerializeField] private LifeVestAttacher vestAttacher;

    [Header("OR Parents (optional: drag TopParentBuckle/MidParentBuckle/BotParentBuckle)")]
    [SerializeField] private Transform topParent;
    [SerializeField] private Transform midParent;
    [SerializeField] private Transform botParent;

    [Header("Indicators (child Images that change color)")]
    [SerializeField] private Image topIndicator;
    [SerializeField] private Image midIndicator;
    [SerializeField] private Image botIndicator;
    [SerializeField] private Image vestIndicator;

    [Header("Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;

    // Keep delegates so we can unsubscribe cleanly
    private UnityAction topOn, topOff, midOn, midOff, botOn, botOff;
    private UnityAction vestAttached, vestDetached;

    // Track previous states to detect changes for global events
    private bool wasAllConnected = false;

    private void Awake()
    {
        // If you didn't drag specific connectors, auto-find one under the parent
        if (topConnector == null && topParent != null)
            topConnector = topParent.GetComponentInChildren<SeatbeltConnector>(true);
        if (midConnector == null && midParent != null)
            midConnector = midParent.GetComponentInChildren<SeatbeltConnector>(true);
        if (botConnector == null && botParent != null)
            botConnector = botParent.GetComponentInChildren<SeatbeltConnector>(true);

        // Auto-find vest attacher if not assigned
        if (vestAttacher == null)
            vestAttacher = FindObjectOfType<LifeVestAttacher>();
    }

    private void OnEnable()
    {
        HookConnector(topConnector, topIndicator, ref topOn, ref topOff);
        HookConnector(midConnector, midIndicator, ref midOn, ref midOff);
        HookConnector(botConnector, botIndicator, ref botOn, ref botOff);
        HookVestAttacher();

        // Initialize UI to current states
        RefreshAll();

        // Initialize the global state tracking
        wasAllConnected = AreAllConnected();
    }

    private void OnDisable()
    {
        UnhookConnector(topConnector, ref topOn, ref topOff);
        UnhookConnector(midConnector, ref midOn, ref midOff);
        UnhookConnector(botConnector, ref botOn, ref botOff);
        UnhookVestAttacher();
    }

    private void HookConnector(SeatbeltConnector conn, Image indicator, ref UnityAction onAct, ref UnityAction offAct)
    {
        if (conn == null || indicator == null) return;

        onAct = () => {
            SetIndicator(indicator, true);
            CheckGlobalState();
        };

        offAct = () => {
            SetIndicator(indicator, false);
            CheckGlobalState();
        };

        conn.onConnectedEvent.AddListener(onAct);
        conn.onDisconnectedEvent.AddListener(offAct);
    }

    private void HookVestAttacher()
    {
        if (vestAttacher == null || vestIndicator == null) return;

        vestAttached = () => {
            SetIndicator(vestIndicator, true);
            CheckGlobalState();
        };

        vestDetached = () => {
            SetIndicator(vestIndicator, false);
            CheckGlobalState();
        };

        // Note: You'll need to add these events to LifeVestAttacher
        // For now, we'll use a polling approach in Update()
    }

    private void UnhookConnector(SeatbeltConnector conn, ref UnityAction onAct, ref UnityAction offAct)
    {
        if (conn == null) return;

        if (onAct != null)
        {
            conn.onConnectedEvent.RemoveListener(onAct);
            onAct = null;
        }

        if (offAct != null)
        {
            conn.onDisconnectedEvent.RemoveListener(offAct);
            offAct = null;
        }
    }

    private void UnhookVestAttacher()
    {
        // Clean up vest attacher listeners if implemented
        vestAttached = null;
        vestDetached = null;
    }

    // Track vest attachment state for polling
    private bool lastVestAttachedState = false;

    private void Update()
    {
        // Poll vest attachment state
        if (vestAttacher != null && vestIndicator != null)
        {
            bool currentState = vestAttacher.IsAttached;
            if (currentState != lastVestAttachedState)
            {
                SetIndicator(vestIndicator, currentState);
                CheckGlobalState();
                lastVestAttachedState = currentState;
            }
        }
    }

    private void RefreshAll()
    {
        if (topIndicator != null)
            SetIndicator(topIndicator, topConnector != null && topConnector.IsConnected);
        if (midIndicator != null)
            SetIndicator(midIndicator, midConnector != null && midConnector.IsConnected);
        if (botIndicator != null)
            SetIndicator(botIndicator, botConnector != null && botConnector.IsConnected);
        if (vestIndicator != null && vestAttacher != null)
            SetIndicator(vestIndicator, vestAttacher.IsAttached);

        // Update tracking state
        if (vestAttacher != null)
            lastVestAttachedState = vestAttacher.IsAttached;
    }

    private void SetIndicator(Image indicator, bool connected)
    {
        if (indicator == null) return;
        indicator.color = connected ? connectedColor : disconnectedColor;
    }

    private void CheckGlobalState()
    {
        bool allConnected = AreAllConnected();

        // Only trigger events on state change
        if (allConnected && !wasAllConnected)
        {
            onConnectedEvent?.Invoke();
        }
        else if (!allConnected && wasAllConnected)
        {
            onDisconnectedEvent?.Invoke();
        }

        wasAllConnected = allConnected;
    }

    private bool AreAllConnected()
    {
        // Check only the connectors that exist
        bool topConnected = topConnector == null || topConnector.IsConnected;
        bool midConnected = midConnector == null || midConnector.IsConnected;
        bool botConnected = botConnector == null || botConnector.IsConnected;
        bool vestAttached = vestAttacher == null || vestAttacher.IsAttached;

        // At least one connector/attacher must exist for this to be meaningful
        bool hasAnyComponent = topConnector != null || midConnector != null || botConnector != null || vestAttacher != null;

        return hasAnyComponent && topConnected && midConnected && botConnected && vestAttached;
    }

    // Public method to manually refresh the UI state
    public void ForceRefresh()
    {
        RefreshAll();
        CheckGlobalState();
    }

    // Public methods to check individual connector states
    public bool IsTopConnected() => topConnector != null && topConnector.IsConnected;
    public bool IsMidConnected() => midConnector != null && midConnector.IsConnected;
    public bool IsBotConnected() => botConnector != null && botConnector.IsConnected;
    public bool IsVestAttached() => vestAttacher != null && vestAttacher.IsAttached;

    // Public method to check if all available connectors are connected
    public bool AreAllAvailableConnected() => AreAllConnected();
}
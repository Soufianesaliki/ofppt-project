using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sequences the "batch reveal" of the motor's internal pieces for Scenario 1.
///
/// Assign each PartAnimationController piece to the batch it belongs to in the
/// Inspector, in reveal order. Progression is driven strictly by AdvanceBatch()
/// (e.g. from the XR controller's Next/B button, via ControllerInputController):
///
///   - 1st press: reveals batch 0 (nothing to hide yet).
///   - Each following press: hides the previously revealed batch first; once
///     that hide has fully finished, reveals the next batch.
///   - Once the last batch has been revealed, one more press hides that last
///     batch (nothing new to reveal), fires onScenario1Complete once it's fully
///     hidden, then restarts — every piece resets back to its initial, visible
///     position, ready to be replayed.
///
/// A press is ignored while a reveal/hide is still in progress.
/// Restart is also available on demand: set the public restart field to true
/// (from the Inspector or from code) to reset every piece at any time.
/// </summary>
public class MotorAnimationManager : MonoBehaviour
{
    [System.Serializable]
    public class RevealBatch
    {
        public string batchName;
        public PartAnimationController[] pieces;
    }

    [Header("Batches")]
    [Tooltip("The pieces belonging to each batch, in the order the batches should be revealed.")]
    [SerializeField] private RevealBatch[] batches;

    [Header("Events")]
    [Tooltip("Invoked once the last batch has been hidden (the extra press after the last reveal).")]
    public UnityEvent onScenario1Complete = new UnityEvent();

    [Header("Editor testing")]
    [Tooltip("Toggle in Play Mode to call AdvanceBatch() without an XR controller. Always resets itself to false.")]
    [SerializeField] private bool advanceBatchDebug;

    [Header("Restart")]
    [Tooltip("Set to true to reset every piece across every batch back to its initial, visible state. Settable from code or the Inspector. Always turns back to false.")]
    public bool restart;

    // -1 = nothing revealed yet; batches.Length = fully done (last batch hidden)
    private int _currentBatchIndex = -1;
    private bool _isBusy;
    private int _pendingCount;
    private bool _completingScenario1;
    private int _pendingRevealIndex = -1; // set when a reveal must wait for the current hide to finish first

    private RevealBatch _batchBeingRevealed;
    private RevealBatch _batchBeingHidden;

    /// <summary>True while a reveal and/or hide triggered by the last AdvanceBatch() is still in progress.</summary>
    public bool isBusy => _isBusy;

    /// <summary>Index of the batch currently/most recently revealed (-1 = none yet, batches.Length = fully done).</summary>
    public int currentBatchIndex => _currentBatchIndex;

    private void Update()
    {
        if (advanceBatchDebug)
        {
            advanceBatchDebug = false;
            AdvanceBatch();
        }

        if (restart)
        {
            restart = false;
            ResetAll();
        }
    }

    /// <summary>
    /// Advances the sequence by one press. Ignored while still busy with the
    /// previous press's reveal/hide.
    /// </summary>
    public void AdvanceBatch()
    {
        if (_isBusy)
        {
            Debug.Log("MotorAnimationManager: still busy, ignoring AdvanceBatch().");
            return;
        }

        if (batches == null || batches.Length == 0)
        {
            Debug.LogWarning("MotorAnimationManager: no batches assigned.");
            return;
        }

        _pendingCount = 0;
        _isBusy = true;
        _batchBeingRevealed = null;
        _batchBeingHidden = null;
        _pendingRevealIndex = -1;

        if (_currentBatchIndex == -1)
        {
            // First press: nothing to hide yet.
            _currentBatchIndex = 0;
            StartReveal(batches[0]);
        }
        else
        {
            int batchToHide = _currentBatchIndex;
            bool wasLastBatch = batchToHide >= batches.Length - 1;

            if (wasLastBatch)
            {
                _currentBatchIndex = batches.Length;
                _completingScenario1 = true;
            }
            else
            {
                _pendingRevealIndex = batchToHide + 1;
            }

            StartHide(batches[batchToHide]);
        }

        // Covers edge cases like an empty batch (0 valid pieces assigned).
        if (_pendingCount == 0)
        {
            FinishCurrentPhase();
        }
    }

    /// <summary>Resets every piece across every batch back to its initial, visible state.</summary>
    public void ResetAll()
    {
        if (batches != null)
        {
            foreach (RevealBatch batch in batches)
            {
                if (batch?.pieces == null) continue;

                foreach (PartAnimationController piece in batch.pieces)
                {
                    if (piece == null) continue;
                    piece.onRevealComplete.RemoveListener(OnPieceOperationComplete);
                    piece.onHideComplete.RemoveListener(OnPieceOperationComplete);
                    piece.reset = true;
                }
            }
        }

        _currentBatchIndex = -1;
        _isBusy = false;
        _pendingCount = 0;
        _completingScenario1 = false;
        _pendingRevealIndex = -1;
        _batchBeingRevealed = null;
        _batchBeingHidden = null;
    }

    private void StartReveal(RevealBatch batch)
    {
        _batchBeingRevealed = batch;
        if (batch?.pieces == null) return;

        // Count/subscribe every valid piece first, then trigger them — so a same-frame
        // completion can never zero the counter before the rest have been triggered.
        foreach (PartAnimationController piece in batch.pieces)
        {
            if (piece == null) continue;
            _pendingCount++;
            piece.onRevealComplete.AddListener(OnPieceOperationComplete);
        }

        foreach (PartAnimationController piece in batch.pieces)
        {
            if (piece == null) continue;
            piece.activate = true;
        }
    }

    private void StartHide(RevealBatch batch)
    {
        _batchBeingHidden = batch;
        if (batch?.pieces == null) return;

        // Same two-pass pattern as StartReveal — critical here since Hide() can complete
        // synchronously (animateHide == false), which would otherwise let pendingCount
        // hit 0 mid-loop and finish this phase before every piece has been hidden.
        foreach (PartAnimationController piece in batch.pieces)
        {
            if (piece == null) continue;
            _pendingCount++;
            piece.onHideComplete.AddListener(OnPieceOperationComplete);
        }

        foreach (PartAnimationController piece in batch.pieces)
        {
            if (piece == null) continue;
            piece.Hide();
        }
    }

    private void OnPieceOperationComplete()
    {
        _pendingCount--;
        if (_pendingCount <= 0)
        {
            FinishCurrentPhase();
        }
    }

    private void FinishCurrentPhase()
    {
        if (_batchBeingHidden != null)
        {
            UnsubscribeBatch(_batchBeingHidden, isReveal: false);
            _batchBeingHidden = null;
        }

        if (_batchBeingRevealed != null)
        {
            UnsubscribeBatch(_batchBeingRevealed, isReveal: true);
            _batchBeingRevealed = null;
        }

        // The hide phase just fully finished and a reveal was waiting on it — start
        // that reveal now, still busy, so the hide is guaranteed complete first.
        if (_pendingRevealIndex >= 0)
        {
            int revealIndex = _pendingRevealIndex;
            _pendingRevealIndex = -1;
            _currentBatchIndex = revealIndex;

            _pendingCount = 0;
            StartReveal(batches[revealIndex]);

            if (_pendingCount == 0)
            {
                FinishCurrentPhase();
            }

            return;
        }

        _isBusy = false;

        if (_completingScenario1)
        {
            _completingScenario1 = false;
            onScenario1Complete.Invoke();
            restart = true; // deferred to next frame's Update() — avoids resetting mid-callback
        }
    }

    private void UnsubscribeBatch(RevealBatch batch, bool isReveal)
    {
        if (batch?.pieces == null) return;

        foreach (PartAnimationController piece in batch.pieces)
        {
            if (piece == null) continue;

            if (isReveal)
                piece.onRevealComplete.RemoveListener(OnPieceOperationComplete);
            else
                piece.onHideComplete.RemoveListener(OnPieceOperationComplete);
        }
    }
}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Attach to an XRSocketInteractor and add this component to its Select Filters list
    /// (in the Inspector, under the interactor's Interactable Events / Filters foldout).
    ///
    /// Rejects every interactable except the one explicitly assigned to _expectedInteractable,
    /// so a socket only ever accepts the single component it was designed for — e.g. one
    /// specific push button, not any push button that happens to fit.
    ///
    /// Reusable for any slotted component (breaker, contactor, relay, push button, etc.),
    /// since matching is by direct reference rather than a shared Interaction Layer.
    /// </summary>
    public class ExpectedComponentSocketFilter : MonoBehaviour, IXRSelectFilter
    {
        [Tooltip("The only interactable this socket is allowed to accept. Leave empty to accept anything.")]
        [SerializeField] private XRGrabInteractable _expectedInteractable;

        public bool canProcess => isActiveAndEnabled;

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return _expectedInteractable == null || ReferenceEquals(interactable, _expectedInteractable);
        }
    }
}

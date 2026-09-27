using UnityEngine.Events;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Common interface for any circuit component that can be placed into a socket on the
    /// DIN rail/control panel. Implemented by PushButtonController, VoyantController,
    /// DisjonctorController, ContactorController and ThermalRelayController so that
    /// WiringSequenceManager (and any future manager) can treat every component type
    /// uniformly instead of branching per type.
    /// </summary>
    public interface ISnappable
    {
        /// <summary>True while this component is placed in its correct circuit slot.</summary>
        bool isSnaped { get; }

        /// <summary>Invoked when isSnaped changes (true = snapped in, false = removed).</summary>
        UnityEvent<bool> onSnappedChanged { get; }
    }
}

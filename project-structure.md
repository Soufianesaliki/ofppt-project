# OFPPT Electrical Workshop — Development Handoff

> **Purpose:** This is the current, code-backed handoff document for a developer or AI agent taking over this Unity project. Read it before changing project behavior. It describes what is actually present in the repository as of 2026-09-30, not only the original intended architecture.

## Quick start

| Item | Value |
| --- | --- |
| Project type | Unity VR electrical-workshop training application |
| Unity editor | `6000.3.24f1` (Unity 6.3 LTS) |
| Rendering | Universal Render Pipeline (URP 17.3.0) |
| XR stack | OpenXR, XR Interaction Toolkit 3.3.2, XR Management, XR Hands and AR Foundation |
| Input | Unity Input System 1.20.0 plus XRI input assets |
| Build scene | `Assets/Scenes/entry-scene.unity` — the only enabled build scene |
| Main custom-code location | `Assets/Scripts/` |
| Primary scene controller objects | `ScenarioFlow-Code`, `Wiring-Code`, `ActivarionPhase-Code`, `Controller-Code`, `App-Code` |

Open the project through Unity Hub with the listed Unity version. `Library/` is generated local state and is intentionally not part of the authored project structure.

## Authoritative documents and scope

- This document is the current technical map and starting point for a takeover.
- `Assets/Scripts/Unity_VR_Electrical_Workshop_Agent_Guide.md` is a useful **original architecture/design brief**, but its “Current Project Status” section is outdated: the component, scenario, snapping, motor-reveal, and activation scripts it anticipated now exist. Do not delete or blindly follow its milestone status.
- The scene is inspector-driven. Serialized references in `entry-scene.unity` connect the scripts; code alone cannot fully describe runtime setup.
- There are no project-owned automated tests or assembly-definition files at present.

## Repository layout

```text
Assets/
├── Audio/                         # Audio clips used by the scene
├── Models/
│   ├── Environment/                # Current workshop/environment art
│   ├── Environment-old/            # Older environment kept as reference
│   └── Meshs/                      # Electrical-device meshes, FBXs, materials, textures
├── Prefabs/
│   ├── Models/                     # Reusable electrical-device prefabs
│   │   └── not-used/               # Explicitly unused legacy breaker variants
│   └── UI/                         # Button, toggle, and component-card prefabs
├── Scenes/
│   └── entry-scene.unity           # Complete runtime scene and build entry point
├── Scripts/                        # Project runtime code and original design brief
├── Settings/                       # URP render-pipeline assets and volume profile
├── UI/
│   ├── Font/                       # Kanit source fonts and TMP font assets
│   └── Icons/                      # Audio/speech/back icon images
├── XR/                             # OpenXR, XR Management, simulation configuration
├── XRI/                            # XR Interaction Toolkit editor/settings assets
├── ElectricalWorkshopInputActions.inputactions # Project-specific X/A/B actions
├── InputSystem_Actions.inputactions             # XRI/default input action asset
└── MasterMixer.mixer               # Mixer parameters used by AppController

Packages/                           # Package manifest and locked dependency graph
ProjectSettings/                    # Unity-wide project, build, physics, input, and XR settings
```

Keep Unity `.meta` files with every asset. Asset GUIDs, including all scene and prefab references, depend on them.

## Runtime architecture

```text
XR controller / simulator
   ├── X (left secondary) ──> ControllerInputController ──> AppController pause menu
   ├── A (right primary) ──> ControllerInputController ──> replay current speech AudioSource
   └── B (right secondary) ──> ControllerInputController ──> MotorAnimationManager

ScenarioFlowManager
   ├── Scenario 1: MotorAnimationManager -> PartAnimationController batches
   └── Scenario 2: WiringSequenceManager -> ActivationPhaseManager
          ├── ordered correct socket snaps
          └── breaker + MAR/ARR buttons -> contactor/lamps/motor state
```

The project has two active training stages, despite earlier planning documents mentioning three initial scenarios:

1. **Scenario 1 — motor reveal.** Each B press reveals a configured batch of motor parts. A subsequent press hides the current batch before revealing the next. An additional press after the final batch hides it, raises completion, and resets the reveal pieces.
2. **Scenario 2 — assembly then activation.** Required electrical parts are placed into their own sockets in a fixed order. Once every required part is snapped, the learner can operate a breaker and MAR/ARR push buttons to start or stop the motor.

`ScenarioFlowManager` initially enables its configured Scenario 1 object group and disables its Scenario 2 group. When Scenario 1 completes, it disables B-driven batch progression, swaps the groups, and starts the wiring sequence. When wiring completes, it begins the activation phase. There is no implemented Scenario 3 or terminal “training complete” screen controller yet.

## Script map: orchestration and UI

All files below are in `Assets/Scripts/`. Most gameplay component scripts use namespace `ElectricalWorkshop.Components`; UI/input scripts use `ElectricalWorkshop.UI`. `MotorAnimationManager` and `PartAnimationController` currently have no custom namespace.

| Script | Responsibility | Key public API / integration |
| --- | --- | --- |
| `ScenarioFlowManager.cs` | Top-level handoff from Scenario 1 to Scenario 2 and from wiring to activation. | Subscribes in code to `MotorAnimationManager.onScenario1Complete` and `WiringSequenceManager.onWiringPhaseComplete`. Keep those inspector event lists empty unless duplicate callbacks are intended. |
| `ControllerInputController.cs` | Enables project-specific XR actions and routes them to UI/reveal behavior. | `SetNextBatchInputEnabled(bool)` only gates B/Next; X/A remain active. Input references must be assigned in the inspector. |
| `AppController.cs` | Pause/resume, scene restart/quit, control-guide visibility, mixer muting, and speech replay. | Public methods are suitable for UI button events: `TogglePauseMenu`, `RestartScene`, `ToggleSound`, `ToggleSpeech`, etc. Pause changes `Time.timeScale`. |
| `MotorAnimationManager.cs` | Finite-state coordinator for batched motor-part reveal/hide animations. | `AdvanceBatch()`, `ResetAll()`, `isBusy`, `currentBatchIndex`, and `onScenario1Complete`. It waits for every part in a batch before continuing. |
| `PartAnimationController.cs` | Animation for one motor part: reveal moves from stored world transform to inspector offset; hide disables renderer/collider, optionally fading. | Set `activate`, `reset`, or call `Hide()`. Completion events are `onRevealComplete` / `onHideComplete`. Requires a `Renderer`. |
| `MotorAnimationController.cs` | Separate generic transform/opacity animation component. | Present in the project but not part of the Scenario 1 manager flow. Inspect before reusing; `PartAnimationController` is the actively orchestrated reveal implementation. |

### Input contract

`Assets/ElectricalWorkshopInputActions.inputactions` contains one map, **Electrical Workshop**:

| Action | Binding | Current result |
| --- | --- | --- |
| `Menu` | Left XR controller `SecondaryButton` (X) | Toggle pause menu |
| `Replay Voice` | Right XR controller `PrimaryButton` (A) | Stop and replay configured speech source |
| `Next` | Right XR controller `SecondaryButton` (B) | Advance Scenario 1 motor-reveal batch while enabled |

Do not edit `InputSystem_Actions.inputactions` for these custom commands unless a change genuinely concerns default XRI actions. In `ControllerInputController`, actions are enabled on `OnEnable`; they are unsubscribed but not explicitly disabled on `OnDisable`.

## Script map: electrical components and scenario state

| Script | Responsibility | Important behavior |
| --- | --- | --- |
| `ISnappable.cs` | Common socket-placement interface. | Deliberate public spellings are `isSnaped` and `onSnappedChanged`; retain them unless every caller and serialized script reference is migrated. |
| `ExpectedComponentSocketFilter.cs` | XRI `IXRSelectFilter` that restricts a socket to one exact `XRGrabInteractable`. | Attach it to a socket and add it to that socket’s Select Filters list. An unassigned expected reference accepts any interactable. |
| `WiringSequenceManager.cs` | Scenario 2 ordered placement state machine. | Each `WiringStep` holds an `ISnappable` component controller and socket-highlight renderers. It displays one socket highlight, advances only on a `true` snap event, and does not rewind on removal. |
| `ActivationPhaseManager.cs` | Scenario 2 motor start/stop state machine after assembly. | `BeginActivationPhase()` sets idle: contactor off, MAR lamp off, ARR lamp on, motor off. MAR starts only if breaker is on; ARR or breaker-off returns to idle. |
| `DisjonctorController.cs` | Grab/snap-capable breaker with a pressable/triggerable animated lever. | `isOn` emits `onStateChanged`; `isSnaped` emits `onSnappedChanged`. The configured switch child needs an `XRSimpleInteractable` and Collider. Both select and activate toggle the state. |
| `PushButtonController.cs` | Grab/snap-capable momentary MAR or ARR button, with press interactions and color visual. | Select/activate causes press; select-exit/deactivate releases. Emits `onPushedChanged(bool)` and snap changes. |
| `ContactorController.cs` | Grab/snap-capable contactor whose coil state is externally controlled. | No pressable child; `ActivationPhaseManager` sets `isOn`. Emits state and snap events. |
| `ThermalRelayController.cs` | Grab/snap-capable thermal relay. | Currently placement only; no trip/overload behavior. |
| `VoyantController.cs` | Grab/snap-capable indicator lamp. | External code changes `isOn`; it derives and applies display color/emission-style visual state, and emits state/snap events. |
| `MotorRunController.cs` | Visual/audio running behavior for the motor. | External code sets `isOn`; `Update` rotates configured part and audio follows the state. |

### Component/socket contract

Every currently placeable electrical component follows the same arrangement:

```text
Component root
├── XRGrabInteractable                 # Learner grabs the physical part
├── [component controller]             # Watches grab select enter/exit
└── required socket (elsewhere)
    ├── XRSocketInteractor
    └── ExpectedComponentSocketFilter  # Usually accepts only this exact grab interactable
```

The controller marks a component snapped only when the selected interactor is its assigned `_correctSocket`. The socket filter prevents an incorrect component from being inserted. Wiring sequencing adds ordering over this physical validation; it does not model wires, terminals, circuit paths, or wrong-wire feedback.

## Scene and inspector wiring

`Assets/Scenes/entry-scene.unity` contains the entire app. Important authored groups include the XR Origin/simulator infrastructure, UI canvases/menu/control guide, audio/mixer configuration, motor reveal pieces, an electrical panel with named `*-attach` socket locations, and code-host GameObjects.

The scene uses these practical naming cues:

- `ScenarioFlow-Code`, `Wiring-Code`, and `ActivarionPhase-Code` host the scenario managers. The spelling **Activarion** is in the scene name; it does not match the correct class spelling but is harmless.
- `Controller-Code` hosts `ControllerInputController`; `App-Code` hosts the app/menu controller.
- Socket/location objects are named `disj-attach`, `cont-attach`, `therm-attach`, `mar-btn-attach`, `arr-btn-attach`, `mar-voy-attach`, and `arr-voy-attach`.
- The scenario group arrays on `ScenarioFlowManager` decide which scene objects appear in each stage. Update those arrays whenever adding a stage-specific object.

When adding a new slotted part, configure all of the following in the Inspector: its grab interactable, its controller’s correct socket, the socket filter’s expected interactable, a `WiringSequenceManager.WiringStep` if it belongs in Scenario 2, and any group visibility/reveal renderer references. Do not rely on a matching object name; all runtime validation is reference-based.

The serialized `m_EditorClassIdentifier` for `AppController` still says `AppMenuController` in the scene YAML, while the source class is `AppController`. Unity resolves MonoBehaviours through the script asset GUID, but open the scene and check that this component is not missing before renaming or moving the script.

## Assets: what is authored vs. imported/sample

| Area | Role | Takeover guidance |
| --- | --- | --- |
| `Assets/Models/Meshs/` | Source FBX models and materials for the whiteboard, lamps, thermal relay, contactor, push button, motor, breakers, etc. | Prefer changes in prefabs/material overrides over editing imported FBX data unless reimport is intended. |
| `Assets/Prefabs/Models/` | Project models used as reusable prefab assets. | Treat as source components. Preserve embedded/script references and `.meta` files. |
| `Assets/Prefabs/Models/not-used/` | Unused legacy breaker prefabs. | Do not treat as runtime assets without deliberately adopting them. |
| `Assets/Models/Environment-old/` | Superseded environment asset. | Reference only; current environment lives under `Environment/`. |
| `Assets/UI/`, `Assets/Prefabs/UI/` | Fonts, sprite icons, and UI prefab building blocks. | UI layout itself is mostly scene-authored rather than a dedicated UI code system. |
| `Assets/Audio/`, `MasterMixer.mixer` | Sound/speech media and mixer configuration. | `AppController` expects exposed mixer parameters `SFXVolume` and `SpeechVolume`; do not rename them without updating its inspector fields/code. |
| `Assets/XR/`, `Assets/XRI/` | Project XR/OpenXR/simulation settings. | Change only when altering platform or interaction configuration. |
| `Assets/Samples/`, `Assets/TextMesh Pro/`, `Assets/TutorialInfo/` | Imported Unity/XRI/TMP sample and support assets. | Avoid editing these as product code; update/reimport through Unity packages when possible. |

## Package and project configuration

Key direct packages declared in `Packages/manifest.json` include:

- URP `17.3.0`
- Input System `1.20.0`
- XR Interaction Toolkit `3.3.2`
- OpenXR `1.16.1`
- XR Management `4.6.1`
- AR Foundation `6.3.5`
- XR Hands `1.9.0`
- AI Navigation `2.0.14`
- Unity Test Framework `1.6.0`

The build list contains only `Assets/Scenes/entry-scene.unity`. The project has OpenXR and simulation settings under `Assets/XR/`; use the XR Device Simulator for desktop development/testing when hardware is unavailable.

## Safe change workflow for an AI/developer

1. Read this document, then the relevant script and its scene/prefab references.
2. Open `entry-scene.unity` in Unity before changing serialized scene configuration. Use Inspector assignments rather than manually editing YAML.
3. Extend the narrowest responsible system. For example, add activation rules in `ActivationPhaseManager`, not in a button controller; add placement ordering in `WiringSequenceManager`, not in `ExpectedComponentSocketFilter`.
4. Preserve existing event ownership. `ScenarioFlowManager` performs its subscriptions in code, so adding the same events in the Inspector duplicates transitions.
5. Maintain the project’s component naming, namespaces, and public serialized field types unless deliberately migrating all references. In particular, `isSnaped` is a compatibility spelling used across `ISnappable` and all component scripts.
6. For a script change, test compilation in Unity, enter Play Mode, exercise both the XR simulator and the affected scenario path, and check the Console for missing-reference warnings.
7. For a prefab/model change, test grab, socket acceptance, correct snap state, and any ordered wiring step in the scene.
8. Update this document when changing the build scene, package versions, input contract, scenario flow, serialized setup conventions, or adding a material subsystem.

## Known boundaries and likely next work

- Scenario 2 simulates logical component placement and motor activation; it is **not** a terminal-and-wire electrical circuit simulator.
- The thermal relay has no overload/trip state yet.
- There is no implemented multimeter, error-feedback system, scenario-completion UI, persistence/save system, or formal test suite.
- `MotorAnimationController` coexists with the Scenario 1 `PartAnimationController` system. Confirm which animation path a target object uses before modifying it.
- Null checks make several manager references optional in code, but missing inspector assignments can quietly omit expected behavior. Validate references in Unity rather than assuming code guarantees setup.
- Many user-facing content and object lists live in the scene Inspector, not in ScriptableObject data assets. A future data-driven scenario layer would be a deliberate architectural addition, not a drop-in refactor.

## Useful inspection commands

From the repository root in PowerShell:

```powershell
# List authored scripts
Get-ChildItem Assets\Scripts -Filter *.cs | Sort-Object Name

# Find references to a class in authored code
rg -n "ActivationPhaseManager|WiringSequenceManager" Assets\Scripts

# Check build scene and configured input/XR assets
Get-Content ProjectSettings\EditorBuildSettings.asset
Get-Content Assets\ElectricalWorkshopInputActions.inputactions

# Check uncommitted work before editing
git status --short
```

## Before declaring a task done

- The project opens in Unity `6000.3.24f1` without compile errors.
- `entry-scene` remains in the build list (unless the task intentionally changes it).
- Existing Scenario 1 → Scenario 2 handoff still happens exactly once.
- Required electrical parts cannot be inserted into the wrong configured socket.
- Scenario 2 still starts activation only after all configured wiring steps are snapped.
- X/A/B behavior still matches the input contract, unless intentionally changed and documented.


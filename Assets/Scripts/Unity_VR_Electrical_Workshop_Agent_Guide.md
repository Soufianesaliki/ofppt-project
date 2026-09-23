# Unity VR Electrical Workshop --- Agent Project Guide

## 1. Purpose of This File

This file is the **living project specification and agent guide** for
the Unity VR Electrical Workshop project.

The AI coding agent working in this repository should:

1.  Read this file before making project changes.
2.  Follow the architecture and decisions documented here.
3.  Prefer simple, modular implementations.
4.  Avoid introducing frameworks, abstractions, or packages unless they
    solve a demonstrated project need.
5.  Update this file after meaningful architectural, input, interaction,
    electrical-logic, scenario, or tooling changes.
6.  Never silently change an established decision. Record the change in
    the Changelog and Decisions sections.
7.  Keep this file synchronized with the actual project whenever
    possible.

This document is intentionally written as a **project guide**, not as a
generic Unity tutorial.

------------------------------------------------------------------------

# 2. Project Overview

## 2.1 Application

The project is a **VR electrical training/workshop application**.

The application presents a virtual electrical workshop containing an
electrical control panel and a three-phase asynchronous motor
installation.

The trainee should be able to:

-   Inspect electrical components.
-   Identify components and understand their function.
-   Pick up and move components where appropriate.
-   Connect electrical wires.
-   Open/close interactive electrical elements.
-   Build simple control and power circuits.
-   Start and stop a motor.
-   Observe the result of correct wiring.
-   Deliberately create wiring mistakes.
-   Receive understandable error feedback.
-   Measure electrical quantities using a virtual multimeter.
-   Follow a pedagogical step-by-step procedure.
-   Optionally complete an evaluation at the end.

The application is intended primarily as a **training/learning
experience**, so interactions should be clear and forgiving rather than
unnecessarily complex.

------------------------------------------------------------------------

# 3. Current Technology Stack

Do not change these versions unless there is a concrete compatibility or
project requirement.

  Technology                Current Version / Choice
  ------------------------- ---------------------------------------
  Unity                     **6000.3.24f1 --- Unity 6.3 LTS**
  XR Plugin Management      **4.6.1**
  XR Interaction Toolkit    **3.3.2**
  XR Core Utilities         **2.6.0**
  OpenXR Plugin             **1.16.1**
  XR Hands                  **1.9.0**
  XR Legacy Input Helpers   **2.1.13**
  Input System              **1.20.0**
  Input approach            **New Input System**
  3D modeling               **Blender 4.5.8**
  XR Origin                 XRI Starter Assets XR Origin / XR Rig
  Desktop testing           **XR Device Simulator**

### Version policy

Do not upgrade or downgrade packages merely because a newer version
exists.

Before changing versions:

1.  Identify an actual problem.
2.  Verify that the problem is version-related.
3.  Check compatibility with the current Unity version.
4.  Explain the expected benefit and risk.
5.  Record the change in this file.

------------------------------------------------------------------------

# 4. XR Input Semantics

The project uses XR Interaction Toolkit conventions for interaction.

## 4.1 User Input Mapping

  -----------------------------------------------------------------------
  User concept            Physical input          Purpose
  ----------------------- ----------------------- -----------------------
  Menu                    Left controller **X**   Open/use application
                          button                  menu

  Select / Interact       Controller **Trigger**  Select UI and interact
                                                  with electrical
                                                  components

  Grab / Snap             Controller **Grip**     Grab and/or snap
                                                  electrical components

  Replay Voice            Right controller **A**  Replay the current
                          button                  voice instruction
  -----------------------------------------------------------------------

Important:

-   **Trigger = Select / Interact**
-   **Grip = Grab / Snap**
-   Snapping is an interaction behavior, not a separate physical input
    action.
-   Do not create unnecessary duplicate input actions for Select and
    Interact.

## 4.2 Custom Input Action Asset

A separate custom Input Action Asset is used instead of modifying XRI's
default input asset.

Current custom actions:

``` text
ElectricalWorkshopInputActions
└── Action Map: Electrical Workshop
    ├── Menu
    └── Replay Voice
```

Bindings:

``` text
Menu
└── <XRController>{LeftHand}/{SecondaryButton}

Replay Voice
└── <XRController>{RightHand}/{PrimaryButton}
```

The XRI Default Input Actions should remain as untouched as practical.

------------------------------------------------------------------------

# 5. XR Device Simulator

The project uses the **XR Device Simulator** for desktop
development/testing.

The simulator is only a development/testing aid. Its controls should not
be confused with the application's real VR interaction design.

The desired simulator workflow is:

``` text
Focus Head
    -> mouse movement rotates Head

Focus Left Controller
    -> mouse movement rotates Left Controller

Focus Right Controller
    -> mouse movement rotates Right Controller
```

The project owner prefers the older/simple mouse-look behavior where
possible, without requiring the right mouse button to be held
continuously.

This is a **testing convenience**, not an application requirement.

Do not change the project's runtime VR input architecture merely to
reproduce a simulator convenience.

------------------------------------------------------------------------

# 6. Main Functional Requirements

## 6.1 Workshop

The scene contains a virtual electrical workshop with:

-   Electrical panel
-   Three-phase asynchronous motor
-   Circuit breaker
-   Contactor
-   Thermal overload relay
-   Push buttons
-   Indicator lamps
-   Wiring
-   Virtual multimeter

The exact visual assets can evolve independently from the code
architecture.

## 6.2 Electrical Training

The trainee should learn:

-   What each component is.
-   What each component does.
-   How components are connected.
-   How a simple control circuit operates.
-   How a simple three-phase motor power circuit operates.
-   How incorrect wiring affects the circuit.

------------------------------------------------------------------------

# 7. Scenarios

There are currently three planned scenarios.

All three should remain **simple/easy difficulty** and contain **no more
than 6 main steps**.

------------------------------------------------------------------------

## Scenario 1 --- Identify and Manipulate Components

### Objective

Teach the trainee to recognize basic electrical components and interact
with them.

### Planned steps

1.  Enter the workshop and locate the electrical panel.
2.  Identify the breaker, contactor, thermal relay, push buttons, lamps,
    and motor.
3.  Pick up/move a component.
4.  Interact with a component to display its name and function.
5.  Open/close an interactive element.
6.  Complete a small identification test.

### Main systems involved

-   Component information
-   Component interaction
-   Grab/move/snap
-   Basic UI
-   Scenario progression
-   Optional voice guidance

------------------------------------------------------------------------

# 8. Scenario 2 --- Simple Control Circuit

### Objective

Build and test a simple contactor control circuit.

### Components

-   STOP push button --- normally closed
-   START push button --- normally open
-   Thermal relay auxiliary contact
-   Contactor coil
-   Contactor auxiliary self-holding contact
-   Green running lamp
-   Red stopped lamp
-   Control supply L/N

### Planned steps

1.  Identify START, STOP, contactor, and indicator components.
2.  Connect wires according to the simple control diagram.
3.  Complete the control circuit.
4.  Press START and observe contactor activation.
5.  Press STOP and observe deactivation.
6.  Create a deliberate bad connection and receive error feedback.

------------------------------------------------------------------------

# 9. Scenario 3 --- Power Circuit / Motor

### Objective

Build and test a simple direct-on-line three-phase motor power circuit.

### Components

-   Three-pole circuit breaker
-   Three-pole contactor
-   Three-phase thermal overload relay
-   Three-phase asynchronous motor
-   Three-phase supply

### Planned steps

1.  Identify breaker, contactor, thermal relay, and motor.
2.  Connect the power circuit.
3.  Perform a visual/checking step.
4.  Use the virtual multimeter to measure voltage.
5.  Power the installation and start the motor.
6.  Observe motor operation and current.

The initial motor circuit should remain a **simple DOL (Direct-On-Line)
circuit**.

Do not introduce star/delta starting unless it becomes a later explicit
requirement.

------------------------------------------------------------------------

# 10. Electrical Schematics

## 10.1 Control Circuit

Current conceptual control circuit:

``` text
L
 |
S0 STOP NC
 |
F2 Thermal Relay 95-96 NC
 |
 +-------- S1 START NO --------+
 |                              |
 +-------- KM1 13-14 NO --------+
                                |
                              KM1 A1
                              KM1 A2
                                |
                                N
```

The START button and KM1 auxiliary contact form the self-holding branch.

### Indicator lamps

Green running lamp:

``` text
L -> KM1 NO -> H1 Green -> N
```

Red stopped lamp:

``` text
L -> KM1 NC -> H2 Red -> N
```

The exact lamp implementation can be simplified if necessary, but the
logical states should remain understandable.

------------------------------------------------------------------------

## 10.2 Power Circuit

``` text
L1 ─┐
L2 ─┼─> Q1 Breaker ─> KM1 Contactor ─> F1 Thermal Relay ─> M1 Motor
L3 ─┘
```

Current terminal convention:

``` text
KM1
1/L1
3/L2
5/L3

2/T1
4/T2
6/T3
```

Motor terminals may use:

``` text
U1
V1
W1
```

No star/delta connection is currently required.

------------------------------------------------------------------------

# 11. Proposed Code Architecture

The architecture should remain intentionally small.

Do not create a large manager class that controls everything.

The initial architecture is:

``` text
Assets/Scripts/
│
├── Core/
│   ├── WorkshopManager.cs
│   └── ScenarioManager.cs
│
├── Interaction/
│   ├── ComponentInteractable.cs
│   ├── GrabbableComponent.cs
│   └── WireConnector.cs
│
├── Electrical/
│   ├── ElectricalComponent.cs
│   ├── ElectricalTerminal.cs
│   ├── ElectricalConnection.cs
│   └── CircuitManager.cs
│
├── UI/
│   ├── ComponentInfoUI.cs
│   ├── ErrorMessageUI.cs
│   └── EvaluationUI.cs
│
└── Audio/
    └── VoiceManager.cs
```

This is a **planned structure**, not a requirement that all files must
immediately exist.

Only create a script when the project actually needs it.

------------------------------------------------------------------------

# 12. Core Design Principle

Each script should have **one clear responsibility**.

Avoid:

``` text
WorkshopManager
    ├── VR input
    ├── grabbing
    ├── wires
    ├── motor physics
    ├── UI
    ├── audio
    ├── scenarios
    └── evaluation
```

Prefer:

``` text
XR Interaction Toolkit
        |
        v
ComponentInteractable
        |
        v
ElectricalComponent
        |
        v
Electrical / Circuit systems
```

And independently:

``` text
ScenarioManager
        |
        +--> checks progress
        +--> advances steps
        +--> requests feedback
```

The scenario system should orchestrate the training experience rather
than contain all electrical behavior.

------------------------------------------------------------------------

# 13. First Code Milestone

The first implementation milestone should be deliberately small.

Create and test only:

``` text
ElectricalComponent.cs
ComponentInteractable.cs
ComponentInfoUI.cs
```

Use **one KM1 contactor** as the first test object.

The first milestone should prove that:

``` text
Player points at KM1
        |
        v
Trigger
        |
        v
Component interaction
        |
        v
Component information appears
```

And:

``` text
Grip
  |
  v
KM1 can be grabbed/moved according to XRI interaction setup
```

Do not implement the entire electrical circuit before this milestone
works.

------------------------------------------------------------------------

# 14. Electrical Component Model

The base `ElectricalComponent` should initially contain only information
that is genuinely common to electrical components.

Conceptually:

``` text
ElectricalComponent
├── Component ID
├── Component Name
├── Component Type
└── Description
```

Example:

``` text
ID: KM1
Name: Contactor
Type: Contactor
Description: Controls the motor power circuit.
```

Avoid adding electrical simulation fields prematurely.

Electrical state, terminals, and connections should be introduced when
the electrical system is implemented.

------------------------------------------------------------------------

# 15. Component Types

A component type system may eventually include:

``` text
CircuitBreaker
Contactor
ThermalRelay
PushButton
IndicatorLamp
Motor
Multimeter
Other
```

Use an enum or similarly simple representation unless there is a
demonstrated reason to use a more complex type system.

------------------------------------------------------------------------

# 16. Interaction Architecture

The interaction layer should be independent from electrical simulation
as much as practical.

Example:

``` text
XR Interaction Toolkit
        |
        v
ComponentInteractable
        |
        v
ElectricalComponent
```

The component interaction should be responsible for things such as:

-   Detecting interaction.
-   Triggering component information.
-   Coordinating with XRI interactables.
-   Supporting grab/move behavior where appropriate.

It should not directly calculate the complete electrical circuit.

------------------------------------------------------------------------

# 17. Electrical Architecture

Electrical connections should eventually be represented using terminals.

Conceptual structure:

``` text
ElectricalComponent
        |
        +-- ElectricalTerminal
        +-- ElectricalTerminal
        +-- ElectricalTerminal
```

A wire connects two terminals:

``` text
Wire
├── Terminal A
└── Terminal B
```

This is preferable to directly storing arbitrary
GameObject-to-GameObject connections.

It allows the system to determine:

-   Correct connection.
-   Incorrect connection.
-   Missing connection.
-   Whether a circuit is complete.

------------------------------------------------------------------------

# 18. Circuit Manager

`CircuitManager` should eventually be responsible for evaluating
electrical connectivity and state.

It should not be responsible for:

-   VR controller input.
-   UI layout.
-   Voice playback.
-   Scenario text.
-   General scene management.

Possible future responsibilities:

``` text
CircuitManager
├── Evaluate connections
├── Determine circuit state
├── Determine whether required paths are complete
├── Detect invalid connections
└── Notify relevant systems of electrical state changes
```

Do not implement a full electrical simulator unless required.

For the initial training scenarios, a **logical circuit-state model**
may be sufficient.

------------------------------------------------------------------------

# 19. Scenario Architecture

`ScenarioManager` should control training progression.

Conceptually:

``` text
ScenarioManager
    |
    +-- Current Scenario
    +-- Current Step
    +-- Step completion
    +-- Next step
    +-- Scenario completion
```

A scenario step should be able to ask for conditions such as:

``` text
Component identified
Component moved
Wire connected
Circuit completed
Button pressed
Motor started
Measurement performed
Error produced
```

The scenario system should not need to know the internal implementation
of every component.

------------------------------------------------------------------------

# 20. Error Feedback

The application intentionally allows trainees to make mistakes.

Errors should be:

-   Clear.
-   Specific.
-   Educational.
-   Non-destructive where possible.

Example:

``` text
Incorrect connection:
KM1 terminal 3/L2 should connect to the second phase supply path.
```

Avoid vague messages such as:

``` text
ERROR
```

The exact feedback system can evolve later.

------------------------------------------------------------------------

# 21. UI Architecture

UI should remain separate from electrical logic.

For example:

``` text
ElectricalComponent
       |
       | information
       v
ComponentInfoUI
```

And:

``` text
CircuitManager
       |
       | error/event
       v
ErrorMessageUI
```

Do not make electrical components directly manipulate complex UI
hierarchies if an event/callback can keep the systems separated.

Keep the initial implementation simple.

------------------------------------------------------------------------

# 22. Voice System

Voice is optional support for the training flow.

`VoiceManager` should eventually handle:

-   Playing the current instruction.
-   Replaying the current instruction.
-   Stopping/replacing current audio.
-   Possibly playing error/help instructions.

The **A button** is reserved for replaying the current voice
instruction.

Voice should not be required for core electrical functionality.

------------------------------------------------------------------------

# 23. Multimeter

The virtual multimeter is a later system.

Initial required capabilities:

-   Select measurement mode.
-   Touch/place probes on measurement points.
-   Measure voltage.
-   Eventually measure current.
-   Display the result.

Do not build a physically accurate electrical simulation unless
required.

A controlled training-oriented model is acceptable if it correctly
represents the expected circuit behavior.

------------------------------------------------------------------------

# 24. Motor Behavior

The motor should eventually have clear states such as:

``` text
OFF
READY / POWER AVAILABLE
RUNNING
FAULT / OVERLOAD
```

The motor should respond to the logical electrical state rather than
directly to a controller button.

For example:

``` text
START button
      |
      v
Control circuit
      |
      v
KM1 activated
      |
      v
Power circuit closed
      |
      v
Motor RUNNING
```

This separation is important.

------------------------------------------------------------------------

# 25. Development Order

Follow this order unless there is a concrete reason to change it.

## Phase 1 --- Foundation

``` text
1. ElectricalComponent
2. ComponentType
3. ComponentInteractable
```

## Phase 2 --- First VR component

``` text
4. KM1 Contactor
5. Component information UI
6. Grab/move/snap behavior
```

## Phase 3 --- Electrical system

``` text
7. ElectricalTerminal
8. ElectricalConnection
9. Wire
10. CircuitManager
```

## Phase 4 --- Motor/control logic

``` text
11. Breaker
12. Contactor
13. Thermal relay
14. Start/Stop
15. Motor
16. Indicator lamps
```

## Phase 5 --- Scenarios

``` text
17. ScenarioManager
18. Scenario steps
19. Error feedback
20. Evaluation
```

## Phase 6 --- Supporting features

``` text
21. VoiceManager
22. Multimeter
23. Animations
24. Sound effects
25. Final UI/polish
```

------------------------------------------------------------------------

# 26. Coding Rules for the Agent

## Keep things simple

Prefer:

``` text
One script
One responsibility
Clear references
Simple events
Inspector-configurable values
```

Avoid unnecessary:

-   Dependency injection frameworks.
-   Service locators.
-   Global singleton managers everywhere.
-   Generic event buses.
-   Large inheritance hierarchies.
-   Custom ECS-style architecture.
-   Premature optimization.

## Do not over-engineer

If a feature can be implemented cleanly with a small MonoBehaviour and a
few serialized fields, do that first.

Introduce abstractions only when there is a real repeated pattern.

## Prefer Unity/XRI components

Use existing Unity and XR Interaction Toolkit functionality where it
already solves the problem.

Do not recreate:

-   Grab interactions.
-   Select interactions.
-   XR controller input.
-   XR UI interaction.
-   Locomotion.

unless the project has a specific requirement that XRI cannot satisfy.

------------------------------------------------------------------------

# 27. Inspector-Friendly Design

Because this is a Unity training project, important configuration should
preferably be visible in the Inspector.

Use `[SerializeField]` for values that designers/training developers may
need to configure.

Examples:

``` text
Component Name
Component Description
Component Type
Terminal references
Expected connections
Scenario step requirements
Audio clips
UI references
```

Do not expose internal implementation details unnecessarily.

------------------------------------------------------------------------

# 28. Events and Communication

Prefer small, explicit events/callbacks between systems when useful.

Example:

``` text
ComponentInteractable
    -> OnInteracted

CircuitManager
    -> OnCircuitCompleted
    -> OnIncorrectConnection
    -> OnCircuitStateChanged

ScenarioManager
    -> OnStepCompleted
    -> OnScenarioCompleted
```

Do not create a universal event system unless the project actually needs
one.

------------------------------------------------------------------------

# 29. Naming Conventions

Use clear names.

Examples:

``` text
ElectricalComponent
ElectricalTerminal
ElectricalConnection
ComponentInteractable
ComponentInfoUI
CircuitManager
ScenarioManager
VoiceManager
```

Electrical identifiers should follow the schematic where practical:

``` text
Q1 = breaker
KM1 = contactor
F1 = thermal relay
S0 = STOP
S1 = START
H1 = green lamp
H2 = red lamp
M1 = motor
```

------------------------------------------------------------------------

# 30. Scene / Prefab Philosophy

Electrical components should eventually be reusable prefabs.

For example:

``` text
Prefabs/
├── Electrical/
│   ├── Q1_Breaker.prefab
│   ├── KM1_Contactor.prefab
│   ├── F1_ThermalRelay.prefab
│   ├── S0_Stop.prefab
│   ├── S1_Start.prefab
│   ├── H1_GreenLamp.prefab
│   ├── H2_RedLamp.prefab
│   └── M1_Motor.prefab
```

The exact folder structure can change if the existing project already
follows another convention.

Do not reorganize the whole project unnecessarily.

------------------------------------------------------------------------

# 31. Testing Strategy

Each major system should be testable independently.

## Component interaction test

Verify:

``` text
Select component
-> information appears
```

## Grab test

Verify:

``` text
Grip component
-> component moves correctly
-> release behaves correctly
```

## Connection test

Verify:

``` text
Connect terminal A to terminal B
-> connection is recorded
```

## Error test

Verify:

``` text
Incorrect terminal
-> error is detected
-> useful feedback is shown
```

## Circuit test

Verify:

``` text
Correct wiring
-> circuit becomes complete
```

## Motor test

Verify:

``` text
Correct control + power circuit
-> motor runs
```

Test one system at a time before combining everything.

------------------------------------------------------------------------

# 32. Important Architectural Separation

The following separation should be preserved:

``` text
INPUT / XR
     |
     v
INTERACTION
     |
     v
COMPONENT
     |
     v
ELECTRICAL LOGIC
     |
     v
SCENARIO
     |
     v
UI / AUDIO FEEDBACK
```

Example:

``` text
Trigger
  ↓
XRI Select
  ↓
ComponentInteractable
  ↓
ElectricalComponent
  ↓
ScenarioManager
  ↓
ComponentInfoUI
```

For an electrical event:

``` text
Wire connected
  ↓
ElectricalConnection
  ↓
CircuitManager
  ↓
ScenarioManager
  ↓
ErrorMessageUI / progression
```

This is the core architectural idea of the project.

------------------------------------------------------------------------

# 33. Current Project Status

## Completed / decided

-   Unity version selected: **6000.3.24f1 / Unity 6.3 LTS**
-   XR package versions selected as documented above.
-   XR Origin obtained from XRI Starter Assets.
-   XR Device Simulator is being used for Windows development/testing.
-   Custom Input Action Asset created.
-   `Menu` action configured.
-   `Replay Voice` action configured.
-   Input semantics established:
    -   Trigger = Select/Interact
    -   Grip = Grab/Snap
    -   X = Menu
    -   A = Replay Voice
-   Three initial training scenarios defined.
-   Control circuit concept defined.
-   Power circuit concept defined.
-   Simple modular code architecture chosen.

## Current coding stage

The project is at the **architecture / initial implementation stage**.

The next coding milestone is:

``` text
ElectricalComponent
ComponentInteractable
ComponentInfoUI
```

using **KM1 contactor** as the first complete test component.

------------------------------------------------------------------------

# 34. Current Decisions

  Decision                                              Status
  ----------------------------------------------------- --------
  Keep architecture simple                              Active
  Keep systems modular                                  Active
  Avoid giant manager scripts                           Active
  Use XRI for VR interaction                            Active
  Use New Input System                                  Active
  Keep XRI default input asset mostly untouched         Active
  Use custom input asset for project-specific actions   Active
  Trigger = Select/Interact                             Active
  Grip = Grab/Snap                                      Active
  X = Menu                                              Active
  A = Replay Voice                                      Active
  DOL motor circuit initially                           Active
  No star/delta initially                               Active
  Maximum 6 steps per scenario                          Active
  Three simple scenarios                                Active
  Build one component completely before scaling         Active

------------------------------------------------------------------------

# 35. Agent Change Procedure

Before modifying the project:

1.  Read this file.
2.  Determine which system the requested change belongs to.
3.  Check whether an existing system already provides the needed
    functionality.
4.  Prefer modifying/extending the smallest appropriate system.
5.  Avoid unrelated refactoring.
6.  Preserve current input semantics and package versions unless
    explicitly instructed otherwise.
7.  Test the change if possible.
8.  Update this file if the change affects:
    -   Architecture
    -   Inputs
    -   Scenarios
    -   Electrical logic
    -   Package versions
    -   Important project decisions
    -   Current implementation status

After a meaningful change:

1.  Update **Current Project Status**.
2.  Update **Current Decisions** if necessary.
3.  Add a concise entry to the **Changelog**.
4.  If the architecture changed, update the relevant architecture
    sections.
5.  Do not rewrite unrelated sections.

------------------------------------------------------------------------

# 36. Changelog

## 2026-09-23

### Project architecture established

-   Defined the VR electrical workshop purpose and requirements.
-   Defined three initial training scenarios.
-   Defined control and power circuit concepts.
-   Established a simple modular architecture.
-   Established the initial script categories:
    -   Core
    -   Interaction
    -   Electrical
    -   UI
    -   Audio
-   Decided to begin implementation with:
    -   `ElectricalComponent`
    -   `ComponentInteractable`
    -   `ComponentInfoUI`
-   Selected KM1 contactor as the first complete test component.
-   Established the principle that electrical logic should remain
    separate from VR interaction.
-   Established that `CircuitManager` should handle circuit logic rather
    than VR/UI responsibilities.
-   Established that `ScenarioManager` should control training
    progression rather than implement electrical behavior.
-   Documented current Unity/XR package versions.
-   Documented custom project input semantics.

------------------------------------------------------------------------

# 37. Future Changes

When a new requirement is introduced, do not immediately redesign the
architecture.

First ask:

1.  Can the requirement fit into an existing system?
2.  Is a new script actually necessary?
3.  Does it introduce a reusable concept?
4.  Does it affect existing scenario behavior?
5.  Does it require a new electrical state?
6.  Does it require a new input action?

Only introduce new architecture when the answer requires it.

------------------------------------------------------------------------

# 38. Agent Instruction Summary

When working on this project:

> Build the smallest clean solution that satisfies the current
> requirement.

> Keep VR interaction, electrical logic, scenario progression, UI, and
> audio separated.

> Reuse Unity/XRI functionality instead of recreating it.

> Do not over-engineer.

> Do not change package versions without a concrete reason.

> Do not modify XRI default input actions when a project-specific input
> action can solve the problem.

> Implement and test one small feature at a time.

> Keep the project understandable to a developer who did not create it.

> Update this document whenever an important project decision or
> implementation state changes.

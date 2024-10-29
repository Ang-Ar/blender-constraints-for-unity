# blender-constraints-for-unity
Version 0.2 (WIP)

Adds a number of custom components that mimic some of Blender's object & bone constraints, or limited versions therof.

WARNING: this was an ad-hoc solution for a personal project. Functionality is very limited to my specific use cases and not rigidly tested. Code is presented as-is without guarantees etc.

## Installation
[TODO]

## Usage
### Quick start
1. Apply the constraint you need on any appropriate object (see below)
2. Select the constrained object and the target object.
3. Make sure all objects & armatures involved are in their rest pose.
4. Press the ``save constrained rest pose`` and ``save target rest pose`` buttons in the constraint's inspector.
5. Configure the rest of the constrain's settings
6. You might not be seeing any results yet in edit mode. See below for details.

### Details
The new constraint components come in two variants: one for use with Unity's Animation Rigging, and one that works out-of-the-box.

If you want to use the Animation Rigging system, check out the documentation here: https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.0/manual/index.html. The new constraints should be added to the Rig just like any built-in Animation Rigging constraint. To visualise the constraint, you'll need to play an animation or timeline in edit mode or enter play mode.

If you are encountering unexpected behavior using the Animation Rigging constraints or don't want to use them for some other reason, use the variants ending in ``Simple`` instead. These can be added to any object in the scene. You can select whether to apply the constraints in ``Update()`` or ``FixedUpdate()``.There is also a toggle for applying the constraint in edit mode.

The constraints' inspector contains a button to easily convert a component between both variants while maintaining settings. Of course the Animation Rigging constraints won't have any effect unless they are in a proper hierachy (see documentation above).

While most of the constraint components' options can be changed at run-time, the constrained- & target object as well as their rest poses cannot.

## Supported constraints
### Copy Rotation (``BlenderCopyRotation`` / ``BlenderCopyRotationSimple``)
- :white_check_mark: Target
- :white_check_mark: Order
- :white_check_mark: Axis
- :x: Invert (all axes are non-inverted)
- :x: Mix (Replace only)
- :x: Target (local only)
- :x: Owner (local only)
- :white_check_mark: Influence

### Damped Track (``BlenderDampedTrack`` / ``BlenderDampedTrackSimple``)
- :white_check_mark: Target
- :white_check_mark: Track Axis
- :white_check_mark: Influence

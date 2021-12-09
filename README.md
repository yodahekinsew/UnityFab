# UnityFab
My final project for 6.839 (Advanced Computer Graphics) - A DSL for rigid-body systems built using the Unity Engine.

## How To Use
1. You can access UnityFab by going to the [website](https://www.yodahe.com/UnityFab/). 
2. You can also clone the website branch of this repo to run the website locally.
3. You can finally clone the main branch of this repo and open the project using Unity Hub in order to access the project locally.

## Code Syntax
The syntax for UnityFab is pretty simple, there are just a few concepts you need to know.
### Rigid-bodies
Rigidbodies make up the primitives of UnityFab, and are the functions you can use to spawn in rigidbodies. There are a couple different rigidbodies that are supported.
#### Rigid-body Parameters
    Cube(
      position = vec3,                // Sets the position of the rigidbody's transform
      rotation = vec3,                // Sets the rotation of the rigidbody's transform
      size | scale = float | vec3,    // Sets the scale of the rigidbody's transform
      normal = vec3,                  // Sets the normal of the rigidbody's transform
      mass = float,                   // Sets the mass of the rigidbody
      bounciness = float,             // Sets how much energy the rigidbody keeps when bouncing off of objects
      color = vec3,                   // Sets the color of the rigidbody's mesh
      locked = bool                   // Locks the rigidbody's position and rotation, so they will not be affected by the scene
    );
#### Supported Rigid-bodies Functions
    Cube(...);
    Sphere(...);
    Pyramid(...);
    Cylinder(...);
    Cone(...);
    Wedge(...);
    Plane(...);                       // Note: Planes are locked by default

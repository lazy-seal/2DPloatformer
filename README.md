# Chubby Bird
Size-Changing Platformer

Todo:
A progress bar that shows how close the bird is to next size change.

Different physics (mass, gravity, jumping power, etc) for different sizes.

Making the sprite to vector so the character renders nicely.

Modifying  the control (coyote jump, air floating time, etc)

An algorithm to check if the change of size makes the character stuck between walls - either makes the character die, or doesn't allow transformation.
For above algorithm, when getting bigger, cast box collider of intentional size and move the collider up (or the opposite direction of the wall), until the lower ground doesn't reach. If the collider finds a position where it is not collided with walls, bird transform. Otherwize, decide what you want to do.

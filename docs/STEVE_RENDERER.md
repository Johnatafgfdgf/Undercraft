# Steve 2D renderer

Undercraft now uses **minecraft-library/asset-renderer** as the authoritative
source for the player presentation pipeline.

Pinned upstream commit:

`844f2cae75844ea56494e0b28f294d4797f0cf45`

## Why this renderer

The previous FIX6-FIX9 player was assembled manually from cropped skin parts and
hand-authored movement. It was useful for validating the runtime, but the silhouette
and animation timing did not match Minecraft closely enough.

The selected renderer supports:

- `PlayerRenderer`;
- full-body player output;
- the vanilla wide-arm Steve texture id;
- `PlayerOptions.Dimension.TWO_D` for a flat 2D composite;
- equipment/armor layering.

Undercraft remains a completely 2D runtime. No live 3D model is added to Undertale.

## Runtime sprite set

The build pipeline converts renderer output into GameMaker sprite frames:

- idle: front/back/left/right;
- walk: front/back/left/right;
- jump;
- fall;
- use-item;
- mining/attack;
- eating;
- hurt.

Items held in the hand remain a separate 2D overlay so the same player animation can
work with every tool/block/item.

## Animation rule

Do not redraw Steve procedurally in GML. GML only selects frames, positions the held
item, draws the shadow and advances animation state.

The old procedural/cropped Steve is no longer considered a production fallback.

## Fidelity

Animation timing should be derived from Minecraft movement/pose behavior where
possible instead of eyeballing rotations. The generated source frames can be reduced
to Undertale-sized pixel art with nearest-neighbour sampling while preserving the
Minecraft proportions.

## Asset policy

Minecraft assets are generated locally and are not committed to the public Undercraft
repository. The pipeline stores only scripts/configuration and expects the builder to
supply or resolve lawful Minecraft assets locally.

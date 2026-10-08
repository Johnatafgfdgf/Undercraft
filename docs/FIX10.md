# FIX10 - Steve 2D overhaul

FIX10 replaces the earlier hand-drawn/procedural player presentation.

## Player presentation

- vanilla wide-arm Steve skin as the source;
- four 2D directions: front, back, left, right;
- eight walk frames per direction;
- dedicated idle, jump and use-item poses;
- arms and legs alternate with a Minecraft-style sinusoidal gait;
- held item remains a separate runtime layer;
- soft oval shadow remains under the player.

The external `minecraft-library/asset-renderer` pipeline remains the preferred
reference renderer for validating proportions and skin output. GitHub Actions cannot
be triggered automatically by the connected GitHub app in this environment, so the
test build itself is generated locally from the same vanilla skin basis rather than
claiming an asset-renderer run that did not happen.

Final test build SHA-256:
`f81707d63339f5c8570b10ab2069bfd6a6addb0a7e908bf13a1262f6800dae79`

The final data file was reopened/decompiled with UndertaleModTool and its 22 chunks
were validated to exact EOF.

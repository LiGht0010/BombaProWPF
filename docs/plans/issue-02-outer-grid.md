# Issue #2 — Double Grid frame around the content area

## What user reported
Two visible nested rectangles wrap the content/right side of the shell. The outer one feels like a redundant frame around the whole work area.

## Diagnosis
**Where it lives:** `BombaProMaxWPF/MainWindow.xaml`, lines 35–246.

The shell body is laid out as:

```
<Grid Grid.Row="1" Margin="16,8,16,16">      ← outer wrapper (line 35)
    <ContentControl Style="NeuSidebarStyle" />   ← raised card (twin shadow, CornerRadius=24)
    <Grid Grid.Column="1">                       ← inner content grid (line 185)
        <ContentControl Style="NeuToolbarStyle" />
        <Frame x:Name="ContentFrame" />
    </Grid>
</Grid>
```

The "double rectangle" effect comes from two overlapping visual cues:
1. The `NeuSidebarStyle` raised card emits a large drop-shadow halo (BlurRadius=28, ShadowDepth=10) that bleeds into the right pane and reads as a soft outer rectangle around the whole content area.
2. The outer `Grid` (line 35) plus the inner content `Grid` (line 185) sit one inside the other with `Margin="16,8,16,16"` on the outer, so any background/shadow from neighbours gets framed by that margin gap.

There's no explicit `Border` or `Background` on the outer/inner grids, so the rectangles are produced **purely by shadow/margin geometry**, not by a literal painted frame. We can't "delete the outer Grid" without losing the layout — the row is what hosts both columns. The right move is to:
- Keep the row structure (it's load-bearing).
- Make sure neither grid paints anything (already the case — they're transparent by default).
- Tighten/zero the redundant margins so the right pane content sits flush against the title-bar/edges instead of inside a 16px halo.
- Reduce the sidebar shadow spill on the right edge if it still reads as a frame after the margin fix.

## Proposed fix (apply only on GO)
- `MainWindow.xaml` line 35: change `<Grid Grid.Row="1" Margin="16,8,16,16">` → `<Grid Grid.Row="1" Margin="0">` so the body fills the window.
- Move the breathing-room margin onto the two children instead, so each card has its own padding without an outer frame:
  - `ContentControl` (sidebar): `Margin="16,8,8,16"`
  - inner `<Grid Grid.Column="1">`: `Margin="8,8,16,16"`
- Verify no `Background` is set on either grid (already true — leave as-is).
- Build to confirm still green and the content area no longer reads as nested rectangles.
- If the halo still reads as a second frame, follow up by reducing `NeuSidebarStyle` outer shadow `BlurRadius` from 28 → 18 (separate micro-fix; flagged here, not applied unless still visible).

## Status
✅ Done — outer body grid margin zeroed; sidebar `Margin="16,8,8,16"` and content grid `Margin="8,8,16,16"`. Build green.

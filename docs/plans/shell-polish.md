# Shell Polish — Global Issue Tracker

Tracks the 5 issues raised after the first sidebar shell pass.
Each issue has its own partial plan in `docs/plans/issue-XX-*.md`.
Status legend: ⏳ pending · 🔍 diagnosing · ⚙️ fixing · ✅ done

| #  | Issue                                                                                  | Partial plan                              | Status |
|----|----------------------------------------------------------------------------------------|-------------------------------------------|--------|
| 1  | Remove the 9 placeholder pages (wrong scope + wrong folder convention)                 | `docs/plans/issue-01-remove-pages.md`     | ✅     |
| 2  | Drop / make-transparent the outer Grid frame around the content area                   | `docs/plans/issue-02-outer-grid.md`       | ✅     |
| 3  | Language switch (Arabe/Français) must change strings only — no FlowDirection flip      | `docs/plans/issue-03-language-only.md`    | ✅     |
| 4  | Theme toggle background glitch (window stays dark after multiple switches)             | `docs/plans/issue-04-theme-glitch-redo.md`| ✅     |
| 5  | Collapsed sidebar should hide labels and keep only icons (header / nav / CTA / footer) | `docs/plans/issue-05-collapsed-sidebar.md`| ✅     |
| 6  | Collapsed sidebar over-clipped: icons invisible, only thin accent strips remain        | `docs/plans/issue-06-collapsed-clipped.md`| ✅     |
| 7  | Collapsed nav: selected indicator butts the icon, HoverFill highlight offset right     | `docs/plans/issue-07-nav-template-offsets.md`| ✅  |
| 8  | Burger placement clipped + focus halo, footer avatar clipped/off-center                | `docs/plans/issue-08-burger-and-footer.md`| ✅     |
| 9  | Neon-Navy dark palette + Forecourt Overview demo dashboard (`Tableau de bord` page)    | `docs/plans/issue-09-neon-dashboard.md`   | ✅     |

## Workflow per issue
1. Diagnose: cause + exact location (no edits).
2. Wait for user to reply `GO`.
3. Apply minimal fix, build, mark ✅.

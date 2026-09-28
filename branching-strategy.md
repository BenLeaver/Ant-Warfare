# Branching Strategy

This project uses a lightweight branching model designed for solo Unity development.  
It keeps `main` stable, allows safe experimentation, and avoids unnecessary complexity.

---

## Main Branches

### **main**
The stable, playable version of the game.  
Contains only tested, working features.  
No crashes, no debug spam, no half-implemented systems.

### **dev**
Daily development branch.  
All normal work happens here.  
When `dev` becomes stable, merge it into `main`.

---

## Feature Branches

Used for new features, refactors, or experiments.

**Naming:**
feature/short-description

**Workflow:**
1. Branch from `dev`
2. Implement and test the feature
3. Merge back into `dev`
4. Delete the feature branch

---

## Hotfix Branches

Used only when `main` has a critical issue that must be fixed immediately.

**Naming:**
hotfix/issue-name


**Workflow:**
1. Branch from `main`
2. Fix the issue
3. Merge into `main`
4. Merge into `dev`
5. Delete the hotfix branch

---

## Release Branches

For preparing public builds or demos.

**Naming:**
release/v0.1
release/v0.2


Allows polishing and bug-fixing without adding new features.

---

## Branch Diagram

main
└── dev
├── feature/<...>
└── hotfix/<...>


---

## Commit Guidelines (Optional)

Use clear, descriptive commit messages:

- `fix: queen AI infinite loop`
- `feat: new pheromone subtype`
- `refactor: clean ant movement`
- `perf: optimize food scanning`

---

This strategy keeps the project stable, organised, and safe while allowing rapid iteration.

## Version Format

Versions follow **Semantic Versioning (SemVer)** with a lightweight interpretation:
MAJOR.MINOR.PATCH


### **MAJOR**
Increment when:
- Large gameplay systems change
- Save‑game compatibility breaks
- Major redesigns or overhauls occur
- The game reaches a new public milestone (e.g., alpha → beta)

### **MINOR**
Increment when:
- New features are added
- Significant improvements are made
- New content is introduced
- Systems are expanded without breaking compatibility

### **PATCH**
Increment when:
- Bugs are fixed
- Performance is improved
- Small tweaks or polish are added
- Minor adjustments are made without adding new features

---

## Tagging Releases

Every stable version on `main` should be tagged:
v1.0.0
v1.1.0
v1.1.1


Tags mark the exact commit used for a build and make it easy to:
- roll back  
- compare versions  
- generate changelogs  
- track progress  

---

## Release Workflow

1. Develop normally in `dev`
2. Merge feature branches into `dev`
3. When stable, merge `dev` → `main`
4. Choose a version number based on the changes
5. Create a tag:
git tag vX.Y.Z
git push --tags
6. (Optional) Create a build from the tagged commit

---


---

## Major Milestones

- **v0.x.x** - Alpha
- **v1.x.x** - Beta (all major systems exist)
- **v2.x.x** - Release


---

## Changelog (Optional)

Maintain a simple `CHANGELOG.md` with entries like:

v0.5.1
Fixed queen AI infinite loop

Improved ant cornering behaviour

Reduced NavMesh jitter in tight spaces


This helps track progress and makes debugging easier.

---

This versioning strategy keeps releases organised, predictable, and easy to manage while allowing rapid iteration and experimentation.

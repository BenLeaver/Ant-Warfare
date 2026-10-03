# Changelog

All notable changes to this project will be documented in this file.  
This changelog begins at **v0.0.0**, marking the start of formal versioning.

---

## v0.0.0 - Project Structure Established
- Set up proper Git version control at the Unity project root  
- Added lightweight branching strategy (`main`, `dev`, feature and hotfix branches)  
- Added semantic versioning model (MAJOR.MINOR.PATCH)  
- Created initial `CHANGELOG.md` to track future development  

## v0.0.1 Fixed AntType Assignment
- Restored proper AntType serialization in `SingleplayerAntWorld`
- Ensured ant prefabs correctly store and expose their type

## v0.0.2 Enabled Bug Spawning
- Allowed bug spawning again every night

## v0.0.3 Fixed 'Ghost Ants'
- No longer unregister from unit manager on ant world disable
- Previously after being thrown ants were invisible to their enemies
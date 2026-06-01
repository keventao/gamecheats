# Clanfolk IL2CPP Research Checklist

## Setup
1. Run game with MelonLoader to generate `MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll`
2. Open `Assembly-CSharp.dll` in dnSpy
3. Search and document the following systems:

## Research Targets (by module)

### 1. Resource / Inventory System
- [ ] How are resources stored? (ResourceManager, Inventory, Stockpile?)
- [ ] How to add resources to player inventory?
- [ ] Resource enum/ID list
- [ ] Storage capacity system

### 2. Build / Construction System
- [ ] Construction manager class
- [ ] Build time calculation
- [ ] Resource cost deduction during build
- [ ] Worker assignment to construction

### 3. Character / Pawn System
- [ ] Character/Pawn class structure
- [ ] Health/hunger/temperature/needs system
- [ ] Skill system
- [ ] Age/lifecycle system

### 4. Damage / Death System
- [ ] Damage calculation
- [ ] Death triggers
- [ ] Invulnerability hooks

### 5. Storage / Inventory System
- [ ] Storage building classes
- [ ] Capacity calculation
- [ ] Stack size limits
- [ ] Inventory extension methods

## Notes
- Preferred save-XML/save-editor over runtime injection when game supports it
- Never guess method/field names — verify in dnSpy
- Mark unverified names "占位,待确认"

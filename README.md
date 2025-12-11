# Magic Shape Helper (ADOFAI + Unity Mod Manager)

Мод показывает в оверлее множитель скорости для текущего тайла, чтобы сохранять постоянную скорость в Magic Shape.

## Как это работает
- Harmony-патчи читают угол текущего тайла (`scrLevelManager`/`scrTile`).
- Множитель рассчитывается по формуле `180 / angle` (180° → 1.0, 90° → 2.0, 45° → 4.0).
- Оверлей (IMGUI) выводит текст в правом верхнем углу: Speed Multiplier + Tile Angle.

## Сборка DLL (make_build.bat)
1. Проверьте пути в `MagicShapeHelper.csproj`:
   - `GameManagedPath` = `C:\\Program Files (x86)\\Steam\\steamapps\\common\\A Dance of Fire and Ice\\A Dance of Fire and Ice_Data\\Managed`
   - `UMMPath` = `$(GameManagedPath)\\UnityModManager`
2. Запустите `make_build.bat` (ищет `msbuild` или `dotnet msbuild`, конфигурация Release).
3. Готовый `MagicShapeHelper.dll` будет скопирован в корень мода.

## Сборка архива (make_release.bat)
1. Убедитесь, что в корне мода есть `MagicShapeHelper.dll` и `Info.json`.
2. Запустите `make_release.bat` (двойной клик или через PowerShell/cmd).
3. Результат: `build/MagicShapeHelper.zip` (внутри только DLL и Info.json, без вложенных папок).

## Сборка архива (make_release.bat)
1. Убедитесь, что в корне мода есть `MagicShapeHelper.dll` и `Info.json`.
2. Запустите `make_release.bat` (двойной клик или через PowerShell/cmd).
3. Результат: `build/MagicShapeHelper.zip` (внутри только DLL и Info.json, без вложенных папок).

## Установка
- Поместите zip в папку `UnityModManager/Mods` (или используйте установщик UMM).

## Настройка
- В UMM включите мод. Оверлей появляется поверх игры.

## Примечания
- Метод-мишени (`scrLevelManager.GetCurrentTileAngle`, `scrTile.angle`) указаны как шаблон и могут отличаться по версии игры. При необходимости скорректируйте патчи после просмотра сборок через dnSpy.



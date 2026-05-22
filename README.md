# KartinkaSpawner для EXILED 9.13.3

Плагин спавнит PNG/JPG картинки в SCP:SL как сетку `PrimitiveObjectToy` кубов.
Картинка появляется перед админом, который выполнил команду в RemoteAdmin.

## Команды RemoteAdmin

```text
kartinka list
kartinka spawn <имя_картинки> [pixel_size] [max_side]
kartinka undo
kartinka clear
kartinka folder
```

Примеры:

```text
kartinka list
kartinka spawn logo
kartinka spawn logo.png
kartinka spawn logo 0.06 48
```

- `pixel_size` — размер одного пикселя в метрах. По умолчанию берется из конфига.
- `max_side` — временно ограничивает и ширину, и высоту картинки, например `48` даст максимум 48x48.

## Куда класть картинки

По умолчанию плагин создает папку:

```text
EXILED/Configs/KartinkaImages
```

Путь можно посмотреть командой:

```text
kartinka folder
```

Поддерживаемые форматы: `.png`, `.jpg`, `.jpeg`, `.bmp`.


## Важная заметка по производительности

Плагин создает **один primitive-куб на один видимый пиксель**. Не ставь слишком большие значения `max_width`/`max_height`.
По умолчанию стоит лимит `2500` кубов на одну картинку.

Его нужно положить в папку EXILED plugins сервера.

## Конфиг

После первого запуска появится `kartinka.yml`. Главные настройки:

```yaml
images_folder: KartinkaImages
max_width: 64
max_height: 64
max_primitives_per_image: 2500
pixel_size: 0.08
pixel_thickness: 0.01
spawn_distance: 1.5
vertical_offset: 0
use_camera_pitch: false
collidable: false
alpha_threshold: 10
required_permission: ''
clear_on_round_restart: true
```


## Исправление ошибки UnityEngine.ImageConversionModule / netstandard 2.1

В текущей версии плагин больше не использует `UnityEngine.ImageConversionModule.dll`. Если у тебя в `.csproj` осталась строка с `UnityEngine.ImageConversionModule`, удали ее. Для сборки нужен только `UnityEngine.CoreModule.dll` и `System.Drawing`.

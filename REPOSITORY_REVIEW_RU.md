# Обзор MWCO-SOURCE-v0.2.3

Дата: 22 сентября 2026 г.  
Репозиторий: `hussaunmays16-wq/MWCO-SOURCE-v0.2.3`.  
Исходный снимок: `a41074fb24f4aca81f04ebff550181d0b861ccb1`.

## Краткий вывод

В исходном снимке репозитория находится один файл — `MWCOClient.zip`. Архив распакован в [MWCOClient/](MWCOClient/), оригинальный ZIP сохранён без изменений. Все распакованные файлы побайтно совпадают с содержимым архива; правки игровой логики не вносились.

Это **клиентская библиотека My Winter Car Online 0.2.3** для запуска внутри Unity-игры, с ролью хоста и клиентов через Steam. Это не самостоятельное приложение и не полный Unity-проект. По невалидным compiler-generated именам, развёрнутым машинам состояний и другим артефактам код представляет собой декомпилированную выгрузку сборки, а не готовые к сборке исходники.

**Охват обзора:** полная инвентаризация всех файлов, проверка проекта и ресурсов, просмотр модулей и их связей; подробнее разобраны запуск, авторизация, сетевые сообщения, синхронизация и работа с сохранениями. Это статический обзор, **не полный построчный аудит, не проверка в игре и не гарантия отсутствия других ошибок**.

## 1. Что лежит в архиве

| Показатель | Значение |
|---|---:|
| Файлов | 147 |
| Исходников C# | 145 |
| Строк в `.cs`, включая пустые, комментарии и сгенерированный код | 34 836 |
| Проектов `.csproj` | 1 |
| Файлов `.resources` | 1 |
| Общий размер извлечённых файлов | 987 800 байт |
| Вложенных архивов | 0 |
| Готовых DLL/EXE | 0 |

`MWCO/Properties/Resources.resources` занимает 180 байт. Его заголовок содержит **0 ресурсов**: модели, текстуры и интерфейс внутри него не спрятаны.

SHA-256 оригинального ZIP:

```text
cb3b085c13a0ffcbd035285955acab85dccfa53ce14e04372171c58183c2dd6f
```

## 2. Платформа и комплектность

[MWCOClient.csproj](MWCOClient/MWCOClient.csproj) задаёт:

- тип результата: библиотека `MWCOClient.dll`;
- целевую платформу: **x64**;
- целевой framework: **.NET Framework 3.5**;
- старый формат MSBuild-проекта, `ToolsVersion="15.0"`;
- конфигурации `Debug` и `Release`.

Все 145 файлов C# перечислены в `Compile` проекта. Пропущенных, лишних или повторяющихся записей `Compile` не обнаружено.

### Зависимости

Проект ссылается на `Assembly-CSharp`, `Assembly-CSharp-firstpass`, `Assembly-UnityScript-firstpass`, `Boo.Lang`, `ES2`, `Mono.Security`, `PlayMaker`, `System`, `System.Core`, `UnityEngine`, `UnityEngine.UI`.

Игровых и сторонних DLL в архиве **нет**; `HintPath` для них не задан. В исходниках также используется namespace `Steamworks`: предоставляющие эти типы совместимые игровые сборки/SDK необходимо определить при восстановлении сборки. По одному архиву нельзя достоверно установить их точные версии.

Во время запуска [Client.cs:23,47](MWCOClient/MWCO/Client.cs#L23) загружает внешний AssetBundle `..\..\data\mpdata`. Этого файла в архиве нет. Из него должны загружаться, в частности:

- `Assets/UI/MWCOCanvas.prefab`;
- `Assets/MPPlayerModel/MPPlayerModel.prefab`;
- шрифты, логотип и другие игровые ассеты, на которые ссылается код.

Нет также:

- полноценного Unity-проекта с `Assets`, `ProjectSettings`, `Packages`;
- отдельного проекта лаунчера и реализации HTTP API;
- README с инструкцией сборки, файла лицензии, тестового проекта и CI-конфигурации.

Отдельного выделенного игрового сервера здесь нет: роль хоста реализована в самом клиенте. Это отличается от отсутствующей серверной части **авторизации**.

Код ориентирован на Windows: есть обращение к реестру и `user32.dll`, а также Windows-пути. Простое переключение framework на современный .NET не восстановит совместимость с Unity и игровыми DLL.

## 3. Как устроена программа

| Часть | Основные файлы | Назначение |
|---|---|---|
| Загрузка библиотеки | `MWCO/Client.cs` | Точки входа `Start()` и `StartEncrypted(filePath)`, логирование, AssetBundle, создание Unity-компонентов. |
| Главный контроллер | `MWCO/MPController.cs` | Инициализация Steam, авторизация, меню, список лобби, скины, смена сцен, управление сохранением и обновлениями. |
| Игровой мир | `MWCO/Game/GameWorld.cs`, `Game*Database.cs` | Поиск объектов сцены, регистрация транспорта, предметов, домов, дверей, погоды и локального игрока. |
| Интеграция с PlayMaker | `Game/EventHook.cs`, `Game/Hooks/PlayMakerActionHooks.cs`, `PlayMakerUtils.cs` | Подключение к игровым FSM и добавление сетевых событий `MP_*`. |
| Сеть | `Network/NetManager.cs`, `NetMessageHandler.cs` | Steam-лобби и P2P, handshake, heartbeat, отправка/разбор сообщений, чат и передача сейвов. |
| Сетевая копия мира | `Network/NetWorld.cs` | Обработчики игровых сообщений, полный снимок мира, создание и удаление объектов. |
| Игроки | `Network/NetLocalPlayer.cs`, `NetPlayer.cs`, `PlayerAnimManager.cs`, `IKManager.cs` | Локальная отправка состояния, удалённая модель игрока, интерполяция, анимации, скины и позы. |
| Владение объектами | `Game/ObjectSyncManager.cs`, `Game/Components/ObjectSyncComponent.cs` | Сетевые ID, владелец объекта, передача управления, периодическая синхронизация и интерполяция. |
| Игровые адаптеры | `Game/Objects/`, `Game/Objects/PickupableTypes/` | Машины, двери, багажники, пассажиры, дома, предметы, детали, болты и жидкости. |
| Локации | `Game/Places/` | Магазин/бар, Peraportti и заправка, фабрика, дистанция активности объектов с учётом игроков. |
| Интерфейс | `Chat.cs`, `UI/`, `Utilities/Styles.cs` | Чат, консоль, кнопки, курсор и стили. |
| Диагностика | `DevTools.cs`, `InstanceDebugger.cs`, `Network/NetStatistics.cs`, `Utilities/Logger.cs` | Команды, дампы объектов, отладочное меню, сетевые счётчики и локальные журналы. |
| Настройки и математика | `UserPreferences.cs`, `Math/`, `Utils.cs` | Настройки внешнего вида/режимов, интерполяторы и вспомогательные преобразования. |

### Основной путь выполнения

1. Внешнее окружение вызывает `Client.Start()` либо `Client.StartEncrypted(...)`.
2. Клиент устанавливает хуки PlayMaker, загружает `mpdata`, создаёт `MPGUI` и `MPController`.
3. `MPController` инициализирует Steam, создаёт `NetManager` и загружает главное меню.
4. Обычный путь запуска читает `Token` и `Email` из переменных окружения и отправляет запрос проверки доступа на `https://www.mywintercar.online/server/data/VerifyLogin.php` через SteamHTTP. Реальных значений учётных данных для этого обзора не запрашивалось; запросы к этому API не выполнялись.
5. Хост создаёт Steam-лобби, клиент присоединяется. Handshake передаёт версию мода, сетевые часы и код скина.
6. Хост передаёт сохранения; после загрузки сцены выполняется синхронизация мира и игроков.
7. Далее сообщения обновляют позиции, анимации, владение объектами, транспорт, FSM, погоду, покупки и другие игровые состояния.

### Формат сети

- Транспорт — `SteamNetworking` P2P и `SteamMatchmaking` для лобби.
- Пакет записывается как 4-байтовый маркер `1836278637`, байт ID сообщения и бинарное тело.
- Тела сериализуются вручную через `BinaryReader`/`BinaryWriter`.
- В `Network/Messages/` находится 41 файл: 31 реализация `INetMessage`, вспомогательные вложенные структуры и описания ID.
- Все ID от 0 до 30 представлены по одному разу; дубликатов не обнаружено.
- Вызовы регистрации обработчиков найдены для 29 из этих классов. Для `ModChangeMessage` и `WorldPeriodicalUpdateMessage` вызовов `BindMessageHandler` не найдено. Это может быть остатком прежней реализации, а не обязательно активной ошибкой.

## 4. Что мешает сборке и требует внимания

Это наблюдения **по текущему тексту выгрузки**. Они не доказывают, что исходная DLL имела ровно те же дефекты: часть проблем могла появиться при декомпиляции.

### 4.1. Блокер: экспорт содержит невалидный C#

Примеры:

- [\-PrivateImplementationDetails-.cs:6](MWCOClient/-PrivateImplementationDetails-.cs#L6): `class <PrivateImplementationDetails>` и типы вида `__StaticArrayInitTypeSize=108`.
- [ValueDuple.cs:11](MWCOClient/ValueDuple.cs#L11): обращения к `<first>k__BackingField`.
- [Chat.cs:211](MWCOClient/MWCO/Chat.cs#L211): развёрнутая машина состояний `<FadeOut>d__24` вместо восстановленной корутины.
- [GameObjectExtensions.cs:10](MWCOClient/MWCO/Utilities/GameObjectExtensions.cs#L10): `vec..ctor(...)`.
- [UI/Button.cs:11,121](MWCOClient/MWCO/UI/Button.cs#L11): событие и поле с одинаковым именем `OnButtonClick`.

Текстовый поиск явных compiler-generated имён обнаружил их в **44 файлах**. Это не количество ошибок компилятора и не полный перечень проблем.

**Вывод:** одних DLL недостаточно. Нужен корректный повторный экспорт из исходной сборки, если она доступна, либо восстановление обычных C#-свойств, событий, замыканий и корутин. Автоматическое удаление всех сгенерированных классов небезопасно: некоторые из них ещё вызываются основным кодом.

### 4.2. Блокер: неполное окружение сборки и запуска

Внешние игровые библиотеки, AssetBundle и окружение авторизованного запуска отсутствуют. См. [проект:35–47](MWCOClient/MWCOClient.csproj#L35), [Client.cs:23–30](MWCOClient/MWCO/Client.cs#L23), [MPController.cs:380–415](MWCOClient/MWCO/MPController.cs#L380).

Сначала нужно установить совместимые версии зависимостей и способ штатного запуска, а не менять сетевую/игровую логику наугад.

### 4.3. Запуск содержит служебную запись в абсолютный путь разработчика

В [MPController.Start():76](MWCOClient/MWCO/MPController.cs#L76) безусловно вызывается генератор `GenerateIdiNahouiMethods(...)` с абсолютным путём на рабочем столе разработчика.

[Метод:1054–1077](MWCOClient/MWCO/MPController.cs#L1054) формирует текст **5000 методов** и записывает его через `File.WriteAllText`. Если родительской папки нет, операция завершится исключением; даже при существующей папке это лишняя работа во время старта.

В строковом шаблоне есть HTTP-проверка, расшифровка DLL и `Assembly.Load`. Важно: здесь это **генерируемый текст**, а не непосредственное исполнение этих операций самим генератором. По одному этому фрагменту делать вывод о вредоносности нельзя.

Перед восстановлением запуска нужно отдельно выяснить назначение этого служебного кода и вынести генерацию из обычной инициализации.

### 4.4. Получение сохранения не проверяет отправителя на роль хоста

[NetWorld.cs:245–251](MWCOClient/MWCO/Network/NetWorld.cs#L245) проверяет только, что локальный экземпляр не является хостом, после чего вызывает `LoadSave(msg)`. Сравнения `sender` с текущим хостом в этом обработчике нет.

[NetManager.cs:100–108](MWCOClient/MWCO/Network/NetManager.cs#L100) принимает входящую P2P-сессию без проверки членства в текущем лобби на уровне этого метода; в [диспетчере](MWCOClient/MWCO/Network/NetMessageHandler.cs#L61) общей проверки отправителя также нет.

Рекомендуется проверять членство в лобби, этап подключения и роль отправителя **до** обработки сообщений, особенно до записи сейва. Реальные сценарии злоупотребления и ограничения Steam в рамках обзора не тестировались.

### 4.5. Работа с сейвами требует отдельной резервной копии

[NetManager.cs:137–203](MWCOClient/MWCO/Network/NetManager.cs#L137) переименовывает локальные `savefile.txt`, `items2.txt`, `carparts.txt` в файлы с суффиксом `_bak`, затем записывает полученные данные. Восстановление удаляет текущие файлы и возвращает резервные.

Риски по коду:

- имена `_bak` фиксированные; уже существующий файл мешает `File.Move`;
- запись трёх файлов не является единой атомарной операцией;
- частичный сбой оставляет промежуточное состояние без немедленного отката;
- резервирование и восстановление дополнительных файлов вложено в условие наличия основного сейва/его резервной копии.

**Не стоит впервые проверять восстановленный клиент на единственной копии игрового прогресса.** Для дальнейших тестов нужны отдельные копии всех файлов сохранения и сценарии сбоя/повторного подключения.

### 4.6. Недостаточно ограничений при разборе сетевых данных

В [NetManager.ProcessMessages():973–1013](MWCOClient/MWCO/Network/NetManager.cs#L973) нет собственной проверки минимального размера 5-байтового заголовка до `ReadUInt32()`/`ReadByte()`.

[SaveMessage.Read():52–81](MWCOClient/MWCO/Network/Messages/SaveMessage.cs#L52), а также `FullWorldSyncMessage`, `PlayerSyncMessage`, `ObjectSyncMessage`, `OrderListMessage` и `PickupableSpawnMessage` используют прочитанные из сети длины массивов без явных верхних лимитов и предварительного сопоставления с оставшимся телом пакета. `try/catch` не заменяет ограничение объёма выделяемой памяти.

Нужны границы размеров, проверка заголовка и структуры тела, допустимых ID/значений и отказ от обработки неизвестных отправителей. Это замечания к защитным проверкам, а не утверждение о проверенной атаке.

### 4.7. Отправляется ёмкость буфера, а не фактическая длина сообщения

[NetManager.cs:701–742](MWCOClient/MWCO/Network/NetManager.cs#L701) передаёт `MemoryStream.GetBuffer()`. [NetPlayer.SendPacket():969–971](MWCOClient/MWCO/Network/NetPlayer.cs#L969) отправляет весь `data.Length`.

`GetBuffer()` может возвращать массив больше `MemoryStream.Length`, поэтому в пакеты попадают лишние байты. Это увеличивает трафик и искажает сравнение размера сериализованного тела с реально отправленными данными. Следует передавать действительную длину либо использовать `ToArray()` с учётом стоимости копирования.

### 4.8. Другие заметные места

- [UI/Console.cs:186–190](MWCOClient/MWCO/UI/Console.cs#L186): при обрезке истории используется `RemoveAt(i)` с возрастающим индексом. Список при этом уменьшается, поэтому элементы пропускаются, а при исходном количестве 21–38 элементов возможен выход за границы. Правильнее удалить диапазон первых элементов либо многократно удалять индекс 0.
- [Utilities/RichTextExtensions.cs:12–19](MWCOClient/MWCO/Utilities/RichTextExtensions.cs#L12): `Italic()` закрывает тегом `</b>`, у `SetSize()` отсутствует завершающий `>` в закрывающем теге.
- [Game/EventHook.cs:66–72](MWCOClient/MWCO/Game/EventHook.cs#L66): при `fsm == null` вызывается `Assert(true, ...)`, который ничего не делает, затем следует обращение к `fsm.Fsm`. В других местах также есть `Assert(true, сообщение_об_ошибке)`.
- [Network/NetLocalPlayer.cs:659–661](MWCOClient/MWCO/Network/NetLocalPlayer.cs#L659): одна перегрузка `WriteHouseSwitchMessage` оставлена с `NotImplementedException`. Само её наличие не доказывает, что она вызывается в штатном сценарии.
- `CarPartOld`, `BoltOld`, `TriggerOld`, незаполненные методы `Firewood`, `Rivett` и `TrafficManager.GetHighwayCars()` показывают наличие старого/незавершённого кода. Их нельзя без проверки считать рабочими функциями или безопасно удалить.
- Много жёстко заданных названий объектов, путей `Transform`, состояний FSM и числовых ID. Совместимость с другими версиями игры нужно проверять отдельно.

## 5. Что проверено фактически

| Проверка | Результат |
|---|---|
| Доступ к указанному GitHub-репозиторию | Метаданные получены через `gh`; репозиторий соответствует рабочей копии по имени. |
| Список файлов исходного Git-снимка | Один `MWCOClient.zip`. |
| Безопасность путей перед распаковкой | Нет выходов за каталог, symlink-записей, повторяющихся имён или зашифрованных записей. |
| CRC ZIP | Успешно для всех записей. |
| Содержимое после распаковки | Все 147 файлов побайтно совпали с ZIP; лишних файлов в `MWCOClient/` нет. |
| Состав `.csproj` | Все 145 исходников перечислены ровно один раз; пропусков нет. |
| Сетевые ID | 31 уникальный ID, полный диапазон 0–30. |
| Бинарный `.resources` | Заголовок прочитан без исполнения .NET; 0 записей ресурсов. |
| Сборка C# | **Не запускалась:** в окружении нет `dotnet`, MSBuild, Mono/C#-компилятора; зависимости отсутствуют, в тексте уже видны синтаксические блокеры. |
| Запуск игры / мультиплеера / авторизации | **Не выполнялся.** |

Архивные исполняемые файлы не запускались; DLL/EXE в самом архиве нет. Никакие учётные данные, сохранения или настройки пользователя для обзора не требовались.

## 6. Логичный порядок дальнейшей работы

1. Сохранить эту выгрузку как исходный материал; по возможности получить оригинальную DLL и корректно восстановленные исходники.
2. Установить совместимые версии игровых библиотек, Unity/PlayMaker/ES2/Steamworks и получить штатный `mpdata` из разрешённой пользователю установки.
3. Добиться воспроизводимой сборки без изменения смысла игры и протокола; документировать подключение внешних DLL, не публикуя их автоматически.
4. Убрать из обычного старта служебную генерацию в абсолютный путь и проверить штатную инициализацию.
5. До публичных сетевых испытаний усилить проверки отправителей, сетевых размеров и безопасную замену сохранений.
6. На копиях сейвов проверить запуск, host/join, выход и повторный вход, предметы/машины/болты, сохранение/восстановление, сон и ошибки получения неполного пакета.

В этом проходе сделаны только распаковка и обзор. Коммиты, push и изменение игровой логики не выполнялись.

## 7. Карта всех файлов архива

Пути ниже относительны к `MWCOClient/`. Количество строк — физические строки исходного файла, включая декомпилированный служебный код. Назначения описывают то, что представлено в коде, а не подтверждённую работоспособность в игре.

### Корень `MWCOClient/`

| Файл | Строк | Назначение |
|---|---:|---|
| [-PrivateImplementationDetails-.cs](MWCOClient/-PrivateImplementationDetails-.cs) | 37 | Декомпилированные служебные структуры и поля статических массивов; невалидные C#-имена. |
| [GameObjectExtensions.cs](MWCOClient/GameObjectExtensions.cs) | 16 | Поиск самого верхнего родителя GameObject. |
| [IKManager.cs](MWCOClient/IKManager.cs) | 87 | Обратная кинематика руки: плечо, предплечье, кисть, локоть и цель. |
| [LobbyItem.cs](MWCOClient/LobbyItem.cs) | 23 | Данные элемента списка лобби: владелец, Steam ID, регион, версии, признак друга. |
| [MWCOClient.csproj](MWCOClient/MWCOClient.csproj) | 200 | MSBuild-проект DLL, x64, .NET Framework 3.5; ссылки и список компиляции. |
| [ValueDuple.cs](MWCOClient/ValueDuple.cs) | 50 | Обобщённый контейнер двух значений; свойства с артефактами backing fields. |
| [ValueTuple.cs](MWCOClient/ValueTuple.cs) | 58 | Обобщённый контейнер предмета, признака заморозки и родителя; артефакты backing fields. |

### `MWCO/`

| Файл | Строк | Назначение |
|---|---:|---|
| [Chat.cs](MWCOClient/MWCO/Chat.cs) | 323 | Игровой чат, история ввода, оформление сообщений, блокировка управления и затухание текста. |
| [CheapOnLevelLoad.cs](MWCOClient/MWCO/CheapOnLevelLoad.cs) | 21 | Компонент вызова MPController.OnGameLoad; сам обработчик сейчас пуст. |
| [Client.cs](MWCOClient/MWCO/Client.cs) | 99 | Вход в мод, пути данных, AssetBundle mpdata, создание и перезапуск контроллера. |
| [DevTools.cs](MWCOClient/MWCO/DevTools.cs) | 1274 | Консольные команды, отладочное/чит-меню, телепортация, состояние игрока и дамп дерева объектов. |
| [InstanceDebugger.cs](MWCOClient/MWCO/InstanceDebugger.cs) | 134 | Отложенная регистрация созданных предметов, назначение ObjectSyncComponent и отправка спавна. |
| [MPController.cs](MWCOClient/MWCO/MPController.cs) | 1648 | Главный MonoBehaviour: Steam, проверка доступа, меню/лобби, скины, сцены, сейвы и служебный генератор. |
| [MessageSeverity.cs](MWCOClient/MWCO/MessageSeverity.cs) | 13 | Категории сообщений: информация, ошибка, предупреждение, игрок, чат. |
| [PlayMakerUtils.cs](MWCOClient/MWCO/PlayMakerUtils.cs) | 87 | Изменение действий, событий и глобальных переходов FSM. |
| [RespawnController.cs](MWCOClient/MWCO/RespawnController.cs) | 99 | Экран смерти, выход из транспорта, восстановление управления и перемещение игрока при возрождении. |
| [UserPreferences.cs](MWCOClient/MWCO/UserPreferences.cs) | 103 | Чтение userPref.txt и запись кода скина; параметры Savefile, Optimized, OptIn. |
| [Utils.cs](MWCOClient/MWCO/Utils.cs) | 441 | Инспекция объектов/FSM, поиск PlayMaker-компонентов, преобразование векторов/цветов, хеширование и сетевые ошибки. |

### `MWCO/Game/`

| Файл | Строк | Назначение |
|---|---:|---|
| [AxController.cs](MWCOClient/MWCO/Game/AxController.cs) | 22 | Отслеживание того, держит ли игрок топор, через включение/выключение компонента. |
| [CarCollisionHandler.cs](MWCOClient/MWCO/Game/CarCollisionHandler.cs) | 138 | Смерть от сильного столкновения с учётом ремня, захват синхронизации при столкновении с тюком, cooldown. |
| [CollisionDetection.cs](MWCOClient/MWCO/Game/CollisionDetection.cs) | 32 | Получение управления синхронизацией машины при столкновении, если её не занимает другой игрок. |
| [EventHook.cs](MWCOClient/MWCO/Game/EventHook.cs) | 332 | Встраивание действий в FSM, реестр событий и их сетевая синхронизация через MP_*. |
| [GameCallbacks.cs](MWCOClient/MWCO/Game/GameCallbacks.cs) | 44 | Общие делегаты событий: мир, игрок, создание/удаление/активация/перемещение и подбор объектов. |
| [GameDoorsManager.cs](MWCOClient/MWCO/Game/GameDoorsManager.cs) | 141 | Поиск обычных дверей, список дверей, события открытия/закрытия и поиск по позиции. |
| [GameHouseDatabase.cs](MWCOClient/MWCO/Game/GameHouseDatabase.cs) | 471 | Регистрация домов и бытовых объектов, телефоны, радио и связанные события мира. |
| [GamePickupableDatabase.cs](MWCOClient/MWCO/Game/GamePickupableDatabase.cs) | 305 | Каталог предметов/префабов, метаданные, распознавание предметов, создание и удаление описаний. |
| [GameVehicleDatabase.cs](MWCOClient/MWCO/Game/GameVehicleDatabase.cs) | 326 | Каталог транспорта, AI-машин, деталей, триггеров, болтов и точек интеграции Rivett. |
| [GameWeatherManager.cs](MWCOClient/MWCO/Game/GameWeatherManager.cs) | 194 | Состояния погоды и облаков; также содержит регистрацию компонентов болтов. |
| [GameWorld.cs](MWCOClient/MWCO/Game/GameWorld.cs) | 730 | Жизненный цикл мира, обход сцены, подключение баз объектов, время/день и создание игрока/предметов. |
| [IGameObjectCollector.cs](MWCOClient/MWCO/Game/IGameObjectCollector.cs) | 14 | Контракт сборщика игровых объектов: сбор, удаление одного объекта и очистка. |
| [IObjectSubtype.cs](MWCOClient/MWCO/Game/IObjectSubtype.cs) | 13 | Небольшой контракт подтипа: массив синхронизируемых значений и возможность синхронизации. |
| [ISyncedObject.cs](MWCOClient/MWCO/Game/ISyncedObject.cs) | 29 | Контракт сетевого объекта: Transform, владение, обновления и синхронизируемые значения. |
| [LightSwitchManager.cs](MWCOClient/MWCO/Game/LightSwitchManager.cs) | 127 | Реестр выключателей света, поиск по объекту/координатам и события использования. |
| [ObjectSyncManager.cs](MWCOClient/MWCO/Game/ObjectSyncManager.cs) | 111 | Словарь сетевых ID, перечисления типов объектов/обновлений и условие периодической синхронизации. |
| [SeatbeltHandler.cs](MWCOClient/MWCO/Game/SeatbeltHandler.cs) | 23 | Установка признака пристёгнутого ремня у локального игрока. |
| [SleepRequest.cs](MWCOClient/MWCO/Game/SleepRequest.cs) | 139 | Запрос совместного сна, отмена ожидания и разрешение перехода в сон. |
| [TrafficManager.cs](MWCOClient/MWCO/Game/TrafficManager.cs) | 85 | Маршруты NPC и каркас периодической рассылки; цикл GetHighwayCars пока не заполняет список. |

### `MWCO/Game/Components/`

| Файл | Строк | Назначение |
|---|---:|---|
| [ObjectSyncComponent.cs](MWCOClient/MWCO/Game/Components/ObjectSyncComponent.cs) | 539 | Основной Unity-компонент синхронизации: ID, владелец, подтип, запросы, дистанции активности, позиция и интерполяция. |
| [ObjectSyncPlayerComponent.cs](MWCOClient/MWCO/Game/Components/ObjectSyncPlayerComponent.cs) | 34 | События входа/выхода объектов из триггера вокруг игрока для синхронизации. |
| [PickupableMetaDataComponent.cs](MWCOClient/MWCO/Game/Components/PickupableMetaDataComponent.cs) | 24 | ID префаба предмета и доступ к его описанию в каталоге. |

### `MWCO/Game/Hooks/`

| Файл | Строк | Назначение |
|---|---:|---|
| [PlayMakerActionHooks.cs](MWCOClient/MWCO/Game/Hooks/PlayMakerActionHooks.cs) | 725 | Подмена типов пяти действий PlayMaker через reflection; также объявления обёрток Save/Load, не установленных методом Install. |

### `MWCO/Game/Objects/`

| Файл | Строк | Назначение |
|---|---:|---|
| [AIVehicle.cs](MWCOClient/MWCO/Game/Objects/AIVehicle.cs) | 576 | Адаптер AI-транспорта: маршруты, движение, активность, события автобуса и синхронизируемые параметры. |
| [Boot.cs](MWCOClient/MWCO/Game/Objects/Boot.cs) | 141 | Адаптер крышки багажника/капота: события открытия и передача вращения. |
| [BootBox.cs](MWCOClient/MWCO/Game/Objects/BootBox.cs) | 259 | Триггер содержимого багажника: список предметов и передача управления их синхронизацией водителю. |
| [CarDoor.cs](MWCOClient/MWCO/Game/Objects/CarDoor.cs) | 82 | Сетевые события обычных автомобильных дверей. |
| [CarDoorF.cs](MWCOClient/MWCO/Game/Objects/CarDoorF.cs) | 75 | Другой вариант адаптера автомобильных дверей с соответствующими FSM. |
| [ComponentUtil.cs](MWCOClient/MWCO/Game/Objects/ComponentUtil.cs) | 50 | Копирование полей и свойств Unity-компонентов через reflection. |
| [GameDoor.cs](MWCOClient/MWCO/Game/Objects/GameDoor.cs) | 118 | Обёртка обычной двери, события MPOPEN/MPCLOSE и обратные вызовы менеджера. |
| [GamePlayer.cs](MWCOClient/MWCO/Game/Objects/GamePlayer.cs) | 507 | Локальный игрок, подбор/бросок предметов, работа с болтами, покраска и сброс показателей персонажа. |
| [GarageDoor.cs](MWCOClient/MWCO/Game/Objects/GarageDoor.cs) | 112 | Адаптер гаражной двери с передачей владения и вращения. |
| [LightSwitch.cs](MWCOClient/MWCO/Game/Objects/LightSwitch.cs) | 88 | Обёртка выключателя света, состояние Switch и событие MPSWITCH. |
| [Logging.cs](MWCOClient/MWCO/Game/Objects/Logging.cs) | 112 | Несмотря на название, адаптер ISyncedObject с дверными событиями и отслеживанием вращения; не файловый логгер. |
| [PassengerSeat.cs](MWCOClient/MWCO/Game/Objects/PassengerSeat.cs) | 287 | Пассажирские/задние места, вход и выход, положение игрока, коллайдеры и управление. |
| [Pickupable.cs](MWCOClient/MWCO/Game/Objects/Pickupable.cs) | 1228 | Основной адаптер переносимых предметов, физика, владение, типовые FSM и создание содержимого упаковок. |
| [PlayerHouse.cs](MWCOClient/MWCO/Game/Objects/PlayerHouse.cs) | 1349 | Бытовые устройства и события: двери, радио/ТВ, телефон, вода, сауна, инструменты и переключатели. |
| [PlayerVehicle.cs](MWCOClient/MWCO/Game/Objects/PlayerVehicle.cs) | 3390 | Крупнейший файл: транспорт, двигатель/управление, пассажиры, багажник, переключатели, топливо и FSM разных машин. |
| [ReapplyJoint.cs](MWCOClient/MWCO/Game/Objects/ReapplyJoint.cs) | 43 | Резервная копия ConfigurableJoint, попытка восстановления соединения и фиксация локального Transform. |
| [Rivett.cs](MWCOClient/MWCO/Game/Objects/Rivett.cs) | 65 | Заготовка адаптера ISyncedObject: Transform реализован, большая часть поведения пустая или возвращает false/null. |
| [SideDoor.cs](MWCOClient/MWCO/Game/Objects/SideDoor.cs) | 107 | Вариант дверного адаптера с передачей управления при открытии/закрытии. |
| [Train.cs](MWCOClient/MWCO/Game/Objects/Train.cs) | 151 | Синхронизация поезда: движение на хосте, события остановок и LOD. |
| [TransformSync.cs](MWCOClient/MWCO/Game/Objects/TransformSync.cs) | 66 | Минимальный адаптер для передачи Transform без дополнительных переменных. |

### `MWCO/Game/Objects/PickupableTypes/`

| Файл | Строк | Назначение |
|---|---:|---|
| [BeerCase.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/BeerCase.cs) | 74 | Состояние ящика пива, использованные бутылки и события их удаления. |
| [Bolt.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/Bolt.cs) | 49 | Передача и применение затягивания/откручивания болтов. |
| [BoltOld.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/BoltOld.cs) | 14 | Старая минимальная структура принадлежности болта объекту. |
| [CarPart.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/CarPart.cs) | 242 | Текущая реализация детали: установка/снятие, связанные FSM, состояние крепления и регистрация болтов. |
| [CarPartOld.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/CarPartOld.cs) | 1892 | Предыдущая большая реализация деталей: сборка, болты, триггеры, жидкости и покраска. |
| [Consumable.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/Consumable.cs) | 56 | События расходования/уничтожения съедобных и питьевых предметов. |
| [Firewood.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/Firewood.cs) | 20 | Заготовка обработчика дров; HookEvents пуст. |
| [Hose.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/Hose.cs) | 29 | Адаптер шланга с запросом исходной синхронизации от хоста. |
| [PubFood.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/PubFood.cs) | 21 | Подключение сетевого события использования еды из паба. |
| [ShoppingBag.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/ShoppingBag.cs) | 37 | Ссылки на FSM пакета покупок; самостоятельная обработка в этом классе минимальна. |
| [Tool.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/Tool.cs) | 191 | Синхронизация моторного подъёмника, угла подъёма и связанных винтов. |
| [Trigger.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/Trigger.cs) | 185 | Точки установки деталей, AssemblyID, состояние жидкостей и применение сетевой установки. |
| [TriggerOld.cs](MWCOClient/MWCO/Game/Objects/PickupableTypes/TriggerOld.cs) | 53 | Старая обёртка триггера установки детали, включая особый случай ремня генератора. |

### `MWCO/Game/Places/`

| Файл | Строк | Назначение |
|---|---:|---|
| [Factory.cs](MWCOClient/MWCO/Game/Places/Factory.cs) | 135 | События фабрики: упаковки, триггер ящика, регуляторы, подключение оборудования и LOD. |
| [LODManager.cs](MWCOClient/MWCO/Game/Places/LODManager.cs) | 112 | Расширение/восстановление дистанции активности объекта с учётом локального и удалённых игроков. |
| [Peraportti.cs](MWCOClient/MWCO/Game/Places/Peraportti.cs) | 785 | Кассы, терминалы, покупки и заправочные пистолеты/топливо на Peraportti. |
| [Shop.cs](MWCOClient/MWCO/Game/Places/Shop.cs) | 444 | Магазин и паб: покупки, касса, почтовый заказ, игровые автоматы и разбиваемые окна. |

### `MWCO/Math/`

| Файл | Строк | Назначение |
|---|---:|---|
| [QuaternionInterpolator.cs](MWCOClient/MWCO/Math/QuaternionInterpolator.cs) | 50 | Интерполяция вращения через Slerp, установка цели и телепортация. |
| [TransformInterpolator.cs](MWCOClient/MWCO/Math/TransformInterpolator.cs) | 64 | Объединение интерполяции позиции и вращения. |
| [Vector3Extensions.cs](MWCOClient/MWCO/Math/Vector3Extensions.cs) | 14 | Дополнительная интерполяция вектора с Time.deltaTime и SmoothStep. |
| [Vector3Interpolator.cs](MWCOClient/MWCO/Math/Vector3Interpolator.cs) | 50 | Интерполяция позиции через Lerp, установка цели и телепортация. |

### `MWCO/Network/`

| Файл | Строк | Назначение |
|---|---:|---|
| [INetMessage.cs](MWCOClient/MWCO/Network/INetMessage.cs) | 14 | Контракт сетевого сообщения: MessageId, Read и Write. |
| [NetLocalPlayer.cs](MWCOClient/MWCO/Network/NetLocalPlayer.cs) | 730 | Отправка состояния и действий локального игрока, денег, FSM, деталей и запросов владения. |
| [NetManager.cs](MWCOClient/MWCO/Network/NetManager.cs) | 1655 | Steam-лобби/P2P, handshake, heartbeat, чат, сохранения, подключение и отключение игроков. |
| [NetMessageHandler.cs](MWCOClient/MWCO/Network/NetMessageHandler.cs) | 103 | Регистрация типизированных обработчиков, чтение сообщения и диспетчеризация по ID. |
| [NetPickupable.cs](MWCOClient/MWCO/Network/NetPickupable.cs) | 13 | Константа недействительного ID предмета: 65535. |
| [NetPlayer.cs](MWCOClient/MWCO/Network/NetPlayer.cs) | 1848 | Удалённый игрок: модель, скин, позы/IK, анимации, интерполяция, транспорт, удерживаемые предметы и отправка P2P. |
| [NetStatistics.cs](MWCOClient/MWCO/Network/NetStatistics.cs) | 331 | Счётчики пакетов/байтов, история трафика, график и диагностическое окно Steam-сессии. |
| [NetWorld.cs](MWCOClient/MWCO/Network/NetWorld.cs) | 1753 | Регистрация игровых обработчиков, полный снимок мира, очередь загрузки, предметы и применение сетевых событий. |
| [PlayerAnimManager.cs](MWCOClient/MWCO/Network/PlayerAnimManager.cs) | 784 | Анимации ходьбы/бега/приседания, жестов и действий; машина состояний и выбор анимационных слоёв. |

### `MWCO/Network/Messages/`

| Файл | Строк | Назначение |
|---|---:|---|
| [AnimSyncMessage.cs](MWCOClient/MWCO/Network/Messages/AnimSyncMessage.cs) | 92 | ID 18. Параметры анимации, положения тела и игровых действий. |
| [AskForWorldStateMessage.cs](MWCOClient/MWCO/Network/Messages/AskForWorldStateMessage.cs) | 48 | ID 8. Запрос полного состояния мира без дополнительных полей. |
| [ChatMessage.cs](MWCOClient/MWCO/Network/Messages/ChatMessage.cs) | 91 | ID 27. Текст чата, сетевое время и необязательное числовое значение. |
| [ColorMessage.cs](MWCOClient/MWCO/Network/Messages/ColorMessage.cs) | 56 | Вложенная структура цвета RGBA из четырёх float; без отдельного сетевого ID. |
| [DisconnectMessage.cs](MWCOClient/MWCO/Network/Messages/DisconnectMessage.cs) | 48 | ID 3. Уведомление об отключении без дополнительных полей. |
| [DoorsInitMessage.cs](MWCOClient/MWCO/Network/Messages/DoorsInitMessage.cs) | 61 | Вложенная структура исходного состояния двери: позиция и открыта/закрыта. |
| [EventHookSyncMessage.cs](MWCOClient/MWCO/Network/Messages/EventHookSyncMessage.cs) | 95 | ID 22. ID FSM/события, признак запроса и необязательное имя события. |
| [FSMValueChangeMessage.cs](MWCOClient/MWCO/Network/Messages/FSMValueChangeMessage.cs) | 153 | ID 25. Изменение именованного значения FSM: bool, float или string. |
| [FullWorldSyncMessage.cs](MWCOClient/MWCO/Network/Messages/FullWorldSyncMessage.cs) | 225 | ID 7. Снимок мира: день/время, двери, предметы, мод-объекты, свет, погода и позиции появления. |
| [HandshakeMessage.cs](MWCOClient/MWCO/Network/Messages/HandshakeMessage.cs) | 60 | ID 0. Версия мода, сетевые часы и код скина при подключении. |
| [HeartbeatMessage.cs](MWCOClient/MWCO/Network/Messages/HeartbeatMessage.cs) | 52 | ID 1. Запрос проверки связи с часами клиента. |
| [HeartbeatResponseMessage.cs](MWCOClient/MWCO/Network/Messages/HeartbeatResponseMessage.cs) | 56 | ID 2. Ответ проверки связи с часами клиента и отправителя. |
| [HouseSwitchMessage.cs](MWCOClient/MWCO/Network/Messages/HouseSwitchMessage.cs) | 95 | ID 24. Переключение устройства дома, булево и необязательное числовое значение. |
| [LightSwitchMessage.cs](MWCOClient/MWCO/Network/Messages/LightSwitchMessage.cs) | 69 | ID 16. Позиция и состояние выключателя света. |
| [MessageIds.cs](MWCOClient/MWCO/Network/Messages/MessageIds.cs) | 39 | Перечисление 31 кода протокола, 0–30. |
| [MessageIdsHelpers.cs](MWCOClient/MWCO/Network/Messages/MessageIdsHelpers.cs) | 16 | Проверка верхней границы кода сообщения; отрицательные int отдельно не отвергаются. |
| [ModChangeMessage.cs](MWCOClient/MWCO/Network/Messages/ModChangeMessage.cs) | 180 | ID 30. Изменение bool/int/string/float у мод-объекта; регистрации обработчика не найдено. |
| [ModOSCMessage.cs](MWCOClient/MWCO/Network/Messages/ModOSCMessage.cs) | 65 | Вложенная структура мод-объекта: имя, ID и Transform. |
| [ObjectSyncMessage.cs](MWCOClient/MWCO/Network/Messages/ObjectSyncMessage.cs) | 187 | ID 20. ID объекта, позиция/вращение, необязательный тип синхронизации, float-массив и признак подбора. |
| [ObjectSyncRequestMessage.cs](MWCOClient/MWCO/Network/Messages/ObjectSyncRequestMessage.cs) | 52 | ID 23. Запрос синхронизации по ID объекта. |
| [ObjectSyncResponseMessage.cs](MWCOClient/MWCO/Network/Messages/ObjectSyncResponseMessage.cs) | 56 | ID 21. Ответ на запрос синхронизации: ID объекта и accepted. |
| [OpenDoorsMessage.cs](MWCOClient/MWCO/Network/Messages/OpenDoorsMessage.cs) | 69 | ID 6. Открыть/закрыть обычную дверь по позиции. |
| [OrderListMessage.cs](MWCOClient/MWCO/Network/Messages/OrderListMessage.cs) | 138 | ID 29. Список заказов и список услуг Fleetari с маской необязательных полей. |
| [PickedUpSync.cs](MWCOClient/MWCO/Network/Messages/PickedUpSync.cs) | 68 | Вложенная структура позиции и вращения удерживаемого предмета. |
| [PickupableActivateMessage.cs](MWCOClient/MWCO/Network/Messages/PickupableActivateMessage.cs) | 56 | ID 13. Включение/выключение предмета по ID. |
| [PickupableDestroyMessage.cs](MWCOClient/MWCO/Network/Messages/PickupableDestroyMessage.cs) | 52 | ID 12. Удаление предмета по ID. |
| [PickupableSetPositionMessage.cs](MWCOClient/MWCO/Network/Messages/PickupableSetPositionMessage.cs) | 69 | ID 14. Установка позиции предмета по ID. |
| [PickupableSpawnMessage.cs](MWCOClient/MWCO/Network/Messages/PickupableSpawnMessage.cs) | 153 | ID 11. Создание предмета: ID, префаб, Transform, активность, родительский триггер и дополнительные данные. |
| [PlayerSyncMessage.cs](MWCOClient/MWCO/Network/Messages/PlayerSyncMessage.cs) | 229 | ID 4. Позиция/вращение игрока, направление взгляда, удерживаемый предмет и необязательные атрибуты. |
| [QuaternionMessage.cs](MWCOClient/MWCO/Network/Messages/QuaternionMessage.cs) | 56 | Вложенная структура кватерниона в порядке w, x, y, z. |
| [SatsumaSwitchMessage.cs](MWCOClient/MWCO/Network/Messages/SatsumaSwitchMessage.cs) | 280 | ID 26. Сборка/разборка детали, болт, триггер, числовое значение и цвет; сохранено историческое название. |
| [SaveMessage.cs](MWCOClient/MWCO/Network/Messages/SaveMessage.cs) | 90 | ID 28. Три массива байтов: основной сейв, предметы и детали машины. |
| [TransformMessage.cs](MWCOClient/MWCO/Network/Messages/TransformMessage.cs) | 68 | Вложенная пара позиции и вращения. |
| [Vector3Message.cs](MWCOClient/MWCO/Network/Messages/Vector3Message.cs) | 52 | Вложенная структура трёх float: x, y, z. |
| [VehicleEnterMessage.cs](MWCOClient/MWCO/Network/Messages/VehicleEnterMessage.cs) | 56 | ID 9. Вход в транспорт по ID и указание места. |
| [VehicleInitMessage.cs](MWCOClient/MWCO/Network/Messages/VehicleInitMessage.cs) | 61 | Вложенная структура исходного состояния транспорта: байтовый ID и Transform. |
| [VehicleLeaveMessage.cs](MWCOClient/MWCO/Network/Messages/VehicleLeaveMessage.cs) | 48 | ID 10. Выход из транспорта без дополнительных полей. |
| [VehicleStateMessage.cs](MWCOClient/MWCO/Network/Messages/VehicleStateMessage.cs) | 95 | ID 5. ID машины, состояние двигателя/зажигания и необязательное время старта. |
| [VehicleSwitchMessage.cs](MWCOClient/MWCO/Network/Messages/VehicleSwitchMessage.cs) | 126 | ID 19. Переключатель транспорта: ID объекта/переключателя, bool и необязательные float/string. |
| [WeatherUpdateMessage.cs](MWCOClient/MWCO/Network/Messages/WeatherUpdateMessage.cs) | 130 | ID 17. Позиция/вращение облаков, погодное состояние и активность облаков. |
| [WorldPeriodicalUpdateMessage.cs](MWCOClient/MWCO/Network/Messages/WorldPeriodicalUpdateMessage.cs) | 73 | ID 15. Время суток, день и погода; регистрации обработчика не найдено. |

### `MWCO/Properties/`

| Файл | Строк | Назначение |
|---|---:|---|
| [Resources.cs](MWCOClient/MWCO/Properties/Resources.cs) | 50 | Обёртка ResourceManager и выбор культуры; собственных свойств игровых ассетов нет. |
| [Resources.resources](MWCOClient/MWCO/Properties/Resources.resources) | — | Пустой бинарный контейнер .NET-ресурсов: 180 байт, 0 записей. |

### `MWCO/UI/`

| Файл | Строк | Назначение |
|---|---:|---|
| [Button.cs](MWCOClient/MWCO/UI/Button.cs) | 131 | Кнопка в 3D-интерфейсе: текст, коллайдер, hover-масштабирование и событие нажатия. |
| [Console.cs](MWCOClient/MWCO/UI/Console.cs) | 256 | Регистрация/выполнение команд, журнал сообщений, история ввода и управление фокусом. |
| [CustomButton.cs](MWCOClient/MWCO/UI/CustomButton.cs) | 170 | Модификация кнопок главного меню, их текста, коллайдеров и FSM-переходов. |
| [HoverUI.cs](MWCOClient/MWCO/UI/HoverUI.cs) | 35 | Показ/скрытие дочернего визуального элемента при наведении. |
| [MPGUI.cs](MWCOClient/MWCO/UI/MPGUI.cs) | 50 | Управление видимостью курсора через счётчик запросов. |

### `MWCO/Utilities/`

| Файл | Строк | Назначение |
|---|---:|---|
| [ErrorHandling.cs](MWCOClient/MWCO/Utilities/ErrorHandling.cs) | 52 | Логирование критических ошибок, Win32 MessageBox, Assert и завершение приложения. |
| [GameObjectExtensions.cs](MWCOClient/MWCO/Utilities/GameObjectExtensions.cs) | 103 | Операции над векторами, деревом Transform и компонентами, клонирование и сохранение объекта между сценами. |
| [IMGUIUtils.cs](MWCOClient/MWCO/Utilities/IMGUIUtils.cs) | 48 | Вспомогательная отрисовка одноцветных прямоугольников и подписей. |
| [Logger.cs](MWCOClient/MWCO/Utilities/Logger.cs) | 125 | Локальные журналы в каталоге MWCO, информация о системе/версии, уровни логирования и вывод в консоль. |
| [RichTextExtensions.cs](MWCOClient/MWCO/Utilities/RichTextExtensions.cs) | 124 | Разметка Unity Rich Text: цвета, размер, жирный/курсив; замечены ошибки закрывающих тегов. |
| [Styles.cs](MWCOClient/MWCO/Utilities/Styles.cs) | 234 | Создание GUIStyle и фоновой текстуры; свойства содержат декомпилированные backing fields. |

### `Properties/`

| Файл | Строк | Назначение |
|---|---:|---|
| [AssemblyInfo.cs](MWCOClient/Properties/AssemblyInfo.cs) | 17 | Метаданные сборки: версия 0.2.3.0, продукт My Winter Car Online, компания MWCO Team. |

### `System/Net/`

| Файл | Строк | Назначение |
|---|---:|---|
| [SecurityProtocolTypeExtensions.cs](MWCOClient/System/Net/SecurityProtocolTypeExtensions.cs) | 11 | Константы TLS 1.1/1.2; совместимость с целевыми framework/Mono нужно проверить при сборке. |

**Итого: описаны все 147 файлов архива.** Единственный файл исходного корня репозитория — [MWCOClient.zip](MWCOClient.zip); этот отчёт добавлен отдельно и в архив не входит.

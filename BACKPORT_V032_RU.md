# Бэкпорт фиксов MWCO по чейнджлогу (v0.2.3 → «v032h1»)

Дата: 22 сентября 2026 г.
Основание: официальные патчноуты из [RELEASE_NOTES_RU.md](RELEASE_NOTES_RU.md), код — [MWCOClient/](MWCOClient/) (декомпилированная v0.2.3).

> ⚠️ **Честная оговорка.** Исходники v0.2.3 декомпилированы и **не компилируются** (см. обзор), рабочего сборщика и самой игры здесь нет — проверить правки компиляцией или в игре негде. Все правки сделаны по образцу существующего кода, с try/catch-защитой в том же стиле, но **это правки вслепую**: прогон на реальной сборке MWC обязателен.
> Из-за расширения протокола версия поднята: `Client.ModVersion = "0.2.3-v032h1"` — лобби с несовпадающей версией штатно отклоняются проверкой версий, молчаливого рассинхрона со стоковой 0.2.3 не будет.

## Реализовано

### 1. Хотфикс машин (v0.3.2)

**1a. «Remote cars now receive the owner's rigidbody velocity with the pose» + «parked cars no longer rubberband»**

- `Network/Messages/ObjectSyncMessage.cs`: в `optionalsMask` добавлены биты `8` (velocity, `Vector3Message`), `16` (angularVelocity, `Vector3Message`), `32` (wheelRpms, `int` + `float[]`) со свойствами `Velocity` / `AngularVelocity` / `WheelRpms` (+`Has*`), запись/чтение в том же порядке бит.
- `Game/Components/ObjectSyncComponent.cs`:
  - `SendObjectSync()` — при наличии rigidbody (не kinematic) прикладывает к позе `velocity`/`angularVelocity`; у машины с локальным водителем — ещё и RPM колёс (`PlayerVehicle.GetWheelRpms()`), rigidbody кешируется (`GetSyncedRigidbody()`);
  - `SetRemoteBodyState(...)` — на приёмной стороне записывает полученные `velocity`/`angularVelocity` в rigidbody объекта: «остаточная физика» от интерполяции больше не тянет машину;
  - `ShouldInterpolate()` — если получена скорость ~0 (машина припаркована), поза применяется мгновенно (snap) вместо интерполяции — убирает rubberband припаркованных машин.
- `Network/NetWorld.cs` — обработчик `ObjectSyncMessage` (и его compiler-generated зеркало) вызывает `SetRemoteBodyState(...)` при наличии бита velocity. Старые объекты/клиенты без бита работают по-старому.
- `Network/NetLocalPlayer.cs` — перегрузка `SendObjectSync(...)` с параметрами `velocity`, `angularVelocity`, `wheelRpms`.

**1b. «Throttle/brake stuck when the driver exited the vehicle» (RPM ревёт с пустым сиденьем)**

- `Game/Objects/PlayerVehicle.cs`: новый метод `ResetRemoteInputs()` — обнуляет `Throttle` и `Brake` (сеттеры пишут и в `mpCarController.remoteThrottleInput/remoteBrakeInput`, и в `axisCarController.throttleInput/brakeInput`).
- `Network/NetPlayer.cs` → `LeaveVehicle()`: при выходе **удалённого** игрока-водителя вызывается `playerVehicle.ResetRemoteInputs()` — наблюдатели больше не застревают с последними значениями газа/тормоза. Соответствует также пункту v0.3.1 «stuck throttle from a leftover pose при посадке» — застревание было из-за несброшенных значений.

**1c. «Teleporting a car using F4 menu no longer makes it fly across the map / roll backwards forever»**

- `DevTools.cs`: новый хелпер `ResetObjectMomentum(GameObject)` — обнуляет `rigidbody.velocity` и `angularVelocity` у телепортируемого объекта; подключён в обе телепортирующие ветки: F4-меню (`spawnVehicle` в `UpdatePlayer()`) и консольная команда `gethere` (inline-версия и её compiler-generated зеркало). Раньше машина сохраняла импульс на новом месте. Удалённые копии дополнительно защищены пунктом 1a (в новой позе приезжает скорость 0).

**1d. «Added wheel roll sync»**

- `PlayerVehicle.GetWheelRpms()` — снимает `WheelCollider.rpm` со всех колёс (массив кешируется лениво, поиск по `ParentGameObject.GetComponentsInChildren<WheelCollider>(true)`).
- Отправка: только когда `DriverIsLocal` (пункт 1a в `ObjectSyncComponent.SendObjectSync`).
- Приём: `ObjectSyncComponent.ApplyReceivedWheelRpms()` (каждый кадр из `Update()`) — интегрирует вращение: `rpm * 6 * Time.deltaTime` градусов по локальной оси X на transform каждого WheelCollider. **Best-effort**: визуальная модель колеса может быть отдельным дочерним мешем — без игровых DLL точное соответствие не гарантируем; ошибки ловятся и глушатся.

### 2. Оптимизация размера пакетов (v0.3.0, «−75% трафика»)

- `Network/NetManager.cs` (3 места: `BroadcastMessage`, `BroadcastMessageToSteamID`, `SendMessage`): `memoryStream.GetBuffer()` → `memoryStream.ToArray()`.
- Раньше отправлялась **ёмкость** внутреннего буфера, а не фактические `stream.Length` байт — хвост каждого пакета был забит мусором (отмечено в нашем обзоре). Именно здесь живёт основная часть заявленной экономии трафика.

## Не переносится вслепую (нужны исходники игры / новый билд MWC / исходники MWCO v0.3.x)

Просто чтобы зафиксировать: следующие пункты чейнджлога требуют знаний о внутренностях игры, которых в v0.2.3 нет, или целых новых подсистем — правильно реализовать их без исходников v0.3.x невозможно:

- v0.3.0: радио, мастерская Флитари, доставка рекламы, блошиный рынок, полиция (блокпосты/погони), дом престарелых, ДТП NPC, предохранители, обмерзание, азартные игры (4 вида), такси/завод/PSK-улучшения, кеш возврата в лобби, сброс мода в главное меню, частицы дыхания, Discord Rich Presence, курсор после кика, «Version mismatch» при инвайтах, мопед Jonnez, звонки, никнеймы, крюк.
- v0.3.1: object sync при модах/позднем входе/реконнекте (надёжность ID), баги базовой игры Corris Rivett (детали двигателя, анимации руля/КПП — у нас `Rivett.cs` ещё заглушка), точный таргет болтов/деталей.
- v0.2.4–v0.2.5: паб Теймо, сауна, гриль, инструмент в руках, скребок, новые запчасти Rivett, рассинхрон мира, завод/магазин без хоста, «Scrape Window», PSK/ATM, shopping bags, AI-трафик, триггеры.

Если пришлёшь дамп/исходники v0.3.x — сделаю настоящий diff и перенесу недостающее по образцу оригинала.

## Проверки выполненных правок

- Каждая правка применена транзакционно с проверкой уникальности якоря (иначе стоп); баланс скобок `{}` совпадает во всех 9 файлах; `GetBuffer()` не осталось; 147 файлов на месте, состав набора не изменился.
- Порядок бит/полей в `ObjectSyncMessage.Write` ↔ `Read` совпадает; старые пути (без новых бит) не менялись.
- Компиляция/запуск недоступны в этом окружении — см. оговорку выше.

## Изменённые файлы (9)

| Файл | Что |
|---|---|
| `MWCO/Client.cs` | `ModVersion` → `0.2.3-v032h1` (гейтинг по версии) |
| `MWCO/Network/NetManager.cs` | `GetBuffer()` → `ToArray()` (×3) |
| `MWCO/Network/Messages/ObjectSyncMessage.cs` | новые биты 8/16/32: velocity, angularVelocity, wheelRpms |
| `MWCO/Network/NetLocalPlayer.cs` | перегрузка `SendObjectSync(...)` с телом состояния |
| `MWCO/Network/NetWorld.cs` | приём velocity/wheels (обработчик + зеркало) |
| `MWCO/Network/NetPlayer.cs` | сброс вводов при выходе удалённого водителя |
| `MWCO/Game/Components/ObjectSyncComponent.cs` | сбор/приём body state, parked-snap, wheel roll |
| `MWCO/Game/Objects/PlayerVehicle.cs` | `ResetRemoteInputs()`, `GetWheelRpms()` + кеш |
| `MWCO/DevTools.cs` | `ResetObjectMomentum()` + вызовы в телепортах |

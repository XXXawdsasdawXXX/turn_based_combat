# turn_based_combat

## Тест
веб: https://itch.io/game/edit/5286606#published

## Старт

Открыть сцену `Assets/Scenes/SampleScene.unity` и нажать Play.  
На сцене находятся `GameScope` и `UIScope` (VContainer LifetimeScope) — через них поднимаются зависимости и стартует UI.

## Поток

1. `StartWindowController` показывает стартовое окно. По кнопке вызывает `BattleController.StartBattle()` и через `UIWindowNavigator` открывает окно боя.
2. `BattleController` управляет сессией: собирает команды из `TeamsConfiguration` / `CharacterConfiguration`, создаёт персонажей через `CharacterFactory` и ведёт раунды с ходами.
3. Состояние боя хранится в `BattleState` (фаза, команды, очередь раунда, текущий ход, combat log). UI подписан на эти данные через R3.
4. Ход проходит так: `PlayerTurnDecider` / `EnemyTurnDecider` → `TurnChoice` → `ActionResolver` (правила способностей) → `ActionRunner` (применение и анимации).
5. Окна паузы и конца игры тоже завязаны на `BattleController` и тот же навигатор.


- **VContainer** — явная регистрация зависимостей в scope’ах, без ручного поиска объектов.
- **UniTask** — async-цикл боя с отменой через `CancellationToken`.
- **R3** — реактивный `BattleState`, UI обновляется по подпискам.
- **DOTween** — анимации персонажей и UI.

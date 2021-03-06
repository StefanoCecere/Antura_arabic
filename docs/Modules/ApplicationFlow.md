# Application Flow

* TOC
{:toc}

`@todo: make sure to add a summary to all the main managers so to generate the docs.`
`@todo: add an app flow diagram too?`

## Core and Minigame code

The code of EA4S is separated in two main sections.

- **core** code is related to the main application.
  This code should not be touched by mini-games developers.
- **minigame** code is produced by minigame developers. This code should be dependant on the core code, but not the other way around.
This allows minigames to be created and removed at will.

Note that part of the *core* code can be used by minigame code, such as *Tutorial* or *UI* code, this is referred to as *shared minigames code* and is part of the core application.


## App Manager

The App Manager represents the core of the application.
It functions as a general manager and entry point for all other systems and managers.
It is instantiated as a Singleton, accessible as **AppManager.Instance****.

The App Manager is used to start, reset, pause, and exit the game.
It also controls the general flow of the application.

The **AppManager.GameSetup()** method functions as the entry point of the application, regardless of player profile.
All subsystems initialisation is carried out in this method.

The **AppManager.InitTeacherForPlayer()** method instead initializes  all subsystems and loads all data related to a specific player profile and must be called whenever a new profile is selected.

## Application Flow

This section details the flow of the player inside the application and what classes and sub-systems are affected.

The flow of the whole application is handled by the *Navigation Manager*, which controls the transitions between different scenes in the application.

### Home and Intro

The entry point for the application is the **Start scene** (`app/_scenes/_Start`), managed by the *Home Manager*.
This scene initialises the *App Manager*, shows the *Profile Selector UI* to allow the user to select a profile throught the *Player Profile Manager*.
This is performed through a set call to the **PlayerProfileManager.CurrentPlayer** property.

The static learning database, the player's logging database, and the teacher system are loaded at this point through a call to **AppManager.InitTeacherForPlayer()**.
After the profile selection is confirmed through the UI, the *Home Manager* calls the *Navigation Manager* to advance the application.

The application flow may then change depending on whether we are using a new profile (*first encounter*) or not.

If we are in the *first encounter* phase, the *Navigation Manager* will first load the **Intro scene**, which is controlled by an *Intro Manager*.
From there, the user eventually accesses the **Map scene**.
If the first encounter is instead passed, the Map is accessed directly.

### The Map scene

The **Map scene** functions as a central hub for the player.
Stages (map levels), learning blocks (ropes) and play sessions (dots, with larger dots representing  assessments) are setup according to the data obtained from the database on stages, learning blocks, and play sessions.
This is achieved through *EA4S.Map.MiniMap.GetAllPlaySessionStateForStage()*, which, for a given stage, obtains the data of available play sessions.
`** Note: this is not yet implemented in the mini map scripts **`

The user may navigate the map through the UI.
This is performed through **EA4S.Map.LetterMovement** (to navigate play sessions and learning blocks inside a stage) and **EA4S.Map.StageManager** (to navigate between different stages).
At each movement, a call to **EA4S.Map.LetterMovement.UpdateCurrenJourneyPosition()** sets the current journey position of the player on the map (see **PlayerProfile.md** for further details on journey positions.)
 @todo: maybe move here the stage-play sessions stuff?

The **Map scene** allows several actions to be performed through its UI:
- The user may access the **Antura Space scene**.
- The user may access the **Player Book scene**.
- The user may start a new *Play Session* by reaching one of the
  available (i.e. unlocked) pins on the map and pressing the *play* button.

### Play Session start

When the user selects *play*, the **EA4S.Map.MiniMap** method is called, which initialises the new play session by notifying the teacher system through **Teacher.TeacherAI.InitialiseCurrentPlaySession()**, which resets the current play session status and selects the minigames to play for that play session (the amount of which is defined by the constant **ConfigAI.numberOfMinigamesPerPlaySession**).
Refer to **Teacher.md** for details on minigame selection for a given play session.

`** WARNING: the CurrentMiniGameInPlaySession data is now handled by the Teacher and PlayerProfile, but this is bad. Refactor it, then detail it here. **`

Depending on whether the next play session is an Assessment or not, the navigation may change.

If the next play session is an Assessment, the *Navigation Manager* calls **GoToGameScene(MiniGameData _miniGame)** directly. Refer to the next section.

If the next play session is not an Assessment, the *Navigation Manager* instead accesses the **Game Selector scene**, which is responsible for showing in a playful way what minigames were selected by the Teacher System.
The Game Selector scene will first automatically call the method **GamesSelector.AutoLoadMinigames()**, which calls **GamesSelector.Show()** passing the list of currently selected minigames.
The method also adds the delegate **GameSelector.GoToMiniGame()** to the **GamesSelector.OnComplete** event, triggered when the user finishes interaction with the Games Selector.
*GameSelector.GoToMiniGame()** will at last signal the *Navigation Manager* to access the first selected minigame.

### MiniGame Start

Any call to **NavigationManager.GoToGameScene(MiniGameData _miniGame)** triggers a subseuqnet call to **MiniGameLauncher.LaunchGame(MiniGameCode miniGameCode)** to launch the next of the minigames for that play session.

The **MiniGameLauncher** is responsible for correcting loading minigames with teacher-approved data, given a **MiniGameCode** that represents what minigame to load.
The start of a minigame is initialised by a call to **MiniGameLauncher.LaunchGame(MiniGameCode miniGameCode)**.
The launcher then calls  **TeacherAI.GetCurrentDifficulty(MiniGameCode miniGameCode)** to obtain the difficulty value for the specific minigame session, generates a **GameConfiguration** instance with the correct difficulty settings,  and starts the minigame through **MiniGameAPI.StartGameMiniGameCode(MiniGameCode _gameCode, GameConfiguration _gameConfiguration)**

The **MiniGameAPI.StartGameMiniGameCode** method first retrieves the data related to the specified minigame from the database for later retrieval and assigns it to **AppManager.CurrentMinigame**
The process then calls **MiniGameAPI.ConfigureMiniGame** to retrieve the concrete **IGameConfiguration** and **IGameContext** for the given minigame code, assigning them to the minigame static **IGameConfiguration** concrete implementation.

At this point, the Teacher System is queried in order to retrieve a set of **QuestionPack** instances that define the learning content that the minigame should access and that are accessible through the **IGameConfiguration.Questions** field.
Refer to **Teacher.md** and **MiniGame.md** for further details on how the learning data is selected and passed to minigames.

At last, the minigame being correctly configured, it can be started, and the *Navigation Manager* will thus load the scene that matches the specific minigame.

### MiniGame Play

Minigames are responsible for handling their internal state, while the core application waits for the minigame to end.
Refer to **MiniGame.md** for details on how the minigame flow is implemented.

### MiniGame End

The minigame logic is required to call **MiniGame.EndGame()** to end gameplay.

As a minigame ends, the end game panel is shown, and  after user interaction the game is exited.
_note that the actual flow is_
```OutcomeGameState.EnterState() ->
MinigamesStarsWidget.Show() ->
GameResultUI.ShowEndgameResult() ->
EndgameResultPanel.Show() ->
EndgameResultPanel.Continue()
```

As a minigame ends, the *Navigation Manager* may either:
- start the next minigame (refer to the *MiniGame Start* section)
- access the **Play Session Results scene** if the minigame was the last for the play session
- or access the **Rewards scene** if an assessment play session was completed

From the Play Session Results scene, or from the Rewards Scene, the player will then return to the Map scene, updating the maximum reached journey position through **NavigationManager.MaxJourneyPositionProgress()** if needed.

## Refactoring notes

- AppManager and InstantiateManager spawn managers in several ways, standardize manager creation and access.
- Many subsystems have their own singleton and should instead be represented in AppManager

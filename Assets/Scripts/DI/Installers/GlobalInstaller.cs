using Attributes;
using FMODScripts;
using SaveLoadSystem.Data;
using Serializers;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;
using VContainer.Unity;

public class GlobalInstaller : LifetimeScope {
    [SerializeField, Required] private DialogueDictionary dialogueDictionary;
    [SerializeField, Required] private AudioDictionarySO audioDictionary;
    [SerializeField, Required] private SaveSlots saveSlots;
    [SerializeField, Required] private GameDataScrubGeneric<SaveGameData> newGameData;

    protected override void Configure(IContainerBuilder builder) {
        // --- DebugLogger System --- \\
        builder.Register<IProvider<DebugLoggerSO>, DebugLoggerProvider>(Lifetime.Singleton);
        
        // --- Dialogue System --- \\
        builder.RegisterInstance(dialogueDictionary);
        
        // --- Pause System --- \\
        builder.RegisterInstance<IPauseManager>(new PauseManager(1));

        // --- Achievement System --- \\
        builder.Register<IAchievementManager, SteamAchievementManager>(Lifetime.Singleton);

        // --- Audio System --- \\
        builder.RegisterInstance(audioDictionary);
        builder.Register<IAudioManager, FMODManager>(Lifetime.Singleton);

        // --- Controller Recognizer System --- \\
        builder.Register<ControllerRecognizer>(Lifetime.Singleton);
        builder.Register<IControlSchemeChanger, ControlSchemeChanger>(Lifetime.Singleton);
        
        // --- Save/Load System --- \\
        builder.RegisterInstance(saveSlots);
        builder.RegisterInstance(newGameData);
        builder.Register<FileDataService<SaveGameData>>(Lifetime.Singleton)
            .As<IDataService<SaveGameData>>();
        builder.Register<JSONSerializer>(Lifetime.Singleton)
            .As<ISerializer>();
        builder.Register<DataBinder>(Lifetime.Singleton);
        builder.Register<CheckpointAndFileSaveSystem>(Lifetime.Singleton)
            .As<ISaveLoadSystemManager>();
    }
}
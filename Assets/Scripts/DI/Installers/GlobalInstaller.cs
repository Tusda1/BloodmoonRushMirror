using Attributes;
using FMODScripts;
using SaveLoadSystem.Data;
using SceneCollections;
using Serializers;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;
using VContainer.Unity;
#if STEAM_BUILD && !DEVELOPMENT_BUILD
using Encryption;
#endif

public class GlobalInstaller : LifetimeScope {
    [SerializeField, Required] private DialogueDictionary dialogueDictionary;
    [SerializeField, Required] private DialogueUI_DataContainer dialogueUIDataContainer;
    
    [SerializeField, Required] private AudioDictionarySO audioDictionary;
    [SerializeField, Required] private SaveSlots saveSlots;
    [SerializeField, Required] private GameDataScrubGeneric<SaveGameData> newGameData;
    
    [SerializeField] private SceneCollectionSO[] sceneCollections;

    protected override void Configure(IContainerBuilder builder) {
        // --- DebugLogger System --- \\
        builder.Register<DebugLoggerProvider>(Lifetime.Singleton)
            .As<IProvider<DebugLoggerSO>>();
        
        // --- Dialogue System --- \\
        builder.RegisterInstance(dialogueDictionary);
        builder.RegisterInstance(dialogueUIDataContainer);
        
        // --- Pause System --- \\
        builder.RegisterInstance<IPauseManager>(new PauseManager(1));

        // --- Achievement System --- \\
        builder.Register<SteamAchievementManager>(Lifetime.Singleton)
            .As<IAchievementManager>();

        // --- Audio System --- \\
        builder.RegisterInstance(audioDictionary);
        builder.Register<FMODManager>(Lifetime.Singleton)
            .As<IAudioManager>();

        // --- Controller Recognizer System --- \\
        builder.Register<ControllerRecognizer>(Lifetime.Singleton);
        builder.Register<IControlSchemeChanger, ControlSchemeChanger>(Lifetime.Singleton);

        // --- Scene Collections --- \\
        builder.Register<SceneCollectionProvider>(Lifetime.Singleton)
            .WithParameter("sceneCollections", sceneCollections)
            .As<IProvider<SceneCollectionSO>>();
        
        // --- Save/Load System --- \\
        builder.RegisterInstance(saveSlots);
        builder.RegisterInstance(newGameData);
        builder.Register<FileDataService<SaveGameData>>(Lifetime.Singleton)
            .As<IDataService<SaveGameData>>();
#if STEAM_BUILD && !DEVELOPMENT_BUILD
        builder.Register<EncryptedSerializer>(Lifetime.Singleton)
            .As<ISerializer>();
#else
        builder.Register<JSONSerializer>(Lifetime.Singleton)
            .As<ISerializer>();
#endif
        builder.Register<DataBinder>(Lifetime.Singleton);
        builder.Register<CheckpointAndFileSaveSystem>(Lifetime.Singleton)
            .As<ISaveLoadSystemManager>();
    }

    private void OnApplicationQuit() {
        Application.Quit();
    }
}
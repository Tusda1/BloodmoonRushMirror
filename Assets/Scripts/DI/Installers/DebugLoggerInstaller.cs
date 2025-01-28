// using UnityEngine;
// using Zenject;
//
// [CreateAssetMenu(
//     fileName = nameof(DebugLoggerInstaller), 
//     menuName = "Installers/" + nameof(DebugLoggerInstaller))]
// public class DebugLoggerInstaller : ScriptableObjectInstaller<DebugLoggerInstaller> {
//     public override void InstallBindings() {
//         var debugLoggers = ScrubUtils.GetAllScrubsInResourceFolder<DebugLoggerSO>("DebugLoggers");
//         
//         foreach (var debugLogger in debugLoggers) {
//             Container.BindInstance(debugLogger).WithId(debugLogger.name);
//         }
//     }
// }
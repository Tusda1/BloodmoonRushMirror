using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Utilities.UI.DisplaySettings {
    [CreateAssetMenu(fileName = "AspectRatioResolutionsScrub", menuName = "Scrubs/SettingsMenu/AspectRatioResolutionsScrub")]
    public class AspectRatioResolutionsScrub : ScriptableObject {
        [SerializeField] public SerializedDictionary<string, Vector2Int[]> aspectRatioResolutions = new() {
            {
                "16:9", new Vector2Int[] {
                    new(3840, 2160),
                    new(2560, 1440),
                    new(1920, 1080),
                    new(1600, 900),
                    new(1366, 768)
                }
            }, {
                "16:10", new Vector2Int[] {
                    new(2560, 1600),
                    new(1920, 1200),
                    new(1680, 1050),
                    new(1440, 900)
                }
            }, {
                "4:3", new Vector2Int[] {
                    new(1600, 1200),
                    new(1280, 1024),
                    new(1024, 768)
                }
            }, {
                "21:9", new Vector2Int[] {
                    new(3440, 1440),
                    new(2560, 1080)
                }
            }
        };
        
        public static string ResolutionToString(Vector2Int resolution) => $"{resolution.x}:{resolution.y}";

        public IEnumerable<Vector2Int> GetResolutionForAspectRatio(string ratio) {
            aspectRatioResolutions.TryGetValue(ratio, out var resolutions);
            return resolutions;
        }

        public static Vector2Int StringToVector2Int(string resolution) {
            var parts = resolution.Split(':');
            return new Vector2Int(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]));
        }
        
        public string FindKeyByValue(Vector2Int target) {
            return (from keyValuePair in aspectRatioResolutions where keyValuePair.Value
                .Any(vector2Int => vector2Int == target) select keyValuePair.Key)
                .FirstOrDefault();
        }
    }
}
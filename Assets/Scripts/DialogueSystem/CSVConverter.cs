#if UNITY_EDITOR
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using static DialogueCharacterSO;
using static DialogueDictionary;

namespace DialogueSystem {
    public static class CSVConverter {
        public static void CSVToSO(string filePath, DialogueDictionary so) {
            var csvContent = File.ReadAllText(filePath);

            SetDictionaryData(so, csvContent);
        }

        private static void SetDictionaryData(DialogueDictionary so, string csvContent) {
            so.dialogueDictionary.Clear();

            var lines = csvContent.Split('\n');
            var csvProperties = SplitLine(lines[0]);

            var idIndex = Array.IndexOf(csvProperties, "ID");
            var characterIndex = Array.IndexOf(csvProperties, "Character");
            var textIndex = Array.IndexOf(csvProperties, "Text");
            var whenIsSFXPlayedIndex = Array.IndexOf(csvProperties, "WhenIsSFXPlayed");
            var autoContinueIndex = Array.IndexOf(csvProperties, "AutoContinue");
            var textSpeedIndex = Array.IndexOf(csvProperties, "TextSpeed");
            var soundIDIndex = Array.IndexOf(csvProperties, "SoundID");
            var characterNameIndex = Array.IndexOf(csvProperties, "CharacterName");

            const string dialogueCharactersPath = "Assets/DialogueSystem/Characters/";

            for (var i = 1; i < lines.Length; i++) {
                var values = SplitLine(lines[i]);

                var characterSOPath = dialogueCharactersPath + values[characterIndex] + ".asset";

                var id = values[idIndex];
                var character = AssetDatabase.LoadAssetAtPath<DialogueCharacterSO>(characterSOPath);
                var text = values[textIndex];
                var whenIsSFXPlayed = Enum.TryParse(values[whenIsSFXPlayedIndex], out WhenIsSfxPlayed sfxPlayedOverride)
                    ? sfxPlayedOverride
                    : character.whenIsSFXPlayed;
                var autoContinue = values[autoContinueIndex] == "yes";
                var textSpeed = float.TryParse(values[textSpeedIndex], out var parsedSpeed)
                    ? parsedSpeed
                    : character.textSpeed;
                var soundID = values[soundIDIndex] != string.Empty
                    ? values[soundIDIndex]
                    : character.talkSoundID;
                var characterName = values[characterNameIndex] != string.Empty
                    ? values[characterNameIndex]
                    : character.characterName;

                text = text
                    .Replace("\\u2019", "\\'")
                    .Replace("\\u2026", "...");

                so.dialogueDictionary.Add(id, new DialogueDictionaryValue(
                    character,
                    text,
                    whenIsSFXPlayed,
                    autoContinue,
                    textSpeed,
                    soundID,
                    characterName));
            }
        }

        private static string[] SplitLine(string line) {
            var rawSplit = Regex.Split(line.TrimEnd('\r'), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            return TrimValues(rawSplit);

            string[] TrimValues(string[] values) {
                for (var j = 0; j < values.Length; j++) {
                    values[j] = values[j].Trim();
                    if (values[j].StartsWith("\"") && values[j].EndsWith("\"")) {
                        values[j] = values[j].Substring(1, values[j].Length - 2);
                    }
                }

                return values;
            }
        }
    }
}
#endif
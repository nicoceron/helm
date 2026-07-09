using System;
using System.IO;
using UnityEngine;

namespace Lionrise
{
    public static class SaveSystem
    {
        private static string ProfilePath => Path.Combine(Application.persistentDataPath, "profile.json");
        private static string RunPath => Path.Combine(Application.persistentDataPath, "run.json");

        public static ProfileState LoadProfile()
        {
            var profile = Load<ProfileState>(ProfilePath) ?? new ProfileState
            {
                playerId = Guid.NewGuid().ToString("N")
            };
            if (string.IsNullOrWhiteSpace(profile.playerId)) profile.playerId = Guid.NewGuid().ToString("N");
            return profile;
        }

        public static RunState LoadRun() => Load<RunState>(RunPath);
        public static void SaveProfile(ProfileState profile) => Write(ProfilePath, profile);
        public static void SaveRun(RunState run) => Write(RunPath, run);

        public static void DeleteRun()
        {
            if (File.Exists(RunPath)) File.Delete(RunPath);
        }

        private static T Load<T>(string path) where T : class
        {
            if (!File.Exists(path)) return null;
            try { return JsonUtility.FromJson<T>(File.ReadAllText(path)); }
            catch (Exception exception)
            {
                Debug.LogWarning($"Ignoring unreadable save at {path}: {exception.Message}");
                return null;
            }
        }

        private static void Write<T>(string path, T value)
        {
            var temporary = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(temporary, JsonUtility.ToJson(value, true));
                if (File.Exists(path)) File.Delete(path);
                File.Move(temporary, path);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not save {path}: {exception.Message}");
            }
        }
    }
}


// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#if UNITY_WSA_10_0 && ENABLE_IL2CPP && !UNITY_EDITOR
using Microsoft.AppCenter.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.AppCenter.Unity.Internal.Utils
{
    public class UnityApplicationSettings : IApplicationSettings
    {
        private const string AppCenterSettingsKey = "AppCenterSettings";

        private static readonly object _lockObject = new object();
        private static IDictionary<string, object> _current;
        private static bool _dirty = false;

        public T GetValue<T>(string key, T defaultValue)
        {
            lock (_lockObject)
            {
                object result;
                var found = _current.TryGetValue(key, out result);
                if (found)
                {
                    // This is a workaround for a bug on UWP where Install-Id cannot be read 
                    // from Unity.PlayerPrefs, because it is saved as System.Guid, but read as a string.
                    if (typeof(T) == typeof(Guid?) && result is string)
                    {
                        try 
                        {
                            object guid = new Guid((string)result);
                            return (T)guid;
                        }
                        catch (FormatException ex)
                        {
                            Debug.LogErrorFormat("Guid cannot be parsed due to a format exception: {0}", ex.Message);
                            return defaultValue;
                        }
                    }
                    return (T)result;
                }
            }
            SetValue(key, defaultValue);
            return defaultValue;
        }

        public void SetValue(string key, object value)
        {
            lock (_lockObject)
            {
                _current[key] = value;
                _dirty = true;
            }
        }

        public bool ContainsKey(string key)
        {
            lock (_lockObject)
            {
                return _current.ContainsKey(key);
            }
        }

        public void Remove(string key)
        {
            lock (_lockObject)
            {
                _current.Remove(key);
                _dirty = true;
            }
        }

        private static IDictionary<string, object> ReadAll()
        {
            var json = PlayerPrefs.GetString(AppCenterSettingsKey, null);
            var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return settings != null ? settings : new Dictionary<string, object>();
        }

        public static void Initialize()
        {
            // Read current values.
            _current = ReadAll();

            // Create helper for coroutine.
            UnityCoroutineHelper.StartCoroutine(MainThreadCoroutine);
        }

        private static IEnumerator MainThreadCoroutine()
        {
            while (true)
            {
                yield return null;
                if (_dirty)
                {
                    string json;
                    lock (_lockObject)
                    {
                        json = JsonConvert.SerializeObject(_current);
                        _dirty = false;
                    }
                    PlayerPrefs.SetString(AppCenterSettingsKey, json);
                    PlayerPrefs.Save();
                }
            }
        }
    }
}
#endif

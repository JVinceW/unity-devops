// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#if UNITY_WSA_10_0 && !UNITY_EDITOR

#if ENABLE_DOTNET
#error .NET scripting backed is not supported. Use IL2CPP instead.
#endif

using Microsoft.AppCenter.Unity.Internal.Utils;
using Microsoft.AppCenter.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace Microsoft.AppCenter.Unity.Internal
{
    using UWPAppCenter = Microsoft.AppCenter.AppCenter;

    class AppCenterInternal
    {
        private static bool _prepared = false;
        private static object _lockObject = new object();

        public static void Configure(string appSecret)
        {
            Prepare();
            UWPAppCenter.Configure(appSecret);
        }

        public static void Start(string appSecret, Type[] services)
        {
            var nativeServiceTypes = ServicesToNativeTypes(services);
            Prepare();
            UWPAppCenter.Start(appSecret, nativeServiceTypes);
        }

        public static void Start(Type[] services)
        {
            var nativeServiceTypes = ServicesToNativeTypes(services);
            Prepare();
            UWPAppCenter.Start(nativeServiceTypes);
        }

        public static void Start(Type service)
        {
        }

        public static string GetSdkVersion()
        {
            return "";
        }

        public static void StartServices(Type[] services, int numServices)
        {
            Prepare();
            UWPAppCenter.Start(services);
        }

        public static void SetLogLevel(int logLevel)
        {
            Prepare();
            UWPAppCenter.LogLevel = (Microsoft.AppCenter.LogLevel)LogLevelFromUnity(logLevel);
        }

        public static void SetUserId(string userId)
        {
            Prepare();
            UWPAppCenter.SetUserId(userId);
        }

        public static int GetLogLevel()
        {
            Prepare();
            return (int)LogLevelFromUnity((int)UWPAppCenter.LogLevel);
        }

        public static bool IsConfigured()
        {
            Prepare();
            return UWPAppCenter.Configured;
        }

        public static void SetLogUrl(string logUrl)
        {
            Prepare();
            UWPAppCenter.SetLogUrl(logUrl);
        }
        
        public static void SetNetworkRequestsAllowed(bool isAllowed) 
        {
            Prepare();
            UWPAppCenter.IsNetworkRequestsAllowed = isAllowed;
        }

        public static bool IsNetworkRequestsAllowed() 
        {
           Prepare();
           return UWPAppCenter.IsNetworkRequestsAllowed;
        }

        public static AppCenterTask SetEnabledAsync(bool isEnabled)
        {
            Prepare();
            return new AppCenterTask(UWPAppCenter.SetEnabledAsync(isEnabled));
        }

        public static AppCenterTask<bool> IsEnabledAsync()
        {
            Prepare();
            return new AppCenterTask<bool>(UWPAppCenter.IsEnabledAsync());
        }

        public static AppCenterTask<string> GetInstallIdAsync()
        {
            Prepare();
            var installIdTask = UWPAppCenter.GetInstallIdAsync();
            var stringTask = new AppCenterTask<string>();
            installIdTask.ContinueWith(t =>
            {
                var installId = t.Result?.ToString();
                stringTask.SetResult(installId);
            });
            return stringTask;
        }

        public static void SetWrapperSdk(string wrapperSdkVersion,
                                         string wrapperSdkName,
                                         string wrapperRuntimeVersion,
                                         string liveUpdateReleaseLabel,
                                         string liveUpdateDeploymentKey,
                                         string liveUpdatePackageHash)
        {
            Prepare();
            var wrapperSdk = new Microsoft.AppCenter.WrapperSdk(wrapperSdkName, wrapperSdkVersion, wrapperRuntimeVersion, liveUpdateReleaseLabel, liveUpdateDeploymentKey, liveUpdatePackageHash);
            UWPAppCenter.SetWrapperSdk(wrapperSdk);
        }

        private static int LogLevelToUnity(int logLevel)
        {
            switch ((Microsoft.AppCenter.LogLevel)logLevel)
            {
                case Microsoft.AppCenter.LogLevel.Verbose:
                    return (int)Microsoft.AppCenter.Unity.LogLevel.Verbose;
                case Microsoft.AppCenter.LogLevel.Debug:
                    return (int)Microsoft.AppCenter.Unity.LogLevel.Debug;
                case Microsoft.AppCenter.LogLevel.Info:
                    return (int)Microsoft.AppCenter.Unity.LogLevel.Info;
                case Microsoft.AppCenter.LogLevel.Warn:
                    return (int)Microsoft.AppCenter.Unity.LogLevel.Warn;
                case Microsoft.AppCenter.LogLevel.Error:
                    return (int)Microsoft.AppCenter.Unity.LogLevel.Error;
                case Microsoft.AppCenter.LogLevel.Assert:
                    return (int)Microsoft.AppCenter.Unity.LogLevel.Assert;
                case Microsoft.AppCenter.LogLevel.None:
                    return (int)Microsoft.AppCenter.Unity.LogLevel.None;
                default:
                    return logLevel;
            }
        }

        private static Microsoft.AppCenter.LogLevel LogLevelFromUnity(int logLevel)
        {
            switch ((Microsoft.AppCenter.Unity.LogLevel)logLevel)
            {
                case Microsoft.AppCenter.Unity.LogLevel.Verbose:
                    return Microsoft.AppCenter.LogLevel.Verbose;
                case Microsoft.AppCenter.Unity.LogLevel.Debug:
                    return Microsoft.AppCenter.LogLevel.Debug;
                case Microsoft.AppCenter.Unity.LogLevel.Info:
                    return Microsoft.AppCenter.LogLevel.Info;
                case Microsoft.AppCenter.Unity.LogLevel.Warn:
                    return Microsoft.AppCenter.LogLevel.Warn;
                case Microsoft.AppCenter.Unity.LogLevel.Error:
                    return Microsoft.AppCenter.LogLevel.Error;
                case Microsoft.AppCenter.Unity.LogLevel.Assert:
                    return Microsoft.AppCenter.LogLevel.Assert;
                case Microsoft.AppCenter.Unity.LogLevel.None:
                    return Microsoft.AppCenter.LogLevel.None;
                default:
                    return (Microsoft.AppCenter.LogLevel)logLevel;
            }
        }

        public static void StartFromLibrary(Type[] services)
        {
        }

        public static Type[] ServicesToNativeTypes(Type[] services)
        {
            var nativeTypes = new List<Type>();
            foreach (var service in services)
            {
                service.GetMethod("AddNativeType").Invoke(null, new object[] { nativeTypes });
            }
            return nativeTypes.ToArray();
        }

        public static void SetMaxStorageSize(long size)
        {
            Prepare();
            UWPAppCenter.SetMaxStorageSizeAsync(size);
        }

        private static void Prepare()
        {
            lock (_lockObject)
            {
                if (!_prepared)
                {
                    UnityScreenSizeProvider.Initialize();
                    DeviceInformationHelper.SetScreenSizeProviderFactory(new UnityScreenSizeProviderFactory());

                    /**
                     * Workaround for known IL2CPP issue.
                     * Currently it is not required, but since this workaround stores settings in separate file, 
                     * and not generic unity mechanism, a migration is required to transition users who store data in old file to built-in mechanism. 
                     * See https://issuetracker.unity3d.com/issues/il2cpp-use-of-windows-dot-foundation-dot-collections-dot-propertyset-throws-a-notsupportedexception-on-uwp
                     *
                     * NotSupportedException: Cannot call method
                     * 'System.Boolean System.Runtime.InteropServices.WindowsRuntime.IMapToIDictionaryAdapter`2::System.Collections.Generic.IDictionary`2.TryGetValue(TKey,TValue&)'.
                     * IL2CPP does not yet support calling this projected method.
                     * TODO: Can be removed after migration for settings is added.
                     */
                    UnityApplicationSettings.Initialize();
                    UWPAppCenter.SetApplicationSettingsFactory(new UnityApplicationSettingsFactory());
                    _prepared = true;
                }
            }
        }
    }
}
#endif

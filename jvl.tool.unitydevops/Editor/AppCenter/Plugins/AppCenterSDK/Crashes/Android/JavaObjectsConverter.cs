// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#if UNITY_ANDROID
using Microsoft.AppCenter.Unity;
using Microsoft.AppCenter.Unity.Crashes;
using Microsoft.AppCenter.Unity.Crashes.Models;
using Microsoft.AppCenter.Unity.Internal.Utility;
using UnityEngine;

public class JavaObjectsConverter
{
    private static readonly AndroidJavaClass _errorAttachmentLogClass = new AndroidJavaClass("com.microsoft.appcenter.crashes.ingestion.models.ErrorAttachmentLog");

    public static ErrorReport ConvertErrorReport(AndroidJavaObject errorReport)
    {
        if (errorReport == null)
        {
            return null;
        }
        try
        {
            var id = errorReport.Call<string>("getId");
            var threadName = errorReport.Call<string>("getThreadName");
            var startTime = JavaDateHelper.DateTimeConvert(errorReport.Call<AndroidJavaObject>("getAppStartTime"));
            var errorTime = JavaDateHelper.DateTimeConvert(errorReport.Call<AndroidJavaObject>("getAppErrorTime"));
            var exception = ConvertException(errorReport.Call<string>("getStackTrace"));
            var device = ConvertDevice(errorReport.Call<AndroidJavaObject>("getDevice"));
            return new ErrorReport(id, startTime, errorTime, exception, device, threadName);
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("Failed to convert error report Java object to .Net object: {0}", e.ToString());
            return null;
        }
    }

    public static Exception ConvertException(string stackTrace)
    {
        return new Exception(null, stackTrace);
    }

    private static Device ConvertDevice(AndroidJavaObject device)
    {
        return new Device(
            device.Call<string>("getSdkName"),
            device.Call<string>("getSdkVersion"),
            device.Call<string>("getModel"),
            device.Call<string>("getOemName"),
            device.Call<string>("getOsName"),
            device.Call<string>("getOsVersion"),
            device.Call<string>("getOsBuild"),
            GetIntValue(device, "getOsApiLevel"),
            device.Call<string>("getLocale"),
            GetIntValue(device, "getTimeZoneOffset"),
            device.Call<string>("getScreenSize"),
            device.Call<string>("getAppVersion"),
            device.Call<string>("getCarrierName"),
            device.Call<string>("getCarrierCountry"),
            device.Call<string>("getAppBuild"),
            device.Call<string>("getAppNamespace"));
    }

    private static int GetIntValue(AndroidJavaObject javaObject, string getterName)
    {
        var integer = javaObject.Call<AndroidJavaObject>(getterName);
        return integer.Call<int>("intValue");
    }

    internal static AndroidJavaObject ToJavaAttachments(ErrorAttachmentLog[] attachments)
    {
        if (attachments == null)
        {
            return null;
        }
        var javaList = new AndroidJavaObject("java.util.ArrayList", attachments.Length);
        foreach (var errorAttachmentLog in attachments)
        {
            if (errorAttachmentLog != null)
            {
                AndroidJavaObject nativeLog = null;
                if (errorAttachmentLog.Type == ErrorAttachmentLog.AttachmentType.Text)
                {
                    nativeLog = _errorAttachmentLogClass.CallStatic<AndroidJavaObject>("attachmentWithText", errorAttachmentLog.Text, errorAttachmentLog.FileName);
                }
                else
                {
                    nativeLog = _errorAttachmentLogClass.CallStatic<AndroidJavaObject>("attachmentWithBinary", errorAttachmentLog.Data, errorAttachmentLog.FileName, errorAttachmentLog.ContentType);
                }
                javaList.Call<bool>("add", nativeLog);
            }
        }
        return javaList;
    }
}
#endif

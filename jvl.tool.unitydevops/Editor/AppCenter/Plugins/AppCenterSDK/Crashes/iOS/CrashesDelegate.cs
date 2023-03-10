// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Microsoft.AppCenter.Unity.Crashes;
using Microsoft.AppCenter.Unity.Internal.Utility;

namespace Microsoft.AppCenter.Unity.Crashes.Internal
{
    public class CrashesDelegate
    {
        static CrashesDelegate()
        {
            app_center_unity_crashes_delegate_set_should_process_error_report_delegate(ShouldProcessErrorReportNativeFunc);
            app_center_unity_crashes_delegate_set_get_error_attachments_delegate(GetErrorAttachmentsNativeFunc);
            app_center_unity_crashes_delegate_set_sending_error_report_delegate(SendingErrorReportNativeFunc);
            app_center_unity_crashes_delegate_set_sent_error_report_delegate(SentErrorReportNativeFunc);
            app_center_unity_crashes_delegate_set_failed_to_send_error_report_delegate(FailedToSendErrorReportNativeFunc);
        }

        public static void SetDelegate()
        {
            app_center_unity_crashes_set_delegate();
        }

#if ENABLE_IL2CPP
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate bool NativeShouldProcessErrorReportDelegate(IntPtr report);
        private static Crashes.ShouldProcessErrorReportHandler shouldProcessReportHandler;

        [MonoPInvokeCallback(typeof(NativeShouldProcessErrorReportDelegate))]
        public static bool ShouldProcessErrorReportNativeFunc(IntPtr report)
        {
            if (shouldProcessReportHandler != null)
            {
                ErrorReport errorReport = CrashesInternal.GetErrorReportFromIntPtr(report);
                return shouldProcessReportHandler(errorReport);
            }
            else
            {
                return true;
            }
        }

        public static void SetShouldProcessErrorReportHandler(Crashes.ShouldProcessErrorReportHandler handler)
        {
            shouldProcessReportHandler = handler;
        }

#if ENABLE_IL2CPP
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate IntPtr NativeGetErrorAttachmentsDelegate(IntPtr report);
        internal static Crashes.GetErrorAttachmentsHandler GetErrorAttachmentsHandler;

        [MonoPInvokeCallback(typeof(NativeGetErrorAttachmentsDelegate))]
        public static IntPtr GetErrorAttachmentsNativeFunc(IntPtr report)
        {
            if (GetErrorAttachmentsHandler == null)
            {
                return IntPtr.Zero;
            }
            var errorReport = CrashesInternal.GetErrorReportFromIntPtr(report);
            var logs = GetErrorAttachmentsHandler(errorReport);
            return NativeObjectsConverter.ToNativeAttachments(logs);
        }

        public static void SetGetErrorAttachmentsHandler(Crashes.GetErrorAttachmentsHandler handler)
        {
            GetErrorAttachmentsHandler = handler;
        }

#if ENABLE_IL2CPP
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate void NativeSendingErrorReportDelegate(IntPtr report);
        public static event Crashes.SendingErrorReportHandler SendingErrorReport;

        [MonoPInvokeCallback(typeof(NativeSendingErrorReportDelegate))]
        public static void SendingErrorReportNativeFunc(IntPtr report)
        {
            if (SendingErrorReport != null)
            {
                ErrorReport errorReport = CrashesInternal.GetErrorReportFromIntPtr(report);
                SendingErrorReport(errorReport);
            }
        }

#if ENABLE_IL2CPP
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate void NativeSentErrorReportDelegate(IntPtr report);
        public static event Crashes.SentErrorReportHandler SentErrorReport;

        [MonoPInvokeCallback(typeof(NativeSentErrorReportDelegate))]
        public static void SentErrorReportNativeFunc(IntPtr report)
        {
            if (SentErrorReport != null)
            {
                ErrorReport errorReport = CrashesInternal.GetErrorReportFromIntPtr(report);
                SentErrorReport(errorReport);
            }
        }

#if ENABLE_IL2CPP
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate void NativeFailedToSendErrorReportDelegate(IntPtr report, IntPtr error);
        public static event Crashes.FailedToSendErrorReportHandler FailedToSendErrorReport;

        [MonoPInvokeCallback(typeof(NativeFailedToSendErrorReportDelegate))]
        public static void FailedToSendErrorReportNativeFunc(IntPtr report, IntPtr error)
        {
            if (FailedToSendErrorReport != null)
            {
                var errorReport = CrashesInternal.GetErrorReportFromIntPtr(report);
                var exception = NSErrorHelper.Convert(error);
                FailedToSendErrorReport(errorReport, exception);
            }
        }

#region External

        [DllImport("__Internal")]
        private static extern void app_center_unity_crashes_set_delegate();

        [DllImport("__Internal")]
        private static extern void app_center_unity_crashes_delegate_set_should_process_error_report_delegate(NativeShouldProcessErrorReportDelegate functionPtr);

        [DllImport("__Internal")]
        private static extern void app_center_unity_crashes_delegate_set_get_error_attachments_delegate(NativeGetErrorAttachmentsDelegate functionPtr);

        [DllImport("__Internal")]
        private static extern void app_center_unity_crashes_delegate_set_sending_error_report_delegate(NativeSendingErrorReportDelegate functionPtr);

        [DllImport("__Internal")]
        private static extern void app_center_unity_crashes_delegate_set_sent_error_report_delegate(NativeSentErrorReportDelegate functionPtr);

        [DllImport("__Internal")]
        private static extern void app_center_unity_crashes_delegate_set_failed_to_send_error_report_delegate(NativeFailedToSendErrorReportDelegate functionPtr);

#endregion

    }
}
#endif

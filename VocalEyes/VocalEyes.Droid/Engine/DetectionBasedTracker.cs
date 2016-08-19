using System;
using System.Runtime.InteropServices;
using Android.Runtime;
using OpenCV.Core;

namespace VocalEyes.Droid.Engine
{
    public class DetectionBasedTracker
    {
        private IntPtr _mNativeObj = IntPtr.Zero;

        public DetectionBasedTracker(string cascadeName, int minFaceSize)
        {
            Java.Lang.String s = new Java.Lang.String(cascadeName);
            _mNativeObj = nativeCreateObject(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), s.Handle, minFaceSize);
        }

        public void Start()
        {
            nativeStart(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), _mNativeObj);
        }

        public void Stop()
        {
            nativeStop(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), _mNativeObj);
        }

        public void SetMinFaceSize(int size)
        {
            nativeSetFaceSize(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), _mNativeObj, size);
        }

        public void Detect(Mat imageGray, MatOfRect faces)
        {
            nativeDetect(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), _mNativeObj, imageGray.NativeObjAddr, faces.NativeObjAddr);
        }

        public void Release()
        {
            nativeDestroyObject(JNIEnv.Handle, JNIEnv.FindClass(typeof(Java.Lang.Object)), _mNativeObj);
            _mNativeObj = IntPtr.Zero;
        }

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeCreateObject")]
        private static extern IntPtr nativeCreateObject(IntPtr env, IntPtr jniClass, IntPtr cascadeName, int minFaceSize);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeDestroyObject")]
        private static extern void nativeDestroyObject(IntPtr env, IntPtr jniClass, IntPtr thiz);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeStart")]
        private static extern void nativeStart(IntPtr env, IntPtr jniClass, IntPtr thiz);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeStop")]
        private static extern void nativeStop(IntPtr env, IntPtr jniClass, IntPtr thiz);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeSetFaceSize")]
        private static extern void nativeSetFaceSize(IntPtr env, IntPtr jniClass, IntPtr thiz, int size);

        [DllImport("libdetection_based_tracker", EntryPoint = "Java_org_opencv_samples_facedetect_DetectionBasedTracker_nativeDetect")]
        private static extern void nativeDetect(IntPtr env, IntPtr jniClass, IntPtr thiz, long inputImage, long faces);
    }
}
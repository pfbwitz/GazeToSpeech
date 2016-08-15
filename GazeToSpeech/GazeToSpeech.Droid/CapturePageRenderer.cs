//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using GazeToSpeech;
//using GazeToSpeech.Droid;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.Android;

//[assembly: ExportRenderer(typeof(CapturePage), typeof(CapturePageRenderer))]

//namespace GazeToSpeech.Droid
//{
//    public class CapturePageRenderer : PageRenderer
//    {
//        public CapturePageRenderer ()
//        {
//        }

//        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
//        {
//            base.OnElementChanged(e);

//            var activity = (Activity)Context;

//            var intent = new Intent(activity, typeof(DetectActivity));
//            //intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(file));
//            activity.StartActivityForResult(intent, 0);

//            //activity.SetContentView(Resource.Layout.Main);
//            //activity.RequestedOrientation = ScreenOrientation.Landscape;

//            //var newFragment = new CaptureFragment();
//            //var ft = activity.FragmentManager.BeginTransaction();
//            //ft.Add(Resource.Id.fragment_container, newFragment);
//            //ft.Commit();
//        }
//    }
//}
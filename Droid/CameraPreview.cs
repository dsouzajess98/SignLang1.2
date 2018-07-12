using System;
using System.Drawing;
using System.Collections.Generic;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using PowerArgs;
using TensorFlow;
using System.IO;
using System.Threading.Tasks;
using Android.Widget;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Renderscripts;

namespace CustomRenderer.Droid
{
	public sealed class CameraPreview : ViewGroup, ISurfaceHolderCallback,Camera.IPreviewCallback
	{
		SurfaceView surfaceView;
		ISurfaceHolder holder;
		Camera.Size previewSize;
		IList<Camera.Size> supportedPreviewSizes;
		Camera camera;
		IWindowManager windowManager;
        
        public bool IsPreviewing { get; set; }
        readonly ImageClassifier imageClassifier = new ImageClassifier();
        public Camera Preview {
			get { return camera; }
			set {
				camera = value;
				if (camera != null) {
					supportedPreviewSizes = Preview.GetParameters ().SupportedPreviewSizes;
					RequestLayout ();
				}
			}
		}

         void Camera.IPreviewCallback.OnPreviewFrame(byte[] data, Camera camera)
        {

            
            try
            {

                Camera.Parameters parameters = camera.GetParameters();
                
                int width = parameters.PreviewSize.Width;
                int height = parameters.PreviewSize.Height;
                Android.Graphics.Bitmap bmp = BytetoMap(data,camera);

                var x=Task.Run(() => imageClassifier.RecognizeImage(bmp));
               // System.Diagnostics.Debug.WriteLine(result);
                CustomRenderer.MainPage.result.Text=x.Result;
            }
            finally
            {
              //  CustomRenderer.MainPage.result.Text = "Changed";
            }
            
            

        }
        public Android.Graphics.Bitmap BytetoMap(byte[] data,Camera camera)
        {
            Camera.Parameters parameters = camera.GetParameters();

            int width = parameters.PreviewSize.Width;
            int height = parameters.PreviewSize.Height;
            Android.Graphics.Bitmap bmp;
            MemoryStream outstr = new MemoryStream();
            Android.Graphics.YuvImage yuv = new Android.Graphics.YuvImage(data, parameters.PreviewFormat, width, height, null);
            yuv.CompressToJpeg(new Android.Graphics.Rect(0, 0, width, height), 50, outstr);
            bmp = Android.Graphics.BitmapFactory.DecodeByteArray(outstr.ToArray(), 0, outstr.ToArray().Length);
            bmp = ToGrayscale(bmp);
            //to save on device.
         /*   string path = Android.OS.Environment.ExternalStorageDirectory + "/DCIM/Collage/hand"+count.ToString()+".jpg";
            count++;
            using (var os = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                bmp.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 100, os);
            }*/
            return bmp;
            
        }
        public Android.Graphics.Bitmap ToGrayscale(Android.Graphics.Bitmap bmpOriginal)
        {
            int width, height;
            height = bmpOriginal.Height;
            width = bmpOriginal.Width;

            float[] mat = new float[]{
            0.3f, 0.59f, 0.11f, 0, 0,
            0.3f, 0.59f, 0.11f, 0, 0,
            0.3f, 0.59f, 0.11f, 0, 0,
            0, 0, 0, 1, 0,};

            Android.Graphics.Bitmap bmpGrayscale = Android.Graphics.Bitmap.CreateBitmap(width, height, Android.Graphics.Bitmap.Config.Argb8888);
            GC.Collect();
            Android.Graphics.Canvas c = new Android.Graphics.Canvas(bmpGrayscale);
            Android.Graphics.ColorMatrixColorFilter filter = new Android.Graphics.ColorMatrixColorFilter(mat);
            Android.Graphics.Paint paint = new Android.Graphics.Paint();
            paint.SetColorFilter(filter);
            c.DrawBitmap(bmpOriginal, 0, 0, paint);
            return bmpGrayscale;
        }
        public CameraPreview (Context context)
			: base (context)
		{
			surfaceView = new SurfaceView (context);
			AddView (surfaceView);

			windowManager = Context.GetSystemService (Context.WindowService).JavaCast<IWindowManager> ();

			IsPreviewing = false;
			holder = surfaceView.Holder;
			holder.AddCallback (this);
		}

		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			int width = ResolveSize (SuggestedMinimumWidth, widthMeasureSpec);
			int height = ResolveSize (SuggestedMinimumHeight, heightMeasureSpec);
			SetMeasuredDimension (width, height);

			if (supportedPreviewSizes != null) {
				previewSize = GetOptimalPreviewSize (supportedPreviewSizes, width, height);
			}
		}

		protected override void OnLayout (bool changed, int l, int t, int r, int b)
		{
			var msw = MeasureSpec.MakeMeasureSpec (r - l, MeasureSpecMode.Exactly);
			var msh = MeasureSpec.MakeMeasureSpec (b - t, MeasureSpecMode.Exactly);

			surfaceView.Measure (msw, msh);
			surfaceView.Layout (0, 0, r - l, b - t);
		}

		public void SurfaceCreated (ISurfaceHolder holder)
		{
			try {
				if (Preview != null) {
					Preview.SetPreviewDisplay (holder);
                    Preview.SetPreviewCallback(this);
                }
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (@"			ERROR: ", ex.Message);
			}
		}

		public void SurfaceDestroyed (ISurfaceHolder holder)
		{
			if (Preview != null) {
				Preview.StopPreview ();
			}
		}

		public void SurfaceChanged (ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
		{
			var parameters = Preview.GetParameters ();
			parameters.SetPreviewSize (previewSize.Width, previewSize.Height);
			RequestLayout ();

			switch (windowManager.DefaultDisplay.Rotation) {
			case SurfaceOrientation.Rotation0:
				camera.SetDisplayOrientation (90);
				break;
			case SurfaceOrientation.Rotation90:
				camera.SetDisplayOrientation (0);
				break;
			case SurfaceOrientation.Rotation270:
				camera.SetDisplayOrientation (180);
				break;
			}

			Preview.SetParameters (parameters);
            Preview.SetPreviewCallback(this);
            Preview.StartPreview ();
            
            IsPreviewing = true;
		}

        
        Camera.Size GetOptimalPreviewSize (IList<Camera.Size> sizes, int w, int h)
		{
			const double AspectTolerance = 0.1;
			double targetRatio = (double)w / h;

			if (sizes == null) {
				return null;
			}

			Camera.Size optimalSize = null;
			double minDiff = double.MaxValue;

			int targetHeight = h;
			foreach (Camera.Size size in sizes) {
				double ratio = (double)size.Width / size.Height;

				if (Math.Abs (ratio - targetRatio) > AspectTolerance)
					continue;
				if (Math.Abs (size.Height - targetHeight) < minDiff) {
					optimalSize = size;
					minDiff = Math.Abs (size.Height - targetHeight);
				}
			}

			if (optimalSize == null) {
				minDiff = double.MaxValue;
				foreach (Camera.Size size in sizes) {
					if (Math.Abs (size.Height - targetHeight) < minDiff) {
						optimalSize = size;
						minDiff = Math.Abs (size.Height - targetHeight);
					}
				}
			}

			return optimalSize;
		}
	}
}

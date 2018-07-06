using Xamarin.Forms;

namespace CustomRenderer
{
	public class CameraPreview : View
	{
		public static readonly BindableProperty CameraProperty = BindableProperty.Create (
			propertyName: "Camera",
			returnType: typeof(CameraOptions),
			declaringType: typeof(CameraPreview),
			defaultValue: CameraOptions.Front);//changed it from rear to front

		public CameraOptions Camera {
			get { return (CameraOptions)GetValue (CameraProperty); }
			set { SetValue (CameraProperty, value); }
		}
	}
}

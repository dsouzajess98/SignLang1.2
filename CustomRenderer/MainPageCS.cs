using Xamarin.Forms;

namespace CustomRenderer
{
	public class MainPageCS : ContentPage
	{
        public static Label result=new Label { ClassId="res",Text = "Bye" };
        public MainPageCS ()
		{
			Title = "Main Page";
            
            Padding = new Thickness (0, 20, 0, 0);
            Content = new StackLayout {
                Children = {
                    new Label { Text = "Camera Preview:" },
                    result,
					new CameraPreview {

						Camera = CameraOptions.Front,//changed from Rear to front
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand
					} 
				}
			};
		}
	}
}

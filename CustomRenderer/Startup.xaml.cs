using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CustomRenderer
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Startup : ContentPage
	{
		public Startup ()
		{
			InitializeComponent ();
		}
        private async void go_to_About(object sender, EventArgs e)
        {

            await Navigation.PushModalAsync(new About());
        }

        private async void go_to_Help(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new Help());
        }
        private async void go_to_Cam(object sender, EventArgs e)
        {

            await Navigation.PushModalAsync(new MainPage());
        }
    }
}
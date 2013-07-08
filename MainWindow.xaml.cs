using Q42.HueApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HueMirror
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private Timer _timer;

      private static HueClient _GetHueClient()
      {
         return new HueClient("172.18.72.207", "newdeveloper"); 
      }

      // Make window draggable.
      private void OnDragDelta(Object sender, DragDeltaEventArgs e)
      {
         Left = Left + e.HorizontalChange;
         Top = Top + e.VerticalChange;
      }

      public MainWindow()
      {
         InitializeComponent();

         // Why is this internal, lame.
         var converter = typeof(HueClient).Assembly.GetType("Q42.HueApi.HueColorConverter").GetMethod("HexFromXy", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Double), typeof(Double) }, null);

         // Poll and update the client.
         var client = _GetHueClient();
         _timer = new Timer(context =>
         {
            client.GetLightAsync("2").ContinueWith(t =>
            {
               var light = t.Result;

               var colourCoords = light.State.ColorCoordinates;
               var rgb = (String)converter.Invoke(null, new Object[] { colourCoords[0], colourCoords[1] });

               ((SynchronizationContext)context).Post(o =>
               {
                  // WindowBorder.Background.Opacity = (Double)light.State.ColorTemperature;
                  MainColour.Color = Color.FromRgb(Byte.Parse(rgb.Substring(0, 2), NumberStyles.HexNumber), Byte.Parse(rgb.Substring(2, 2), NumberStyles.HexNumber), Byte.Parse(rgb.Substring(4, 2), NumberStyles.HexNumber));
               }, null);
            }, TaskContinuationOptions.OnlyOnRanToCompletion)
            .ContinueWith(t =>
            {
               client = _GetHueClient(); // just get a new one ey
            }, TaskContinuationOptions.OnlyOnFaulted);
         }, SynchronizationContext.Current, 0, 200);
      }
   }
}

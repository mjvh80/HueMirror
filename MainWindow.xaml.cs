using Q42.HueApi;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HueMirror
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public Color MainColour
      {
         set
         {
            // I'm a wpf n00b.
            ((RadialGradientBrush)this.Resources["brush"]).GradientStops[1].Color = value;
         }
      }

      private Timer _timer;

      private static HueClient _GetHueClient()
      {
         return new HueClient("172.18.72.207", "newdeveloper"); 
      }

      // Make window draggable.
      private void OnDragDelta(Object sender, DragDeltaEventArgs e)
      {
      //   Left = Left + e.HorizontalChange;
     //    Top = Top + e.VerticalChange;
      }

      public void OnCloseClick(Object sender, EventArgs e)
      {
         this.Close();
      }

      public MainWindow()
      {
         InitializeComponent();

         // Why is this internal, lame.
         var converter = typeof(HueClient).Assembly.GetType("Q42.HueApi.HueColorConverter")
                                                   .GetMethod("HexFromXy", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Double), typeof(Double) }, null);

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
                  if (light.State.On)
                  {
                    // var alpha = (Byte)(255 * (light.State.Brightness / 255.0) * (light.State.ColorTemperature / (500.0 - 153)));
                     var alpha = (Byte)255;
                     Func<String, Byte> @byte = s => Byte.Parse(s, NumberStyles.HexNumber);
                     MainColour = Color.FromArgb(alpha, @byte(rgb.Substring(0, 2)), @byte(rgb.Substring(2, 2)), @byte(rgb.Substring(4, 2)));
                  }
                  else
                  {
                     // light is off
                     MainColour = Color.FromRgb(0, 0, 0);
                  }
               }, null);
            }, TaskContinuationOptions.OnlyOnRanToCompletion)
            .ContinueWith(t =>
            {
               // Maybe the client timed out or whatever, just get a new one ey.
               client = _GetHueClient();
            }, TaskContinuationOptions.OnlyOnFaulted);
         }, SynchronizationContext.Current, 0, 200); // poll every 200 ms say
      }

      private void DoubleAnimation_Completed(object sender, EventArgs e)
      {

      }

      private void DoubleAnimation_Completed_1(object sender, EventArgs e)
      {

      }

      private void Thumb_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         var p = e.GetPosition((IInputElement)sender);
      }
   }
}

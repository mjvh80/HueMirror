using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Xaml;

namespace HueMirror
{
   public class CustomEventTrigger : EventTrigger
   {
      public CustomEventTrigger()
      {
        
      }

      protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
      {
 	       base.OnPropertyChanged(e);
      }
   }

   public enum EventHookType
   {
      Hook,
      Obtain
   }

  // [MarkupExtensionReturnType(typeof(RoutedEventHandler))]
   public class EventExtension : MarkupExtension
   {
      // private static ConditionalWeakTable<Object, RoutedEvent> _table = new ConditionalWeakTable<Object, RoutedEvent>();


   //   private static DependencyProperty _prop = DependencyProperty.Register("EventRegister", typeof(RoutedEventArgs), typeof(EventExtension), new PropertyMetadata
   //  // private static DependencyProperty _propTarget = DependencyProperty.Register("EventInvalidation", typeof(Action), typeof(EventExtension), new PropertyMetadata
   //   {
   //       PropertyChangedCallback = _OnPropertyChanged
   //   });
   ////   private static DependencyProperty _propTarget = DependencyProperty.Register("EventInvalidation", typeof(Action), typeof(EventExtension));

   //   private static RoutedEvent _lastEvent;

   //   private static void _OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   //   {

   //   }

      private readonly EventHookType _hookType;

      public EventExtension(EventHookType hookType = EventHookType.Obtain) 
      {
         _hookType = hookType;
      }

      //public EventExtension(FrameworkElement target, Object @default)
      //{
      //   _target = target;
      //   _default = @default;
      //}



      public Object Default { get; set; }

      public String Path { get; set; }

      private static event RoutedEventHandler HandleRoutedEvent;

      //public RoutedEventArgs GetEventArgs(DependencyObject o)
      //{
      //   return o.GetValue(_prop) as RoutedEventArgs;
      //}

      //private Boolean init = false;

      public override Object ProvideValue(IServiceProvider serviceProvider)
      {
         var provideValue = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
         var target = (DependencyObject)provideValue.TargetObject;

         var rootProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
         var root = (DependencyObject)rootProvider.RootObject;


         if (_hookType == EventHookType.Hook)
         {
            return new RoutedEventHandler((o, e) =>
            {
               HandleRoutedEvent(o, e);
            });
         }
         else
         {
            var targetProp = (DependencyProperty)provideValue.TargetProperty;
            HandleRoutedEvent += (o, e) =>
            {
               var left = ((Window)root).Left;
               var top = ((Window)root).Top;

               var newVal = e.GetType().GetProperty(Path).GetValue(e);
               target.SetValue(targetProp, newVal);
            };

            if (targetProp.PropertyType.IsGenericType)
            {
               if (targetProp.PropertyType.GetGenericTypeDefinition() == typeof(System.Nullable<>))
               {
                  return TypeDescriptor.GetConverter(targetProp.PropertyType).ConvertTo(Default, targetProp.PropertyType.GetGenericArguments()[0]);
               }
            }

            return TypeDescriptor.GetConverter(targetProp.PropertyType).ConvertTo(Default, targetProp.PropertyType);
         }
      }
   
}

   //public class EventDataMarkupExtension : MarkupExtension
   //{
   //   public EventDataMarkupExtension() { }

   //   public override Object ProvideValue(IServiceProvider serviceProvider)
   //   {
   //      var provider = (IAmbientProvider)serviceProvider.GetService(typeof(IAmbientProvider));
         
   //      return 1.0;
   //   }
   //}
}

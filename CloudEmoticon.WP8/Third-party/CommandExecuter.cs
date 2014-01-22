
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
using System;

namespace NaveenDhaka
{
    public class CommandExecuter
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(CommandExecuter), new PropertyMetadata(CommandPropertyChangedCallback));

        public static readonly DependencyProperty OnEventProperty =
            DependencyProperty.RegisterAttached("OnEvent", typeof(string), typeof(CommandExecuter), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(CommandExecuter), new PropertyMetadata(null));

        public static void CommandPropertyChangedCallback(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            string onEvent = (string)depObj.GetValue(OnEventProperty);
            Debug.Assert(onEvent != null, "OnEvent must be set.");
            var eventInfo = depObj.GetType().GetEvent(onEvent);
            if (eventInfo != null)
            {
                var mInfo = typeof(CommandExecuter).GetMethod("OnRoutedEvent", BindingFlags.NonPublic | BindingFlags.Static);
                eventInfo.GetAddMethod().Invoke(depObj, new object[] { Delegate.CreateDelegate(eventInfo.EventHandlerType, mInfo) });
            }
        }
        public static ICommand GetCommand(UIElement element)
        {
            return (ICommand)element.GetValue(CommandProperty);
        }
        public static void SetCommand(UIElement element, ICommand command)
        {
            element.SetValue(CommandProperty, command);
        }
        public static string GetOnEvent(UIElement element)
        {
            return (string)element.GetValue(OnEventProperty);
        }
        public static void SetOnEvent(UIElement element, string evnt)
        {
            element.SetValue(OnEventProperty, evnt);
        }
        public static object GetCommandParameter(UIElement element)
        {
            return (object)element.GetValue(CommandParameterProperty);
        }
        public static void SetCommandParameter(UIElement element, object commandParam)
        {
            element.SetValue(CommandParameterProperty, commandParam);
        }
        private static void OnRoutedEvent(object sender, RoutedEventArgs e)
        {
            UIElement element = (UIElement)sender;
            if (element != null)
            {
                ICommand command = element.GetValue(CommandProperty) as ICommand;
                if (command != null && command.CanExecute(element.GetValue(CommandParameterProperty)))
                {
                    command.Execute(element.GetValue(CommandParameterProperty));
                }
            }
        }
    }
}
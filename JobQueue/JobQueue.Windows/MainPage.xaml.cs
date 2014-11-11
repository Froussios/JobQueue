using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ReactiveUI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JobQueue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ReactiveList<NoteViewModel> notes;
        public MainPage()
        {
            System.Diagnostics.Debug.WriteLine("App started");

            this.InitializeComponent();

            NotesViewModel notesvm = new NotesViewModel();
            notes = notesvm.Notes;

            NotesContainer.ItemsSource = notes;

            IObservable<EventPattern<RoutedEventArgs>> addNoteEvs = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                new Action<RoutedEventHandler>(ev => AddButton.Click += ev),
                new Action<RoutedEventHandler>(ev => AddButton.Click -= ev)
            );

            

            //IObservable<EventPattern<RoutedEventArgs>> deteleNoteEvs = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
            //    new Action<RoutedEventHandler>(ev => AddButton.Click += ev),
            //    new Action<RoutedEventHandler>(ev => AddButton.Click -= ev)
            //);

            
            

            
            

            addNoteEvs.Select(x => NewNoteTextBox.Text)
                       .Subscribe(x =>
                       {
                           NoteViewModel note = new NoteViewModel() { Content = x };
                           notesvm.Notes.Add(note);
                           System.Diagnostics.Debug.WriteLine("Added new note");
                       });

            
        }





        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Delete clicked");

            FrameworkElement templateTop = getParent(sender as DependencyObject, "ParentTop");
            NoteViewModel note = getDataContext<NoteViewModel>(sender as DependencyObject);

            System.Diagnostics.Debug.WriteLine(note.Content);
        }



        /// <summary>
        /// Gets all the children of this element in a flat list
        /// </summary>
        /// <param name="parent">The element whose children to get</param>
        /// <returns>A flat list of all the children</returns>
        private List<Control> allChildren(DependencyObject parent)
        {
            var list = new List<Control>();
            for (int i=0 ; i < VisualTreeHelper.GetChildrenCount(parent) ; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Control)
                    list.Add(child as Control);
                list.AddRange(allChildren(child));
            }
            return list;
        }


        private DependencyObject FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;
                // Not a framework element or is null
                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child;
                }
                else
                {
                    // Not found it - search children
                    DependencyObject nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel != null)
                        return nextLevel;
                }
            }
            return null;
        }



        private FrameworkElement getParent(DependencyObject ob, String parentName)
        {
            DependencyObject parent = ob;
            while (parent != null)
            {
                if ((parent as FrameworkElement).Name == parentName)
                    return parent as FrameworkElement;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private T getDataContext<T>(DependencyObject ob)
        {
            DependencyObject scan = ob;
            while (scan != null)
            {
                if (scan is FrameworkElement)
                {
                    FrameworkElement element = scan as FrameworkElement;
                    if (element.DataContext is T)
                        return (T) element.DataContext;
                }
                scan = VisualTreeHelper.GetParent(scan);
            }
            return default(T);
        }
    }
}

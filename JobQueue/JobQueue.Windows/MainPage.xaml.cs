using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        Subject<NoteViewModel> deleteStream = new Subject<NoteViewModel>();

        ReactiveList<NoteViewModel> notes;


        public MainPage()
        {
            System.Diagnostics.Debug.WriteLine("App started");

            this.InitializeComponent();

            NotesViewModel notesvm = new NotesViewModel();
            notes = notesvm.Notes;

            NotesContainer.ItemsSource = notes;

            IObservable<EventPattern<RoutedEventArgs>> addNoteStream = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                new Action<RoutedEventHandler>(ev => AddButton.Click += ev),
                new Action<RoutedEventHandler>(ev => AddButton.Click -= ev)
            );

            deleteStream.Subscribe(x => notes.Remove(x));
            
            addNoteStream.Select(x => NewNoteTextBox.Text)
                            .Subscribe(x =>
                            {
                                NoteViewModel note = new NoteViewModel() { Content = x };
                                notesvm.Notes.Add(note);
                                System.Diagnostics.Debug.WriteLine("Added new note");
                            });

            
        }



        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement templateTop = getParent(sender as DependencyObject, "ParentTop");
            NoteViewModel note = getDataContext<NoteViewModel>(sender as DependencyObject);

            deleteStream.OnNext(note);
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


        /// <summary>
        /// Get the first parent with the given name
        /// </summary>
        /// <param name="ob">The element to start the search from</param>
        /// <param name="parentName">The target parent name</param>
        /// <returns>The parent element</returns>
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


        /// <summary>
        /// Finds the first parent with a DataContext value of type T
        /// </summary>
        /// <typeparam name="T">The type of the data context</typeparam>
        /// <param name="ob">The object to start the search from</param>
        /// <returns>The DataContext value found, or null</returns>
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

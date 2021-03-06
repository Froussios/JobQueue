﻿using System;
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
using Windows.System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JobQueue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ReactiveList<NoteViewModel> notes;

        Subject<NoteViewModel> deleteStream = new Subject<NoteViewModel>();
        Subject<NoteViewModel> editStream = new Subject<NoteViewModel>();

        Subject<VirtualKey> keyUpStream = new Subject<VirtualKey>();
        Subject<VirtualKey> keyDownStream = new Subject<VirtualKey>();

        Subject<VirtualKey> newNote_KeyUpStream = new Subject<VirtualKey>();

        Subject<NoteViewModel> gotFocusStream = new Subject<NoteViewModel>();
        Subject<NoteViewModel> lostFocusStream = new Subject<NoteViewModel>();

        IObservable<EventPattern<RoutedEventArgs>> addNoteStream = null;


        public MainPage()
        {
            this.InitializeComponent();
            

            //
            // Initialise model
            //

            NotesViewModel notesvm = new NotesViewModel();
            notes = notesvm.Notes;

            NotesContainer.ItemsSource = notes;


            //
            // Create streams
            //

            IObservable<EventPattern<RoutedEventArgs>> addNoteStream = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                new Action<RoutedEventHandler>(ev => AddButton.Click += ev),
                new Action<RoutedEventHandler>(ev => AddButton.Click -= ev)
            );


            //
            // Behaviour
            //

            deleteStream.Subscribe(x => notes.Remove(x));
            editStream  .GroupBy(x => x.Id)
                        .Subscribe(group => 
                                   group.Throttle(new TimeSpan(1000*1000*10))
                                        .DistinctUntilChanged(x => x.Content)
                                        .Subscribe(x => x.SaveNote())
                        );
            
            addNoteStream.Select(x => true)
                         .Merge(newNote_KeyUpStream.Where(x => x == VirtualKey.Enter).Select(x => true))
                         .Select(x => NewNoteTextBox.Text)
                         .Subscribe(x =>
                         {
                             NoteViewModel note = new NoteViewModel() { Content = x };
                             notesvm.Notes.Add(note);
                             System.Diagnostics.Debug.WriteLine("Added new note");
                             NewNoteTextBox.Text = "";
                         });

            newNote_KeyUpStream.Where(x => x == VirtualKey.Escape)
                               .Subscribe(x => NewNoteTextBox.Text = "");

            
        }



        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            NoteViewModel note = getDataContext<NoteViewModel>(sender as DependencyObject);
            deleteStream.OnNext(note);
        }


        private void Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            NoteViewModel note = getDataContext<NoteViewModel>(sender as DependencyObject);
            note.Content = (sender as TextBox).Text; //Two-way binding does this after firing this event
            editStream.OnNext(note);
        }


        private void Content_LostFocus(object sender, RoutedEventArgs e)
        {
            NoteViewModel note = getDataContext<NoteViewModel>(sender as DependencyObject);
            lostFocusStream.OnNext(note);
        }

        private void Content_GotFocus(object sender, RoutedEventArgs e)
        {
            NoteViewModel note = getDataContext<NoteViewModel>(sender as DependencyObject);
            gotFocusStream.OnNext(note);
        }

        private void Content_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            keyDownStream.OnNext(e.Key);
        }

        private void Content_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            keyDownStream.OnNext(e.Key);
        }

        private void KeyUp(object sender, KeyRoutedEventArgs e)
        {
            newNote_KeyUpStream.OnNext(e.Key);
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

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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JobQueue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<NoteViewModel> notes;
        public MainPage()
        {
            this.InitializeComponent();

            NotesViewModel notesvm = new NotesViewModel();
            notes = notesvm.GetNotes();

            NotesContainer.ItemsSource = notes;

            IObservable<EventPattern<RoutedEventArgs>> addNoteEvs = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                new Action<RoutedEventHandler>(ev => AddButton.Click += ev),
                new Action<RoutedEventHandler>(ev => AddButton.Click -= ev)
            );

            IObservable<EventPattern<RoutedEventArgs>> deteleNoteEvs = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                new Action<RoutedEventHandler>(ev => AddButton.Click += ev),
                new Action<RoutedEventHandler>(ev => AddButton.Click -= ev)
            );
            

            addNoteEvs.Select(x => NewNoteTextBox.Text)
                       .Subscribe(x =>
                       {
                           NoteViewModel note = new NoteViewModel() { Content = x };
                           note.SaveNote(note);
                           System.Diagnostics.Debug.WriteLine("Added new note");
                       });

            
        }

        void Items_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs @event)
        {
            throw new NotImplementedException();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace JobQueue
{
    public class NotesViewModel : ViewModelBase
    {
        private ReactiveUI.ReactiveList<NoteViewModel> notes;
        public ReactiveUI.ReactiveList<NoteViewModel> Notes
        {
            get
            {
                return notes;
            }

            set
            {
                notes = value;
                RaisePropertyChanged("Notes");
            }
        }

        private JobQueue.App app = (Application.Current as App);


        public NotesViewModel()
        {
            notes = getNotes();

            Notes.ItemsAdded.Subscribe(NoteViewModel.SaveNote);
            Notes.ItemsRemoved.Subscribe(NoteViewModel.DeleteNote);
        }


        private ReactiveUI.ReactiveList<NoteViewModel> getNotes()
        {
            notes = new ReactiveUI.ReactiveList<NoteViewModel>();
            using (var db = new SQLite.SQLiteConnection(app.DBPath))
            {
                var query = db.Table<Note>();
                foreach (var _project in query)
                {
                    var project = new NoteViewModel()
                    {
                        Id = _project.Id,
                        Content = _project.Content,
                    };
                    notes.Add(project);
                }
            }
            return notes;
        }

    }
}

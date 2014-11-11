using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace JobQueue
{
    public class NoteViewModel : ViewModelBase
    {
        #region Properties

        private int id = 0;
        public int Id
        {
            get
            { return id; }

            set
            {
                if (id == value)
                { return; }

                id = value;
                RaisePropertyChanged("Id");
            }
        }

        private string content = string.Empty;
        public string Content
        {
            get
            { return content; }

            set
            {
                if (content == value)
                { return; }

                content = value;
                isDirty = true;
                RaisePropertyChanged("Content");
            }
        }


        private bool isDirty = false;
        public bool IsDirty
        {
            get
            {
                return isDirty;
            }

            set
            {
                isDirty = value;
                RaisePropertyChanged("IsDirty");
            }
        }

        #endregion "Properties"

        static private JobQueue.App app = (Application.Current as App);

        static public NoteViewModel GetNote(int projectId)
        {
            var note = new NoteViewModel();
            using (var db = new SQLite.SQLiteConnection(app.DBPath))
            {
                var _note = (db.Table<Note>().Where(
                    c => c.Id == projectId)).Single();
                note.Id = _note.Id;
                note.Content = _note.Content;
            }
            return note;
        }

        static public void SaveNote(NoteViewModel note)
        {
            string result = string.Empty;
            using (var db = new SQLite.SQLiteConnection(app.DBPath))
            {
                string change = string.Empty;
                try
                {
                    var existingNote = (db.Table<Note>().Where(
                        c => c.Id == note.Id)).SingleOrDefault();

                    if (existingNote != null)
                    {
                        existingNote.Content = note.Content;
                        int success = db.Update(existingNote);
                    }
                    else
                    {
                        int success = db.Insert(new Note()
                        {
                            Content = note.Content,
                        });
                    }
                    result = "Success";
                }
                catch (Exception ex)
                {
                    result = "This project was not saved.";
                }
            }
            //return result;
        }

        static public void DeleteNote(int noteId)
        {
            string result = string.Empty;
            using (var db = new SQLite.SQLiteConnection(app.DBPath))
            {
                var existingNote = (db.Table<Note>().Where(
                    c => c.Id == noteId)).Single();

                if (db.Delete(existingNote) > 0)
                {
                    result = "Success";
                }
                else
                {
                    result = "This project was not removed";
                }
            }
            //return result;
        }

        static public void DeleteNote(NoteViewModel vm)
        {
            DeleteNote(vm.Id);
        }

        static public int GetNewNoteId()
        {
            int projectId = 0;
            using (var db = new SQLite.SQLiteConnection(app.DBPath))
            {
                var projects = db.Table<Note>();
                if (projects.Count() > 0)
                {
                    projectId = (from c in db.Table<Note>()
                                 select c.Id).Max();
                    projectId += 1;
                }
                else
                {
                    projectId = 1;
                }
            }
            return projectId;
        }

    }
}

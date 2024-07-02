using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPM.Model
{
    public class Projects : INotifyPropertyChanged
    {
        private List<string> projectList = new List<string>();
        public List<string> ProjectList
        {
            get { return projectList; }
            set { projectList = value; RaisePropertyChanged("ProjectList"); }
        }

        public ObservableCollection<ProjectData> StoredProjects = new ObservableCollection<ProjectData>();


        public void AddProject(ProjectData project)
        {
            StoredProjects.Add(project);
        }

        public void NewProject(string name)
        {
            if (!StoredProjects.Any(x => x.Namn == name))
            {
                StoredProjects.Add(new ProjectData
                {
                    Namn = name
                });

                SetProjectlist();
            }
        }

        public ProjectData GetProject(string name)
        {
            return StoredProjects.FirstOrDefault(x => x.Namn == name);
        }

        public void RemoveProject(ProjectData project)
        {
            StoredProjects.Remove(project);
            SetProjectlist();
        }

        public void SetProjectlist()
        {
            ProjectList.Clear();

            foreach (ProjectData project in StoredProjects)
            {
                ProjectList.Add(project.Namn);
            }
        }

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

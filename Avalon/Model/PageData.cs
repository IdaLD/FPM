using System.ComponentModel;

namespace Avalon.Model
{
    public class PageData : INotifyPropertyChanged
    {
        private int pageNr;
        public int PageNr
        {
            get { return pageNr; }
            set { pageNr = value; RaisePropertyChanged("PageNr"); }
        }

        private string pageName = string.Empty;
        public string PageName
        {
            get { return pageName; }
            set { pageName = value; RaisePropertyChanged("PageName"); }
        }


        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

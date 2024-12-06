using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace WPF_App.Core
{
    public class ViewNavigator : ObservableObject
    {
        public ICommand NavigateBackCommand { get; set; }

        public ObservableCollection<ViewObject> Views { get; }
        public ICollectionView ViewsCollectionView { get; set; }

        public ViewObject CurrentViewObject { get; set; }

        private object _currentView; public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }


        private Stack<ViewObject> _viewStack;


        public ViewNavigator()
        {
            Views = new ObservableCollection<ViewObject>();
            ViewsCollectionView = CollectionViewSource.GetDefaultView(Views);

            _viewStack = new Stack<ViewObject>();

            NavigateBackCommand = new RelayCommand(
                action => NavigateBack(),
                condition => this._viewStack.Count > 1);
        }


        public ViewObject AddView(Type view, string caption, bool selected = false, bool singleton = false, object visual = null, Predicate<object> canExecute = null, Predicate<object> isVisible = null)
        {
            var viewObject = new ViewObject(this, view, caption, selected, singleton, visual, canExecute, isVisible);
            viewObject.OnNavigate += Navigate;

            this.Views.Add(viewObject);

            return viewObject;
        }

        public void OrderByCaption()
        {
            ViewsCollectionView.SortDescriptions.Add(new SortDescription(nameof(ViewObject.Caption), ListSortDirection.Ascending));
        }

        public void Navigate(ViewObject viewObject)
        {
            this.NavigateToView(viewObject);
        }
        public void NavigateBack()
        {
            if (this._viewStack.Count > 1)
            {
                this._viewStack.Pop();
                this.NavigateToView(this._viewStack.Pop());
            }
        }

        private void NavigateToView(ViewObject viewObject)
        {
            var view = viewObject.GetView();

            if (this.CurrentView != view && view != null)
            {
                this.CurrentView = view;
                this.CurrentViewObject = viewObject;

                if (_viewStack.Any() && _viewStack.Peek() == viewObject)
                {
                    return;
                }

                this._viewStack.Push(viewObject);

                //Set the IsSelected property to true for the selected view
                foreach (var viewItem in Views)
                {
                    viewItem.IsSelected = viewItem == viewObject;
                }
            }
        }
    }

    public class ViewObject : ObservableObject
    {
        public event Action<ViewObject> OnNavigate;

        private ViewNavigator _viewNavigator;
        private bool _singleton;
        private object _instance;
        private Predicate<object> _isVisiblePredicate;

        public Type ViewType { get; }
        public bool IsVisible => this._isVisiblePredicate?.Invoke(this) == true;
        public object Visual { get; set; }

        private bool _isSelected; public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(); }
        }
        private string _caption; public string Caption
        {
            get { return _caption; }
            set { _caption = value; OnPropertyChanged(); }
        }

        public ICommand NavigateCommand { get; set; }

        public ViewObject(ViewNavigator viewNavigator, Type view,
            string caption, bool selected = false, bool singleton = false, object visual = null,
            Predicate<object> canExecute = null, Predicate<object> isVisible = null)
        {
            this._viewNavigator = viewNavigator;
            this.ViewType = view;
            this.Caption = caption;
            this._singleton = singleton;
            this.Visual = visual;
            this._isVisiblePredicate = isVisible ?? new Predicate<object>(condition => true);

            NavigateCommand = new RelayCommand(
                action => Navigate(),
                canExecute ?? new Predicate<object>(condition => true));
        }

        public void Navigate()
        {
            OnNavigate?.Invoke(this);
        }

        public object GetView()
        {
            if (_singleton)
            {
                if (_instance == null)
                {
                    _instance = Activator.CreateInstance(ViewType);
                }
                return _instance;
            }

            return Activator.CreateInstance(ViewType);
        }
    }
}

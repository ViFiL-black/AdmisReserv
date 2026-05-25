using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Admissions_Reserve.View
{
    public partial class PrioritiesPage : Page
    {
        // Модель приоритета
        public class PriorityItem : INotifyPropertyChanged
        {
            private int _priority;
            private string _programCode;
            private string _programName;
            private string _studyForm;
            private string _educationBase;
            private string _department;
            private string _admissionType;
            private string _branch;
            private bool _isSelected;

            public int Priority
            {
                get => _priority;
                set { _priority = value; OnPropertyChanged(nameof(Priority)); }
            }
            public string ProgramCode
            {
                get => _programCode;
                set { _programCode = value; OnPropertyChanged(nameof(ProgramCode)); }
            }
            public string ProgramName
            {
                get => _programName;
                set { _programName = value; OnPropertyChanged(nameof(ProgramName)); }
            }
            public string StudyForm
            {
                get => _studyForm;
                set { _studyForm = value; OnPropertyChanged(nameof(StudyForm)); }
            }
            public string EducationBase
            {
                get => _educationBase;
                set { _educationBase = value; OnPropertyChanged(nameof(EducationBase)); }
            }
            public string Department
            {
                get => _department;
                set { _department = value; OnPropertyChanged(nameof(Department)); }
            }
            public string AdmissionType
            {
                get => _admissionType;
                set { _admissionType = value; OnPropertyChanged(nameof(AdmissionType)); }
            }
            public string Branch
            {
                get => _branch;
                set { _branch = value; OnPropertyChanged(nameof(Branch)); }
            }
            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
            }

            public string DisplayName => $"{ProgramCode} {ProgramName} / {StudyForm} форма, {EducationBase}, {Department} / {AdmissionType}";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<PriorityItem> _priorities;
        private Point _dragStartPoint;
        private PriorityItem _draggedItem;

        public PrioritiesPage()
        {
            InitializeComponent();
            InitializeData();
            SetupDragDrop();
        }

        private void InitializeData()
        {
            _priorities = new ObservableCollection<PriorityItem>();

            // Загрузка демонстрационных данных
            LoadSamplePriorities();

            PrioritiesGrid.ItemsSource = _priorities;
        }

        private void SetupDragDrop()
        {
            PrioritiesGrid.PreviewMouseLeftButtonDown += PrioritiesGrid_PreviewMouseLeftButtonDown;
            PrioritiesGrid.PreviewMouseMove += PrioritiesGrid_PreviewMouseMove;
            PrioritiesGrid.Drop += PrioritiesGrid_Drop;
            PrioritiesGrid.DragOver += PrioritiesGrid_DragOver;
        }

        private void LoadSamplePriorities()
        {
            var priorities = new[]
            {
                new PriorityItem
                {
                    Priority = 1,
                    ProgramCode = "09.02.11",
                    ProgramName = "Разработка и управление программным обеспечением",
                    StudyForm = "очная",
                    EducationBase = "Осн. общ.",
                    Department = "Отделение автоматики и электромеханики",
                    AdmissionType = "общий",
                    Branch = "Головная орг.",
                    IsSelected = true
                },
                new PriorityItem
                {
                    Priority = 2,
                    ProgramCode = "08.02.09",
                    ProgramName = "Монтаж, наладка и эксплуатация электрооборудования промышленных и гражданских зданий",
                    StudyForm = "очная",
                    EducationBase = "Осн. общ.",
                    Department = "Отделение автоматики",
                    AdmissionType = "общий",
                    Branch = "Головная орг.",
                    IsSelected = true
                },
                new PriorityItem
                {
                    Priority = 3,
                    ProgramCode = "15.02.17",
                    ProgramName = "Монтаж, техническое обслуживание, эксплуатация и ремонт промышленного оборудования",
                    StudyForm = "очная",
                    EducationBase = "Осн. общ.",
                    Department = "Отделение автоматики и электромеханики",
                    AdmissionType = "общий",
                    Branch = "Головная орг.",
                    IsSelected = false
                },
                new PriorityItem
                {
                    Priority = 4,
                    ProgramCode = "27.02.04",
                    ProgramName = "Автоматические системы управления",
                    StudyForm = "очная",
                    EducationBase = "Осн. общ.",
                    Department = "Отделение автоматики и электромеханики (в том числе с применением автоматических систем управления)",
                    AdmissionType = "общий",
                    Branch = "Головная орг.",
                    IsSelected = false
                }
            };

            foreach (var item in priorities)
            {
                _priorities.Add(item);
            }
        }

        // Перетаскивание строк
        private void PrioritiesGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            var hit = VisualTreeHelper.HitTest(PrioritiesGrid, e.GetPosition(PrioritiesGrid));
            var row = FindVisualParent<DataGridRow>(hit.VisualHit);

            if (row != null)
            {
                _draggedItem = row.Item as PriorityItem;
            }
        }

        private void PrioritiesGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
            {
                Point currentPoint = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPoint;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DataObject dragData = new DataObject("PriorityItem", _draggedItem);
                    DragDrop.DoDragDrop(PrioritiesGrid, dragData, DragDropEffects.Move);
                    _draggedItem = null;
                }
            }
        }

        private void PrioritiesGrid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("PriorityItem"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void PrioritiesGrid_Drop(object sender, DragEventArgs e)
        {
            var targetItem = GetItemAtDropPosition(e.GetPosition(PrioritiesGrid));
            var draggedItem = e.Data.GetData("PriorityItem") as PriorityItem;

            if (draggedItem != null && targetItem != null && draggedItem != targetItem)
            {
                int oldIndex = _priorities.IndexOf(draggedItem);
                int newIndex = _priorities.IndexOf(targetItem);

                if (oldIndex != newIndex)
                {
                    _priorities.Move(oldIndex, newIndex);
                    RecalculatePriorities();
                }
            }
        }

        private PriorityItem GetItemAtDropPosition(Point dropPoint)
        {
            var hit = VisualTreeHelper.HitTest(PrioritiesGrid, dropPoint);
            var row = FindVisualParent<DataGridRow>(hit.VisualHit);
            return row?.Item as PriorityItem;
        }

        // Кнопка перемещения вверх
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as PriorityItem;

            if (item != null)
            {
                int currentIndex = _priorities.IndexOf(item);
                if (currentIndex > 0)
                {
                    _priorities.Move(currentIndex, currentIndex - 1);
                    RecalculatePriorities();
                }
            }
        }

        // Кнопка перемещения вниз
        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as PriorityItem;

            if (item != null)
            {
                int currentIndex = _priorities.IndexOf(item);
                if (currentIndex < _priorities.Count - 1)
                {
                    _priorities.Move(currentIndex, currentIndex + 1);
                    RecalculatePriorities();
                }
            }
        }

        // Пересчет приоритетов
        private void RecalculatePriorities()
        {
            for (int i = 0; i < _priorities.Count; i++)
            {
                _priorities[i].Priority = i + 1;
            }
            PrioritiesGrid.Items.Refresh();
        }

        // Вспомогательный метод для поиска родительского элемента
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        // Получение выбранных приоритетов
        public ObservableCollection<PriorityItem> GetSelectedPriorities()
        {
            return new ObservableCollection<PriorityItem>(_priorities.Where(p => p.IsSelected));
        }

        // Сохранение приоритетов
        public void SavePriorities()
        {
            var selected = GetSelectedPriorities();
            // Здесь можно добавить сохранение в базу данных
        }
    }
}
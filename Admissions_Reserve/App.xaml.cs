// App.xaml.cs
using System;
using System.IO;
using System.Windows;
using Admissions_Reserve.Model;

namespace Admissions_Reserve
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Инициализация БД происходит автоматически в статическом конструкторе DatabaseHelper
            // Здесь можно добавить логирование или проверку, но не выводим лишних сообщений.
        }
    }
}
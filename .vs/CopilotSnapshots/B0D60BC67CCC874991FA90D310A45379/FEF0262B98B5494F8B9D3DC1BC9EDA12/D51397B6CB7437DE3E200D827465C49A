// Core.cs
using System;
using System.Data.SQLite;

namespace Admissions_Reserve.Model
{
    /// <summary>
    /// Core class for managing database connections via SQLite
    /// Works with local database file (App_Data/AdmissionsReserve.db)
    /// </summary>
    public class Core : IDisposable
    {
        public SQLiteConnection Connection { get; set; }
        public AdmissionsReseDBEntities context { get; set; }

        public Core()
        {
            Connection = DatabaseHelper.GetConnection();
            context = new AdmissionsReseDBEntities();
        }

        public void Dispose()
        {
            context?.Dispose();
            Connection?.Close();
            Connection?.Dispose();
        }
    }
}
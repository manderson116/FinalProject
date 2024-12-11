using SQLite;
using FinalProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;
using static System.Net.Mime.MediaTypeNames;

namespace FinalProject
{
    public class MediaRepository
    {
        string _dbPath;
        private SQLiteConnection conn;

        public MediaRepository(string dbPath)
        {
            _dbPath = dbPath;
        }

        public void Init()
        {
            conn = new SQLiteConnection(_dbPath);
            conn.CreateTable<Media>();
        }

        public List<Media> GetAll()
        {
            Init();
            return conn.Table<Media>().ToList();
        }

        public string String(Media media)
        {
            Init();
            return (media.Order.ToString() + " (" + media.Id.ToString() + "). " + media.Filepath.ToString());
        }

        public int Count()
        {
            Init();
            var media = conn.Table<Media>().Count();
            return media;
        }

        public int GetId(Media media)
        {
            Init();
            return media.Id;
        }

        public Media GetById(int id)
        {
            Init();
            var media = from u in conn.Table<Media>()
                        where u.Id == id
                        select u;
            return media.FirstOrDefault();
        }

        public string GetName(Media media)
        {
            Init();
            return media.Name;
        }

        public string GetFilepath(Media media)
        {
            Init();
            return media.Filepath;
        }

        public Media GetByFilepath(string filepath)
        {
            Init();
            var media = from u in conn.Table<Media>()
                        where u.Filepath == filepath
                        select u;
            return media.FirstOrDefault();
        }

        public Media GetByOrder(int order)
        {
            Init();
            var media = from u in conn.Table<Media>()
                        where u.Order == order
                        select u;
            return media.FirstOrDefault();
        }

        public int GetMaxOrder()
        {
            Init();
            if (conn.Table<Media>().Count() <= 0)
                return 0;

            var media = conn.Table<Media>().Max(u => u.Order);
            return media;
        }

        //public void Add(Media media)
        //{
        //    conn = new SQLiteConnection(_dbPath);
        //    conn.Insert(media);
        //}
        public void Add(string name, string filepath, int order)
        {
            try
            {
                Init();
                conn.Insert(new Media { Name = name, Filepath = filepath, Order = order });
            }
            catch (Exception ex)
            {
                //nothing
            }
        }

        public void UpdateOrder(Media media, int newOrder)
        {
            Init();
            media.Order = newOrder;
            conn.Update(media);
        }

        public void Delete(int id)
        {
            Init();
            conn.Delete<Media>(id);
            //conn.Delete(new { Id = id });
        }

        public void DeleteAll()
        {
            Init();
            conn.DeleteAll<Media>();
            //conn.Delete(new { Id = id });
        }
    }
}

using ImageService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace ImageService.Modal
{
    public class ImageServiceModal : IImageServiceModal
    {
        #region Members
        private string m_OutputFolder;            // The Output Folder
        private int m_thumbnailSize;             // The Size Of The Thumbnail Size
        /// <summary>
        /// constructor.
        /// receive all his data from the AppConfig.
        /// </summary>
        public ImageServiceModal()
        {
            this.m_OutputFolder = ConfigurationManager.AppSettings["OutputDir"];
            string tumbnailPath = this.m_OutputFolder + "/Thumbnails";
            DirectoryInfo info = Directory.CreateDirectory(this.m_OutputFolder);
            info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            Directory.CreateDirectory(tumbnailPath);
            if (!Int32.TryParse(ConfigurationManager.AppSettings["ThumbnailSize"], out this.m_thumbnailSize))
            {

            }

        }
        #endregion
        /// <summary>
        /// create an appropriate folder (if doesnt exist already) with year and month.
        /// </summary>
        /// <param name="year">year that photo has taken</param>
        /// <param name="month">month that photo has taken</param>
        /// <returns>return path to new folder</returns>
        public string CreateFolder(string year, string month)
        {
            string newPath = m_OutputFolder + "/" + year;
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            newPath = newPath + "/" + month;
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            return newPath;
        }
        /// <summary>
        /// add file to appropriate folder (and also thumbnails folder), a part of interface IImageModal.
        /// </summary>
        /// <param name="path"> path to the file</param>
        /// <param name="result"> boolean result of process</param>
        /// <returns> ......</returns>
        public string AddFile(string path, out bool result)
        {
            try
            {

                if (File.Exists(path))
                {
                    DateTime date = this.ExtractDate(path);
                    string newPath = this.CreateFolder(date.Year.ToString(), date.Month.ToString());
                    string tumb = this.CreateTumbnailFolder(date.Year.ToString(), date.Month.ToString());
                    string fileName = Path.GetFileName(path);
                    string newFileName = this.ChangeFileName(fileName, newPath, tumb);
                    File.Move(path, newFileName);
                    Image image = Image.FromFile(newFileName);
                    Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(TumbnailCallback);
                    Image tumbImage = image.GetThumbnailImage(this.m_thumbnailSize, this.m_thumbnailSize, myCallback, IntPtr.Zero);
                    tumbImage.Save(tumb + "/" + Path.GetFileName(newFileName));
                    image.Dispose();
                    tumbImage.Dispose();
                    if (path.Equals(newPath))
                    {
                        result = false;
                        return "file already exist";
                    }
                    result = true;
                    return "success in moving " + Path.GetFileName(path);
                }
                result = false;
                return "File doesnt exit";
            } catch (Exception e)
            {
                result = false;
                return e.ToString();
            }

        }
        /// <summary>
        /// changeFileName.
        /// the function check if there is a file with similar name in the dirs.
        /// if yes -> change the name by adding 1, else return the original file name.
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="newPath">the wanted dir</param>
        /// <param name="tumbPath">tumb path</param>
        /// <returns></returns>
        private string ChangeFileName(string fileName, string newPath, string tumbPath)
        {
            bool flag = false;

            while (!flag)
            {
                if (File.Exists(newPath + "/" + fileName) || File.Exists(tumbPath + "/" + fileName))
                {
                    fileName = "1" + fileName;
                    flag = false;
                }
                else
                {
                    flag = true;
                }
            }
            return newPath + "/" + fileName;
        }
        /// <summary>
        /// create a thumbnail folder with year and month.
        /// </summary>
        /// <param name="year">year</param>
        /// <param name="month">month</param>
        /// <returns>path to new folder</returns>
        private string CreateTumbnailFolder(string year, string month)
        {
            string tumbnailPath = this.m_OutputFolder + "/Thumbnails";
            string newThumbPath = tumbnailPath + "/" + year;
            if (!Directory.Exists(newThumbPath))
            {
                Directory.CreateDirectory(newThumbPath);
            }
            newThumbPath = newThumbPath + "/" + month;
            if (!Directory.Exists(newThumbPath))
            {
                Directory.CreateDirectory(newThumbPath);
            }
            return newThumbPath;

        }
        /// <summary>
        /// .....
        /// </summary>
        /// <returns>bool</returns>
        private bool TumbnailCallback()
        {
            return false;
        }
        /// <summary>
        /// Extract Date.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private DateTime ExtractDate(string path)
        {
            Regex regex = new Regex(":");
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (Image myImage = Image.FromStream(fs, false, false))
                {
                    PropertyItem propItem = myImage.GetPropertyItem(36867);
                    string dateTaken = regex.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    DateTime dt = DateTime.Parse(dateTaken);
                    return dt;
                }
            }
            catch (Exception e)
            {
                e.ToString();
                return File.GetCreationTime(path);

            }
        }
    }

}

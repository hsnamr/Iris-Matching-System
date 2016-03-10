using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace Iris_Matching_System
{
    public partial class storeDB : Form
    {
        string databasePath = "";

        public storeDB()
        {
            InitializeComponent();
        }

        private void stroeDB_Click(object sender, EventArgs e)
        {
            try
            {
                //Database1DataSet im = new Database1DataSet();
                String path;
                String Fname="";
                byte[] imageData;
                int id = 1;
                for (int i = 1; i < 246; i++)
                {
                    for (int j = 1; j < 10; j++)
                    {
                        path = txtPath.Text;
                        if (i < 10)
                        {
                            path = path + "\\00" + i.ToString() + "\\L\\S100" + i.ToString() + "L0" + j + ".jpg";
                            Fname = "\\00" + i.ToString() + "\\L\\S100" + i.ToString() + "L0" + j;
                        }

                        else if (i > 9 && i <100)
                        {
                            path = path + "\\0" + i.ToString() + "\\L\\S10" + i.ToString() + "L0" + j + ".jpg";
                            Fname = "\\00" + i.ToString() + "\\L\\S10" + i.ToString() + "L0" + j + ".jpg";

                        }
                        
                        else if (i < 250)
                        {
                            path = path + "\\" + i.ToString() + "\\L\\S1" + i.ToString() + "L0" + j + ".jpg";
                            Fname = "\\" + i.ToString() + "\\L\\S1" + i.ToString() + "L0" + j + ".jpg";
                        }
                        

                        //Read Image Bytes into a byte array
                        FileInfo fInfo = new FileInfo(path);
                        ///
                        MemoryStream memoryStream1;
                        System.Drawing.Image imageC;
                        //imageC = 
                        //long numBytes = fInfo.Length;
                        if (fInfo.Exists)
                        {
                            //imageData = ReadFile(path);


                            memoryStream1 = new MemoryStream();
                            //memoryStream.Write(imageData, 0, imageData.Length);
                            //imageDB = System.Drawing.Image.FromStream(memoryStream);
                            imageC = (Image)System.Drawing.Image.FromFile(path);
                            imageC = Crop(imageC, 256, 256, AnchorPosition.Center);
                            //MemoryStream ms = new MemoryStream();
                            imageC.Save(memoryStream1, System.Drawing.Imaging.ImageFormat.Jpeg);
                            imageData = memoryStream1.ToArray();


                            //DataSet1TableAdapters.ImagesStoreTableAdapter im = new StoreImagesInSQLServer.DataSet1TableAdapters.ImagesStoreTableAdapter();
                            irisDBDataSetTableAdapters.DataTable1TableAdapter p = new Iris_Matching_System.irisDBDataSetTableAdapters.DataTable1TableAdapter();
                            irisDBDataSetTableAdapters.iris_imagesTableAdapter im = new Iris_Matching_System.irisDBDataSetTableAdapters.iris_imagesTableAdapter();

                            //Fname = Fname.Substring(Fname.LastIndexOf("S") + 2, Filename.LastIndexOf("L") - (Filename.LastIndexOf("S") + 2));
                            
                            p.insert_person(id, Fname, "casia");

                            Fname = Fname.Substring(Fname.LastIndexOf("L") + 1, 2);
                            im.insert_iris(i, int.Parse(Fname),path, imageData);
                            id++;
                        }
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        byte[] ReadFile(string sPath)
        {
            //Initialize byte array with a null value initially.
            byte[] data = null;

            //Use FileInfo object to get file size.
            FileInfo fInfo = new FileInfo(sPath);
            long numBytes = fInfo.Length;

            //Open FileStream to read file
            FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);

            //Use BinaryReader to read file stream into byte array.
            BinaryReader br = new BinaryReader(fStream);

            //When you use BinaryReader, you need to supply number of bytes to read from file.
            //In this case we want to read entire file. So supplying total number of bytes.
            data = br.ReadBytes((int)numBytes);
            return data;
        }

        enum Dimensions
        {
            Width,
            Height
        }
        enum AnchorPosition
        {
            Top,
            Center,
            Bottom,
            Left,
            Right
        }

        static System.Drawing.Image Crop(System.Drawing.Image imgPhoto, int Width, int Height, AnchorPosition Anchor)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                switch (Anchor)
                {
                    case AnchorPosition.Top:
                        destY = 0;
                        break;
                    case AnchorPosition.Bottom:
                        destY = (int)(Height - (sourceHeight * nPercent));
                        break;
                    default:
                        destY = (int)((Height - (sourceHeight * nPercent)) / 2);
                        break;
                }
            }
            else
            {
                nPercent = nPercentH;
                switch (Anchor)
                {
                    case AnchorPosition.Left:
                        destX = 0;
                        break;
                    case AnchorPosition.Right:
                        destX = (int)(Width - (sourceWidth * nPercent));
                        break;
                    default:
                        destX = (int)((Width - (sourceWidth * nPercent)) / 2);
                        break;
                }
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        private void browseDB_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog selectFolder = new FolderBrowserDialog();
                selectFolder.ShowDialog();
                databasePath = selectFolder.SelectedPath;        // This is the path selected by the user
                txtPath.Text = databasePath;

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
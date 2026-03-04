using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Iris_Matching_System;

public partial class storeDB : Form
{
    private string _databasePath = "";

    public storeDB()
    {
        InitializeComponent();
    }

    private void stroeDB_Click(object sender, EventArgs e)
        {
            try
            {
                int id = 1;
                for (int i = 1; i < 246; i++)
                {
                    for (int j = 1; j < 10; j++)
                    {
                        string path = txtPath.Text;
                        string fname;
                        if (i < 10)
                        {
                            path = $"{path}\\00{i}\\L\\S100{i}L0{j}.jpg";
                            fname = $"\\00{i}\\L\\S100{i}L0{j}";
                        }
                        else if (i < 100)
                        {
                            path = $"{path}\\0{i}\\L\\S10{i}L0{j}.jpg";
                            fname = $"\\00{i}\\L\\S10{i}L0{j}.jpg";
                        }
                        else
                        {
                            path = $"{path}\\{i}\\L\\S1{i}L0{j}.jpg";
                            fname = $"\\{i}\\L\\S1{i}L0{j}.jpg";
                        }

                        var fInfo = new FileInfo(path);
                        if (!fInfo.Exists) continue;

                        using var imageC = (Image)System.Drawing.Image.FromFile(path);
                        using var cropped = (Image)Crop(imageC, 256, 256, AnchorPosition.Center);
                        using var memoryStream = new MemoryStream();
                        cropped.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] imageData = memoryStream.ToArray();

                        var p = new irisDBDataSetTableAdapters.DataTable1TableAdapter();
                        var im = new irisDBDataSetTableAdapters.iris_imagesTableAdapter();

                        p.insert_person(id, fname, "casia");
                        fname = fname.Substring(fname.LastIndexOf("L", StringComparison.Ordinal) + 1, 2);
                        im.insert_iris(i, int.Parse(fname), path, imageData);
                        id++;
                    }
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    byte[]? ReadFile(string sPath)
    {
        var fInfo = new FileInfo(sPath);
        long numBytes = fInfo.Length;
        using var fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fStream);
        return br.ReadBytes((int)numBytes);
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

            using (var grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }
            return bmPhoto;
        }

    private void browseDB_Click(object sender, EventArgs e)
    {
        try
        {
            using var selectFolder = new FolderBrowserDialog();
            if (selectFolder.ShowDialog() == DialogResult.OK)
            {
                _databasePath = selectFolder.SelectedPath;
                txtPath.Text = _databasePath;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
}
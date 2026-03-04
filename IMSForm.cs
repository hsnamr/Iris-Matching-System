using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Threading;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

using AForge;
using AForge.Math;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;

using Iris_Matching_System;

namespace DemoApp;

public partial class IMSForm : Form
{
    private Bitmap? _image;
    private Bitmap? _dbImage;
    private int _width;
    private int _height;
    private string _filename = "";
    private string _extension = "";
    private ComplexImage? _cImage;
    private ComplexImage? _cImageDB;
    private ComplexImage? _fftResult;
    private Bitmap? _ifftResult;
    private Bitmap? _fftResultDB;
    private Bitmap? _local;
    private TabPage _newImage;
    private TabPage _result;
    private TabPage _imageInfo;
    private TabPage _databaseBrowse;
    private string _name = "";
    private Iris_Matching_System.irisDBDataSet.iris_imagesDataTable? _d;
    private readonly int[] _rIndex = new int[10];
    private int[,] _results = null!;
    private int _index;
    private bool _matchDone = true;
    private Bitmap[] _imaArray = null!;
        //private IDocumentsHost host = null;
        //private System.Drawing.Bitmap backup = null;
        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        //Complex[,] comImageDB = new Complex[cImageDB.Height, cImageDB.Width];           // This will be replaced by image(s) from database
        int number = 15;                        // assuming database has 15 images      
        //Double[,] max = new Double[number, 2];      // [image index in DB, match value]

        public IMSForm()
        {
            InitializeComponent();
            _newImage = tabPage4;
            _result = tabPage1;
            _imageInfo = tabPage3;
            _databaseBrowse = tabPage2;
            tabCtrl.TabPages.Remove(tabPage3);
            tabCtrl.TabPages.Remove(tabPage1);
            tabCtrl.TabPages.Remove(tabPage2);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void LoadImage()
        {
            try
            {
                using var open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.jng; *.jp2; *.j2c; *.pcx; *.gif; *.tiff; *.png; *.bmp)|*.jpg; *.jpeg; *.gif; *.tiff; *.png; *.bmp";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    _filename = open.FileName;
                    _extension = Path.GetExtension(_filename).ToLowerInvariant();
                    _image = (Bitmap)Crop(new Bitmap(_filename), 256, 256, AnchorPosition.Center);
                    picBox.Image = _image;
                    _width = picBox.Image.Width;
                    _height = picBox.Image.Height;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed loading image", ex);
            }

            openImageToolStripMenuItem.Enabled = false;
            clearToolStripMenuItem.Enabled = true;
            reloadToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            viewToolStripMenuItem.Enabled = true;
            matchToolStripMenuItem.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            try
            {
                picBox.Image = null;
                _image = null;
                _dbImage = null;
                _width = 0;
                _height = 0;
                _cImage = null;
                _cImageDB = null;
                _fftResult = null;
                _fftResultDB = null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed clearing picture box", ex);
            }

            openImageToolStripMenuItem.Enabled = true;
            clearToolStripMenuItem.Enabled = false;
            reloadToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            viewToolStripMenuItem.Enabled = false;
            matchToolStripMenuItem.Enabled = false;
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Clear();
            LoadImage();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveImageToFile();
        }

        private void SaveImageToFile()
        {
            try
            {
                using var save = new SaveFileDialog();
                save.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.exif; *.tiff; *.png; *.bmp)|*.jpg; *.jpeg; *.gif; *.exif; *.tiff; *.png; *.bmp";
                save.FileName = Path.GetFileName(_filename);
                save.FilterIndex = 0;

                if (save.ShowDialog() == DialogResult.OK && picBox.Image != null)
                {
                    save.CheckFileExists = true;
                    picBox.Image.Save(save.FileName);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed saving image", ex);
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
            LoadImage();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageToFile();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabCtrl.TabPages.RemoveAt(tabCtrl.TabPages.IndexOf(tabCtrl.SelectedTab));
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {DemoApp.Complex mb ;
                tabCtrl.TabPages.Remove(NewImage);
                tabCtrl.TabPages.Remove(Result);
                tabCtrl.TabPages.Remove(ImageInfo);
                tabCtrl.TabPages.Remove(DatabaseBrowse);
                //tabCtrl.TabPages.Remove(DatabaseSearch);
                tabCtrl.TabPages.Add(NewImage);
                Clear();
            }
            catch
            {
            }
        }

        private void sToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabCtrl.TabPages.Add(Result);
            // showResults();
        }

        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetImagesFromDatabase();
            tabCtrl.TabPages.Add(DatabaseBrowse);
        }


        private void forwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ForwardFourierTransformation();
            _fftResult = _cImage;
            picBox.Image = _cImage!.ToBitmap();

            forwardToolStripMenuItem.Enabled = false;
            backwordToolStripMenuItem.Enabled = true;
        }

        private void backwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackwardFourierTransform();
        }

        private void ForwardFourierTransformation()
        {
            if (!Tools.IsPowerOf2(_width) || !Tools.IsPowerOf2(_height))
            {
                MessageBox.Show("Fourier transformation can be applied to an image with width and height of power of 2", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            _cImage = ComplexImage.FromBitmap(_image!);
            _cImage.ForwardFourierTransform();
        }

        private void BackwardFourierTransform()
        {
            var cimg = _fftResult!;
            cimg.BackwardFourierTransform();
            _ifftResult = cimg.ToBitmap();
            picBox.Image = _ifftResult;

            forwardToolStripMenuItem.Enabled = true;
            backwordToolStripMenuItem.Enabled = false;
        }

        private void blurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFilter(new Blur());
        }

        private void ApplyFilter(IFilter filter)
        {
            try
            {
                var filteredImage = filter.Apply(_image!);
                picBox.Image = filteredImage;
                _image = filteredImage;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Selected filter cannot be applied to the image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sharpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyFilter(new Sharpen());
        }

        private void toGrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Grayscale();
        }

        private void Grayscale()
        {
            if (_image!.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                MessageBox.Show("The image is already grayscale", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            ApplyFilter(new GrayscaleBT709());
        }

        void GetImagesFromDatabase()
        {
            const string connectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=|DataDirectory|\\irisDB.mdf;Integrated Security=True;User Instance=True";
            try
            {
                using var cn = new SqlConnection(connectionString);
                using var adap = new SqlDataAdapter("SELECT * FROM iris_images", cn);
                var ds = new DataSet();
                adap.Fill(ds, "iris_images");
                dataGridView1.DataSource = ds.Tables["iris_images"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        void readDBImage()
        {
            if (_dbImage == null || _cImageDB == null) return;

            if (!Tools.IsPowerOf2(_dbImage.Width) || !Tools.IsPowerOf2(_dbImage.Height))
            {
                MessageBox.Show("Fourier transformation can be applied to an image with width and height of power of 2", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            _cImageDB.ForwardFourierTransform();
        }

        private void importDB_Click(object sender, EventArgs e)
        {
            using var store = new storeDB();
            store.ShowDialog();
            GetImagesFromDatabase();
        }

        private void importDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var store = new storeDB();
            store.ShowDialog();
            GetImagesFromDatabase();
        }



        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _matchDone = false;
            progressBar1.Visible = true;

            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (_, _) => applyMatch();
            worker.RunWorkerCompleted += (_, _) =>
            {
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                tabCtrl.TabPages.Add(_result);
            };

            var timer = new System.Windows.Forms.Timer { Interval = 500 };
            timer.Tick += (_, _) =>
            {
                if (!_matchDone) progressBar1.Increment(10);
                if (progressBar1.Value >= 100) progressBar1.Value = 0;
            };
            timer.Start();
            worker.RunWorkerCompleted += (_, _) => { timer.Stop(); timer.Dispose(); };
            worker.RunWorkerAsync();
        }

        private void applyMatch()
        {
            if (_image == null || _cImage == null) return;

            for (int i = 0; i < 10; i++)
                _rIndex[i] = -2;

            ForwardFourierTransformation();

            int rows = _cImage.Height, cols = _cImage.Width;
            var result = new Complex[rows, cols];
            var rimage = new ComplexImage(_cImage.Width, _cImage.Height);
            var comImageFile = new Complex[rows, cols];
            var comDB = new Complex[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    comImageFile[i, j] = new Complex(_cImage.data[i, j].Re, _cImage.data[i, j].Im);

            var im = new Iris_Matching_System.irisDBDataSetTableAdapters.iris_imagesTableAdapter();
            _d = im.GetData();
            int count = _d.Count;

            _imaArray = new Bitmap[count];
            _results = new int[count, 3];

            _name = _filename.Substring(_filename.LastIndexOf("L", StringComparison.Ordinal) + 1, 2);
            string name2 = _filename.Substring(_filename.LastIndexOf("S", StringComparison.Ordinal) + 2, 3);

            for (int i2 = 0; i2 < count; i2++)
            {
                if (int.Parse(_name) != (int)_d[i2][2]! || int.Parse(name2) != (int)_d[i2][0]!)
                {
                    byte[] byte1 = (byte[])_d[i2][1]!;
                    using var memoryStream = new MemoryStream(byte1);
                    var imageDB1 = (Bitmap)System.Drawing.Image.FromStream(memoryStream);
                    _imaArray[i2] = imageDB1;

                    _cImageDB = ComplexImage.FromBitmap(imageDB1);
                    _cImageDB.ForwardFourierTransform();

                    for (int i = 0; i < _cImageDB.Height; i++)
                        for (int j = 0; j < _cImageDB.Width; j++)
                            comDB[i, j] = new Complex(_cImageDB.data[i, j].Re, _cImageDB.data[i, j].Im);

                    result = ComplexOperations.Match(comImageFile, comDB);

                    for (int i = 0; i < _cImage!.Height; i++)
                        for (int j = 0; j < _cImage.Width; j++)
                            _cImageDB.data[i, j] = new AForge.Math.Complex(result[i, j].Re, result[i, j].Im);

                    _cImageDB.BackwardFourierTransform();

                    _results[i2, 1] = (int)_d[i2][0]!;
                    _results[i2, 0] = i2;
                    for (int i = 0; i < _cImageDB.Height; i++)
                        for (int j = 0; j < _cImageDB.Width; j++)
                            _results[i2, 2] = Math.Max(_results[i2, 2], (int)_cImageDB.data[i, j].Re);
                }
            }

            _name = _filename.Substring(_filename.LastIndexOf("S", StringComparison.Ordinal) + 2, 3);

            SelectionSort(_results);

            int ri = 0;
            int i3 = int.Parse(_name);
            for (int i = 0; i < _results.GetLength(0); i++)
            {
                if (i3 == _results[i, 1])
                {
                    _rIndex[ri] = i;
                    ri++;
                }
            }

            _index = 0;
            Top10(1, 10);
            _matchDone = true;
        }

        private void findMax()
        {
            for (int idx = 0; idx < 15; idx++)
                for (int i = 0; i < _height; i++)
                    for (int j = 0; j < _width; j++)
                    {
                        // Reserved for max value tracking per image
                    }
        }

        private void Top10(int a, int x)
        {
            try
            {
                pictureBox11.Image = _image;
                rank.Text = "rank:" + _rIndex[0];
                for (int i = 1; i < 10; i++)
                {
                    if (_rIndex[i] != -2)
                        rank.Text = rank.Text + "; " + _rIndex[i];
                }
                if (a == 1)
                {
                    if (x == 100)
                        _index += 100;

                    R1.Text = _index.ToString();
                    pictureBox1.Image = _imaArray[_results[_index++, 0]];
                    pictureBox1.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R2.Text = _index.ToString();
                    pictureBox2.Image = _imaArray[_results[_index++, 0]];
                    pictureBox2.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R3.Text = _index.ToString();
                    pictureBox3.Image = _imaArray[_results[_index++, 0]];
                    pictureBox3.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R4.Text = _index.ToString();
                    pictureBox4.Image = _imaArray[_results[_index++, 0]];
                    pictureBox4.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R5.Text = _index.ToString();
                    pictureBox5.Image = _imaArray[_results[_index++, 0]];
                    pictureBox5.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R6.Text = _index.ToString();
                    pictureBox6.Image = _imaArray[_results[_index++, 0]];
                    pictureBox6.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R7.Text = _index.ToString();
                    pictureBox7.Image = _imaArray[_results[_index++, 0]];
                    pictureBox7.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R8.Text = _index.ToString();
                    pictureBox8.Image = _imaArray[_results[_index++, 0]];
                    pictureBox8.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R9.Text = _index.ToString();
                    pictureBox9.Image = _imaArray[_results[_index++, 0]];
                    pictureBox9.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R10.Text = _index.ToString();
                    pictureBox10.Image = _imaArray[_results[_index++, 0]];
                    pictureBox10.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);
                }
                else if (a == -1)
                {
                    _index -= 20;
                    if (x == 100)
                        _index -= 100;

                    if (_index < 0)
                        _index = 0;

                    R1.Text = _index.ToString();
                    pictureBox1.Image = _imaArray[_results[_index++, 0]];
                    pictureBox1.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R2.Text = _index.ToString();
                    pictureBox2.Image = _imaArray[_results[_index++, 0]];
                    pictureBox2.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R3.Text = _index.ToString();
                    pictureBox3.Image = _imaArray[_results[_index++, 0]];
                    pictureBox3.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R4.Text = _index.ToString();
                    pictureBox4.Image = _imaArray[_results[_index++, 0]];
                    pictureBox4.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R5.Text = _index.ToString();
                    pictureBox5.Image = _imaArray[_results[_index++, 0]];
                    pictureBox5.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R6.Text = _index.ToString();
                    pictureBox6.Image = _imaArray[_results[_index++, 0]];
                    pictureBox6.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R7.Text = _index.ToString();
                    pictureBox7.Image = _imaArray[_results[_index++, 0]];
                    pictureBox7.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R8.Text = _index.ToString();
                    pictureBox8.Image = _imaArray[_results[_index++, 0]];
                    pictureBox8.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R9.Text = _index.ToString();
                    pictureBox9.Image = _imaArray[_results[_index++, 0]];
                    pictureBox9.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);

                    R10.Text = _index.ToString();
                    pictureBox10.Image = _imaArray[_results[_index++, 0]];
                    pictureBox10.Padding = IsExist(_index - 1) ? new Padding(3) : new Padding(0);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The method or operation is not implemented.", ex);
            }
        }

        private bool IsExist(int index)
        {
            for (int i = 0; i < 10; i++)
            {
                if (_rIndex[i] == index)
                    return true;
            }
            return false;
        }

        private void sortArrayByMatchValue()
        {
            // Reserved: sort by match value
        }

        public static void SelectionSort(int[,] array)
        {
            int l = array.GetLength(0) - 1;
            for (int i = 0; i <= l; i++)
            {
                int max = i;
                for (int j = i + 1; j <= l; j++)
                {
                    if (array[j, 2] > array[max, 2])
                        max = j;
                }
                (array[i, 0], array[max, 0]) = (array[max, 0], array[i, 0]);
                (array[i, 1], array[max, 1]) = (array[max, 1], array[i, 1]);
                (array[i, 2], array[max, 2]) = (array[max, 2], array[i, 2]);
            }
        }
        ///////////////////////

        //////////////
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var imageData = (byte[]?)dataGridView1.Rows[e.RowIndex].Cells["iris_Image"].Value;
                if (imageData == null) return;

                using var ms = new MemoryStream(imageData, 0, imageData.Length);
                ms.Write(imageData, 0, imageData.Length);
                var newImage = System.Drawing.Image.FromStream(ms);
                pictureBoxDB.Image = newImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Previous_Click(object sender, EventArgs e) => Top10(-1, 10);

        private void next_Click(object sender, EventArgs e) => Top10(1, 10);

        //////////////////////

        //public IFilter Filter
        //{
            //get { return filter; }
        //}

        private void ApplyFilter2(IFilter filter)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var newImage = filter.Apply(_image!);
                _image = newImage;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Selected filter cannot be applied to the image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void cannyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new CannyDetectorForm();
            form.Image = _image;

            if (form.ShowDialog() == DialogResult.OK)
                ApplyFilter(form.Filter);
        }

                // Extract separate blobs
        
        private void blobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_image!.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                MessageBox.Show("Blob extractor can be applied to binary images only", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var blobCounter = new BlobCounter();
            blobCounter.ProcessImage(_image);
            Blob[] blobs = blobCounter.GetObjects(_image);

            float lmyh = 0;
            int myi = 0;
            for (int i = 0; i < blobs.Length; i++)
            {
                float myhigh = blobs[i].Image.PhysicalDimension.Height;
                if (myhigh > lmyh)
                {
                    lmyh = myhigh;
                    myi = i;
                }
            }
            picBox.Image = blobs[myi].Image;
        }

        private void localize(int x, int y)
        {
            _local = _image;
            _cImageDB = ComplexImage.FromBitmap(_image!);
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

        static System.Drawing.Image Crop(System.Drawing.Image imgPhoto, int width, int height, AnchorPosition anchor)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercentW = (float)width / sourceWidth;
            float nPercentH = (float)height / sourceHeight;
            float nPercent;
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                destY = anchor switch
                {
                    AnchorPosition.Top => 0,
                    AnchorPosition.Bottom => (int)(height - (sourceHeight * nPercent)),
                    _ => (int)((height - (sourceHeight * nPercent)) / 2)
                };
            }
            else
            {
                nPercent = nPercentH;
                destX = anchor switch
                {
                    AnchorPosition.Left => 0,
                    AnchorPosition.Right => (int)(width - (sourceWidth * nPercent)),
                    _ => (int)((width - (sourceWidth * nPercent)) / 2)
                };
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            var bmPhoto = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
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

        private void button1_Click(object sender, EventArgs e) => Top10(1, 100);

        private void button2_Click(object sender, EventArgs e) => Top10(-1, 100);

        private void IMSForm_Load(object sender, EventArgs e)
        {
            iris_imagesTableAdapter.Fill(irisDBDataSet.iris_images);
        }
    }
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Collections;
using System.IO;
using System.Threading;
using System.Data.SqlClient;

using AForge;
using AForge.Math;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;
using Iris_Matching_System;

namespace DemoApp
{
    public partial class IMSForm : Form
    {
        private System.Drawing.Bitmap image = null;
        private System.Drawing.Bitmap DBimage = null;
        private int width;
        private int height;
        private string Filename;
        private string extension;
        ComplexImage cImage;
        ComplexImage cImageDB;
        ComplexImage fftResult;
        Bitmap ifftResult;
        Bitmap fftResultDB;
        Bitmap local;
        TabPage NewImage;
        TabPage Result;
        TabPage ImageInfo;
        TabPage DatabaseBrowse;
        string name = "";
        Iris_Matching_System.irisDBDataSet.iris_imagesDataTable d;
        int[] Rindex = new int[10];
        int[,] results;
        int index = 0;
        Boolean matchDone = true;
        System.Drawing.Bitmap[] IMAarray;
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
            NewImage = tabPage4;
            Result = tabPage1;
            ImageInfo = tabPage3;
            DatabaseBrowse = tabPage2;
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
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.jng; *.jp2; *.j2c; *.pcx; *.gif; *.tiff; *.png; *.bmp)|*.jpg; *.jpeg; *.gif; *.tiff; *.png; *.bmp";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    Filename = open.FileName;
                    extension = Path.GetExtension(Filename).ToLower();
                    //picBox.Image = new Bitmap(Filename);
                    image = new Bitmap(Filename);
                    image = (Bitmap)Crop(image, 256, 256, AnchorPosition.Center);
                    picBox.Image = image;
                    width = picBox.Image.Width;
                    height = picBox.Image.Height;
             
                }

            }
            catch
            {
                throw new ApplicationException("Failed loading image");
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
                image = null;
                DBimage = null;
                width = 0;
                //DBwidth = 0;
                height = 0;
                //DBheight = 0;
                cImage = null;
                cImageDB = null;
                fftResult = null;
                fftResultDB = null;
            }
            catch
            {
                throw new ApplicationException("Failed clearing picture box");
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
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.exif; *.tiff; *.png; *.bmp)|*.jpg; *.jpeg; *.gif; *.exif; *.tiff; *.png; *.bmp";

                save.FileName = Path.GetFileName(Filename);
                save.FilterIndex = 0;

                if (save.ShowDialog() == DialogResult.OK)
                {
                    save.CheckFileExists = true;
                    picBox.Image.Save(save.FileName);
                }
            }

            catch
            {
                throw new ApplicationException("Failed clearing picture box");
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
            fftResult = cImage;
            picBox.Image = cImage.ToBitmap();

            forwardToolStripMenuItem.Enabled = false;
            backwordToolStripMenuItem.Enabled = true;
        }

        private void backwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackwardFourierTransform();
        }

        private void ForwardFourierTransformation()
        {
            if ((!AForge.Math.Tools.IsPowerOf2(width)) || (!AForge.Math.Tools.IsPowerOf2(height)))
            {
                MessageBox.Show("Fourier trasformation can be applied to an image with width and height of power of 2", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            cImage = ComplexImage.FromBitmap(image);
            cImage.ForwardFourierTransform();
        }

        private void BackwardFourierTransform()
        {
            ComplexImage cimg = fftResult;
            cimg.BackwardFourierTransform();
            ifftResult = cimg.ToBitmap();
            picBox.Image = ifftResult;

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
                Bitmap filteredImage = filter.Apply(image);

                picBox.Image = filteredImage;
                image = filteredImage;

            }
            catch (ArgumentException)
            {
                MessageBox.Show("Selected filter can not be applied to the image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                MessageBox.Show("The image is already grayscale", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            ApplyFilter(new GrayscaleBT709());
        }

        void GetImagesFromDatabase()
        {
            try
            {
                //Initialize SQL Server connection.
                SqlConnection CN = new SqlConnection("Data Source=.\\SQLEXPRESS;AttachDbFilename=|DataDirectory|\\irisDB.mdf;Integrated Security=True;User Instance=True");

                //Initialize SQL adapter.
                SqlDataAdapter ADAP = new SqlDataAdapter("Select * from iris_images", CN);

                //Initialize Dataset.
                DataSet DS = new DataSet();

                //Fill dataset with ImagesStore table.
                ADAP.Fill(DS, "iris_images");

                //Fill Grid with dataset.
                dataGridView1.DataSource = DS.Tables["iris_images"];
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        // To-Do
        void readDBImage()
        {
            //DBimage = "image from database";    // A query to read the image from database and put it in variable DBimage
            //cImageDB = ComplexImage.FromBitmap(DBimage);


            if ((!AForge.Math.Tools.IsPowerOf2(DBimage.Width)) || (!AForge.Math.Tools.IsPowerOf2(DBimage.Height)))
            {
                MessageBox.Show("Fourier trasformation can be applied to an image with width and height of power of 2", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            cImageDB.ForwardFourierTransform();

            //for (int i = 0; i < cImageDB.Height; i++)
                //for (int j = 0; j < cImageDB.Width; j++)
                    //comImageDB[i, j] = new Complex(cImageDB.data[i, j].Re, cImageDB.data[i, j].Im);

        }

        private void importDB_Click(object sender, EventArgs e)
        {
            storeDB store = new storeDB();
            //Supply connection string from this form to frmNewImage form.
            //store.txtConnecti = txtConnectionString.Text;
            store.ShowDialog();

            //Refresh Image
            GetImagesFromDatabase();
        }

        private void importDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            storeDB store = new storeDB();
            store.ShowDialog();
            GetImagesFromDatabase();
        }
        
        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/



        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //progressBar1.InitializeLifetimeService();
            //progressBar1.Step = 10;
            //progressBar1.Visible = true;
            Thread matchTH = new Thread(new ThreadStart(applyMatch));
            matchTH.Start();
            //applyMatch();
            matchDone = false;
            progressBar1.Visible = true;
            //progressBar1.Value = 100;
            while (!matchDone)
            {
                progressBar1.Increment(10);
                System.Threading.Thread.Sleep(500);
                if (progressBar1.Value == 100)
                    progressBar1.Value = 0;
            }
            progressBar1.Value = 0;
            progressBar1.Visible = false;
            matchTH.Abort();
            tabCtrl.TabPages.Add(Result);
        }

        private void applyMatch()
        {
            //progressBar1.Maximum = 100;
            //progressBar1.Minimum = 0;
            //progressBar1.Increment(10);

            for (int i = 0; i < 10; i++)
                Rindex[i] = -2;
                ForwardFourierTransformation();         // This applies to image from file only!
            ComplexImage cimageDB;
            Complex[,] result = new Complex[cImage.Height, cImage.Width];
            ComplexImage rimage = new ComplexImage(cImage.Width, cImage.Height);
            Complex[,] comImageFile = new Complex[cImage.Height, cImage.Width];
            Complex[,] comDB = new Complex[cImage.Height, cImage.Width];
            //int[][] results;
            for (int i = 0; i < cImage.Height; i++)
                for (int j = 0; j < cImage.Width; j++)
                    comImageFile[i, j] = new Complex(cImage.data[i, j].Re, cImage.data[i, j].Im);

            ////////////////////////////////////////////

            //////////////////////////////
            // import all images  in DB into dataset d
            // d is a data set like 2d array ---
            //d[][0] holds id ---- d[][1] holds image in byte[]
            Iris_Matching_System.irisDBDataSetTableAdapters.iris_imagesTableAdapter im = new Iris_Matching_System.irisDBDataSetTableAdapters.iris_imagesTableAdapter();
            d = new irisDBDataSet.iris_imagesDataTable();
            d = im.GetData();
            // d[][0] holds id's  ,,, d[][1] holds images in bytes

            MemoryStream memoryStream;// = new MemoryStream();
            System.Drawing.Image imageDB;
            byte[] byte1;
            Bitmap imageDB1;
            IMAarray = new System.Drawing.Bitmap[d.Count.GetHashCode()];
            results = new int[d.Count.GetHashCode(),3];
            string name2;
            name = Filename.Substring(Filename.LastIndexOf("L") + 1, 2);
            name2 = Filename.Substring(Filename.LastIndexOf("S") + 2, 3);

            for (int i2 = 0; i2 < d.Count.GetHashCode(); i2++)
            {
                if (!(int.Parse(name) == (int)d[i2][2] && int.Parse(name2) == (int)d[i2][0]))
                {
                    // convert image in byte[] to bitmap image
                    byte1 = null;
                    byte1 = (byte[])d[i2][1];
                    //convert from byte to image
                    memoryStream = new MemoryStream();
                    memoryStream.Write(byte1, 0, byte1.Length);
                    imageDB1 = (Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
                    // keep copy of images in bitmap[] to show the images later
                    IMAarray[i2] = imageDB1;


                    cImageDB = ComplexImage.FromBitmap(imageDB1);
                    cImageDB.ForwardFourierTransform();

                    for (int i = 0; i < cImageDB.Height; i++)
                        for (int j = 0; j < cImageDB.Width; j++)
                            comDB[i, j] = new Complex(cImageDB.data[i, j].Re, cImageDB.data[i, j].Im);

                    result = ComplexOperations.Match(comImageFile, comDB);

                    for (int i = 0; i < cImage.Height; i++)
                        for (int j = 0; j < cImage.Width; j++)
                            //cImage.data[i, j] = new AForge.Math.Complex(result[i, j].Re, result[i, j].Im);
                            cImageDB.data[i, j] = new AForge.Math.Complex(result[i, j].Re, result[i, j].Im);
                    cImageDB.BackwardFourierTransform();
                    // use 2d array to save index, id, match result
                    results[i2, 1] = (int)d[i2][0]; // get the id
                    results[i2, 0] = i2; // get the index to find the image after sorting
                    // get the match value (max value for each image)
                    for (int i = 0; i < cImageDB.Height; i++)
                        for (int j = 0; j < cImageDB.Width; j++)
                            results[i2, 2] = Math.Max(results[i2, 1], (int)cImageDB.data[i, j].Re);

                }
            }

            //name = Filename.Substring(Filename.LastIndexOf("S") + 2, Filename.LastIndexOf("L") - (Filename.LastIndexOf("S")+2));
            name = Filename.Substring(Filename.LastIndexOf("S") + 2, 3);

            //name = Filename.Substring(
            selectionSort(results); // sort by max value
            int ri =0;
            int i3 = int.Parse(name);
            string s3;//= results[i, 1].ToString();
            for (int i = 0; i < results.GetLength(0); i++)
            {
                s3 = results[i, 1].ToString();
                if (i3 == results[i, 1])
                {
                    Rindex[ri] = i;
                    ri++;
                }
            }

            index = 0;// to be used in top10()
            top10(1,10);
            //tabCtrl.TabPages.Add(Result);
            //progressBar1.Value = 100;
            matchDone = true;
            //tabCtrl.TabPages.Add(Result);
    
        }

        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        private void findMax()
        {
            for(int index = 0; index < 15; index++)
                for(int i = 0; i < height; i++)
                    for(int j = 0; j < width; j++)
                    {
                        //max[index, 0] = index;                                      // image index in DB
                        //max[index, 1] = Math.Max(max, cImage.data[i,j].Re);         // match value
                    }
        }

        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        /*****************************************************************************************************************************************/
        private void top10(int a, int x)
        {
            // if a = 1 (show next 10)
            try
            {
                pictureBox11.Image = image;
                rank.Text = "rank:" + Rindex[0];
                for (int i = 1; i < 10; i++)
                {
                    if(Rindex[i]!= -2)
                        rank.Text = rank.Text + "; " + Rindex[i];
                }
                    if (a == 1)
                    {
                        if (x == 100)
                            index = index + 100;
                        //for (int i = 1; i < 10; i++)
                        //{
                            //pictureBox1.Paint = "red";
                            R1.Text = index.ToString();
                            pictureBox1.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox1.Padding = new Padding(3);
                            else
                                pictureBox1.Padding = new Padding(0);
                            //pictureBox1.Paint();

                            R2.Text = index.ToString();
                            pictureBox2.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox2.Padding = new Padding(3);
                            else
                                pictureBox2.Padding = new Padding(0);

                            R3.Text = index.ToString();
                            pictureBox3.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox3.Padding = new Padding(3);
                            else
                                pictureBox3.Padding = new Padding(0);

                            R4.Text = index.ToString();
                            pictureBox4.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox4.Padding = new Padding(3);
                            else
                                pictureBox4.Padding = new Padding(0);

                            R5.Text = index.ToString();
                            pictureBox5.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox5.Padding = new Padding(3);
                            else
                                pictureBox5.Padding = new Padding(0);

                            R6.Text = index.ToString();
                            pictureBox6.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox6.Padding = new Padding(3);
                            else
                                pictureBox6.Padding = new Padding(0);

                            R7.Text = index.ToString();
                            pictureBox7.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox7.Padding = new Padding(3);
                            else
                                pictureBox7.Padding = new Padding(0);

                            R8.Text = index.ToString();
                            pictureBox8.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox8.Padding = new Padding(3);
                            else
                                pictureBox8.Padding = new Padding(0);

                            R9.Text = index.ToString();
                            pictureBox9.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox9.Padding = new Padding(3);
                            else
                                pictureBox9.Padding = new Padding(0);

                            R10.Text = index.ToString();
                            pictureBox10.Image = IMAarray[results[index++, 0]];
                            if (isExist(index - 1))
                                pictureBox10.Padding = new Padding(3);
                            else
                                pictureBox10.Padding = new Padding(0);

                            
                        //}
                    }
                    // if a = -1 (show previous 10)
                    else if (a == -1)
                    {
                        index = index - 20;
                        if (x == 100)
                            index = index - 100;

                        if (index < 0)
                            index = 0;
                        R1.Text = index.ToString();
                        pictureBox1.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox1.Padding = new Padding(3);
                        else
                            pictureBox1.Padding = new Padding(0);
                        //pictureBox1.Paint();

                        R2.Text = index.ToString();
                        pictureBox2.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox2.Padding = new Padding(3);
                        else
                            pictureBox2.Padding = new Padding(0);

                        R3.Text = index.ToString();
                        pictureBox3.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox3.Padding = new Padding(3);
                        else
                            pictureBox3.Padding = new Padding(0);

                        R4.Text = index.ToString();
                        pictureBox4.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox4.Padding = new Padding(3);
                        else
                            pictureBox4.Padding = new Padding(0);

                        R5.Text = index.ToString();
                        pictureBox5.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox5.Padding = new Padding(3);
                        else
                            pictureBox5.Padding = new Padding(0);

                        R6.Text = index.ToString();
                        pictureBox6.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox6.Padding = new Padding(3);
                        else
                            pictureBox6.Padding = new Padding(0);

                        R7.Text = index.ToString();
                        pictureBox7.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox7.Padding = new Padding(3);
                        else
                            pictureBox7.Padding = new Padding(0);

                        R8.Text = index.ToString();
                        pictureBox8.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox8.Padding = new Padding(3);
                        else
                            pictureBox8.Padding = new Padding(0);

                        R9.Text = index.ToString();
                        pictureBox9.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox9.Padding = new Padding(3);
                        else
                            pictureBox9.Padding = new Padding(0);

                        R10.Text = index.ToString();
                        pictureBox10.Image = IMAarray[results[index++, 0]];
                        if (isExist(index - 1))
                            pictureBox10.Padding = new Padding(3);
                        else
                            pictureBox10.Padding = new Padding(0);
                    }
            }
            catch { throw new Exception("The method or operation is not implemented."); };
        }

        private Boolean isExist(int index)
        {
            for (int i = 0; i < 10; i++)
            {
                if (Rindex[i] == index)
                    return true;
            }
            return false;
        }


        private object IndexOutOfRangeException()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        private void sortArrayByMatchValue()
        {
            // the array max is a two dimensional array
            // [index, match value]
            // it is initially sorted by index, we need it to be sorted by match value
        }

        public static void selectionSort(int[,] Array)
        {
            int i, j;
            int max, temp, temp2, temp3;
            int l = Array.GetLength(0) - 1;
            for (i = 0; i <= Array.GetLength(0) - 1; i++)
            {
                max = i;
                for (j = i + 1; j <= Array.GetLength(0) - 1; j++)
                {
                    if (Array[j, 2] > Array[max, 2])
                        max = j;
                }
                temp = Array[i, 1];
                temp2 = Array[i, 0];
                temp3 = Array[i, 2];
                Array[i, 1] = Array[max, 1];
                Array[i, 0] = Array[max, 0];
                Array[i, 2] = Array[max, 2];
                Array[max, 1] = temp;
                Array[max, 0] = temp2;
                Array[max, 2] = temp3;
            }
        }
        ///////////////////////

        //////////////
        //When user changes row selection, display image of selected row in picture box.
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //Get image data from gridview column.
                byte[] imageData = (byte[])dataGridView1.Rows[e.RowIndex].Cells["iris_Image"].Value;

                //Initialize image variable
                System.Drawing.Image newImage;
                //Read image data into a memory stream
                using (MemoryStream ms = new MemoryStream(imageData, 0, imageData.Length))
                {
                    ms.Write(imageData, 0, imageData.Length);

                    //Set image variable value using memory stream.
                    newImage = System.Drawing.Image.FromStream(ms, true);
                }

                //set picture
                
                pictureBoxDB.Image = newImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            top10(-1,10);
        }

        private void next_Click(object sender, EventArgs e)
        {
            top10(1,10);
        }

        //////////////////////

        //public IFilter Filter
        //{
            //get { return filter; }
        //}

        private void ApplyFilter2(IFilter filter)
        {
            try
            {
                // set wait cursor
                this.Cursor = Cursors.WaitCursor;

                // apply filter to the image
                Bitmap newImage = filter.Apply(image);

                //if (host.CreateNewDocumentOnChange)
                //{
                    // open new image in new document
                  //  host.NewDocument(newImage);
                //}
                //else
                //{
                    //if (host.RememberOnChange)
                    //{
                        // backup current image
                      //  if (backup != null)
                        //    backup.Dispose();

                        //backup = image;
                    //}
                    //else
                    //{
                        // release current image
                      //  image.Dispose();
                    //}

                    image = newImage;

                    // update
                    //UpdateNewImage();
                //}
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Selected filter can not be applied to the image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // restore cursor
                this.Cursor = Cursors.Default;
            }
        }

        private void cannyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CannyDetectorForm form = new CannyDetectorForm();
            //myImage = image;
            form.Image = image;

            if (form.ShowDialog() == DialogResult.OK)
            {
                ApplyFilter(form.Filter);
                //form.Filter;
            }
        }

                // Extract separate blobs
        
        private void blobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                MessageBox.Show("Blob extractor can be applied to binary images only", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(image);
            Blob[] blobs = blobCounter.GetObjects(image);
            float myhigh, myw, Lmyh = 0, Lmyw = 0;
            int myi = 0, xAxis, yAxis;
            Point P;
            //myCimage = new ComplexImage();
            //foreach ( Blob blob in blobs )
            //{

            for (int i = 0; i < blobs.Length; i++)
            {

                myhigh = blobs[i].Image.PhysicalDimension.Height;
                myw = blobs[i].Image.PhysicalDimension.Height;
                if (myhigh > Lmyh)
                {
                    Lmyh = myhigh;
                    myi = i;
                }
                //host.NewDocument(blobs[myi].Image);
                //host.NewDocument(blob[i]);
            }
            //host.NewDocument(blobs[myi].Image);
            picBox.Image = blobs[myi].Image;
            P = blobs[myi].Location;
            
            xAxis = blobs[myi].Image.Width;
            yAxis = blobs[myi].Image.Height;
            //int L = blobs[myi].Image;
            //myImage = (Bitmap)blobs[myi].Image;
            //myImage = image;
            //myCimage = ComplexImage.FromBitmap(myImage);
            //Complex[,] myCom = new Complex[myImage.Width, myImage.Height];

            //for (int i = 0; i < myImage.Width; i++)
            //for (int j = 0; j < myImage.Width; j++)
            //if (!(i > P.X && i < (P.X + xAxis) && j > P.Y && j < (P.Y + yAxis)))
            //myCom[i, j] = myCimage.
            //}
        }

        private void localize(int x, int y)
        {
            local = image;

            cImageDB = ComplexImage.FromBitmap(image);
            //cImageDB.ForwardFourierTransform();

            //for (int i = 0; i < cImageDB.Height; i++)
                //for (int j = 0; j < cImageDB.Width; j++)
                    //if(i > x-20 && i<)
                    //comDB[i, j] = new Complex(cImageDB.data[i, j].Re, cImageDB.data[i, j].Im);
            
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

        
        private void button1_Click(object sender, EventArgs e)
        {
            top10(1, 100);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            top10(-1, 100);
        }

        private void IMSForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'irisDBDataSet.iris_images' table. You can move, or remove it, as needed.
            this.iris_imagesTableAdapter.Fill(this.irisDBDataSet.iris_images);

        }

    }
}
using RayTracer.Lib;
using RayTracer.Test;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayTracer.WinForms
{
    public partial class RayTraceForm : Form
    {
        private delegate void FromAccessDelegate();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly bool saveFiles;

        private PictureBox pictureBox;

        private Bitmap[] bitmaps = new Bitmap[2];
        private int currentBitmap = 0;

        private int width;
        private int height;
        private BitmapWriter bitmapWriter;
        private FromAccessDelegate refresh;
        private FromAccessDelegate close;
        private string dirName;
        private string dir;

        public RayTraceForm(bool saveFiles)
        {
            this.saveFiles = saveFiles;
            InitializeComponent();
            RenderFormIcon();

            width = 400;
            height = (int)(width * (9f / 16f));
            Text = "RayTracer";
            Width = width + 30;
            Height = height + 50;

            bitmaps[0] = new Bitmap(width, height);
            bitmaps[1] = new Bitmap(width, height);

            pictureBox = new PictureBox();
            pictureBox.BackColor = Color.Black;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox.Dock = DockStyle.Fill;

            Controls.Add(pictureBox);

            dir = Directory.GetCurrentDirectory();
            var startTime = DateTime.Now;

            refresh = new FromAccessDelegate(Refresh);
            close = new FromAccessDelegate(Close);
            dirName = Path.Combine(dir, startTime.ToString("O").Replace(':', '-'));

            bitmapWriter = new BitmapWriter();

            if (saveFiles)
                Directory.CreateDirectory(dirName);

            Activated += RayTraceForm_Activated;
            FormClosing += RayTraceForm_FormClosing;
        }

        private void RayTraceForm_Activated(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var colors = new Color[width * height];
                var frameIndex = 0;
                var token = cancellationTokenSource.Token;
                while (WindowState != FormWindowState.Minimized && ActiveForm == this && !token.IsCancellationRequested)
                {
                    Thread.Sleep(1);

                    currentBitmap = (currentBitmap + 1) % 2;
                    TestScene.RenderScene(colors, width, height, true, cancellationTokenSource.Token);

                    if (token.IsCancellationRequested) 
                        break;

                    bitmapWriter.WriteToBitmap(bitmaps[currentBitmap], colors, width, height);
                    pictureBox.Image = bitmaps[currentBitmap];

                    if (saveFiles) 
                        bitmaps[currentBitmap].Save(Path.Combine(dirName, $"{frameIndex++:0000}.jpg"), ImageFormat.Jpeg);

                    if (token.IsCancellationRequested) 
                        break;

                    Invoke(refresh);
                    TestScene.Sphere.Move(-1 * TestScene.SphereCenter);
                    TestScene.SphereCenter = MeshObject.RotateVector(TestScene.SphereCenter, 0, -1 * MathF.PI / 32, 0);
                    TestScene.Sphere.Move(TestScene.SphereCenter);
                    TestScene.Sphere.Bound(4);
                    TestScene.MeshObj.Rotate(-1 * MathF.PI / 128, -1 * MathF.PI / 128, 0);
                }

                if (token.IsCancellationRequested) 
                    Invoke(close);
            });
        }

        private void RayTraceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            }
        }

        private void RenderFormIcon()
        {
            var iconBitmap = AppIconScene.RenderIconBitmap(128);
            var hicon = iconBitmap.GetHicon();
            Icon = Icon.FromHandle(hicon);
        }
    }
}

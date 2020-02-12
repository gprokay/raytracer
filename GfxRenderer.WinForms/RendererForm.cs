using GfxRenderer.Lib;
using GfxRenderer.Test;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GfxRenderer.WinForms
{
    public partial class RendererForm : Form
    {
        private delegate void FromAccessDelegate();

        private RasterizationTestScene scene = new RasterizationTestScene(6);

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly bool saveFiles;

        private PictureBox pictureBox;

        private DirectBitmap[] bitmaps = new DirectBitmap[2];
        private int currentBitmap = 0;

        private int width;
        private int height;
        private BitmapWriter bitmapWriter;
        private FromAccessDelegate refresh;
        private FromAccessDelegate close;
        private string dirName;
        private string dir;

        private int threadCount = 0;
        private Task task;

        public RendererForm(bool saveFiles)
        {
            this.saveFiles = saveFiles;
            InitializeComponent();
            RenderFormIcon();

            width = 1600;
            height = (int)(width * (9f / 16f));
            Text = "Renderer";
            Width = width + 30;
            Height = height + 50;

            bitmaps[0] = new DirectBitmap(width, height);
            bitmaps[1] = new DirectBitmap(width, height);

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

        private async void RayTraceForm_Activated(object sender, EventArgs e)
        {
            var runningTask = task;
            if (runningTask != null)
            {
                await runningTask;
            }

            Interlocked.Increment(ref threadCount);
            if (threadCount > 1)
            {
                throw new InvalidOperationException();
            }

            task = Task.Factory.StartNew(() =>
            {
                var colors = new int[width * height];
                var frameIndex = 0;
                var token = cancellationTokenSource.Token;
                while (WindowState != FormWindowState.Minimized && ActiveForm == this && !token.IsCancellationRequested)
                {
                    Thread.Sleep(1);

                    currentBitmap = (currentBitmap + 1) % 2;
                    var msg = scene.RenderScene(colors, width, height, true, cancellationTokenSource.Token);

                    if (token.IsCancellationRequested)
                        break;

                    bitmaps[currentBitmap].SetPixels(colors);
                    pictureBox.Image = bitmaps[currentBitmap].Bitmap;
                    
                    using(var gfx = Graphics.FromImage(bitmaps[currentBitmap].Bitmap))
                    {
                        gfx.DrawString(msg, Font, Brushes.Black, 0, 0);
                    }

                    if (saveFiles)
                        bitmaps[currentBitmap].Bitmap.Save(Path.Combine(dirName, $"{frameIndex++:0000}.jpg"), ImageFormat.Jpeg);

                    if (token.IsCancellationRequested)
                        break;

                    Invoke(refresh);
                    scene.Sphere.Mesh.Move(-1 * scene.SphereCenter);
                    scene.SphereCenter = MeshObject.RotateVector(scene.SphereCenter, 0, -1 * MathF.PI / 32, 0);
                    scene.Sphere.Mesh.Move(scene.SphereCenter);
                    scene.Cube.Mesh.Rotate(-1 * MathF.PI / 128, -1 * MathF.PI / 128, 0);
                }

                Interlocked.Decrement(ref threadCount);

                if (token.IsCancellationRequested)
                    Invoke(close);
            });

            await task;
            task = null;
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

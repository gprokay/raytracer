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

        private ITestScene rayTracerScene = new RayTracerTestScene();
        private ITestScene rasterizationScene = new RasterizationTestScene(meshcomplexity: 4);

        private ITestScene scene;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly bool saveFiles;

        private PictureBox pictureBox;

        private DirectBitmap[] bitmaps = new DirectBitmap[2];
        private int currentBitmap = 0;

        private bool changed = false;
        private int width;
        private int height;
        private FromAccessDelegate refresh;
        private FromAccessDelegate close;
        private string dirName;
        private string dir;

        private int threadCount = 0;
        private Task task;

        public RendererForm(bool saveFiles)
        {
            this.saveFiles = saveFiles;
            scene = rasterizationScene;
            InitializeComponent();
            RenderFormIcon();

            width = 800;
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

            if (saveFiles)
                Directory.CreateDirectory(dirName);

            Activated += RayTraceForm_Activated;
            FormClosing += RayTraceForm_FormClosing;
            pictureBox.Click += RendererForm_Click;
        }

        private void RendererForm_Click(object sender, EventArgs e)
        {
            changed = true;
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
                RenderScene(bitmaps, scene);
            });

            await task;
            task = null;
        }

        private void RenderScene(DirectBitmap[] bitmaps, ITestScene scene)
        {
            var colors = new int[width * height];
            var frameIndex = 0;
            var token = cancellationTokenSource.Token;
            while (WindowState != FormWindowState.Minimized && ActiveForm == this && !token.IsCancellationRequested)
            {
                currentBitmap = (currentBitmap + 1) % 2;
                var msg = scene.RenderScene(colors, width, height, true, cancellationTokenSource.Token);

                if (token.IsCancellationRequested)
                    break;

                bitmaps[currentBitmap].SetPixels(colors);
                pictureBox.Image = bitmaps[currentBitmap].Bitmap;

                using (var gfx = Graphics.FromImage(bitmaps[currentBitmap].Bitmap))
                {
                    gfx.DrawString(Thread.CurrentThread.ManagedThreadId + ": " + msg, Font, Brushes.Black, 0, 0);
                }

                if (saveFiles)
                    bitmaps[currentBitmap].Bitmap.Save(Path.Combine(dirName, $"{frameIndex++:0000}.jpg"), ImageFormat.Jpeg);

                if (token.IsCancellationRequested)
                    break;

                Invoke(refresh);
                rayTracerScene.Animate();
                rasterizationScene.Animate();
                if (changed)
                {
                    changed = false;
                    scene = scene is RayTracerTestScene
                        ? rasterizationScene
                        : rayTracerScene;
                }
            }

            Interlocked.Decrement(ref threadCount);

            if (token.IsCancellationRequested)
                Invoke(close);
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

using RayTracer.Lib;
using RayTracer.Test;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayTracer.WinForms
{
    public partial class Form1 : Form
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public Form1()
        {
            var width = 800;
            var height = (int)(width * (9f / 16f));

            InitializeComponent();

            Width = width + 30;
            Height = height + 50;

            var pictureBox = new PictureBox();
            pictureBox.BackColor = Color.Black;
            pictureBox.Width = width;
            pictureBox.Height = height;

            var bitmap = new Bitmap(width, height);
            pictureBox.Image = bitmap;

            var bitmapWriter = new BitmapWriter();
            Controls.Add(pictureBox);

            Activated += async (object o, EventArgs e) =>
            {
                while (true)
                {
                    await Task.Delay(1);
                    Color[] result = await Task<Color[]>.Factory.StartNew(() => SceneTest.RenderScene(width, height, true, cancellationTokenSource.Token));
                    if (cancellationTokenSource.IsCancellationRequested) break;
                    bitmapWriter.WriteToBitmap(bitmap, result, width, height);
                    Refresh();
                    SceneTest.Sphere.Center = MeshObject.RotateVector(SceneTest.Sphere.Center, 0, MathF.PI / 64, 0);
                    SceneTest.MeshObj.Rotate(-1 * MathF.PI / 128, -1 * MathF.PI / 128, 0);
                }
            };
            FormClosing += (object o, FormClosingEventArgs e) => { cancellationTokenSource.Cancel(); };
        }


    }
}

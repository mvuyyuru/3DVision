//modified from 
//http://stackoverflow.com/questions/5935411/how-to-control-stereo-frames-separately-with-c-nvidia-3d-shutter-glasses
//to be called from matlab and to preload image bitmaps, etc.
//to reduce time lag between request stereo image change in
//matlab and actual stereo image change on screen

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Diagnostics;

namespace NVStereoTest
{
    public class WinForm : System.Windows.Forms.Form
    {
        public Device _device;
        public System.ComponentModel.Container components = null;
        public Surface _imageBuf;
        public Surface _backBuf;
        public List<Surface> _imageLeftList = new List<Surface>();
        public List<Surface> _imageRightList = new List<Surface>();
        //1920, 1080 or 1366,768 or ...
        public Rectangle _size = new Rectangle(0, 0, 1366, 768);
        public WinForm new_form;
        

        static void Main()
        {
            WinForm new_form = new WinForm();              
            new_form.InitializeComponent();
            new_form.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            new_form.InitializeDevice();
            new_form.LoadSurfaces();
            List<String> leftSources = new List<String>();
            List<String> rightSources = new List<String>();
            leftSources.Add("Blue.png");
            rightSources.Add("Red.png");
            new_form.LoadSurfacesImg(leftSources, rightSources);
            new_form.Set3D(0, 0);                
            Application.Run(new_form);
        }


        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Size = new System.Drawing.Size(_size.Width, _size.Height);
            this.Text = "Nvidia 3D Vision test";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        public void InitializeDevice()
        {
            PresentParameters presentParams = new PresentParameters();

            presentParams.Windowed = false;
            presentParams.BackBufferFormat = Format.A8R8G8B8;
            presentParams.BackBufferWidth = _size.Width;
            presentParams.BackBufferHeight = _size.Height;
            presentParams.BackBufferCount = 1;
            presentParams.SwapEffect = SwapEffect.Discard;
            presentParams.PresentationInterval = PresentInterval.One;            
            _device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);

        }

        public void LoadSurfaces()
        {
            _imageBuf = _device.CreateOffscreenPlainSurface(_size.Width * 2, _size.Height + 1, Format.A8R8G8B8, Pool.Default);
            
        }

        public void LoadSurfacesImg(List<String> leftImageListSrc, List<String> rightImageListSrc)
        {
            foreach (var leftImageSrc in leftImageListSrc)
            {
                _imageLeftList.Add(Surface.FromBitmap(_device, (Bitmap)Bitmap.FromFile(leftImageSrc), Pool.Default));               
            }
            foreach (var rightImageSrc in rightImageListSrc)
            {
                _imageRightList.Add(Surface.FromBitmap(_device, (Bitmap)Bitmap.FromFile(rightImageSrc), Pool.Default));
            }
        }

        public void Set3D(int left_index, int right_index)
        {
            Rectangle destRect = new Rectangle(0, 0, _size.Width, _size.Height);
            _device.StretchRectangle(_imageLeftList[left_index], _size, _imageBuf, destRect, TextureFilter.None);
            destRect.X = _size.Width;
            _device.StretchRectangle(_imageRightList[right_index], _size, _imageBuf, destRect, TextureFilter.None);

            GraphicsStream gStream = _imageBuf.LockRectangle(LockFlags.None);

            byte[] data = new byte[] {0x4e, 0x56, 0x33, 0x44,   //NVSTEREO_IMAGE_SIGNATURE         = 0x4433564e
                                      0x00, 0x0F, 0x00, 0x00,   //Screen width * 2 = 1920*2 = 3840 = 0x00000F00;
                                      0x38, 0x04, 0x00, 0x00,   //Screen height = 1080             = 0x00000438;
                                      0x20, 0x00, 0x00, 0x00,   //dwBPP = 32                       = 0x00000020;
                                      0x02, 0x00, 0x00, 0x00};  //dwFlags = SIH_SCALE_TO_FIT       = 0x00000002;

            gStream.Seek(_size.Width * 2 * _size.Height * 4, System.IO.SeekOrigin.Begin);   //last row
            gStream.Write(data, 0, data.Length);

            gStream.Close();

            _imageBuf.UnlockRectangle();
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            _device.BeginScene();

            // Get the Backbuffer then Stretch the Surface on it.
            _backBuf = _device.GetBackBuffer(0, 0, BackBufferType.Mono);
            _device.StretchRectangle(_imageBuf, new Rectangle(0, 0, _size.Width * 2, _size.Height + 1), _backBuf, new Rectangle(0, 0, _size.Width, _size.Height), TextureFilter.None);
            _backBuf.ReleaseGraphics();            

            _device.EndScene();

            _device.Present();

            this.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }        
    }
}

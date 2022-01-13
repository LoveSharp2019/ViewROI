using HalconDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ViewROI
{
	public partial class Form1 : Form
    {

		public HWndCtrl viewController;

		public ROIController roiController;
		public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
			String fileName = "patras";
			HImage image;

			viewController = new HWndCtrl(hWindowControl1);
			roiController = new ROIController();
			viewController.useROIController(roiController);
			viewController.setViewState(HWndCtrl.MODE_VIEW_NONE);

			try
			{
				image = new HImage(fileName);
			}
			catch (HOperatorException)
			{
				MessageBox.Show("Problem occured while reading file!",
					"InteractROIForm",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			viewController.addIconicVar(image);
			viewController.repaint();
		}

        private void Rect1Button_Click(object sender, EventArgs e)
        {
			roiController.setROIShape(new ROIRectangle1());
		}

        private void Rect2Button_Click(object sender, EventArgs e)
        {
			roiController.setROIShape(new ROIRectangle2());
		}

        private void CircleButton_Click(object sender, EventArgs e)
        {
			roiController.setROIShape(new ROICircle());
		}

        private void CircArcButton_Click(object sender, EventArgs e)
        {
			roiController.setROIShape(new ROICircularArc());
		}

        private void LineButton_Click(object sender, EventArgs e)
        {
			roiController.setROIShape(new ROILine());
		}

        private void DelActROIButton_Click(object sender, EventArgs e)
        {
			roiController.removeActive();
		}

        private void ResetButton_Click(object sender, EventArgs e)
        {
			viewController.resetAll();
			viewController.repaint();
		}

        private void radioButtonNone_CheckedChanged(object sender, EventArgs e)
        {
			viewController.setViewState(HWndCtrl.MODE_VIEW_NONE);
		}

        private void radioButtonZoom_CheckedChanged(object sender, EventArgs e)
        {
			viewController.setViewState(HWndCtrl.MODE_VIEW_ZOOM);
		}

        private void radioButtonMove_CheckedChanged(object sender, EventArgs e)
        {
			viewController.setViewState(HWndCtrl.MODE_VIEW_MOVE);
		}
    }

    public class FunctionPlot
	{
		public const int AXIS_RANGE_FIXED = 3;

		public const int AXIS_RANGE_INCREASING = 4;

		public const int AXIS_RANGE_ADAPTING = 5;

		private Graphics gPanel;

		private Graphics backBuffer;

		private Pen pen;

		private Pen penCurve;

		private Pen penCursor;

		private SolidBrush brushCS;

		private SolidBrush brushFuncPanel;

		private Font drawFont;

		private StringFormat format;

		private Bitmap functionMap;

		private float panelWidth;

		private float panelHeight;

		private float margin;

		private float originX;

		private float originY;

		private PointF[] points;

		private HFunction1D func;

		private int axisAdaption;

		private float axisXLength;

		private float axisYLength;

		private float scaleX;

		private float scaleY;

		private int PreX;

		private int BorderRight;

		private int BorderTop;

		public FunctionPlot(Control panel, bool useMouseHandle)
		{
			gPanel = panel.CreateGraphics();
			panelWidth = panel.Size.Width - 32;
			panelHeight = panel.Size.Height - 22;
			originX = 32f;
			originY = panel.Size.Height - 22;
			margin = 5f;
			BorderRight = (int)(panelWidth + originX - margin);
			BorderTop = (int)panelHeight;
			PreX = 0;
			scaleX = (scaleY = 0f);
			axisAdaption = 5;
			axisXLength = 10f;
			axisYLength = 10f;
			pen = new Pen(Color.DarkGray, 1f);
			penCurve = new Pen(Color.Blue, 1f);
			penCursor = new Pen(Color.LightSteelBlue, 1f);
			penCursor.DashStyle = DashStyle.Dash;
			brushCS = new SolidBrush(Color.Black);
			brushFuncPanel = new SolidBrush(Color.White);
			drawFont = new Font("Arial", 6f);
			format = new StringFormat();
			format.Alignment = StringAlignment.Far;
			functionMap = new Bitmap(panel.Size.Width, panel.Size.Height);
			backBuffer = Graphics.FromImage(functionMap);
			resetPlot();
			panel.Paint += paint;
			if (useMouseHandle)
			{
				panel.MouseMove += mouseMoved;
			}
		}

		public FunctionPlot(Control panel)
			: this(panel, useMouseHandle: false)
		{
		}

		public void setOrigin(int x, int y)
		{
			if (x >= 1 && y >= 1)
			{
				float num = originX;
				originX = x;
				originY = y;
				panelWidth = panelWidth + num - originX;
				panelHeight = originY;
				BorderRight = (int)(panelWidth + originX - margin);
				BorderTop = (int)panelHeight;
			}
		}

		public void setAxisAdaption(int mode, float val)
		{
			if (mode == 3)
			{
				axisAdaption = mode;
				axisYLength = ((val > 0f) ? val : 255f);
			}
			else
			{
				axisAdaption = mode;
			}
		}

		public void setAxisAdaption(int mode)
		{
			setAxisAdaption(mode, -1f);
		}

		public void plotFunction(double[] grayValues)
		{
			drawFunction(new HTuple(grayValues));
		}

		public void plotFunction(float[] grayValues)
		{
			drawFunction(new HTuple(grayValues));
		}

		public void plotFunction(int[] grayValues)
		{
			drawFunction(new HTuple(grayValues));
		}

		private void drawFunction(HTuple tuple)
		{
			if (tuple.Length == 0)
			{
				resetPlot();
				return;
			}
			HTuple hTuple = tuple.TupleSortIndex();
			int num = tuple.Length - 1;
			int num2 = (int)tuple[hTuple.Length - 1].D;
			axisXLength = num;
			switch (axisAdaption)
			{
				case 5:
					axisYLength = num2;
					break;
				case 4:
					axisYLength = (((float)num2 > axisYLength) ? ((float)num2) : axisYLength);
					break;
			}
			backBuffer.Clear(Color.WhiteSmoke);
			backBuffer.FillRectangle(brushFuncPanel, originX, 0f, panelWidth, panelHeight);
			float stepOffset = drawXYLabels();
			drawLineCurve(tuple, stepOffset);
			backBuffer.Flush();
			gPanel.DrawImageUnscaled(functionMap, 0, 0);
			gPanel.Flush();
		}

		public void resetPlot()
		{
			backBuffer.Clear(Color.WhiteSmoke);
			backBuffer.FillRectangle(brushFuncPanel, originX, 0f, panelWidth, panelHeight);
			func = null;
			drawXYLabels();
			backBuffer.Flush();
			repaint();
		}

		private void repaint()
		{
			gPanel.DrawImageUnscaled(functionMap, 0, 0);
			gPanel.Flush();
		}

		private void drawLineCurve(HTuple tuple, float stepOffset)
		{
			if (stepOffset > 1f)
			{
				points = scaleDispValue(tuple);
			}
			else
			{
				points = scaleDispBlockValue(tuple);
			}
			int num = points.Length;
			func = new HFunction1D(tuple);
			for (int i = 0; i < num - 1; i++)
			{
				backBuffer.DrawLine(penCurve, points[i], points[i + 1]);
			}
		}

		private PointF[] scaleDispValue(HTuple tup)
		{
			float num = axisXLength;
			float num2 = axisYLength;
			scaleX = ((num != 0f) ? ((panelWidth - margin) / num) : 0f);
			scaleY = ((num2 != 0f) ? ((panelHeight - margin) / num2) : 0f);
			int length = tup.Length;
			PointF[] array = new PointF[length];
			for (int i = 0; i < length; i++)
			{
				float num3 = (float)tup[i].D;
				float x = originX + (float)i * scaleX;
				float y = panelHeight - num3 * scaleY;
				ref PointF reference = ref array[i];
				reference = new PointF(x, y);
			}
			return array;
		}

		private PointF[] scaleDispBlockValue(HTuple tup)
		{
			float num = axisXLength;
			float num2 = axisYLength;
			scaleX = ((num != 0f) ? ((panelWidth - margin) / num) : 0f);
			scaleY = ((num2 != 0f) ? ((panelHeight - margin) / num2) : 0f);
			int length = tup.Length;
			PointF[] array = new PointF[length * 2];
			float y = 0f;
			int num3 = 0;
			float num4;
			float x;
			for (int i = 0; i < length; i++)
			{
				num4 = (float)tup[i].D;
				x = originX + (float)i * scaleX - scaleX / 2f;
				y = panelHeight - num4 * scaleY;
				ref PointF reference = ref array[num3];
				reference = new PointF(x, y);
				num3++;
				x = originX + (float)i * scaleX + scaleX / 2f;
				ref PointF reference2 = ref array[num3];
				reference2 = new PointF(x, y);
				num3++;
			}
			num3--;
			x = originX + (float)(length - 1) * scaleX;
			ref PointF reference3 = ref array[num3];
			reference3 = new PointF(x, y);
			num3 = 0;
			num4 = (float)tup[num3].D;
			x = originX;
			y = panelHeight - num4 * scaleY;
			ref PointF reference4 = ref array[num3];
			reference4 = new PointF(x, y);
			return array;
		}

		private float drawXYLabels()
		{
			float num = 0f;
			float num2 = 5f;
			float num3 = originX;
			float num4 = originY;
			float num5 = axisXLength;
			if ((double)num5 != 0.0)
			{
				num = (panelWidth - margin) / num5;
			}
			float num6 = (((double)num > 10.0) ? 1f : ((num > 2f) ? 10f : ((!((double)num > 0.2)) ? 1000f : 100f)));
			float num7 = 0f;
			float num8 = num4;
			float num9 = num * num5;
			backBuffer.DrawLine(pen, num3, num4, num3 + panelWidth - margin, num4);
			backBuffer.DrawLine(pen, num3 + num7, num8, num3 + num7, num8 + 6f);
			backBuffer.DrawString(string.Concat(0), drawFont, brushCS, num3 + num7 + 4f, num8 + 8f, format);
			backBuffer.DrawLine(pen, num3 + num9, num8, num3 + num9, num8 + 6f);
			backBuffer.DrawString(string.Concat((int)num5), drawFont, brushCS, num3 + num9 + 4f, num8 + 8f, format);
			float num10 = (int)(num5 / num6);
			num10 = ((num6 == 10f) ? (num10 - 1f) : num10);
			for (int i = 1; (float)i <= num10; i++)
			{
				num7 = num6 * (float)i * num;
				if (!(num9 - num7 < 20f))
				{
					backBuffer.DrawLine(pen, num3 + num7, num8, num3 + num7, num8 + 6f);
					backBuffer.DrawString(string.Concat((int)((float)i * num6)), drawFont, brushCS, num3 + num7 + 5f, num8 + 8f, format);
				}
			}
			float result = num6;
			float num11 = axisYLength;
			if ((double)num11 != 0.0)
			{
				num = (panelHeight - margin) / num11;
			}
			num6 = (((double)num > 10.0) ? 1f : ((num > 2f) ? 10f : (((double)num > 0.8) ? 50f : ((!((double)num > 0.12)) ? 1000f : 100f))));
			num7 = num3;
			num8 = 5f;
			float num12 = num4 - num * num11;
			backBuffer.DrawLine(pen, num3, num4, num3, num4 - (panelHeight - margin));
			backBuffer.DrawLine(pen, num7, num4, num7 - 10f, num4);
			backBuffer.DrawString(string.Concat(0), drawFont, brushCS, num7 - 12f, num4 - num2, format);
			backBuffer.DrawLine(pen, num7, num12, num7 - 10f, num12);
			backBuffer.DrawString(string.Concat(num11), drawFont, brushCS, num7 - 12f, num12 - num2, format);
			num10 = (int)(num11 / num6);
			num10 = ((num6 == 10f) ? (num10 - 1f) : num10);
			for (int i = 1; (float)i <= num10; i++)
			{
				num8 = num4 - num6 * (float)i * num;
				if (!(num8 - num12 < 10f))
				{
					backBuffer.DrawLine(pen, num7, num8, num7 - 10f, num8);
					backBuffer.DrawString(string.Concat((int)((float)i * num6)), drawFont, brushCS, num7 - 12f, num8 - num2, format);
				}
			}
			return result;
		}

		private void mouseMoved(object sender, MouseEventArgs e)
		{
			int x = e.X;
			if (PreX != x && !((float)x < originX) && x <= BorderRight && func != null)
			{
				PreX = x;
				int num = (int)Math.Round(((float)x - originX) / scaleX);

				HTuple yValueFunct1d = func.GetYValueFunct1d(new HTuple(num), "zero");
				float num2 = (float)yValueFunct1d[0].D;
				float num3 = panelHeight - num2 * scaleY;
				gPanel.DrawImageUnscaled(functionMap, 0, 0);
				gPanel.DrawLine(penCursor, x, 0, x, BorderTop);
				gPanel.DrawLine(penCursor, originX, num3, (float)BorderRight + margin, num3);
				gPanel.DrawString("X = " + num, drawFont, brushCS, panelWidth - margin, 10f);
				gPanel.DrawString("Y = " + (int)num2, drawFont, brushCS, panelWidth - margin, 20f);
				gPanel.Flush();
			}
		}

		private void paint(object sender, PaintEventArgs e)
		{
			repaint();
		}
	}

	public class GraphicsContext
	{
		public const string GC_COLOR = "Color";

		public const string GC_COLORED = "Colored";

		public const string GC_LINEWIDTH = "LineWidth";

		public const string GC_DRAWMODE = "DrawMode";

		public const string GC_SHAPE = "Shape";

		public const string GC_LUT = "Lut";

		public const string GC_PAINT = "Paint";

		public const string GC_LINESTYLE = "LineStyle";

		private Hashtable graphicalSettings;

		public Hashtable stateOfSettings;

		private IEnumerator iterator;

		public Action<string> gcNotification;

		public GraphicsContext()
		{
			graphicalSettings = new Hashtable(10, 0.2f);
			gcNotification = dummy;
			stateOfSettings = new Hashtable(10, 0.2f);
		}

		public GraphicsContext(Hashtable settings)
		{
			graphicalSettings = settings;
			gcNotification = dummy;
			stateOfSettings = new Hashtable(10, 0.2f);
		}

		public void applyContext(HWindow window, Hashtable cContext)
		{
			string text = "";
			string text2 = "";
			int num = -1;
			HTuple hTuple = null;
			iterator = cContext.Keys.GetEnumerator();
			try
			{
				while (iterator.MoveNext())
				{
					text = (string)iterator.Current;
					if (stateOfSettings.Contains(text) && stateOfSettings[text] == cContext[text])
					{
						continue;
					}
					switch (text)
					{
						case "Color":
							text2 = (string)cContext[text];
							window.SetColor(text2);
							if (stateOfSettings.Contains("Colored"))
							{
								stateOfSettings.Remove("Colored");
							}
							break;
						case "Colored":
							num = (int)cContext[text];
							window.SetColored(num);
							if (stateOfSettings.Contains("Color"))
							{
								stateOfSettings.Remove("Color");
							}
							break;
						case "DrawMode":
							text2 = (string)cContext[text];
							window.SetDraw(text2);
							break;
						case "LineWidth":
							num = (int)cContext[text];
							window.SetLineWidth(num);
							break;
						case "Lut":
							text2 = (string)cContext[text];
							window.SetLut(text2);
							break;
						case "Paint":
							text2 = (string)cContext[text];
							window.SetPaint(text2);
							break;
						case "Shape":
							text2 = (string)cContext[text];
							window.SetShape(text2);
							break;
						case "LineStyle":
							hTuple = (HTuple)cContext[text];
							window.SetLineStyle(hTuple);
							break;
					}
					if (num != -1)
					{
						if (stateOfSettings.Contains(text))
						{
							stateOfSettings[text] = num;
						}
						else
						{
							stateOfSettings.Add(text, num);
						}
						num = -1;
					}
					else if (text2 != "")
					{
						if (stateOfSettings.Contains(text))
						{
							stateOfSettings[text] = num;
						}
						else
						{
							stateOfSettings.Add(text, num);
						}
						text2 = "";
					}
					else if (hTuple != null)
					{
						if (stateOfSettings.Contains(text))
						{
							stateOfSettings[text] = num;
						}
						else
						{
							stateOfSettings.Add(text, num);
						}
						hTuple = null;
					}
				}
			}
			catch (HOperatorException ex)
			{
				gcNotification(ex.Message);
			}
		}

		public void setColorAttribute(string val)
		{
			if (graphicalSettings.ContainsKey("Colored"))
			{
				graphicalSettings.Remove("Colored");
			}
			addValue("Color", val);
		}

		public void setColoredAttribute(int val)
		{
			if (graphicalSettings.ContainsKey("Color"))
			{
				graphicalSettings.Remove("Color");
			}
			addValue("Colored", val);
		}

		public void setDrawModeAttribute(string val)
		{
			addValue("DrawMode", val);
		}

		public void setLineWidthAttribute(int val)
		{
			addValue("LineWidth", val);
		}

		public void setLutAttribute(string val)
		{
			addValue("Lut", val);
		}

		public void setPaintAttribute(string val)
		{
			addValue("Paint", val);
		}

		public void setShapeAttribute(string val)
		{
			addValue("Shape", val);
		}

		public void setLineStyleAttribute(HTuple val)
		{
			addValue("LineStyle", val);
		}

		private void addValue(string key, int val)
		{
			if (graphicalSettings.ContainsKey(key))
			{
				graphicalSettings[key] = val;
			}
			else
			{
				graphicalSettings.Add(key, val);
			}
		}

		private void addValue(string key, string val)
		{
			if (graphicalSettings.ContainsKey(key))
			{
				graphicalSettings[key] = val;
			}
			else
			{
				graphicalSettings.Add(key, val);
			}
		}

		private void addValue(string key, HTuple val)
		{
			if (graphicalSettings.ContainsKey(key))
			{
				graphicalSettings[key] = val;
			}
			else
			{
				graphicalSettings.Add(key, val);
			}
		}

		public void clear()
		{
			graphicalSettings.Clear();
		}

		public GraphicsContext copy()
		{
			return new GraphicsContext((Hashtable)graphicalSettings.Clone());
		}

		public object getGraphicsAttribute(string key)
		{
			if (graphicalSettings.ContainsKey(key))
			{
				return graphicalSettings[key];
			}
			return null;
		}

		public Hashtable copyContextList()
		{
			return (Hashtable)graphicalSettings.Clone();
		}

		public void dummy(string val)
		{
		}
	}

	public class HObjectEntry
	{
		public Hashtable gContext;

		public HObject HObj;

		public HObjectEntry(HObject obj, Hashtable gc)
		{
			gContext = gc;
			HObj = obj;
		}

		public void clear()
		{
			gContext.Clear();
			HObj.Dispose();
		}
	}

	public class HWndCtrl
	{
		public const int MODE_VIEW_NONE = 10;

		public const int MODE_VIEW_ZOOM = 11;

		public const int MODE_VIEW_MOVE = 12;

		public const int MODE_VIEW_ZOOMWINDOW = 13;

		public const int MODE_INCLUDE_ROI = 1;

		public const int MODE_EXCLUDE_ROI = 2;

		public const int EVENT_UPDATE_IMAGE = 31;

		public const int ERR_READING_IMG = 32;

		public const int ERR_DEFINING_GC = 33;

		private const int MAXNUMOBJLIST = 50;

		private int stateView;

		private bool mousePressed = false;

		private double startX;

		private double startY;

		private HWindowControl viewPort;

		private ROIController roiManager;

		private int dispROI;

		private int windowWidth;

		private int windowHeight;

		private int imageWidth;

		private int imageHeight;

		private int[] CompRangeX;

		private int[] CompRangeY;

		private int prevCompX;

		private int prevCompY;

		private double stepSizeX;

		private double stepSizeY;

		private double ImgRow1;

		private double ImgCol1;

		private double ImgRow2;

		private double ImgCol2;

		public string exceptionText = "";

		public Action addInfoDelegate;

		public Action<int> NotifyIconObserver;

		private HWindow ZoomWindow;

		private double zoomWndFactor;

		private double zoomAddOn;

		private int zoomWndSize;

		private ArrayList HObjList;

		private GraphicsContext mGC;

		public HWndCtrl(HWindowControl view)
		{
			viewPort = view;
			stateView = 10;
			windowWidth = viewPort.Size.Width;
			windowHeight = viewPort.Size.Height;
			zoomWndFactor = (double)imageWidth / (double)viewPort.Width;
			zoomAddOn = Math.Pow(0.9, 5.0);
			zoomWndSize = 150;
			CompRangeX = new int[2] { 0, 100 };
			CompRangeY = new int[2] { 0, 100 };
			prevCompX = (prevCompY = 0);
			dispROI = 1;
			viewPort.HMouseUp += mouseUp;
			viewPort.HMouseDown += mouseDown;
			viewPort.HMouseMove += mouseMoved;
			addInfoDelegate = dummyV;
			NotifyIconObserver = dummy;
			HObjList = new ArrayList(20);
			mGC = new GraphicsContext();
			mGC.gcNotification = exceptionGC;
		}

		public void useROIController(ROIController rC)
		{
			roiManager = rC;
			rC.setViewController(this);
		}

		private void setImagePart(HImage image)
		{
			image.GetImagePointer1(out string _, out int width, out int height);
			setImagePart(0, 0, height, width);
		}

		private void setImagePart(int r1, int c1, int r2, int c2)
		{
			ImgRow1 = r1;
			ImgCol1 = c1;
			ImgRow2 = (imageHeight = r2);
			ImgCol2 = (imageWidth = c2);
			Rectangle imagePart = viewPort.ImagePart;
			imagePart.X = (int)ImgCol1;
			imagePart.Y = (int)ImgRow1;
			imagePart.Height = imageHeight;
			imagePart.Width = imageWidth;
			viewPort.ImagePart = imagePart;
		}

		public void setViewState(int mode)
		{
			stateView = mode;
			if (roiManager != null)
			{
				roiManager.resetROI();
			}
		}

		private void dummy(int val)
		{
		}

		private void dummyV()
		{
		}

		private void exceptionGC(string message)
		{
			exceptionText = message;
			NotifyIconObserver(33);
		}

		public void setDispLevel(int mode)
		{
			dispROI = mode;
		}

		private void zoomImage(double x, double y, double scale)
		{
			double num = (x - ImgCol1) / (ImgCol2 - ImgCol1);
			double num2 = (y - ImgRow1) / (ImgRow2 - ImgRow1);
			double num3 = (ImgCol2 - ImgCol1) * scale;
			double num4 = (ImgRow2 - ImgRow1) * scale;
			ImgCol1 = x - num3 * num;
			ImgCol2 = x + num3 * (1.0 - num);
			ImgRow1 = y - num4 * num2;
			ImgRow2 = y + num4 * (1.0 - num2);
			int num5 = (int)Math.Round(num3);
			int num6 = (int)Math.Round(num4);
			Rectangle imagePart = viewPort.ImagePart;
			imagePart.X = (int)Math.Round(ImgCol1);
			imagePart.Y = (int)Math.Round(ImgRow1);
			imagePart.Width = ((num5 <= 0) ? 1 : num5);
			imagePart.Height = ((num6 <= 0) ? 1 : num6);
			viewPort.ImagePart = imagePart;
			zoomWndFactor *= scale;
			repaint();
		}

		public void zoomImage(double scaleFactor)
		{
			if (ImgRow2 - ImgRow1 == scaleFactor * (double)imageHeight && ImgCol2 - ImgCol1 == scaleFactor * (double)imageWidth)
			{
				repaint();
				return;
			}
			ImgRow2 = ImgRow1 + (double)imageHeight;
			ImgCol2 = ImgCol1 + (double)imageWidth;
			double imgCol = ImgCol1;
			double imgRow = ImgRow1;
			zoomWndFactor = (double)imageWidth / (double)viewPort.Width;
			zoomImage(imgCol, imgRow, scaleFactor);
		}

		public void scaleWindow(double scale)
		{
			ImgRow1 = 0.0;
			ImgCol1 = 0.0;
			ImgRow2 = imageHeight;
			ImgCol2 = imageWidth;
			viewPort.Width = (int)(ImgCol2 * scale);
			viewPort.Height = (int)(ImgRow2 * scale);
			zoomWndFactor = (double)imageWidth / (double)viewPort.Width;
		}

		public void setZoomWndFactor()
		{
			zoomWndFactor = (double)imageWidth / (double)viewPort.Width;
		}

		public void setZoomWndFactor(double zoomF)
		{
			zoomWndFactor = zoomF;
		}

		private void moveImage(double motionX, double motionY)
		{
			ImgRow1 += 0.0 - motionY;
			ImgRow2 += 0.0 - motionY;
			ImgCol1 += 0.0 - motionX;
			ImgCol2 += 0.0 - motionX;
			Rectangle imagePart = viewPort.ImagePart;
			imagePart.X = (int)Math.Round(ImgCol1);
			imagePart.Y = (int)Math.Round(ImgRow1);
			viewPort.ImagePart = imagePart;
			repaint();
		}

		public void resetAll()
		{
			ImgRow1 = 0.0;
			ImgCol1 = 0.0;
			ImgRow2 = imageHeight;
			ImgCol2 = imageWidth;
			zoomWndFactor = (double)imageWidth / (double)viewPort.Width;
			Rectangle imagePart = viewPort.ImagePart;
			imagePart.X = (int)ImgCol1;
			imagePart.Y = (int)ImgRow1;
			imagePart.Width = imageWidth;
			imagePart.Height = imageHeight;
			viewPort.ImagePart = imagePart;
			if (roiManager != null)
			{
				roiManager.reset();
			}
		}

		public void resetWindow()
		{
			ImgRow1 = 0.0;
			ImgCol1 = 0.0;
			ImgRow2 = imageHeight;
			ImgCol2 = imageWidth;
			zoomWndFactor = (double)imageWidth / (double)viewPort.Width;
			Rectangle imagePart = viewPort.ImagePart;
			imagePart.X = (int)ImgCol1;
			imagePart.Y = (int)ImgRow1;
			imagePart.Width = imageWidth;
			imagePart.Height = imageHeight;
			viewPort.ImagePart = imagePart;
		}

		private void mouseDown(object sender, HMouseEventArgs e)
		{
			mousePressed = true;
			int num = -1;
			if (roiManager != null && dispROI == 1)
			{
				num = roiManager.mouseDownAction(e.X, e.Y);
			}
			if (num == -1)
			{
				switch (stateView)
				{
					case 12:
						startX = e.X;
						startY = e.Y;
						break;
					case 11:
						zoomImage(scale: (e.Button != MouseButtons.Left) ? 1.1111111111111112 : 0.9, x: e.X, y: e.Y);
						break;
					case 10:
						break;
					case 13:
						activateZoomWindow((int)e.X, (int)e.Y);
						break;
				}
			}
		}

		private void activateZoomWindow(int X, int Y)
		{
			if (ZoomWindow != null)
			{
				ZoomWindow.Dispose();
			}
			HOperatorSet.SetSystem("border_width", 10);
			ZoomWindow = new HWindow();
			double num = ((double)X - ImgCol1) / (ImgCol2 - ImgCol1) * (double)viewPort.Width;
			double num2 = ((double)Y - ImgRow1) / (ImgRow2 - ImgRow1) * (double)viewPort.Height;
			int num3 = (int)((double)(zoomWndSize / 2) * zoomWndFactor * zoomAddOn);
			ZoomWindow.OpenWindow((int)num2 - zoomWndSize / 2, (int)num - zoomWndSize / 2, zoomWndSize, zoomWndSize, viewPort.HalconID, "visible", "");
			ZoomWindow.SetPart(Y - num3, X - num3, Y + num3, X + num3);
			repaint(ZoomWindow);
			ZoomWindow.SetColor("black");
		}

		private void mouseUp(object sender, HMouseEventArgs e)
		{
			mousePressed = false;
			if (roiManager != null && roiManager.activeROIidx != -1 && dispROI == 1)
			{
				roiManager.NotifyRCObserver(50);
			}
			else if (stateView == 13)
			{
				ZoomWindow.Dispose();
			}
		}

		private void mouseMoved(object sender, HMouseEventArgs e)
		{
			if (!mousePressed)
			{
				return;
			}
			if (roiManager != null && roiManager.activeROIidx != -1 && dispROI == 1)
			{
				roiManager.mouseMoveAction(e.X, e.Y);
			}
			else if (stateView == 12)
			{
				double num = e.X - startX;
				double num2 = e.Y - startY;
				if ((int)num != 0 || (int)num2 != 0)
				{
					moveImage(num, num2);
					startX = e.X - num;
					startY = e.Y - num2;
				}
			}
			else if (stateView == 13)
			{
				HSystem.SetSystem("flush_graphic", "false");
				ZoomWindow.ClearWindow();
				double num3 = (e.X - ImgCol1) / (ImgCol2 - ImgCol1) * (double)viewPort.Width;
				double num4 = (e.Y - ImgRow1) / (ImgRow2 - ImgRow1) * (double)viewPort.Height;
				double num5 = (double)(zoomWndSize / 2) * zoomWndFactor * zoomAddOn;
				ZoomWindow.SetWindowExtents((int)num4 - zoomWndSize / 2, (int)num3 - zoomWndSize / 2, zoomWndSize, zoomWndSize);
				ZoomWindow.SetPart((int)(e.Y - num5), (int)(e.X - num5), (int)(e.Y + num5), (int)(e.X + num5));
				repaint(ZoomWindow);
				HSystem.SetSystem("flush_graphic", "true");
				ZoomWindow.DispLine(-100.0, -100.0, -100.0, -100.0);
			}
		}

		public void setGUICompRangeX(int[] xRange, int Init)
		{
			CompRangeX = xRange;
			int num = xRange[1] - xRange[0];
			prevCompX = Init;
			stepSizeX = (double)imageWidth / (double)num * (double)(imageWidth / windowWidth);
		}

		public void setGUICompRangeY(int[] yRange, int Init)
		{
			CompRangeY = yRange;
			int num = yRange[1] - yRange[0];
			prevCompY = Init;
			stepSizeY = (double)imageHeight / (double)num * (double)(imageHeight / windowHeight);
		}

		public void resetGUIInitValues(int xVal, int yVal)
		{
			prevCompX = xVal;
			prevCompY = yVal;
		}

		public void moveXByGUIHandle(int valX)
		{
			double num = (double)(valX - prevCompX) * stepSizeX;
			if (num != 0.0)
			{
				moveImage(num, 0.0);
				prevCompX = valX;
			}
		}

		public void moveYByGUIHandle(int valY)
		{
			double num = (double)(valY - prevCompY) * stepSizeY;
			if (num != 0.0)
			{
				moveImage(0.0, num);
				prevCompY = valY;
			}
		}

		public void zoomByGUIHandle(double valF)
		{
			double x = ImgCol1 + (ImgCol2 - ImgCol1) / 2.0;
			double y = ImgRow1 + (ImgRow2 - ImgRow1) / 2.0;
			double num = (ImgCol2 - ImgCol1) / (double)imageWidth;
			double scale = 1.0 / num * (100.0 / valF);
			zoomImage(x, y, scale);
		}

		public void repaint()
		{
			repaint(viewPort.HalconWindow);
		}

		public void repaint(HWindow window)
		{
			int count = HObjList.Count;
			HSystem.SetSystem("flush_graphic", "false");
			window.ClearWindow();
			mGC.stateOfSettings.Clear();
			for (int i = 0; i < count; i++)
			{
				HObjectEntry hObjectEntry = (HObjectEntry)HObjList[i];
				mGC.applyContext(window, hObjectEntry.gContext);
				window.DispObj(hObjectEntry.HObj);
			}
			addInfoDelegate();
			if (roiManager != null && dispROI == 1)
			{
				roiManager.paintData(window);
			}
			HSystem.SetSystem("flush_graphic", "true");
			window.SetColor("black");
			window.DispLine(-100.0, -100.0, -101.0, -101.0);
		}

		public void addIconicVar(HObject obj)
		{
			if (obj == null)
			{
				return;
			}
			if (obj is HImage)
			{
				double row;
				double column;
				int num = ((HImage)obj).GetDomain().AreaCenter(out row, out column);
				((HImage)obj).GetImagePointer1(out string _, out int width, out int height);
				if (num == width * height)
				{
					clearList();
					if (height != imageHeight || width != imageWidth)
					{
						imageHeight = height;
						imageWidth = width;
						zoomWndFactor = (double)imageWidth / (double)viewPort.Width;
						setImagePart(0, 0, height, width);
					}
				}
			}
			HObjectEntry value = new HObjectEntry(obj, mGC.copyContextList());
			HObjList.Add(value);
			if (HObjList.Count > 50)
			{
				HObjList.RemoveAt(1);
			}
		}

		public void clearList()
		{
			HObjList.Clear();
		}

		public int getListCount()
		{
			return HObjList.Count;
		}

		public void changeGraphicSettings(string mode, string val)
		{
			switch (mode)
			{
				case "Color":
					mGC.setColorAttribute(val);
					break;
				case "DrawMode":
					mGC.setDrawModeAttribute(val);
					break;
				case "Lut":
					mGC.setLutAttribute(val);
					break;
				case "Paint":
					mGC.setPaintAttribute(val);
					break;
				case "Shape":
					mGC.setShapeAttribute(val);
					break;
			}
		}

		public void changeGraphicSettings(string mode, int val)
		{
			switch (mode)
			{
				case "Colored":
					mGC.setColoredAttribute(val);
					break;
				case "LineWidth":
					mGC.setLineWidthAttribute(val);
					break;
			}
		}

		public void changeGraphicSettings(string mode, HTuple val)
		{
			if (mode != null && mode == "LineStyle")
			{
				mGC.setLineStyleAttribute(val);
			}
		}

		public void clearGraphicContext()
		{
			mGC.clear();
		}

		public Hashtable getGraphicContext()
		{
			return mGC.copyContextList();
		}
	}

	public class ROI
	{
		public const int POSITIVE_FLAG = 21;

		public const int NEGATIVE_FLAG = 22;

		public const int ROI_TYPE_LINE = 10;

		public const int ROI_TYPE_CIRCLE = 11;

		public const int ROI_TYPE_CIRCLEARC = 12;

		public const int ROI_TYPE_RECTANCLE1 = 13;

		public const int ROI_TYPE_RECTANGLE2 = 14;

		protected int NumHandles;

		protected int activeHandleIdx;

		protected int OperatorFlag;

		public HTuple flagLineStyle;

		protected HTuple posOperation = new HTuple();

		protected HTuple negOperation = new HTuple(2, 2);

		public virtual void createROI(double midX, double midY)
		{
		}

		public virtual void draw(HWindow window)
		{
		}

		public virtual double distToClosestHandle(double x, double y)
		{
			return 0.0;
		}

		public virtual void displayActive(HWindow window)
		{
		}

		public virtual void moveByHandle(double x, double y)
		{
		}

		public virtual HRegion getRegion()
		{
			return null;
		}

		public virtual double getDistanceFromStartPoint(double row, double col)
		{
			return 0.0;
		}

		public virtual HTuple getModelData()
		{
			return null;
		}

		public int getNumHandles()
		{
			return NumHandles;
		}

		public int getActHandleIdx()
		{
			return activeHandleIdx;
		}

		public int getOperatorFlag()
		{
			return OperatorFlag;
		}

		public void setOperatorFlag(int flag)
		{
			OperatorFlag = flag;
			switch (OperatorFlag)
			{
				case 21:
					flagLineStyle = posOperation;
					break;
				case 22:
					flagLineStyle = negOperation;
					break;
				default:
					flagLineStyle = posOperation;
					break;
			}
		}
	}

	public class ROICircle : ROI
	{
		private double radius;

		private double row1;

		private double col1;

		private double midR;

		private double midC;

		public ROICircle()
		{
			NumHandles = 2;
			activeHandleIdx = 1;
		}

		public override void createROI(double midX, double midY)
		{
			midR = midY;
			midC = midX;
			radius = 100.0;
			row1 = midR;
			col1 = midC + radius;
		}

		public override void draw(HWindow window)
		{
			window.DispCircle(midR, midC, radius);
			window.DispRectangle2(row1, col1, 0.0, 5.0, 5.0);
			window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
		}

		public override double distToClosestHandle(double x, double y)
		{
			double num = 10000.0;
			double[] array = new double[NumHandles];
			array[0] = HMisc.DistancePp(y, x, row1, col1);
			array[1] = HMisc.DistancePp(y, x, midR, midC);
			for (int i = 0; i < NumHandles; i++)
			{
				if (array[i] < num)
				{
					num = array[i];
					activeHandleIdx = i;
				}
			}
			return array[activeHandleIdx];
		}

		public override void displayActive(HWindow window)
		{
			switch (activeHandleIdx)
			{
				case 0:
					window.DispRectangle2(row1, col1, 0.0, 5.0, 5.0);
					break;
				case 1:
					window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
					break;
			}
		}

		public override HRegion getRegion()
		{
			HRegion hRegion = new HRegion();
			hRegion.GenCircle(midR, midC, radius);
			return hRegion;
		}

		public override double getDistanceFromStartPoint(double row, double col)
		{
			double rowA = midR;
			double columnA = midC + 1.0 * radius;
			double num = HMisc.AngleLl(midR, midC, rowA, columnA, midR, midC, row, col);
			if (num < 0.0)
			{
				num += Math.PI * 2.0;
			}
			return radius * num;
		}

		public override HTuple getModelData()
		{
			return new HTuple(midR, midC, radius);
		}

		public override void moveByHandle(double newX, double newY)
		{
			switch (activeHandleIdx)
			{
				case 0:
					{
						row1 = newY;
						col1 = newX;
						HOperatorSet.DistancePp(new HTuple(row1), new HTuple(col1), new HTuple(midR), new HTuple(midC), out var distance);
						radius = distance[0].D;
						break;
					}
				case 1:
					{
						double num = midR - newY;
						double num2 = midC - newX;
						midR = newY;
						midC = newX;
						row1 -= num;
						col1 -= num2;
						break;
					}
			}
		}
	}

	public class ROICircularArc : ROI
	{
		private double midR;

		private double midC;

		private double sizeR;

		private double sizeC;

		private double startR;

		private double startC;

		private double extentR;

		private double extentC;

		private double radius;

		private double startPhi;

		private double extentPhi;

		private HXLDCont contour;

		private HXLDCont arrowHandleXLD;

		private string circDir;

		private double TwoPI;

		private double PI;

		public ROICircularArc()
		{
			NumHandles = 4;
			activeHandleIdx = 0;
			contour = new HXLDCont();
			circDir = "";
			TwoPI = Math.PI * 2.0;
			PI = Math.PI;
			arrowHandleXLD = new HXLDCont();
			arrowHandleXLD.GenEmptyObj();
		}

		public override void createROI(double midX, double midY)
		{
			midR = midY;
			midC = midX;
			radius = 100.0;
			sizeR = midR;
			sizeC = midC - radius;
			startPhi = PI * 0.25;
			extentPhi = PI * 1.5;
			circDir = "positive";
			determineArcHandles();
			updateArrowHandle();
		}

		public override void draw(HWindow window)
		{
			contour.Dispose();
			contour.GenCircleContourXld(midR, midC, radius, startPhi, startPhi + extentPhi, circDir, 1.0);
			window.DispObj(contour);
			window.DispRectangle2(sizeR, sizeC, 0.0, 5.0, 5.0);
			window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
			window.DispRectangle2(startR, startC, startPhi, 10.0, 2.0);
			window.DispObj(arrowHandleXLD);
		}

		public override double distToClosestHandle(double x, double y)
		{
			double num = 10000.0;
			double[] array = new double[NumHandles];
			array[0] = HMisc.DistancePp(y, x, midR, midC);
			array[1] = HMisc.DistancePp(y, x, sizeR, sizeC);
			array[2] = HMisc.DistancePp(y, x, startR, startC);
			array[3] = HMisc.DistancePp(y, x, extentR, extentC);
			for (int i = 0; i < NumHandles; i++)
			{
				if (array[i] < num)
				{
					num = array[i];
					activeHandleIdx = i;
				}
			}
			return array[activeHandleIdx];
		}

		public override void displayActive(HWindow window)
		{
			switch (activeHandleIdx)
			{
				case 0:
					window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
					break;
				case 1:
					window.DispRectangle2(sizeR, sizeC, 0.0, 5.0, 5.0);
					break;
				case 2:
					window.DispRectangle2(startR, startC, startPhi, 10.0, 2.0);
					break;
				case 3:
					window.DispObj(arrowHandleXLD);
					break;
			}
		}

		public override void moveByHandle(double newX, double newY)
		{
			switch (activeHandleIdx)
			{
				case 0:
					{
						double num = midR - newY;
						double x = midC - newX;
						midR = newY;
						midC = newX;
						sizeR -= num;
						sizeC -= x;
						determineArcHandles();
						break;
					}
				case 1:
					{
						sizeR = newY;
						sizeC = newX;
						HOperatorSet.DistancePp(new HTuple(sizeR), new HTuple(sizeC), new HTuple(midR), new HTuple(midC), out var distance);
						radius = distance[0].D;
						determineArcHandles();
						break;
					}
				case 2:
					{
						double num = newY - midR;
						double x = newX - midC;
						startPhi = Math.Atan2(0.0 - num, x);
						if (startPhi < 0.0)
						{
							startPhi = PI + (startPhi + PI);
						}
						setStartHandle();
						double value = extentPhi;
						extentPhi = HMisc.AngleLl(midR, midC, startR, startC, midR, midC, extentR, extentC);
						if (extentPhi < 0.0 && value > PI * 0.8)
						{
							extentPhi = PI + extentPhi + PI;
						}
						else if (extentPhi > 0.0 && value < (0.0 - PI) * 0.7)
						{
							extentPhi = 0.0 - PI - (PI - extentPhi);
						}
						break;
					}
				case 3:
					{
						double num = newY - midR;
						double x = newX - midC;
						double value = extentPhi;
						double num2 = Math.Atan2(0.0 - num, x);
						if (num2 < 0.0)
						{
							num2 = PI + (num2 + PI);
						}
						if (circDir == "positive" && startPhi >= num2)
						{
							extentPhi = num2 + TwoPI - startPhi;
						}
						else if (circDir == "positive" && num2 > startPhi)
						{
							extentPhi = num2 - startPhi;
						}
						else if (circDir == "negative" && startPhi >= num2)
						{
							extentPhi = -1.0 * (startPhi - num2);
						}
						else if (circDir == "negative" && num2 > startPhi)
						{
							extentPhi = -1.0 * (startPhi + TwoPI - num2);
						}
						double num3 = Math.Max(Math.Abs(value), Math.Abs(extentPhi));
						double num4 = Math.Min(Math.Abs(value), Math.Abs(extentPhi));
						if (num3 - num4 >= PI)
						{
							extentPhi = ((circDir == "positive") ? (-1.0 * num4) : num4);
						}
						setExtentHandle();
						break;
					}
			}
			circDir = ((extentPhi < 0.0) ? "negative" : "positive");
			updateArrowHandle();
		}

		public override HRegion getRegion()
		{
			contour.Dispose();
			contour.GenCircleContourXld(midR, midC, radius, startPhi, startPhi + extentPhi, circDir, 1.0);
			return new HRegion(contour);
		}

		public override HTuple getModelData()
		{
			return new HTuple(midR, midC, radius, startPhi, extentPhi);
		}

		private void determineArcHandles()
		{
			setStartHandle();
			setExtentHandle();
		}

		private void setStartHandle()
		{
			startR = midR - radius * Math.Sin(startPhi);
			startC = midC + radius * Math.Cos(startPhi);
		}

		private void setExtentHandle()
		{
			extentR = midR - radius * Math.Sin(startPhi + extentPhi);
			extentC = midC + radius * Math.Cos(startPhi + extentPhi);
		}

		private void updateArrowHandle()
		{
			double num = 15.0;
			double num2 = 15.0;
			arrowHandleXLD.Dispose();
			arrowHandleXLD.GenEmptyObj();
			double num3 = extentR;
			double num4 = extentC;
			double num5 = startPhi + extentPhi + Math.PI / 2.0;
			double num6 = ((circDir == "negative") ? (-1.0) : 1.0);
			double num7 = num3 + num6 * Math.Sin(num5) * 20.0;
			double num8 = num4 - num6 * Math.Cos(num5) * 20.0;
			double num9 = HMisc.DistancePp(num7, num8, num3, num4);
			if (num9 == 0.0)
			{
				num9 = -1.0;
			}
			double num10 = (num3 - num7) / num9;
			double num11 = (num4 - num8) / num9;
			double num12 = num2 / 2.0;
			double num13 = num7 + (num9 - num) * num10 + num12 * num11;
			double num14 = num7 + (num9 - num) * num10 - num12 * num11;
			double num15 = num8 + (num9 - num) * num11 - num12 * num10;
			double num16 = num8 + (num9 - num) * num11 + num12 * num10;
			if (num9 == -1.0)
			{
				arrowHandleXLD.GenContourPolygonXld(num7, num8);
				return;
			}
			arrowHandleXLD.GenContourPolygonXld(new HTuple(num7, num3, num13, num3, num14, num3), new HTuple(num8, num4, num15, num4, num16, num4));
		}
	}

	public class ROIController
	{
		public const int MODE_ROI_POS = 21;

		public const int MODE_ROI_NEG = 22;

		public const int MODE_ROI_NONE = 23;

		public const int EVENT_UPDATE_ROI = 50;

		public const int EVENT_CHANGED_ROI_SIGN = 51;

		public const int EVENT_MOVING_ROI = 52;

		public const int EVENT_DELETED_ACTROI = 53;

		public const int EVENT_DELETED_ALL_ROIS = 54;

		public const int EVENT_ACTIVATED_ROI = 55;

		public const int EVENT_CREATED_ROI = 56;

		private ROI roiMode;

		private int stateROI;

		private double currX;

		private double currY;

		public int activeROIidx;

		public int deletedIdx;

		public ArrayList ROIList;

		public HRegion ModelROI;

		private string activeCol = "green";

		private string activeHdlCol = "red";

		private string inactiveCol = "yellow";

		public HWndCtrl viewController;

		public Action<int> NotifyRCObserver;

		public ROIController()
		{
			stateROI = 23;
			ROIList = new ArrayList();
			activeROIidx = -1;
			ModelROI = new HRegion();
			NotifyRCObserver = dummyI;
			deletedIdx = -1;
			currX = (currY = -1.0);
		}

		public void setViewController(HWndCtrl view)
		{
			viewController = view;
		}

		public HRegion getModelRegion()
		{
			return ModelROI;
		}

		public ArrayList getROIList()
		{
			return ROIList;
		}

		public ROI getActiveROI()
		{
			if (activeROIidx != -1)
			{
				return (ROI)ROIList[activeROIidx];
			}
			return null;
		}

		public int getActiveROIIdx()
		{
			return activeROIidx;
		}

		public void setActiveROIIdx(int active)
		{
			activeROIidx = active;
		}

		public int getDelROIIdx()
		{
			return deletedIdx;
		}

		public void setROIShape(ROI r)
		{
			roiMode = r;
			roiMode.setOperatorFlag(stateROI);
		}

		public void setROISign(int mode)
		{
			stateROI = mode;
			if (activeROIidx != -1)
			{
				((ROI)ROIList[activeROIidx]).setOperatorFlag(stateROI);
				viewController.repaint();
				NotifyRCObserver(51);
			}
		}

		public void removeActive()
		{
			if (activeROIidx != -1)
			{
				ROIList.RemoveAt(activeROIidx);
				deletedIdx = activeROIidx;
				activeROIidx = -1;
				viewController.repaint();
				NotifyRCObserver(53);
			}
		}

		public bool defineModelROI()
		{
			if (stateROI == 23)
			{
				return true;
			}
			HRegion hRegion = new HRegion();
			HRegion hRegion2 = new HRegion();
			hRegion.GenEmptyRegion();
			hRegion2.GenEmptyRegion();
			for (int i = 0; i < ROIList.Count; i++)
			{
				switch (((ROI)ROIList[i]).getOperatorFlag())
				{
					case 21:
						{
							HRegion region = ((ROI)ROIList[i]).getRegion();
							hRegion = region.Union2(hRegion);
							break;
						}
					case 22:
						{
							HRegion region = ((ROI)ROIList[i]).getRegion();
							hRegion2 = region.Union2(hRegion2);
							break;
						}
				}
			}
			ModelROI = null;
			if (hRegion.AreaCenter(out double row, out double column) > 0)
			{
				HRegion region = hRegion.Difference(hRegion2);
				if (region.AreaCenter(out row, out column) > 0)
				{
					ModelROI = region;
				}
			}
			if (ModelROI == null || ROIList.Count == 0)
			{
				return false;
			}
			return true;
		}

		public void reset()
		{
			ROIList.Clear();
			activeROIidx = -1;
			ModelROI = null;
			roiMode = null;
			NotifyRCObserver(54);
		}

		public void resetROI()
		{
			activeROIidx = -1;
			roiMode = null;
		}

		public void setDrawColor(string aColor, string aHdlColor, string inaColor)
		{
			if (aColor != "")
			{
				activeCol = aColor;
			}
			if (aHdlColor != "")
			{
				activeHdlCol = aHdlColor;
			}
			if (inaColor != "")
			{
				inactiveCol = inaColor;
			}
		}

		public void paintData(HWindow window)
		{
			window.SetDraw("margin");
			window.SetLineWidth(1);
			if (ROIList.Count > 0)
			{
				window.SetColor(inactiveCol);
				window.SetDraw("margin");
				for (int i = 0; i < ROIList.Count; i++)
				{
					window.SetLineStyle(((ROI)ROIList[i]).flagLineStyle);
					((ROI)ROIList[i]).draw(window);
				}
				if (activeROIidx != -1)
				{
					window.SetColor(activeCol);
					window.SetLineStyle(((ROI)ROIList[activeROIidx]).flagLineStyle);
					((ROI)ROIList[activeROIidx]).draw(window);
					window.SetColor(activeHdlCol);
					((ROI)ROIList[activeROIidx]).displayActive(window);
				}
			}
		}

		public int mouseDownAction(double imgX, double imgY)
		{
			int num = -1;
			double num2 = 10000.0;
			double num3 = 0.0;
			double num4 = 35.0;
			if (roiMode != null)
			{
				roiMode.createROI(imgX, imgY);
				ROIList.Add(roiMode);
				roiMode = null;
				activeROIidx = ROIList.Count - 1;
				viewController.repaint();
				NotifyRCObserver(56);
			}
			else if (ROIList.Count > 0)
			{
				activeROIidx = -1;
				for (int i = 0; i < ROIList.Count; i++)
				{
					num3 = ((ROI)ROIList[i]).distToClosestHandle(imgX, imgY);
					if (num3 < num2 && num3 < num4)
					{
						num2 = num3;
						num = i;
					}
				}
				if (num >= 0)
				{
					activeROIidx = num;
					NotifyRCObserver(55);
				}
				viewController.repaint();
			}
			return activeROIidx;
		}

		public void mouseMoveAction(double newX, double newY)
		{
			if (newX != currX || newY != currY)
			{
				((ROI)ROIList[activeROIidx]).moveByHandle(newX, newY);
				viewController.repaint();
				currX = newX;
				currY = newY;
				NotifyRCObserver(52);
			}
		}

		public void dummyI(int v)
		{
		}
	}

	public class ROILine : ROI
	{
		private double row1;

		private double col1;

		private double row2;

		private double col2;

		private double midR;

		private double midC;

		private HXLDCont arrowHandleXLD;

		public ROILine()
		{
			NumHandles = 3;
			activeHandleIdx = 2;
			arrowHandleXLD = new HXLDCont();
			arrowHandleXLD.GenEmptyObj();
		}

		public override void createROI(double midX, double midY)
		{
			midR = midY;
			midC = midX;
			row1 = midR;
			col1 = midC - 50.0;
			row2 = midR;
			col2 = midC + 50.0;
			updateArrowHandle();
		}

		public override void draw(HWindow window)
		{
			window.DispLine(row1, col1, row2, col2);
			window.DispRectangle2(row1, col1, 0.0, 5.0, 5.0);
			window.DispObj(arrowHandleXLD);
			window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
		}

		public override double distToClosestHandle(double x, double y)
		{
			double num = 10000.0;
			double[] array = new double[NumHandles];
			array[0] = HMisc.DistancePp(y, x, row1, col1);
			array[1] = HMisc.DistancePp(y, x, row2, col2);
			array[2] = HMisc.DistancePp(y, x, midR, midC);
			for (int i = 0; i < NumHandles; i++)
			{
				if (array[i] < num)
				{
					num = array[i];
					activeHandleIdx = i;
				}
			}
			return array[activeHandleIdx];
		}

		public override void displayActive(HWindow window)
		{
			switch (activeHandleIdx)
			{
				case 0:
					window.DispRectangle2(row1, col1, 0.0, 5.0, 5.0);
					break;
				case 1:
					window.DispObj(arrowHandleXLD);
					break;
				case 2:
					window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
					break;
			}
		}

		public override HRegion getRegion()
		{
			HRegion hRegion = new HRegion();
			hRegion.GenRegionLine(row1, col1, row2, col2);
			return hRegion;
		}

		public override double getDistanceFromStartPoint(double row, double col)
		{
			return HMisc.DistancePp(row, col, row1, col1);
		}

		public override HTuple getModelData()
		{
			return new HTuple(row1, col1, row2, col2);
		}

		public override void moveByHandle(double newX, double newY)
		{
			switch (activeHandleIdx)
			{
				case 0:
					row1 = newY;
					col1 = newX;
					midR = (row1 + row2) / 2.0;
					midC = (col1 + col2) / 2.0;
					break;
				case 1:
					row2 = newY;
					col2 = newX;
					midR = (row1 + row2) / 2.0;
					midC = (col1 + col2) / 2.0;
					break;
				case 2:
					{
						double num = row1 - midR;
						double num2 = col1 - midC;
						midR = newY;
						midC = newX;
						row1 = midR + num;
						col1 = midC + num2;
						row2 = midR - num;
						col2 = midC - num2;
						break;
					}
			}
			updateArrowHandle();
		}

		private void updateArrowHandle()
		{
			double num = 15.0;
			double num2 = 15.0;
			arrowHandleXLD.Dispose();
			arrowHandleXLD.GenEmptyObj();
			double num3 = row1 + (row2 - row1) * 0.8;
			double num4 = col1 + (col2 - col1) * 0.8;
			double num5 = HMisc.DistancePp(num3, num4, row2, col2);
			if (num5 == 0.0)
			{
				num5 = -1.0;
			}
			double num6 = (row2 - num3) / num5;
			double num7 = (col2 - num4) / num5;
			double num8 = num2 / 2.0;
			double num9 = num3 + (num5 - num) * num6 + num8 * num7;
			double num10 = num3 + (num5 - num) * num6 - num8 * num7;
			double num11 = num4 + (num5 - num) * num7 - num8 * num6;
			double num12 = num4 + (num5 - num) * num7 + num8 * num6;
			if (num5 == -1.0)
			{
				arrowHandleXLD.GenContourPolygonXld(num3, num4);
				return;
			}
			arrowHandleXLD.GenContourPolygonXld(new HTuple(num3, row2, num9, row2, num10, row2), new HTuple(num4, col2, num11, col2, num12, col2));
		}
	}

	public class ROIRectangle1 : ROI
	{
		private double row1;

		private double col1;

		private double row2;

		private double col2;

		private double midR;

		private double midC;

		public ROIRectangle1()
		{
			NumHandles = 5;
			activeHandleIdx = 4;
		}

		public override void createROI(double midX, double midY)
		{
			midR = midY;
			midC = midX;
			row1 = midR - 50.0;
			col1 = midC - 50.0;
			row2 = midR + 50.0;
			col2 = midC + 50.0;
		}

		public override void draw(HWindow window)
		{
			window.DispRectangle1(row1, col1, row2, col2);
			window.DispRectangle2(row1, col1, 0.0, 5.0, 5.0);
			window.DispRectangle2(row1, col2, 0.0, 5.0, 5.0);
			window.DispRectangle2(row2, col2, 0.0, 5.0, 5.0);
			window.DispRectangle2(row2, col1, 0.0, 5.0, 5.0);
			window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
		}

		public override double distToClosestHandle(double x, double y)
		{
			double num = 10000.0;
			double[] array = new double[NumHandles];
			midR = (row2 - row1) / 2.0 + row1;
			midC = (col2 - col1) / 2.0 + col1;
			array[0] = HMisc.DistancePp(y, x, row1, col1);
			array[1] = HMisc.DistancePp(y, x, row1, col2);
			array[2] = HMisc.DistancePp(y, x, row2, col2);
			array[3] = HMisc.DistancePp(y, x, row2, col1);
			array[4] = HMisc.DistancePp(y, x, midR, midC);
			for (int i = 0; i < NumHandles; i++)
			{
				if (array[i] < num)
				{
					num = array[i];
					activeHandleIdx = i;
				}
			}
			return array[activeHandleIdx];
		}

		public override void displayActive(HWindow window)
		{
			switch (activeHandleIdx)
			{
				case 0:
					window.DispRectangle2(row1, col1, 0.0, 5.0, 5.0);
					break;
				case 1:
					window.DispRectangle2(row1, col2, 0.0, 5.0, 5.0);
					break;
				case 2:
					window.DispRectangle2(row2, col2, 0.0, 5.0, 5.0);
					break;
				case 3:
					window.DispRectangle2(row2, col1, 0.0, 5.0, 5.0);
					break;
				case 4:
					window.DispRectangle2(midR, midC, 0.0, 5.0, 5.0);
					break;
			}
		}

		public override HRegion getRegion()
		{
			HRegion hRegion = new HRegion();
			hRegion.GenRectangle1(row1, col1, row2, col2);
			return hRegion;
		}

		public override HTuple getModelData()
		{
			return new HTuple(row1, col1, row2, col2);
		}

		public override void moveByHandle(double newX, double newY)
		{
			switch (activeHandleIdx)
			{
				case 0:
					row1 = newY;
					col1 = newX;
					break;
				case 1:
					row1 = newY;
					col2 = newX;
					break;
				case 2:
					row2 = newY;
					col2 = newX;
					break;
				case 3:
					row2 = newY;
					col1 = newX;
					break;
				case 4:
					{
						double num = (row2 - row1) / 2.0;
						double num2 = (col2 - col1) / 2.0;
						row1 = newY - num;
						row2 = newY + num;
						col1 = newX - num2;
						col2 = newX + num2;
						break;
					}
			}
			if (row2 <= row1)
			{
				double num3 = row1;
				row1 = row2;
				row2 = num3;
			}
			if (col2 <= col1)
			{
				double num3 = col1;
				col1 = col2;
				col2 = num3;
			}
			midR = (row2 - row1) / 2.0 + row1;
			midC = (col2 - col1) / 2.0 + col1;
		}
	}

	public class ROIRectangle2 : ROI
	{
		private double length1;

		private double length2;

		private double midR;

		private double midC;

		private double phi;

		private HTuple rowsInit;

		private HTuple colsInit;

		private HTuple rows;

		private HTuple cols;

		private HHomMat2D hom2D;

		private HHomMat2D tmp;

		public ROIRectangle2()
		{
			NumHandles = 6;
			activeHandleIdx = 4;
		}

		public override void createROI(double midX, double midY)
		{
			midR = midY;
			midC = midX;
			length1 = 100.0;
			length2 = 50.0;
			phi = 0.0;
			rowsInit = new HTuple(-1.0, -1.0, 1.0, 1.0, 0.0, 0.0);
			colsInit = new HTuple(-1.0, 1.0, 1.0, -1.0, 0.0, 0.6);
			hom2D = new HHomMat2D();
			tmp = new HHomMat2D();
			updateHandlePos();
		}

		public override void draw(HWindow window)
		{
			window.DispRectangle2(midR, midC, 0.0 - phi, length1, length2);
			for (int i = 0; i < NumHandles; i++)
			{
				window.DispRectangle2(rows[i].D, cols[i].D, 0.0 - phi, 5.0, 5.0);
			}
			window.DispArrow(midR, midC, midR + Math.Sin(phi) * length1 * 1.2, midC + Math.Cos(phi) * length1 * 1.2, 2.0);
		}

		public override double distToClosestHandle(double x, double y)
		{
			double num = 10000.0;
			double[] array = new double[NumHandles];
			for (int i = 0; i < NumHandles; i++)
			{
				array[i] = HMisc.DistancePp(y, x, rows[i].D, cols[i].D);
			}
			for (int i = 0; i < NumHandles; i++)
			{
				if (array[i] < num)
				{
					num = array[i];
					activeHandleIdx = i;
				}
			}
			return array[activeHandleIdx];
		}

		public override void displayActive(HWindow window)
		{
			window.DispRectangle2(rows[activeHandleIdx].D, cols[activeHandleIdx].D, 0.0 - phi, 5.0, 5.0);
			if (activeHandleIdx == 5)
			{
				window.DispArrow(midR, midC, midR + Math.Sin(phi) * length1 * 1.2, midC + Math.Cos(phi) * length1 * 1.2, 2.0);
			}
		}

		public override HRegion getRegion()
		{
			HRegion hRegion = new HRegion();
			hRegion.GenRectangle2(midR, midC, 0.0 - phi, length1, length2);
			return hRegion;
		}

		public override HTuple getModelData()
		{
			return new HTuple(midR, midC, phi, length1, length2);
		}

		public override void moveByHandle(double newX, double newY)
		{
			double num = 0.0;
			double qy = 0.0;
			switch (activeHandleIdx)
			{
				case 0:
				case 1:
				case 2:
				case 3:
					tmp = hom2D.HomMat2dInvert();
					num = tmp.AffineTransPoint2d(newX, newY, out qy);
					length2 = Math.Abs(qy);
					length1 = Math.Abs(num);
					checkForRange(num, qy);
					break;
				case 4:
					midC = newX;
					midR = newY;
					break;
				case 5:
					{
						double y = newY - rows[4].D;
						double x = newX - cols[4].D;
						phi = Math.Atan2(y, x);
						break;
					}
			}
			updateHandlePos();
		}

		private void updateHandlePos()
		{
			hom2D.HomMat2dIdentity();
			hom2D = hom2D.HomMat2dTranslate(midC, midR);
			hom2D = hom2D.HomMat2dRotateLocal(phi);
			tmp = hom2D.HomMat2dScaleLocal(length1, length2);
			cols = tmp.AffineTransPoint2d(colsInit, rowsInit, out rows);
		}

		private void checkForRange(double x, double y)
		{
			switch (activeHandleIdx)
			{
				case 0:
					if (!(x < 0.0) || !(y < 0.0))
					{
						if (x >= 0.0)
						{
							length1 = 0.01;
						}
						if (y >= 0.0)
						{
							length2 = 0.01;
						}
					}
					break;
				case 1:
					if (!(x > 0.0) || !(y < 0.0))
					{
						if (x <= 0.0)
						{
							length1 = 0.01;
						}
						if (y >= 0.0)
						{
							length2 = 0.01;
						}
					}
					break;
				case 2:
					if (!(x > 0.0) || !(y > 0.0))
					{
						if (x <= 0.0)
						{
							length1 = 0.01;
						}
						if (y <= 0.0)
						{
							length2 = 0.01;
						}
					}
					break;
				case 3:
					if (!(x < 0.0) || !(y > 0.0))
					{
						if (x >= 0.0)
						{
							length1 = 0.01;
						}
						if (y <= 0.0)
						{
							length2 = 0.01;
						}
					}
					break;
			}
		}
	}

}

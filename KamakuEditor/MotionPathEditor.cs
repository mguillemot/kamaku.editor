using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;

namespace Kamaku
{
    public partial class MotionPathEditor : Form
    {
        private const int N = 100;
        private const double DefaultSpeed = 5d;
        private const double MaxSpeed = 10d;

        private Object _mutex = new object();
        private Surface _frame, _graph;
        private List<Point> _nodes = new List<Point>();
        private List<Point> _path = new List<Point>();
        private List<double[]> _segmentDistance = new List<double[]>();
        private int _currentSegment = 0;
        private Point _currentPosition = new Point();
        private PointF _currentSpeed = new PointF();
        private double _currentDistanceFromSegmentStart = 0;
        private Point _cursor;
        private bool _hasHover = false;
        private Point _hover;
        private List<Point> _selectedNodes = new List<Point>();
        private List<double> _targetSpeed = new List<double>();
        private int _hoverSpeed = -1;
        private Point _speedHover;

        public MotionPathEditor()
        {
            InitializeComponent();
        }

        private void MotionPathEditor_Load(object sender, EventArgs e)
        {
            _frame = new Surface(surfaceControl1.Width, surfaceControl1.Height);
            _graph = new Surface(surfaceControlSpeedGraph.Width, surfaceControlSpeedGraph.Height);

            SdlDotNet.Core.Events.Fps = Settings.Fps;
            SdlDotNet.Core.Events.Tick += new EventHandler<TickEventArgs>(Events_Tick);
            Thread thread = new Thread(new ThreadStart(SdlDotNet.Core.Events.Run));
            thread.IsBackground = true;
            thread.Name = "SDL";
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
        }

        private void Events_Tick(object sender, TickEventArgs e)
        {
            // Draw motion frame
            _frame.Fill(Color.Black);
            Box screen = new Box(50, 50, 240+50, 320+50);
            _frame.Draw(screen, Color.DarkRed);
            lock (_mutex)
            {
                foreach (Point p in _nodes)
                {
                    bool selected = _selectedNodes.Contains(p);
                    p.Offset(50, 50);
                    Line l1 = new Line(Convert.ToInt16(p.X - 2), Convert.ToInt16(p.Y), Convert.ToInt16(p.X + 2), Convert.ToInt16(p.Y));
                    Line l2 = new Line(Convert.ToInt16(p.X), Convert.ToInt16(p.Y - 2), Convert.ToInt16(p.X), Convert.ToInt16(p.Y + 2));
                    _frame.Draw(l1, Color.Red);
                    _frame.Draw(l2, Color.Red);
                    if (selected)
                    {
                        Circle c = new Circle(p, 4);
                        _frame.Draw(c, Color.Red);
                    }
                }
                if (_hasHover)
                {
                    Point hover = _hover;
                    hover.Offset(50, 50);
                    Circle sel = new Circle(hover, 5);
                    _frame.Draw(sel, Color.Pink);
                }
                foreach (Point p in _path)
                {
                    p.Offset(50, 50);
                    _frame.Draw(p, Color.Green);
                }
                Point cp = _currentPosition;
                cp.Offset(50, 50);
                Line ll1 =
                    new Line(Convert.ToInt16(cp.X - 2), Convert.ToInt16(cp.Y), Convert.ToInt16(cp.X + 2),
                             Convert.ToInt16(cp.Y));
                Line ll2 =
                    new Line(Convert.ToInt16(cp.X), Convert.ToInt16(cp.Y - 2), Convert.ToInt16(cp.X),
                             Convert.ToInt16(cp.Y + 2));
                _frame.Draw(ll1, Color.Blue);
                _frame.Draw(ll2, Color.Blue);
                Point cps = _currentPosition;
                cps.Offset(50, 50);
                cps.Offset((int)(_currentSpeed.X * 0.5), (int)(_currentSpeed.Y * 0.5));
                Line speedVec = new Line(cp, cps);
                _frame.Draw(speedVec, Color.Yellow);
                if (_segmentDistance.Count > 0 && _currentSegment < _segmentDistance.Count)
                {
                    double speed0 = _targetSpeed[_currentSegment + 1];
                    double speed1 = _targetSpeed[_currentSegment + 2];
                    double dist = _currentDistanceFromSegmentStart;
                    double totalDist = _segmentDistance[_currentSegment][N - 1];
                    double frameSpeed = speed0 + dist/totalDist*(speed1 - speed0);
                    Advance(frameSpeed);
                }
            }
            surfaceControl1.Blit(_frame);

            // Draw speed graph
            _graph.Fill(Color.Black);
            lock (_mutex)
            {
                if (_segmentDistance.Count >= 1)
                {
                    double closerPointDistance;
                    short[] x = new short[_segmentDistance.Count + 1];
                    double[] distances = new double[_segmentDistance.Count];
                    double totalDistance = 0;
                    for (int i = 0; i < _segmentDistance.Count; i++)
                    {
                        distances[i] = _segmentDistance[i][N - 1];
                        totalDistance += distances[i];
                    }
                    double d = 0;
                    {
                        double val = _targetSpeed[1];
                        Line l1 = new Line(-2, SpeedToElevation(val), 2, SpeedToElevation(val));
                        Line l2 = new Line(0, (short)(SpeedToElevation(val) - 2), 0, (short)(SpeedToElevation(val) - 2));
                        _graph.Draw(l1, Color.Red);
                        _graph.Draw(l2, Color.Red);
                        closerPointDistance = _speedHover.X * _speedHover.X;
                        _hoverSpeed = 0;
                    }
                    _graph.Draw(new Point(0, 5), Color.Red);
                    for (int i = 0; i < _segmentDistance.Count; i++)
                    {
                        d += (_graph.Width - 1) * (distances[i] / totalDistance);
                        double val = _targetSpeed[i + 2];
                        x[i + 1] = (short) d;
                        Line l1 = new Line((short)(d - 2), SpeedToElevation(val), (short)(d + 2), SpeedToElevation(val));
                        Line l2 = new Line((short)d, (short)(SpeedToElevation(val) - 2), (short)d, (short)(SpeedToElevation(val) + 2));
                        _graph.Draw(l1, Color.Red);
                        _graph.Draw(l2, Color.Red);
                        double toCursor = (_speedHover.X - d) * (_speedHover.X - d);
                        if (toCursor < closerPointDistance)
                        {
                            closerPointDistance = toCursor;
                            _hoverSpeed = i + 1;
                        }
                    }
                    for (int i = 1; i < x.Length; i++)
                    {
                        _graph.Draw(new Line(x[i - 1], SpeedToElevation(_targetSpeed[i]), x[i], SpeedToElevation(_targetSpeed[i + 1])), Color.DeepPink);
                    }
                    Line hoverLine = new Line(x[_hoverSpeed], 0, x[_hoverSpeed], (short)_graph.Height);
                    _graph.Draw(hoverLine, Color.DarkGoldenrod);
                }
            }
            surfaceControlSpeedGraph.Blit(_graph);

            // Redraw interface
            if (InvokeRequired)
            {
                //Invoke(new RefreshDelegate(RefreshValues));
            }
        }

        private short SpeedToElevation(double s)
        {
            double elev = (1 - s / MaxSpeed) * _graph.Height;
            return (short)elev;
        }

        private void RefreshValues()
        {
            listBoxNodes.Items.Clear();
            int i = 0;
            foreach (Point p in _nodes)
            {
                listBoxNodes.Items.Add(string.Format("#{0} ({1} - {2})", i, p.X, p.Y));
                i++;
            }
        }

        private static double Distance(PointF a, PointF b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static PointF Spline(Point p0, Point p1, Point p2, Point p3, double t)
        {
            double x = 0.5 *
                       ((2 * p1.X) + (p2.X - p0.X) * t + (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t * t +
                        (3 * p1.X + p3.X - p0.X - 3 * p2.X) * t * t * t);
            double y = 0.5 *
                       ((2 * p1.Y) + (p2.Y - p0.Y) * t + (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t * t +
                        (3 * p1.Y + p3.Y - p0.Y - 3 * p2.Y) * t * t * t);
            return new PointF((float)x, (float)y);
        }

        private static PointF SplineSpeed(Point p0, Point p1, Point p2, Point p3, double t)
        {
            double x = 0.5 *
                       (p2.X - p0.X + 2 * t * (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) +
                        3 * t * t * (3 * p1.X + p3.X - p0.X - 3 * p2.X));
            double y = 0.5 *
                       (p2.Y - p0.Y + 2 * t * (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) +
                        3 * t * t * (3 * p1.Y + p3.Y - p0.Y - 3 * p2.Y));
            return new PointF((float)x, (float)y);
        }

        private bool Advance(double d)
        {
            lock (_mutex)
            {
                if (_currentSegment >= _segmentDistance.Count)
                {
                    return false;
                }
                double[] currentSegmentDistances = _segmentDistance[_currentSegment];
                double searched = _currentDistanceFromSegmentStart + d;
                double currentSegmentLength = currentSegmentDistances[N - 1];
                while (searched > currentSegmentLength)
                {
                    _currentDistanceFromSegmentStart = searched - currentSegmentLength;
                    searched -= currentSegmentLength;
                    _currentSegment++;
                    if (_currentSegment == _segmentDistance.Count)
                    {
                        return false; // Out of the path
                    }
                    currentSegmentDistances = _segmentDistance[_currentSegment];
                    currentSegmentLength = currentSegmentDistances[N - 1];
                }
                int i;
                for (i = 0; i < N; i++)
                {
                    if (currentSegmentDistances[i] >= searched)
                    {
                        // Searched point is between (i) and (i-1)
                        break;
                    }
                }
                double a = searched;
                double a0 = currentSegmentDistances[i - 1];
                double a1 = currentSegmentDistances[i];
                double t0 = (double) (i - 1) / N;
                double t1 = (double) i / N;
                double t = t0 + ((a - a0) / (a1 - a0)) * (t1 - t0);
                Point p0 = _nodes[_currentSegment];
                Point p1 = _nodes[_currentSegment + 1];
                Point p2 = _nodes[_currentSegment + 2];
                Point p3 = _nodes[_currentSegment + 3];
                PointF curPos = Spline(p0, p1, p2, p3, t);
                _currentPosition = new Point((int) curPos.X, (int) curPos.Y);
                _currentSpeed = SplineSpeed(p0, p1, p2, p3, t);
                _currentDistanceFromSegmentStart = searched;
                return true;
            }
        }

        private void surfaceControl1_MouseUp(object sender, MouseEventArgs e)
        {
            lock (_mutex)
            {
                Point click = e.Location;
                click.Offset(-50, -50);
                _nodes.Add(click);
                _targetSpeed.Add(DefaultSpeed);
                if (_nodes.Count >= 4)
                {
                    // Calculate path & distances of the segment
                    double[] dists = new double[N];
                    double t = 0;
                    double totalDist = 0;
                    PointF previous = new PointF();
                    Point p0 = _nodes[_nodes.Count - 4];
                    Point p1 = _nodes[_nodes.Count - 3];
                    Point p2 = _nodes[_nodes.Count - 2];
                    Point p3 = _nodes[_nodes.Count - 1];
                    for (int i = 0; i < N; i++)
                    {
                        t += 1d / N;
                        PointF current = Spline(p0, p1, p2, p3, t);
                        _path.Add(new Point((int)current.X, (int)current.Y));
                        if (i == 0)
                        {
                            dists[i] = 0;
                        }
                        else
                        {
                            totalDist += Distance(current, previous);
                            dists[i] = totalDist;
                        }
                        previous = current;
                    }
                    _segmentDistance.Add(dists);
                }
            }
            RefreshValues();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lock (_mutex)
            {
                _currentSegment = 0;
                _currentDistanceFromSegmentStart = 0;
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            lock (_mutex)
            {
                _nodes.Clear();
                _path.Clear();
                _segmentDistance.Clear();
                _targetSpeed.Clear();
                _currentSegment = 0;
                _currentDistanceFromSegmentStart = 0;
            }
        }

        private void surfaceControl1_MouseMove(object sender, MouseEventArgs e)
        {
            _cursor = e.Location;
            _cursor.Offset(-50, -50);
            lock (_mutex)
            {
                _hasHover = false;
                foreach (Point p in _nodes)
                {
                    if (Distance(p, _cursor) <= 5)
                    {
                        _hover = p;
                        _hasHover = true;
                        break;
                    }
                }
            }
        }

        private void surfaceControlSpeedGraph_MouseUp(object sender, MouseEventArgs e)
        {
            lock (_mutex)
            {
                _targetSpeed[_hoverSpeed + 1] = MaxSpeed * (1 - (double) e.Y / (double) _graph.Height);
            }
        }

        private void surfaceControlSpeedGraph_MouseMove(object sender, MouseEventArgs e)
        {
            _speedHover = e.Location;
        }
    }
}
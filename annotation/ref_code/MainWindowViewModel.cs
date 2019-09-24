using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageCrop2
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region [Fields]
        private BitmapSource originalImageSource = null;
        private BitmapSource croppedImageSource1 = null;
        private BitmapSource croppedImageSource2 = null;
        private BitmapSource croppedImageSource3 = null;
        private BitmapSource croppedImageSource4 = null;
        private String approximatedAspectRatio;
        private List<String> images = new List<string>();
        private Visibility image1Visibility = Visibility.Collapsed;
        private Visibility image2Visibility = Visibility.Collapsed;
        private Visibility image3Visibility = Visibility.Collapsed;
        private Visibility image4Visibility = Visibility.Collapsed;
        private Visibility gizmoVisibility = Visibility.Collapsed;
        private Visibility gizmo1Visibility = Visibility.Collapsed;
        private Visibility gizmo2Visibility = Visibility.Collapsed;
        private Visibility gizmo3Visibility = Visibility.Collapsed;
        private Visibility gizmo4Visibility = Visibility.Collapsed;
        private Visibility image1OutOfRangeVisibility = Visibility.Collapsed;
        private Visibility image2OutOfRangeVisibility = Visibility.Collapsed;
        private Visibility image3OutOfRangeVisibility = Visibility.Collapsed;
        private Visibility image4OutOfRangeVisibility = Visibility.Collapsed;
        private int image1Score = -1;
        private int image2Score = -1;
        private int image3Score = -1;
        private int image4Score = -1;
        private int focusedOption = -1;
        #endregion

        #region [Readonly]
        #region [CROP_MAPPING]

        // (Matlab-style, x:vertical, y:horizontal)
        // x1, y1, x2, y2 
        private static readonly int[,] CROP_MAPPING
            = new int[,] { {0, 0, 8, 9},
                           {0, 0, 8, 10},
                           {0, 0, 8, 11},
                           {0, 0, 9, 8},
                           {0, 0, 9, 9},
                           {0, 0, 9, 10},
                           {0, 0, 9, 11},
                           {0, 0, 10, 8},
                           {0, 0, 10, 9},
                           {0, 0, 10, 10},
                           {0, 0, 10, 11},
                           {0, 0, 11, 8},
                           {0, 0, 11, 9},
                           {0, 0, 11, 10},
                           {0, 0, 11, 11},
                           {0, 1, 8, 10},
                           {0, 1, 8, 11},
                           {0, 1, 9, 9},
                           {0, 1, 9, 10},
                           {0, 1, 9, 11},
                           {0, 1, 10, 9},
                           {0, 1, 10, 10},
                           {0, 1, 10, 11},
                           {0, 1, 11, 8},
                           {0, 1, 11, 9},
                           {0, 1, 11, 10},
                           {0, 1, 11, 11},
                           {0, 2, 8, 11},
                           {0, 2, 9, 10},
                           {0, 2, 9, 11},
                           {0, 2, 10, 10},
                           {0, 2, 10, 11},
                           {0, 2, 11, 9},
                           {0, 2, 11, 10},
                           {0, 2, 11, 11},
                           {0, 3, 9, 11},
                           {0, 3, 10, 11},
                           {0, 3, 11, 10},
                           {0, 3, 11, 11},
                           {1, 0, 8, 11},
                           {1, 0, 9, 9},
                           {1, 0, 9, 10},
                           {1, 0, 9, 11},
                           {1, 0, 10, 8},
                           {1, 0, 10, 9},
                           {1, 0, 10, 10},
                           {1, 0, 10, 11},
                           {1, 0, 11, 8},
                           {1, 0, 11, 9},
                           {1, 0, 11, 10},
                           {1, 0, 11, 11},
                           {1, 1, 9, 10},
                           {1, 1, 9, 11},
                           {1, 1, 10, 9},
                           {1, 1, 10, 10},
                           {1, 1, 10, 11},
                           {1, 1, 11, 9},
                           {1, 1, 11, 10},
                           {1, 1, 11, 11},
                           {1, 2, 9, 11},
                           {1, 2, 10, 10},
                           {1, 2, 10, 11},
                           {1, 2, 11, 10},
                           {1, 2, 11, 11},
                           {1, 3, 10, 11},
                           {1, 3, 11, 11},
                           {2, 0, 9, 11},
                           {2, 0, 10, 9},
                           {2, 0, 10, 10},
                           {2, 0, 10, 11},
                           {2, 0, 11, 8},
                           {2, 0, 11, 9},
                           {2, 0, 11, 10},
                           {2, 0, 11, 11},
                           {2, 1, 10, 10},
                           {2, 1, 10, 11},
                           {2, 1, 11, 9},
                           {2, 1, 11, 10},
                           {2, 1, 11, 11},
                           {2, 2, 10, 11},
                           {2, 2, 11, 10},
                           {2, 2, 11, 11},
                           {2, 3, 11, 11},
                           {3, 0, 10, 11},
                           {3, 0, 11, 9},
                           {3, 0, 11, 10},
                           {3, 0, 11, 11},
                           {3, 1, 11, 10},
                           {3, 1, 11, 11},
                           {3, 2, 11, 11}};
        #endregion

        private static readonly int BINS = 12;

        private static readonly float UPPER_RATIO = 2.2f;

        private static readonly float LOWER_RATIO = 0.5f;

        private static readonly float[] RATIOS =  {
            16.0f / 9.0f,
            3.0f / 2.0f,
            4.0f / 3.0f,
            1.0f / 1.0f,
            2.0f/ 3.0f,
            9.0f / 16.0f
        };

        private static readonly String[] RATIO_NAMES = {
            "16:9",
            "3:2",
            "4:3",
            "1:1",
            "2:3",
            "9:16"
        };

        private static readonly String[] SUPPORT_IMAGE_EXTENSION =
            {
            "jpg",
            "jpeg",
            "png",
            "tiff"
        };

        private static readonly String PROGRESS_FILE_NAME =
            "progress.txt";

        private static readonly String IMAGES_ROOT_NAME =
            "images";

        private static readonly String SCORES_ROOT_NAME =
            "scores";

        #endregion

        #region [Private Members]

        private TimeSpan accumulatedTimeSpan = TimeSpan.Zero;

        private DateTime lastCheckPoint = DateTime.MinValue;

        private DispatcherTimer dispatcherTimer = null;

        private int current_image_index = -1;

        private int current_cropping_indice_bias = -1;

        // Matlab order
        private List<int> categories = new List<int>();

        // Matlab order
        private List<int> scores = new List<int>();

        // UI order
        private List<int> sorted_categories = new List<int>();

        // An LUT from Matlab script to UI order
        private List<int> sorted_indices = new List<int>();

        // Matlab order
        private List<float> ratios = new List<float>();

        #endregion

        #region [Properties for Binding]
        public BitmapSource OriginalImageSource
        {
            get
            {
                return originalImageSource;
            }
            set
            {
                if (value != originalImageSource)
                {
                    originalImageSource = value;
                    RaisePropertyChanged("OriginalImageSource");
                }
            }
        }

        public BitmapSource CroppedImageSource1
        {
            get
            {
                return croppedImageSource1;
            }
            set
            {
                if (value != croppedImageSource1)
                {
                    croppedImageSource1 = value;
                    RaisePropertyChanged("CroppedImageSource1");

                    if (value == null)
                    {
                        Image1Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Image1Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public BitmapSource CroppedImageSource2
        {
            get
            {
                return croppedImageSource2;
            }
            set
            {
                if (value != croppedImageSource2)
                {
                    croppedImageSource2 = value;
                    RaisePropertyChanged("CroppedImageSource2");

                    if (value == null)
                    {
                        Image2Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Image2Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public BitmapSource CroppedImageSource3
        {
            get
            {
                return croppedImageSource3;
            }
            set
            {
                if (value != croppedImageSource3)
                {
                    croppedImageSource3 = value;
                    RaisePropertyChanged("CroppedImageSource3");

                    if (value == null)
                    {
                        Image3Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Image3Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public BitmapSource CroppedImageSource4
        {
            get
            {
                return croppedImageSource4;
            }
            set
            {
                if (value != croppedImageSource4)
                {
                    croppedImageSource4 = value;
                    RaisePropertyChanged("CroppedImageSource4");

                    if (value == null)
                    {
                        Image4Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Image4Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public Visibility Image1Visibility
        {
            get
            {
                return image1Visibility;
            }
            set
            {
                if (value != image1Visibility)
                {
                    image1Visibility = value;
                    RaisePropertyChanged("Image1Visibility");
                }
            }
        }

        public Visibility Image2Visibility
        {
            get
            {
                return image2Visibility;
            }
            set
            {
                if (value != image2Visibility)
                {
                    image2Visibility = value;
                    RaisePropertyChanged("Image2Visibility");
                }
            }
        }

        public Visibility Image3Visibility
        {
            get
            {
                return image3Visibility;
            }
            set
            {
                if (value != image3Visibility)
                {
                    image3Visibility = value;
                    RaisePropertyChanged("Image3Visibility");
                }
            }
        }

        public Visibility Image4Visibility
        {
            get
            {
                return image4Visibility;
            }
            set
            {
                if (value != image4Visibility)
                {
                    image4Visibility = value;
                    RaisePropertyChanged("Image4Visibility");
                }
            }
        }

        public Visibility GizmoVisibility
        {
            get
            {
                return gizmoVisibility;
            }
            set
            {
                if (gizmoVisibility != value)
                {
                    gizmoVisibility = value;
                    RaisePropertyChanged("GizmoVisibility");
                }

                Gizmo1Visibility = (Image1Visibility == Visibility.Visible && value == Visibility.Visible) ?
                    Visibility.Visible : Visibility.Collapsed;
                Gizmo2Visibility = (Image2Visibility == Visibility.Visible && value == Visibility.Visible) ?
                    Visibility.Visible : Visibility.Collapsed;
                Gizmo3Visibility = (Image3Visibility == Visibility.Visible && value == Visibility.Visible) ?
                    Visibility.Visible : Visibility.Collapsed;
                Gizmo4Visibility = (Image4Visibility == Visibility.Visible && value == Visibility.Visible) ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility Gizmo1Visibility
        {
            get
            {
                return gizmo1Visibility;
            }
            set
            {
                if (value != gizmo1Visibility)
                {
                    gizmo1Visibility = value;
                    RaisePropertyChanged("Gizmo1Visibility");
                }
            }
        }

        public Visibility Gizmo2Visibility
        {
            get
            {
                return gizmo2Visibility;
            }
            set
            {
                if (value != gizmo2Visibility)
                {
                    gizmo2Visibility = value;
                    RaisePropertyChanged("Gizmo2Visibility");
                }
            }
        }

        public Visibility Gizmo3Visibility
        {
            get
            {
                return gizmo3Visibility;
            }
            set
            {
                if (value != gizmo3Visibility)
                {
                    gizmo3Visibility = value;
                    RaisePropertyChanged("Gizmo3Visibility");
                }
            }
        }

        public Visibility Gizmo4Visibility
        {
            get
            {
                return gizmo4Visibility;
            }
            set
            {
                if (value != gizmo4Visibility)
                {
                    gizmo4Visibility = value;
                    RaisePropertyChanged("Gizmo4Visibility");
                }
            }
        }

        public Visibility Image1OutOfRangeVisibility
        {
            get
            {
                return image1OutOfRangeVisibility;
            }
            set
            {
                if (value != image1OutOfRangeVisibility)
                {
                    image1OutOfRangeVisibility = value;
                    RaisePropertyChanged("Image1OutOfRangeVisibility");
                }
            }
        }

        public Visibility Image2OutOfRangeVisibility
        {
            get
            {
                return image2OutOfRangeVisibility;
            }
            set
            {
                if (value != image2OutOfRangeVisibility)
                {
                    image2OutOfRangeVisibility = value;
                    RaisePropertyChanged("Image2OutOfRangeVisibility");
                }
            }
        }

        public Visibility Image3OutOfRangeVisibility
        {
            get
            {
                return image3OutOfRangeVisibility;
            }
            set
            {
                if (value != image3OutOfRangeVisibility)
                {
                    image3OutOfRangeVisibility = value;
                    RaisePropertyChanged("Image3OutOfRangeVisibility");
                }
            }
        }

        public Visibility Image4OutOfRangeVisibility
        {
            get
            {
                return image4OutOfRangeVisibility;
            }
            set
            {
                if (value != image4OutOfRangeVisibility)
                {
                    image4OutOfRangeVisibility = value;
                    RaisePropertyChanged("Image4OutOfRangeVisibility");
                }
            }
        }

        public int Image1Score
        {
            get
            {
                return image1Score;
            }
            set
            {
                if (image1Score != value)
                {
                    image1Score = value;
                    RaisePropertyChanged("Image1Score");
                }
            }
        }

        public int Image2Score
        {
            get
            {
                return image2Score;
            }
            set
            {
                if (image2Score != value)
                {
                    image2Score = value;
                    RaisePropertyChanged("Image2Score");
                }
            }
        }

        public int Image3Score
        {
            get
            {
                return image3Score;
            }
            set
            {
                if (image3Score != value)
                {
                    image3Score = value;
                    RaisePropertyChanged("Image3Score");
                }
            }
        }

        public int Image4Score
        {
            get
            {
                return image4Score;
            }
            set
            {
                if (image4Score != value)
                {
                    image4Score = value;
                    RaisePropertyChanged("Image4Score");
                }
            }
        }

        public String ApproximatedAspectRatio
        {
            get
            {
                return approximatedAspectRatio;
            }
            set
            {
                if (value != approximatedAspectRatio)
                {
                    approximatedAspectRatio = value;
                    RaisePropertyChanged("ApproximatedAspectRatio");
                }
            }
        }

        public String ImagePath
        {
            get
            {
                if (current_image_index >= 0)
                {
                    return Path.GetFileName(images[current_image_index])
                        + String.Format(" ({0}/{1})", current_image_index + 1, images.Count);
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public String Progress
        {
            get
            {
                if (current_image_index >= 0)
                {
                    return String.Format("{0}/{1}", current_image_index + 1, images.Count);
                }
                else
                {
                    return "-/-";
                }
            }
        }

        public String CurrentSessionElapsedTime
        {
            get
            {
                if (accumulatedTimeSpan.TotalDays >= 1.0)
                {
                    return String.Format("{0} days {1} hours {2} min",
                        accumulatedTimeSpan.Days,
                        accumulatedTimeSpan.Hours,
                        accumulatedTimeSpan.Minutes);
                }
                else if (accumulatedTimeSpan.Hours >= 1.0)
                {
                    return String.Format("{0} hours {1} min",
                        accumulatedTimeSpan.Hours,
                        accumulatedTimeSpan.Minutes);
                }
                else if (accumulatedTimeSpan.Minutes >= 10.0)
                {
                    return accumulatedTimeSpan.Minutes + " min";
                }
                else if (accumulatedTimeSpan.Minutes >= 1.0)
                {
                    return String.Format("{0} min {1} sec",
                        accumulatedTimeSpan.Minutes,
                        accumulatedTimeSpan.Seconds);
                }
                else
                {
                    return accumulatedTimeSpan.Seconds + " sec";
                }
            }
        }

        public String CroppedInfo
        {
            get
            {
                if (null == scores || scores.Count == 0)
                {
                    return "-/-";
                }
                else
                {
                    int invalid_num = scores.Count(x => x == -2);
                    int done_num = scores.Count(x => x >= 0);

                    return String.Format("{0}/{1}", done_num, scores.Count - invalid_num);
                }
            }
        }

        public String ScoreDistribution
        {
            get
            {
                if (scores.Count == 0)
                    return string.Empty;
                else
                {
                    var distribution = computeScoreDistribution();
                    return distribution.Select(x => String.Format("{0}\':{1:p0}", x.Key + 1, x.Value))
                        .Aggregate((x, y) => x + ", " + y);
                }
            }
        }

        public ICommand WndLoaded
        {
            get
            {
                return new RelayCommand(OnWndLoaded);
            }
        }

        public ICommand MouseWheelCmd
        {
            get
            {
                return new RelayCommand<MouseWheelEventArgs>(OnMouseWheel);
            }
        }

        public ICommand ClosingCmd
        {
            get
            {
                return new RelayCommand(OnClosing);
            }
        }

        public ICommand KeyDownCmd
        {
            get
            {
                return new RelayCommand<KeyEventArgs>(OnKeyDown);
            }
        }

        public ICommand Image1ScoreChangedCmd
        {
            get
            {
                return new RelayCommand<SelectionChangedEventArgs>(OnImage1ScoreChanged);
            }
        }

        public ICommand Image2ScoreChangedCmd
        {
            get
            {
                return new RelayCommand<SelectionChangedEventArgs>(OnImage2ScoreChanged);
            }
        }

        public ICommand Image3ScoreChangedCmd
        {
            get
            {
                return new RelayCommand<SelectionChangedEventArgs>(OnImage3ScoreChanged);
            }
        }

        public ICommand Image4ScoreChangedCmd
        {
            get
            {
                return new RelayCommand<SelectionChangedEventArgs>(OnImage4ScoreChanged);
            }
        }

        public ICommand StateChangedCmd
        {
            get
            {
                return new RelayCommand<Window>(OnStateChanged);
            }
        }

        #endregion

        #region [Private Functions]

        private void UpdateApproximatedAspectRatio()
        {
            if (current_cropping_indice_bias == -1)
                ApproximatedAspectRatio = String.Empty;
            else
                ApproximatedAspectRatio = RATIO_NAMES[sorted_categories[current_cropping_indice_bias]];
        }

        private void UpdateCroppedImage()
        {
            if (-1 == current_cropping_indice_bias || null == OriginalImageSource)
                return;

            UpdateApproximatedAspectRatio();

            int start_cropping_index = sorted_indices[current_cropping_indice_bias];
            int category = sorted_categories[current_cropping_indice_bias];

            #region [CroppedImageSource1]

            CroppedImageSource1 = GenerateCroppedImage(start_cropping_index);
            Image1Score = scores[start_cropping_index];
            Gizmo1Visibility = (Image1Visibility == Visibility.Visible && GizmoVisibility == Visibility.Visible) ?
                    Visibility.Visible : Visibility.Collapsed;
            Image1OutOfRangeVisibility = (Image1Score == -2) ? Visibility.Visible : Visibility.Collapsed;

            #endregion

            #region [CroppedImageSource2]
#if false
            if (current_cropping_indice_bias + 1 <= 90)
#else
            if (current_cropping_indice_bias + 1 < 90)
#endif
            {
                if (category != sorted_categories[current_cropping_indice_bias + 1])
                {
                    CroppedImageSource2 = null;
                    CroppedImageSource3 = null;
                    CroppedImageSource4 = null;
                    Gizmo2Visibility = Visibility.Collapsed;
                    Gizmo3Visibility = Visibility.Collapsed;
                    Gizmo4Visibility = Visibility.Collapsed;
                    Image2OutOfRangeVisibility = Visibility.Collapsed;
                    Image3OutOfRangeVisibility = Visibility.Collapsed;
                    Image4OutOfRangeVisibility = Visibility.Collapsed;

                    return;
                }
                else
                {
                    CroppedImageSource2 = GenerateCroppedImage(sorted_indices[current_cropping_indice_bias + 1]);
                    Image2Score = scores[sorted_indices[current_cropping_indice_bias + 1]];
                    Gizmo2Visibility = (Image2Visibility == Visibility.Visible && GizmoVisibility == Visibility.Visible) ?
                        Visibility.Visible : Visibility.Collapsed;
                    Image2OutOfRangeVisibility = (Image2Score == -2) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                CroppedImageSource2 = null;
                CroppedImageSource3 = null;
                CroppedImageSource4 = null;
                Gizmo2Visibility = Visibility.Collapsed;
                Gizmo3Visibility = Visibility.Collapsed;
                Gizmo4Visibility = Visibility.Collapsed;
                Image2OutOfRangeVisibility = Visibility.Collapsed;
                Image3OutOfRangeVisibility = Visibility.Collapsed;
                Image4OutOfRangeVisibility = Visibility.Collapsed;

                return;
            }
            #endregion

            #region [CroppedImageSource3]
#if false
            if (current_cropping_indice_bias + 2 <= 90)
#else
            if (current_cropping_indice_bias + 2 < 90)
#endif
            {
                if (category != sorted_categories[current_cropping_indice_bias + 2])
                {
                    CroppedImageSource3 = null;
                    CroppedImageSource4 = null;
                    Gizmo3Visibility = Visibility.Collapsed;
                    Gizmo4Visibility = Visibility.Collapsed;
                    Image3OutOfRangeVisibility = Visibility.Collapsed;
                    Image4OutOfRangeVisibility = Visibility.Collapsed;
                    return;
                }
                else
                {
                    CroppedImageSource3 = GenerateCroppedImage(sorted_indices[current_cropping_indice_bias + 2]);
                    Image3Score = scores[sorted_indices[current_cropping_indice_bias + 2]];
                    Gizmo3Visibility = (Image3Visibility == Visibility.Visible && GizmoVisibility == Visibility.Visible) ?
                        Visibility.Visible : Visibility.Collapsed;
                    Image3OutOfRangeVisibility = (Image3Score == -2) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                CroppedImageSource3 = null;
                CroppedImageSource4 = null;
                Gizmo3Visibility = Visibility.Collapsed;
                Gizmo4Visibility = Visibility.Collapsed;
                Image3OutOfRangeVisibility = Visibility.Collapsed;
                Image4OutOfRangeVisibility = Visibility.Collapsed;
                return;
            }
            #endregion

            #region [CroppedImageSource4]
#if false
            if (current_cropping_indice_bias + 3 <= 90)
#else
            if (current_cropping_indice_bias + 3 < 90)
#endif
            {
                if (category != sorted_categories[current_cropping_indice_bias + 3])
                {
                    CroppedImageSource4 = null;
                    Gizmo4Visibility = Visibility.Collapsed;
                    Image4OutOfRangeVisibility = Visibility.Collapsed;
                    return;
                }
                else
                {
                    CroppedImageSource4 = GenerateCroppedImage(sorted_indices[current_cropping_indice_bias + 3]);
                    Image4Score = scores[sorted_indices[current_cropping_indice_bias + 3]];
                    Gizmo4Visibility = (Image4Visibility == Visibility.Visible && GizmoVisibility == Visibility.Visible) ?
                        Visibility.Visible : Visibility.Collapsed;
                    Image4OutOfRangeVisibility = (Image4Score == -2) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                CroppedImageSource4 = null;
                Gizmo4Visibility = Visibility.Collapsed;
                Image4OutOfRangeVisibility = Visibility.Collapsed;
                return;
            }
            #endregion


        }

        private BitmapSource GenerateCroppedImage(int idx)
        {
            if (idx == 90)
            {
                return originalImageSource.Clone();
            }
            else if (idx > 90)
            {
                return null;
            }
            else
            {
                float step_y = (float)originalImageSource.PixelWidth / (float)BINS;
                float step_x = (float)originalImageSource.PixelHeight / (float)BINS;

                int x1 = CROP_MAPPING[idx, 0];
                int y1 = CROP_MAPPING[idx, 1];
                int x2 = CROP_MAPPING[idx, 2];
                int y2 = CROP_MAPPING[idx, 3];

                float top = step_x * (0.5f + (float)x1) + 0.5f;
                float left = step_y * (0.5f + (float)y1) + 0.5f;
                float bottom = step_x * (0.5f + (float)x2) + 0.5f;
                float right = step_y * (0.5f + (float)y2) + 0.5f;

                return new CroppedBitmap(
                    originalImageSource,
                    new System.Windows.Int32Rect(
                        (int)left,
                        (int)top,
                        (int)(right - left + 1),
                        (int)(bottom - top + 1)));
            }
        }

        private void ComputeCategories()
        {
            categories.Clear();
            sorted_categories.Clear();
            sorted_indices.Clear();
            ratios.Clear();

            float step_y = (float)originalImageSource.PixelWidth / (float)BINS;
            float step_x = (float)originalImageSource.PixelHeight / (float)BINS;

            var cost = new List<float>(RATIOS);

            float min_cost, width, height, top, left, bottom, right, this_ratio;
            int x1, x2, y1, y2, i;

            for (int idx = 0; idx < 90; ++idx)
            {
                x1 = CROP_MAPPING[idx, 0];
                y1 = CROP_MAPPING[idx, 1];
                x2 = CROP_MAPPING[idx, 2];
                y2 = CROP_MAPPING[idx, 3];

                top = step_x * (0.5f + (float)x1) + 0.5f;
                left = step_y * (0.5f + (float)y1) + 0.5f;
                bottom = step_x * (0.5f + (float)x2) + 0.5f;
                right = step_y * (0.5f + (float)y2) + 0.5f;

                width = right - left + 1.0f;
                height = bottom - top + 1.0f;

                this_ratio = width / height;
                ratios.Add(this_ratio);

                for (i = 0; i < RATIOS.Length; ++i)
                {
                    cost[i] = Math.Abs(this_ratio - RATIOS[i]);
                }

                min_cost = cost.Min();
                categories.Add(cost.IndexOf(min_cost));
            }
#if false
            // compute category of the original image
            this_ratio = (float)originalImageSource.PixelWidth / (float)originalImageSource.PixelHeight;

            for (i = 0; i < cost.Count; ++i)
            {
                cost[i] = Math.Abs(this_ratio - RATIOS[i]);
            }

            min_cost = cost.Min();
            categories.Add(cost.IndexOf(min_cost));
#endif

            var groups = categories
                .Select((x, si) => new KeyValuePair<int, int>(x, si))
                .GroupBy(x => x.Key)
                .OrderBy(x => x.Key);

            foreach (var g in groups)
            {
                var group_cost = g.Select(x => new KeyValuePair<float, int>(ratios[x.Value], x.Value))
                    .OrderByDescending(x => x.Key)
                    .ToList();

                for (int it = 0; it < g.Count(); ++it)
                {
                    sorted_categories.Add(g.Key);
                }
                sorted_indices.AddRange(group_cost.Select(x => x.Value));
            }
        }

        private void ComputeCategories(BitmapSource src, out List<int> out_categories, out List<int> out_sorted_categories, out List<int> out_sorted_indices, out List<float> out_ratios)
        {
            out_categories = new List<int>();
            out_sorted_categories = new List<int>();
            out_sorted_indices = new List<int>();

            List<float> all_ratios = new List<float>();

            float step_y = (float)src.PixelWidth / (float)BINS;
            float step_x = (float)src.PixelHeight / (float)BINS;

            var cost = new List<float>(RATIOS);

            float min_cost, width, height, top, left, bottom, right, this_ratio;
            int x1, x2, y1, y2, i;

            for (int idx = 0; idx < 90; ++idx)
            {
                x1 = CROP_MAPPING[idx, 0];
                y1 = CROP_MAPPING[idx, 1];
                x2 = CROP_MAPPING[idx, 2];
                y2 = CROP_MAPPING[idx, 3];

                top = step_x * (0.5f + (float)x1) + 0.5f;
                left = step_y * (0.5f + (float)y1) + 0.5f;
                bottom = step_x * (0.5f + (float)x2) + 0.5f;
                right = step_y * (0.5f + (float)y2) + 0.5f;

                width = right - left + 1.0f;
                height = bottom - top + 1.0f;

                this_ratio = width / height;
                all_ratios.Add(this_ratio);

                for (i = 0; i < RATIOS.Length; ++i)
                {
                    cost[i] = Math.Abs(this_ratio - RATIOS[i]);
                }

                min_cost = cost.Min();
                out_categories.Add(cost.IndexOf(min_cost));
            }
            out_ratios = all_ratios;
#if false
            // compute category of the original image
            this_ratio = (float)originalImageSource.PixelWidth / (float)originalImageSource.PixelHeight;

            for (i = 0; i < cost.Count; ++i)
            {
                cost[i] = Math.Abs(this_ratio - RATIOS[i]);
            }

            min_cost = cost.Min();
            out_categories.Add(cost.IndexOf(min_cost));
#endif

            var groups = categories
                .Select((x, si) => new KeyValuePair<int, int>(x, si))
                .GroupBy(x => x.Key)
                .OrderBy(x => x.Key);

            foreach (var g in groups)
            {
                var group_cost = g.Select(x => new KeyValuePair<float, int>(ratios[x.Value], x.Value))
                    .OrderByDescending(x => x.Key)
                    .ToList();

                for (int it = 0; it < g.Count(); ++it)
                {
                    out_sorted_categories.Add(g.Key);
                }
                out_sorted_indices.AddRange(group_cost.Select(x => x.Value));
            }
        }

        private void OnDispatcherTimerTick(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            var t = currentTime - lastCheckPoint;
            accumulatedTimeSpan += t;
            lastCheckPoint = currentTime;

            App.Current.Dispatcher.Invoke(() =>
            {
                RaisePropertyChanged("CurrentSessionElapsedTime");
            });
        }

        private void OnWndLoaded()
        {
            LoadImages();
            RestoreProgress();

            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background,
                    OnDispatcherTimerTick, Dispatcher.CurrentDispatcher);

                lastCheckPoint = DateTime.Now;
                dispatcherTimer.Start();
            }
        }

        private void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            else
            {
                if (e.Delta < 0)
                {
                    OnNext();
                    e.Handled = true;
                }
                else if (e.Delta > 0)
                {
                    OnPrevious();
                    e.Handled = true;
                }
            }
        }

        private void OnNext()
        {
            int bias = NextBatchBias();

            if (bias == 0)
            {
                SaveImageScore(images[current_image_index]);
                if (current_image_index + 1 >= images.Count)
                {
                    // We're now at the last image, do nothing.
                }
                else
                {
                    bool completed, need_to_load_new_image;
                    CheckProgress4CurrentImage(out completed, out need_to_load_new_image);

                    if (need_to_load_new_image)
                    {
                        PrepareUI4NewImage(images[++current_image_index]);
                    }
                }
            }
            else
            {
                current_cropping_indice_bias += bias;
                UpdateCroppedImage();
            }

        }

        private void CheckProgress4CurrentImage(out bool completed, out bool need_load_new_image)
        {
            need_load_new_image = true;
            completed = scores.Contains(-1);
            if (completed)
            {
                var result = MessageBox.Show(
                    "Some cropped images are not evaluated yet. Click Yes to evaluate them now. Click no to load a new image.",
                    "Warning",
                    MessageBoxButton.YesNo);

                need_load_new_image = (MessageBoxResult.No == result);

                if (!need_load_new_image)
                {
                    int first_missing_image_crop = -1;
                    for (int i = 0; i < scores.Count; ++i)
                    {
                        if (-1 == scores[sorted_indices[i]])
                        {
                            first_missing_image_crop = i;
                            break;
                        }
                    }

                    var category = sorted_categories[first_missing_image_crop];
                    var first_such_category_idx = sorted_categories.IndexOf(category);

                    current_cropping_indice_bias = (first_missing_image_crop - first_such_category_idx) / 4 * 4 + first_such_category_idx;
                    UpdateCroppedImage();
                }
            }
        }

        private void OnPrevious()
        {
            int bias = PreviousBatchBias();


            if (bias == 0)
            {
                SaveImageScore(images[current_image_index]);
                if (current_image_index == 0)
                {
                    // We're now at the first image, do nothing.
                }
                else
                {
#if false
                    bool completed, need_to_load_new_image;
                    CheckProgress4CurrentImage(out completed, out need_to_load_new_image);

                    if (need_to_load_new_image)
                    {
                        ReversePrepareUI4NewImage(images[--current_image_index]);
                    }
#endif
                    ReversePrepareUI4NewImage(images[--current_image_index]);
                }

            }
            else
            {
                current_cropping_indice_bias -= bias;
                UpdateCroppedImage();
            }

        }

        private int NextBatchBias()
        {
            int count = 0;

            if (CroppedImageSource1 != null)
                ++count;
            if (CroppedImageSource2 != null)
                ++count;
            if (CroppedImageSource3 != null)
                ++count;
            if (CroppedImageSource4 != null)
                ++count;

            if (current_cropping_indice_bias + count >= 90)
                return 0;
            else
                return count;
        }

        private int PreviousBatchBias()
        {
            int count = 0;

            if (current_cropping_indice_bias == 0)
                return count;

            if (sorted_categories[current_cropping_indice_bias]
                == sorted_categories[current_cropping_indice_bias - 1])
            {
                return 4;
            }

            count = (sorted_categories.Take(current_cropping_indice_bias)
                    .Reverse()
                    .TakeWhile(x => x == sorted_categories[current_cropping_indice_bias - 1])
                    .Count()) % 4;

            if (0 == count)
            {
                count = 4;
            }

            return count;
        }

        private void LoadImages()
        {
            images.Clear();
            var di = new DirectoryInfo(IMAGES_ROOT_NAME);


            foreach (var ext in SUPPORT_IMAGE_EXTENSION)
            {
                var files = di.GetFiles("*." + ext);

                images.AddRange(files.Select(x => x.FullName).OrderBy(x => x));
            }
        }

        private void RestoreProgress()
        {
            if (!File.Exists(PROGRESS_FILE_NAME))
            {
                current_image_index = 0;
                PrepareUI4NewImage(images.First());
            }
            else
            {
                int[] segs = null;
                using (var fs = new FileStream(PROGRESS_FILE_NAME, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        var line = sr.ReadLine();
                        segs = line
                            .Split(" ".ToCharArray())
                            .Select(x => int.Parse(x))
                            .ToArray();
                    }
                }

                current_image_index = segs[0];
                PrepareUI4NewImage(images[current_image_index], segs[1]);
            }
        }

        private void PrepareUI4NewImage(String path, int bias = 0)
        {
            OriginalImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
            RaisePropertyChanged("ImagePath");
            RaisePropertyChanged("Progress");

            current_cropping_indice_bias = bias;

            ComputeCategories();

            RestoreImageScore(path);

            for (int i = 0; i < ratios.Count; ++i)
            {
                if (ratios[sorted_indices[i]] < LOWER_RATIO || ratios[sorted_indices[i]] > UPPER_RATIO)
                {
                    scores[sorted_indices[i]] = -2;
                }
            }

            UpdateCroppedImage();
            RaisePropertyChanged("CroppedInfo");
            RaisePropertyChanged("ScoreDistribution");
        }

        private void ReversePrepareUI4NewImage(String path)
        {
            OriginalImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
            RaisePropertyChanged("ImagePath");
            RaisePropertyChanged("Progress");

            ComputeCategories();

            int bias = sorted_categories.Count - sorted_categories.FindIndex(x => x == sorted_categories.Last());
            int val = bias % 4;

            if (val == 0)
            {
                current_cropping_indice_bias = sorted_categories.Count - 4;
            }
            else
            {
                current_cropping_indice_bias = sorted_categories.Count - val;
            }

            RestoreImageScore(path);

            for (int i = 0; i < ratios.Count; ++i)
            {
                if (ratios[sorted_indices[i]] < LOWER_RATIO || ratios[sorted_indices[i]] > UPPER_RATIO)
                {
                    scores[sorted_indices[i]] = -2;
                }
            }

            UpdateCroppedImage();
            RaisePropertyChanged("CroppedInfo");
            RaisePropertyChanged("ScoreDistribution");
        }

        private void RestoreImageScore(String image_path)
        {
            String score_path = Path.Combine(
                    SCORES_ROOT_NAME,
                    Path.GetFileNameWithoutExtension(image_path) + ".txt");

            if (!Directory.Exists(SCORES_ROOT_NAME))
            {
                Directory.CreateDirectory(SCORES_ROOT_NAME);

                scores.Clear();

                for (int i = 0; i < 90; ++i)
                {
                    scores.Add(-1);
                }
            }
            else if (!File.Exists(score_path))
            {
                scores.Clear();

                for (int i = 0; i < 90; ++i)
                {
                    scores.Add(-1);
                }
            }
            else
            {
                using (var fs = new FileStream(score_path, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        var content = sr.ReadLine();
                        scores.Clear();

                        scores.AddRange(
                            content.Split(" ".ToCharArray())
                            .Select(x =>
                            {
                                var val = int.Parse(x);
                                return (val == -1) ? val : (val - 1);
                            }));
                    }
                }
            }
        }

        private void SaveImageScore(String image_path)
        {
            String score_path = Path.Combine(
                    SCORES_ROOT_NAME,
                    Path.GetFileNameWithoutExtension(image_path) + ".txt");

            if (!Directory.Exists(SCORES_ROOT_NAME))
            {
                Directory.CreateDirectory(SCORES_ROOT_NAME);
            }

            using (var fs = new FileStream(score_path, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(
                        scores.Select(x => (x < 0) ? x.ToString() : (x + 1).ToString())
                        .Aggregate((x, y) => x + " " + y));
                }
            }
        }

        private void OnImage1ScoreChanged(SelectionChangedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListBox;

            if (null != lb && scores[sorted_indices[current_cropping_indice_bias]] != -2)
            {
                scores[sorted_indices[current_cropping_indice_bias]] = lb.SelectedIndex;

                RaisePropertyChanged("CroppedInfo");
                RaisePropertyChanged("ScoreDistribution");
            }
        }

        private void OnImage2ScoreChanged(SelectionChangedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListBox;

            if (null != lb && scores[sorted_indices[current_cropping_indice_bias + 1]] != -2)
            {
                scores[sorted_indices[current_cropping_indice_bias + 1]] = lb.SelectedIndex;

                RaisePropertyChanged("CroppedInfo");
                RaisePropertyChanged("ScoreDistribution");
            }
        }

        private void OnImage3ScoreChanged(SelectionChangedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListBox;

            if (null != lb && scores[sorted_indices[current_cropping_indice_bias + 2]] != -2)
            {
                scores[sorted_indices[current_cropping_indice_bias + 2]] = lb.SelectedIndex;

                RaisePropertyChanged("CroppedInfo");
                RaisePropertyChanged("ScoreDistribution");
            }
        }

        private void OnImage4ScoreChanged(SelectionChangedEventArgs e)
        {
            ListBox lb = e.OriginalSource as ListBox;

            if (null != lb && scores[sorted_indices[current_cropping_indice_bias + 3]] != -2)
            {
                scores[sorted_indices[current_cropping_indice_bias + 3]] = lb.SelectedIndex;

                RaisePropertyChanged("CroppedInfo");
                RaisePropertyChanged("ScoreDistribution");
            }
        }

        private void OnClosing()
        {
            SaveImageScore(images[current_image_index]);

            using (var fs = new FileStream(PROGRESS_FILE_NAME, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine("{0} {1}", current_image_index, current_cropping_indice_bias);
                    sw.Flush();

                    sw.Close();
                }

                fs.Close();
            }
        }

        private void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.G:
                    {
                        GizmoVisibility = (GizmoVisibility == Visibility.Visible)
                            ? Visibility.Collapsed : Visibility.Visible;
                    }
                    break;
                case Key.OemQuestion:
                    {
                        CheckMissingOnes();
                    }
                    break;
                case Key.PageDown:
                    {
                        OnNext();
                    }
                    break;
                case Key.PageUp:
                    {
                        OnPrevious();
                    }
                    break;
                default:
                    break;
            }
        }
        
        private void CheckMissingOnes()
        {
            int num_to_do = images.Count;
            int num_done = 0;
            int first_missing_image_index = -1;
            int first_missing_image_crop = -1;
            int image_index = -1;
            List<int> this_categories = null;
            List<int> this_sorted_categories = null;
            List<int> this_sorted_indices = null;
            List<float> this_ratios = null;
            BitmapImage src = null;

            SaveImageScore(images[current_image_index]);

            if (Directory.Exists(SCORES_ROOT_NAME))
            {
                foreach (var image_path in images)
                {
                    ++image_index;
                    String score_path = Path.Combine(
                    SCORES_ROOT_NAME,
                    Path.GetFileNameWithoutExtension(image_path) + ".txt");

                    if (File.Exists(score_path))
                    {
                        ++num_done;

                        if (-1 == first_missing_image_index)
                        {
                            using (var fs = new FileStream(score_path, FileMode.Open, FileAccess.Read))
                            {
                                using (var sr = new StreamReader(fs))
                                {
                                    var content = sr.ReadLine();
                                    var scores = content.Split(" ".ToCharArray())
                                        .Select(x => int.Parse(x)).ToList();

                                    if (scores.Contains(-1))
                                    {
                                        first_missing_image_index = image_index;

                                        src = new BitmapImage(new Uri(image_path, UriKind.Absolute));

                                        ComputeCategories(src, out this_categories, out this_sorted_categories, out this_sorted_indices, out this_ratios);

                                        for (int i = 0; i < scores.Count; ++i)
                                        {
                                            if (-1 == scores[this_sorted_indices[i]])
                                            {
                                                first_missing_image_crop = i;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (first_missing_image_crop == -1 && first_missing_image_index == -1)
            {
                if (num_to_do == num_done)
                {
                    MessageBox.Show(
                        String.Format("All {0} images are marked! Thanks!", num_to_do),
                        "Congrats",
                        MessageBoxButton.OK);
                    return;
                }
                else
                {
                    MessageBox.Show(
                        String.Format("{0} of {1} images are marked! Thanks! Please continue to complete the rest.", num_done, num_to_do),
                        "Congrats",
                        MessageBoxButton.OK);
                    return;
                }
            }
            else
            {
                var result = MessageBox.Show(
                        String.Format("Some cropped images are not evaluated yet! Click yes to jump to the first missing one. Cilck no to stay here.", num_to_do),
                        "Warning",
                        MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    current_image_index = first_missing_image_index;
                    OriginalImageSource = src;
                    RaisePropertyChanged("ImagePath");
                    RaisePropertyChanged("Progress");

                    categories.Clear();
                    categories.AddRange(this_categories);
                    sorted_categories.Clear();
                    sorted_categories.AddRange(this_sorted_categories);
                    sorted_indices.Clear();
                    sorted_indices.AddRange(this_sorted_indices);
                    ratios.Clear();
                    ratios.AddRange(this_ratios);


                    // category of the first missing cropped image
                    var category = sorted_categories[first_missing_image_crop];
                    var first_category_index = sorted_categories.IndexOf(category);

                    int first_such_category_bias = sorted_categories.IndexOf(category);
                    int relative_bias = (first_missing_image_crop - first_such_category_bias) / 4 * 4;

                    current_cropping_indice_bias = first_such_category_bias + relative_bias;

                    RestoreImageScore(images[first_missing_image_index]);

                    for (int i = 0; i < this_ratios.Count; ++i)
                    {
                        if (this_ratios[this_sorted_indices[i]] < LOWER_RATIO || this_ratios[this_sorted_indices[i]] > UPPER_RATIO)
                        {
                            scores[sorted_indices[i]] = -2;
                        }
                    }

                    UpdateCroppedImage();
                }
                else
                {
                    // Do nothing and we stay here.
                }
            }
        }

        private void OnStateChanged(Window wnd)
        {
            switch (wnd.WindowState)
            {
                case WindowState.Maximized:
                case WindowState.Normal:
                    {
                        if (!dispatcherTimer.IsEnabled)
                        {
                            lastCheckPoint = DateTime.Now;
                            dispatcherTimer.IsEnabled = true;
                        }
                    }
                    break;
                case WindowState.Minimized:
                    {
                        dispatcherTimer.IsEnabled = false;
                    }
                    break;
            }
        }

        private Dictionary<int, float> computeScoreDistribution()
        {
            float[] vals = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
            foreach (var s in scores)
            {
                if (s >= 0)
                {
                    vals[s] += 1.0f;
                }
            }

            int i = 0;
            var result = new Dictionary<int, float>();

            if (vals.Any(x => x != 0.0f))
            {
                var sum = vals.Sum();

                for (i = 0; i < vals.Count(); ++i)
                {
                    result.Add(i, vals[i] / sum);
                }
            }
            else
            {
                for (i = 0; i < vals.Count(); ++i)
                {
                    result.Add(i, 0.0f);
                }
            }

            return result;
        }
        
        #endregion
    }
}
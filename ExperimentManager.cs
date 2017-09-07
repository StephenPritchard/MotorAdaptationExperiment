using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;
using System.Text;
using Leap.Unity;
using UnityEngine.SceneManagement;
using UnityEngine.VR;

public class ExperimentManager : MonoBehaviour
{
    // Parameters from File
    private int _numberOfPracticeTrials;
    private int _numberOfBaselineTrials;
    private int _numberOfAdaptationTrials;
    private int _numberOfWashoutTrials;
    private string _block1LabelText;
    private string _block2LabelText;
    private string _block3LabelText;
    private int _practiceFeedback;
    private int _baselineFeedback;
    private int _adaptationFeedback;
    private int _washoutFeedback;
    private int _delayBeforeStartingTrial;
    private int _timeoutMilliseconds;
    private float _finger2WallDistanceForTouchRegistration;
    private float _distanceOfEyesToWall;
    public int PermittedHorizontalDeviation;
    public int PermittedVerticalDeviation;
    private float _handOffsetInCm;
    private float _cursorDisplayDuration;
    private int _horizontalLaptopDisplayFeedback;
    private int _verticalLaptopDisplayFeedback;
    private int _displayPointCloud;
    private int _restartFlag;
    private float _pointSize;
    private int _centroidType;
    private Color _baselineUpColor;
    private Color _baselineCentreColor;
    private Color _baselineDownColor;
    private Color _adaptationUpColor;
    private Color _adaptationCentreColor;
    private Color _adaptationDownColor;
    private Color _washoutUpColor;
    private Color _washoutCentreColor;
    private Color _washoutDownColor;
    private string _studentSaveLocation;
    private string _masterSaveLocation;

    private GameObject _lmHeadMountedRig;
    private GameObject _leapHandController;
    private CycleHandPairs _cycleHandPairs;
    private Vector3 _touchPosition;
    private bool _buttonOneDown;
    private bool _calibrationPerformed;
    private bool _experimentCommenced;
    private bool _withinBlock;
    private bool _wallTouched;
    private bool _timeout;
    private int _studentDirNumber = 1;
    private int _masterDirSuffix = 1;
    private GameObject _titleScreen;
    private GameObject _instructionsScreen;
    private GameObject _countDownScreen;
    private GameObject _pointCloudScreen;
    private GameObject _finishScreen;
    private GameObject _laptopUi;
    private GameObject _crossHair;
    private GameObject _discTarget;
    private GameObject _readyTextObject;
    private GameObject[] _rightHandObjects;
    private Text _reachHorizontalLocationText;
    private Text _reachVerticalLocationText;
    private Text _countDownIntroText;
    private Text _countDownNumberText;
    private Text _instructText;
    private Text _titleScreenTitleText;

    private Vector3 _baselineUpCentroid = new Vector3(0,0,0);
    private Vector3 _adaptationUpCentroid = new Vector3(0,0,0);
    private Vector3 _washoutUpCentroid = new Vector3(0,0,0);
    private Vector3 _baselineCentreCentroid = new Vector3(0, 0, 0);
    private Vector3 _adaptationCentreCentroid = new Vector3(0, 0, 0);
    private Vector3 _washoutCentreCentroid = new Vector3(0, 0, 0);
    private Vector3 _baselineDownCentroid = new Vector3(0, 0, 0);
    private Vector3 _adaptationDownCentroid = new Vector3(0, 0, 0);
    private Vector3 _washoutDownCentroid = new Vector3(0, 0, 0);
    private int _baselineUpTrials;
    private int _baselineCentreTrials;
    private int _baselineDownTrials;
    private int _adaptationUpTrials;
    private int _adaptationCentreTrials;
    private int _adaptationDownTrials;
    private int _washoutUpTrials;
    private int _washoutCentreTrials;
    private int _washoutDownTrials;


    private DirectoryInfo _mainStudentDirectory;
    private DirectoryInfo _mainMasterDirectory;
    private DirectoryInfo _currentStudentSubDirectory;
    private DirectoryInfo _currentMasterSubDirectory;
    private readonly FileInfo _fileParameters = new FileInfo("parameters.txt");
    private readonly FileInfo _fileInstructions = new FileInfo("instructions.txt");
    private Stopwatch _sw;
    private System.Random _random;
    
    public int CountDownDuration;
    public GameObject TouchingHandPart;
    public GameObject[] RightHandObjects;
    public GameObject[] TargetPositions;
    public GameObject Cursor;
    public GameObject PointPrefab;
    public GameObject CentroidPrefab;

    [HideInInspector]
    public int HandCondition { get; private set; }


    private void Awake ()
    {
        _random = new System.Random();
        _sw = new Stopwatch();
        

        LoadParameters();

        _mainStudentDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + _studentSaveLocation);
        _mainMasterDirectory = new DirectoryInfo(_masterSaveLocation);
        _lmHeadMountedRig = GameObject.Find("LMHeadMountedRig");
        _leapHandController = GameObject.Find("LeapHandController");
        _cycleHandPairs = _leapHandController.GetComponent<CycleHandPairs>();
        _cycleHandPairs.CurrentGroup = HandCondition;
        
        TouchingHandPart.GetComponent<ProximityDetector>().OnDistance = _finger2WallDistanceForTouchRegistration / 100;
        TouchingHandPart.GetComponent<ProximityDetector>().OffDistance = (_finger2WallDistanceForTouchRegistration+0.5f) / 100;
        _titleScreen = GameObject.Find("TitleScreen");
        _titleScreenTitleText = GameObject.Find("TitleScreenTitleText").GetComponent<Text>();
        _titleScreen.SetActive(true);
        _instructionsScreen = GameObject.Find("InstructionsScreen"); 
        _instructText = GameObject.Find("InstructText").GetComponent<Text>();
        LoadInstructions();
        _instructionsScreen.SetActive(false);
        _laptopUi = GameObject.Find("LaptopUICanvas");
        _reachHorizontalLocationText = GameObject.Find("ReachHorizontalPositionReport").GetComponent<Text>();
        _reachVerticalLocationText = GameObject.Find("ReachVerticalPositionReport").GetComponent<Text>();
        _laptopUi.SetActive(false);
        _countDownIntroText = GameObject.Find("CountDownIntroText").GetComponent<Text>();
        _countDownNumberText = GameObject.Find("CountDownNumberText").GetComponent<Text>();
        _countDownScreen = GameObject.Find("CountDownScreen");
        _countDownScreen.SetActive(false);
        _readyTextObject = GameObject.Find("ReadyText");
        _readyTextObject.SetActive(false);
        _crossHair = GameObject.Find("Crosshair");
        _crossHair.SetActive(false);
        _discTarget = GameObject.Find("DiscTarget");
        _discTarget.SetActive(false);
        _rightHandObjects = GameObject.FindGameObjectsWithTag("RightHand");
        foreach (var hand in _rightHandObjects)
        {
            hand.GetComponent<OffsetRightHand>().HandOffsetInCm = _handOffsetInCm;
        }

        _finishScreen = GameObject.Find("FinishScreen");
        _finishScreen.SetActive(false);
        _pointCloudScreen = GameObject.Find("PointCloudScreen");
        GameObject.Find("BaselineUpLegend").GetComponent<RawImage>().color = _baselineUpColor;
        GameObject.Find("BaselineCentreLegend").GetComponent<RawImage>().color = _baselineCentreColor;
        GameObject.Find("BaselineDownLegend").GetComponent<RawImage>().color = _baselineDownColor;
        GameObject.Find("AdaptationUpLegend").GetComponent<RawImage>().color = _adaptationUpColor;
        GameObject.Find("AdaptationCentreLegend").GetComponent<RawImage>().color = _adaptationCentreColor;
        GameObject.Find("AdaptationDownLegend").GetComponent<RawImage>().color = _adaptationDownColor;
        GameObject.Find("WashoutUpLegend").GetComponent<RawImage>().color = _washoutUpColor;
        GameObject.Find("WashoutCentreLegend").GetComponent<RawImage>().color = _washoutCentreColor;
        GameObject.Find("WashoutDownLegend").GetComponent<RawImage>().color = _washoutDownColor;
        GameObject.Find("BaselineCentroidLegend").GetComponent<RawImage>().color = _baselineUpColor;
        GameObject.Find("AdaptationCentroidLegend").GetComponent<RawImage>().color = _adaptationUpColor;
        GameObject.Find("WashoutCentroidLegend").GetComponent<RawImage>().color = _washoutUpColor;
        GameObject.Find("Block1LegendText").GetComponent<Text>().text = _block1LabelText;
        GameObject.Find("Block2LegendText").GetComponent<Text>().text = _block2LabelText;
        GameObject.Find("Block3LegendText").GetComponent<Text>().text = _block3LabelText;

        _pointCloudScreen.SetActive(false);
    }


    // Update is called once per frame
    private void Update ()
    {
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Button.One))
        {
            _buttonOneDown = true;
            if (_experimentCommenced) return;
            if (!_calibrationPerformed)
            {
                _titleScreenTitleText.text = "CALIBRATE VIEW FIRST!";
                _titleScreenTitleText.color = Color.white;
                return;
            }
            _experimentCommenced = true;
            StartCoroutine(RunExperiment());
        }
        else
        {
            _buttonOneDown = false;
        }

        if (OVRInput.Get(OVRInput.Button.DpadLeft) && !_experimentCommenced)
        {
            _lmHeadMountedRig.transform.position = new Vector3(0f, 1.7f, -_distanceOfEyesToWall);
            InputTracking.Recenter();
            _calibrationPerformed = true;
        }
    }


    public void Touched()
    {
        if (_wallTouched) return;
        _wallTouched = true;
        _touchPosition = TouchingHandPart.transform.position;
    }


    private void LoadInstructions()
    {
        StreamReader streamR = null;
        string line;
        var textBuilder = new StringBuilder();

        try
        {
            streamR = _fileInstructions.OpenText();
        }
        catch
        {
            // TODO: error dialog popup
            Environment.Exit(0);
        }

        do
        {
            line = streamR.ReadLine();
            if (line == null)
                continue;

            textBuilder.AppendLine(line);
        } while (line != null);
        _instructText.text = textBuilder.ToString();
    }


    private void LoadParameters()
    {
        StreamReader streamR = null;
        string line;

        try
        {
            streamR = _fileParameters.OpenText();
        }
        catch
        {
            // TODO: error dialog popup
            Environment.Exit(0);
        }

        do
        {
            line = streamR.ReadLine();
            if (line == null)
                continue;

            var splitline = line.Split(new char[] { ' ' });

            switch (splitline[0])
            {
                case "NumberOfPracticeTrials:":
                    _numberOfPracticeTrials = int.Parse(splitline[1]);
                    break;

                case "NumberOfBaselineTrials:":
                    _numberOfBaselineTrials = int.Parse(splitline[1]);
                    break;

                case "NumberOfAdaptationTrials:":
                    _numberOfAdaptationTrials = int.Parse(splitline[1]);
                    break;

                case "NumberOfWashoutTrials:":
                    _numberOfWashoutTrials = int.Parse(splitline[1]);
                    break;

                case "Block1Label:":
                    _block1LabelText = splitline[1];
                    break;

                case "Block2Label:":
                    _block2LabelText = splitline[1];
                    break;

                case "Block3Label:":
                    _block3LabelText = splitline[1];
                    break;

                case "PracticeFeedback:":
                    _practiceFeedback = int.Parse(splitline[1]);
                    break;

                case "BaselineFeedback:":
                    _baselineFeedback = int.Parse(splitline[1]);
                    break;

                case "AdaptationFeedback:":
                    _adaptationFeedback = int.Parse(splitline[1]);
                    break;

                case "WashoutFeedback:":
                    _washoutFeedback = int.Parse(splitline[1]);
                    break;

                case "HandCondition:":
                    HandCondition = int.Parse(splitline[1]);
                    break;

                case "DelayBeforeStartingTrial:":
                    _delayBeforeStartingTrial = int.Parse(splitline[1]);
                    break;

                case "Timeout:":
                    _timeoutMilliseconds = int.Parse(splitline[1]);
                    break;

                case "Finger2WallDistanceForTouchRegistration:":
                    _finger2WallDistanceForTouchRegistration = float.Parse(splitline[1]);
                    break;

                case "DistanceOfEyesToWall:":
                    _distanceOfEyesToWall = float.Parse(splitline[1]) / 100f;
                    break;

                case "PermittedHorizontalDeviation:":
                    PermittedHorizontalDeviation = int.Parse(splitline[1]);
                    break;

                case "PermittedVerticalDeviation:":
                    PermittedVerticalDeviation = int.Parse(splitline[1]);
                    break;

                case "HandOffsetInCm:":
                    _handOffsetInCm = float.Parse(splitline[1]);
                    break;

                case "DisplayCursorDuration:":
                    _cursorDisplayDuration = float.Parse(splitline[1]);
                    break;

                case "HorizontalLaptopDisplayFeedback:":
                    _horizontalLaptopDisplayFeedback = int.Parse(splitline[1]);
                    break;

                case "VerticalLaptopDisplayFeedback:":
                    _verticalLaptopDisplayFeedback = int.Parse(splitline[1]);
                    break;

                case "DisplayPointCloud:":
                    _displayPointCloud = int.Parse(splitline[1]);
                    break;

                case "PointSize:":
                    _pointSize = float.Parse(splitline[1]);
                    break;

                case "CentroidType:":
                    _centroidType = int.Parse(splitline[1]);
                    break;

                case "BaselineUpColor:":
                    _baselineUpColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "BaselineCentreColor:":
                    _baselineCentreColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "BaselineDownColor:":
                    _baselineDownColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "AdaptationUpColor:":
                    _adaptationUpColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "AdaptationCentreColor:":
                    _adaptationCentreColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "AdaptationDownColor:":
                    _adaptationDownColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "WashoutUpColor:":
                    _washoutUpColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "WashoutCentreColor:":
                    _washoutCentreColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "WashoutDownColor:":
                    _washoutDownColor = new Color(float.Parse(splitline[1]), float.Parse(splitline[2]), float.Parse(splitline[3]));
                    break;

                case "StudentSaveLocation:":
                    _studentSaveLocation = splitline[1];
                    break;

                case "MasterSaveLocation:":
                    _masterSaveLocation = splitline[1];
                    break;

                default:
                    break;
            }

        } while (line != null);

        streamR.Close();
    }


    private IEnumerator RunExperiment()
    {
        _buttonOneDown = false;
        // Create new subdirectory for this experiment run.
        while (Directory.Exists(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "\\", _studentSaveLocation, "\\Experiment", _studentDirNumber.ToString())))
        {
            _studentDirNumber++;
        }
        _currentStudentSubDirectory = _mainStudentDirectory.CreateSubdirectory(string.Concat("Experiment", _studentDirNumber.ToString()));

        if (Directory.Exists(string.Concat(_masterSaveLocation, "\\Experiment", _studentDirNumber.ToString())))
        {
            while (Directory.Exists(string.Concat(_masterSaveLocation, "\\Experiment", _studentDirNumber.ToString(), "_", _masterDirSuffix.ToString())))
            {
                _masterDirSuffix++;
            }
            _currentMasterSubDirectory = _mainMasterDirectory.CreateSubdirectory(string.Concat("Experiment", _studentDirNumber.ToString(), "_", _masterDirSuffix.ToString()));
        }
        else
        {
            _currentMasterSubDirectory = _mainMasterDirectory.CreateSubdirectory(string.Concat("Experiment", _studentDirNumber.ToString()));
        }
        

        // Hide title screen and display instructions screen.
        _titleScreen.SetActive(false);
        _instructionsScreen.SetActive(true);

        // Prevent advance from instruction screen until button pressed and
        // at least 1 second has passed since the instruction screen was first displayed.
        yield return new WaitForSeconds(1);
        while (!_buttonOneDown)
        {
            yield return null;
        }
        
        _readyTextObject.GetComponent<Text>().text =
            "Hold down center button with\nright index finger to begin\nthe practice trials";
        _instructionsScreen.SetActive(false);
        _readyTextObject.SetActive(true);
        yield return new WaitForSeconds(1);

        // Wait on ready screen until button one is held down.
        while (!_buttonOneDown)
        {
            yield return null;
        }
        yield return StartCoroutine("RunCountDown", "Starting practice trials in...");
        yield return StartCoroutine(RunPracticeTrials());

        _readyTextObject.GetComponent<Text>().text =
            "Hold down center button with\nright index finger to begin\nthe first experiment block";
        _readyTextObject.SetActive(true);
        yield return new WaitForSeconds(1);

        // Wait on ready screen until button one is held down.
        while (!_buttonOneDown)
        {
            yield return null;
        }
        yield return StartCoroutine("RunCountDown", "Starting 1st block in...");
        yield return StartCoroutine(RunFirstExperimentBlock());

        _readyTextObject.GetComponent<Text>().text =
            "Hold down center button with\nright index finger to begin\nthe second experiment block";
        _readyTextObject.SetActive(true);
        yield return new WaitForSeconds(1);

        // Wait on ready screen until button one is held down.
        while (!_buttonOneDown)
        {
            yield return null;
        }
        yield return StartCoroutine("RunCountDown", "Starting 2nd block in...");
        yield return StartCoroutine(RunSecondExperimentBlock());

        _readyTextObject.GetComponent<Text>().text =
            "Hold down center button with\nright index finger to begin\nthe final experiment block";
        _readyTextObject.SetActive(true);
        yield return new WaitForSeconds(1);

        // Wait on ready screen until button one is pressed.
        while (!_buttonOneDown)
        {
            yield return null;
        }
        yield return StartCoroutine("RunCountDown", "Starting 3rd block in...");
        yield return StartCoroutine(RunThirdExperimentBlock());

        yield return DisplayFinishScreen();
        _finishScreen.SetActive(false);

        if (_displayPointCloud == 1)
        {
            yield return DisplayPointCloud();
        }
        else
        {
            _restartFlag = 1;
        }

        if (_restartFlag == 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Application.Quit();
        }
    }


    private IEnumerator RunCountDown(string introText)
    {
        _readyTextObject.SetActive(false);
        _countDownNumberText.text = CountDownDuration.ToString();
        _countDownIntroText.text = introText;
        _countDownScreen.SetActive(true);
        for (var i = CountDownDuration; i > 0; i--)
        {
            _countDownNumberText.GetComponent<Text>().text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        _countDownScreen.SetActive(false);
    }


    private IEnumerator RunPracticeTrials()
    {
        _withinBlock = true;
        var fileStudentExperimentData = new FileInfo(Path.Combine(_currentStudentSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var studentStreamW = new StreamWriter(fileStudentExperimentData.FullName, true);
        var fileMasterExperimentData = new FileInfo(Path.Combine(_currentMasterSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var masterStreamW = new StreamWriter(fileMasterExperimentData.FullName, true);

        studentStreamW.WriteLine();
        studentStreamW.WriteLine("PRACTICE, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");
        masterStreamW.WriteLine();
        masterStreamW.WriteLine("PRACTICE, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");

        for (var i = 0; i < _numberOfPracticeTrials; i++)
        {
            yield return DoSingleTrial(studentStreamW, masterStreamW, "Practice" + i, 0);
        }
        studentStreamW.Close();
        masterStreamW.Close();
        _withinBlock = false;
    }


    private IEnumerator RunFirstExperimentBlock()
    {
        _withinBlock = true;
        var fileStudentExperimentData = new FileInfo(Path.Combine(_currentStudentSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var studentStreamW = new StreamWriter(fileStudentExperimentData.FullName, true);
        var fileMasterExperimentData = new FileInfo(Path.Combine(_currentMasterSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var masterStreamW = new StreamWriter(fileMasterExperimentData.FullName, true);

        studentStreamW.WriteLine();
        studentStreamW.WriteLine("FIRST BLOCK, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");
        masterStreamW.WriteLine();
        masterStreamW.WriteLine("FIRST BLOCK, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");


        for (var i = 0; i < _numberOfBaselineTrials; i++)
        {
            yield return DoSingleTrial(studentStreamW, masterStreamW, _block1LabelText + i, 1);
        }
        studentStreamW.Close();
        masterStreamW.Close();
        _withinBlock = false;
    }


    private IEnumerator RunSecondExperimentBlock()
    {
        _withinBlock = true;
        if ((_adaptationFeedback != 2) && (_adaptationFeedback != 3))
        {
            foreach (var hand in RightHandObjects)
            {
                hand.GetComponent<OffsetRightHand>().HandOffsetFlag = true;
            }
        }
        var fileStudentExperimentData = new FileInfo(Path.Combine(_currentStudentSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var studentStreamW = new StreamWriter(fileStudentExperimentData.FullName, true);
        var fileMasterExperimentData = new FileInfo(Path.Combine(_currentMasterSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var masterStreamW = new StreamWriter(fileMasterExperimentData.FullName, true);

        studentStreamW.WriteLine();
        studentStreamW.WriteLine("SECOND BLOCK, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");
        masterStreamW.WriteLine();
        masterStreamW.WriteLine("SECOND BLOCK, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");


        for (var i = 0; i < _numberOfAdaptationTrials; i++)
        {
            yield return DoSingleTrial(studentStreamW, masterStreamW, _block2LabelText + i, 2);
        }
        studentStreamW.Close();
        masterStreamW.Close();
        _withinBlock = false;
    }


    private IEnumerator RunThirdExperimentBlock()
    {
        _withinBlock = true;
        
        foreach (var hand in RightHandObjects)
        {
            hand.GetComponent<OffsetRightHand>().HandOffsetFlag = false;
        }

        var fileStudentExperimentData = new FileInfo(Path.Combine(_currentStudentSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var studentStreamW = new StreamWriter(fileStudentExperimentData.FullName, true);
        var fileMasterExperimentData = new FileInfo(Path.Combine(_currentMasterSubDirectory.FullName, "experimentdata" + _studentDirNumber + ".csv"));
        var masterStreamW = new StreamWriter(fileMasterExperimentData.FullName, true);

        studentStreamW.WriteLine();
        studentStreamW.WriteLine("THIRD BLOCK, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");
        masterStreamW.WriteLine();
        masterStreamW.WriteLine("THIRD BLOCK, TARGETPOS, REACH X-POS, REACH Y-POS, Time-to-touch(ms)");


        for (var i = 0; i < _numberOfWashoutTrials; i++)
        {
            yield return DoSingleTrial(studentStreamW, masterStreamW, _block3LabelText + i, 3);
        }
        studentStreamW.Close();
        masterStreamW.Close();
        _withinBlock = false;
    }

    private IEnumerator DoSingleTrial(StreamWriter studentStreamW, StreamWriter masterStreamW, string trialLabel, int block)
    {
        int feedbackConditionForTrial;
        switch (block)
        {
            case 0:
                feedbackConditionForTrial = _practiceFeedback;
                break;
            case 1:
                feedbackConditionForTrial = _baselineFeedback;
                break;
            case 2:
                feedbackConditionForTrial = _adaptationFeedback;
                break;
            case 3:
                feedbackConditionForTrial = _washoutFeedback;
                break;
            // Default continuous view
            default:
                feedbackConditionForTrial = 1;
                break;
        }

        switch (feedbackConditionForTrial)
        {
            case 1:
                _cycleHandPairs.CurrentGroup = HandCondition;
                break;
            case 2:
                _cycleHandPairs.DisableAllGroups();
                break;
            case 3:
                _cycleHandPairs.DisableAllGroups();
                break;
            default:
                _cycleHandPairs.CurrentGroup = HandCondition;
                break;
        }

        _crossHair.SetActive(true);
        yield return new WaitForSeconds(1);
        while (!_buttonOneDown)
        {
            yield return null;
        }
        yield return new WaitForSeconds(_delayBeforeStartingTrial/1000f);
        _crossHair.SetActive(false);
        var index = _random.Next(TargetPositions.Length);
        _discTarget.transform.position = TargetPositions[index].transform.position;
        _discTarget.SetActive(true);
        _wallTouched = false;
        _sw.Start();

        while (!_wallTouched)
        {
            if (_sw.ElapsedMilliseconds > _timeoutMilliseconds)
            {
                _timeout = true;
                break;
            }
            yield return null;
        }
        
        _sw.Stop();
        if (!_timeout)
        {
            Color currentPointColor;
            string discTargetPosition;
            if ((_discTarget.transform.position.y - _crossHair.transform.position.y) > 0.05f)
            {
                discTargetPosition = "Top";
                switch (block)
                {
                    case 1:
                        currentPointColor = _baselineUpColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _baselineUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _baselineUpCentroid += _touchPosition;
                                _baselineUpTrials++;
                                break;
                            case 3:
                                _baselineUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;

                    case 2:
                        currentPointColor = _adaptationUpColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _adaptationUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _adaptationUpCentroid += _touchPosition;
                                _adaptationUpTrials++;
                                break;
                            case 3:
                                _adaptationUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;

                    case 3:
                        currentPointColor = _washoutUpColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _washoutUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _washoutUpCentroid += _touchPosition;
                                _washoutUpTrials++;
                                break;
                            case 3:
                                _washoutUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;
                    default:
                        currentPointColor = Color.clear;
                        break;
                }
            }
            else if ((_discTarget.transform.position.y - _crossHair.transform.position.y) < -0.05f)
            {
                discTargetPosition = "Bottom";
                switch (block)
                {
                    case 1:
                        currentPointColor = _baselineDownColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _baselineUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _baselineDownCentroid += _touchPosition;
                                _baselineDownTrials++;
                                break;
                            case 3:
                                _baselineUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;

                    case 2:
                        currentPointColor = _adaptationDownColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _adaptationUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _adaptationDownCentroid += _touchPosition;
                                _adaptationDownTrials++;
                                break;
                            case 3:
                                _adaptationUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;

                    case 3:
                        currentPointColor = _washoutDownColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _washoutUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _washoutDownCentroid += _touchPosition;
                                _washoutDownTrials++;
                                break;
                            case 3:
                                _washoutUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;
                    default:
                        currentPointColor = Color.clear;
                        break;
                }
            }
            else
            {
                discTargetPosition = "Centre";
                switch (block)
                {
                    case 1:
                        currentPointColor = _baselineCentreColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _baselineUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _baselineCentreCentroid += _touchPosition;
                                _baselineCentreTrials++;
                                break;
                            case 3:
                                _baselineUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;

                    case 2:
                        currentPointColor = _adaptationCentreColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _adaptationUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _adaptationCentreCentroid += _touchPosition;
                                _adaptationCentreTrials++;
                                break;
                            case 3:
                                _adaptationUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;

                    case 3:
                        currentPointColor = _washoutCentreColor;
                        switch (_centroidType)
                        {
                            case 1:
                                _washoutUpCentroid += _touchPosition;
                                break;
                            case 2:
                                _washoutCentreCentroid += _touchPosition;
                                _washoutCentreTrials++;
                                break;
                            case 3:
                                _washoutUpCentroid += new Vector3(_touchPosition.x, 0f, _touchPosition.z);
                                break;
                        }
                        break;
                    default:
                        currentPointColor = Color.clear;
                        break;
                }
            }

            var currentPointInstantiated = Instantiate(PointPrefab, _touchPosition, Quaternion.identity);
            currentPointInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
            currentPointInstantiated.transform.parent = _pointCloudScreen.transform;
            currentPointInstantiated.GetComponent<RawImage>().color = currentPointColor;


            studentStreamW.WriteLine("{0}, {1}, {2:F1}, {3:F1}, {4}", trialLabel,
                discTargetPosition,
                _touchPosition.x * 100,
                (_touchPosition.y - _crossHair.transform.position.y) * 100,
                _sw.ElapsedMilliseconds);

            masterStreamW.WriteLine("{0}, {1}, {2:F1}, {3:F1}, {4}", trialLabel,
                discTargetPosition,
                _touchPosition.x * 100,
                (_touchPosition.y - _crossHair.transform.position.y) * 100,
                _sw.ElapsedMilliseconds);

            if (_horizontalLaptopDisplayFeedback == 1)
            {
                _reachHorizontalLocationText.text = string.Concat("Reach x position: ",
                    (_touchPosition.x * 100).ToString("F1"), "cm");
            }
            else
            {
                _reachHorizontalLocationText.text = "";
            }
            if (_verticalLaptopDisplayFeedback == 1)
            {
                _reachVerticalLocationText.text = string.Concat("Reach y position: ",
                    ((_touchPosition.y - _crossHair.transform.position.y) * 100).ToString("F1"), "cm");
            }
            else
            {
                _reachVerticalLocationText.text = "";
            }

            if (feedbackConditionForTrial == 2)
            {
                yield return DisplayCursor(block);
            }
        }
        else
        {
            studentStreamW.WriteLine("{0}: reaching timeout", trialLabel);
            masterStreamW.WriteLine("{0}: reaching timeout", trialLabel);
            _reachHorizontalLocationText.text = "timeout";
            _reachVerticalLocationText.text = "";
        }
        _sw.Reset();
        
        _laptopUi.SetActive(true);
        _timeout = false;
        _discTarget.SetActive(false);
    }


    private IEnumerator DisplayFinishScreen()
    {
        _discTarget.SetActive(false);
        _finishScreen.SetActive(true);
        while (!_buttonOneDown)
        {
            yield return null;
        }
    }
    private IEnumerator DisplayPointCloud()
    {
        if (_centroidType == 1)
        {
            _baselineUpCentroid = _baselineUpCentroid / _numberOfBaselineTrials;
            _adaptationUpCentroid = _adaptationUpCentroid / _numberOfAdaptationTrials;
            _washoutUpCentroid = _washoutUpCentroid / _numberOfWashoutTrials;

            var currentCentroidInstantiated = Instantiate(CentroidPrefab, _baselineUpCentroid, Quaternion.identity);
            //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
            currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
            currentCentroidInstantiated.GetComponent<RawImage>().color = _baselineUpColor;

            currentCentroidInstantiated = Instantiate(CentroidPrefab, _adaptationUpCentroid, Quaternion.identity);
            //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
            currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
            currentCentroidInstantiated.GetComponent<RawImage>().color = _adaptationUpColor;

            currentCentroidInstantiated = Instantiate(CentroidPrefab, _washoutUpCentroid, Quaternion.identity);
            //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
            currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
            currentCentroidInstantiated.GetComponent<RawImage>().color = _washoutUpColor;
        }
        else if (_centroidType == 2)
        {
            _baselineUpCentroid = _baselineUpCentroid / _baselineUpTrials;
            _baselineCentreCentroid = _baselineCentreCentroid / _baselineCentreTrials;
            _baselineDownCentroid = _baselineDownCentroid / _baselineDownTrials;
            _adaptationUpCentroid = _adaptationUpCentroid / _adaptationUpTrials;
            _adaptationCentreCentroid = _adaptationCentreCentroid / _adaptationCentreTrials;
            _adaptationDownCentroid = _adaptationDownCentroid / _adaptationDownTrials;
            _washoutUpCentroid = _washoutUpCentroid / _washoutUpTrials;
            _washoutCentreCentroid = _washoutCentreCentroid / _washoutCentreTrials;
            _washoutDownCentroid = _washoutDownCentroid / _washoutDownTrials;

            if (_baselineUpTrials != 0)
            {
                var currentCentroidInstantiated = Instantiate(CentroidPrefab, _baselineUpCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _baselineUpColor;
            }
            if (_baselineCentreTrials != 0)
            {
                var currentCentroidInstantiated = Instantiate(CentroidPrefab, _baselineCentreCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _baselineCentreColor;
            }
            if (_baselineDownTrials != 0)
            {
                var currentCentroidInstantiated =
                    Instantiate(CentroidPrefab, _baselineDownCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _baselineDownColor;
            }
            if (_adaptationUpTrials != 0)
            {
                var currentCentroidInstantiated =
                    Instantiate(CentroidPrefab, _adaptationUpCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _adaptationUpColor;
            }
            if (_adaptationCentreTrials != 0)
            {
                var currentCentroidInstantiated =
                    Instantiate(CentroidPrefab, _adaptationCentreCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _adaptationCentreColor;
            }
            if (_adaptationDownTrials != 0)
            {
                var currentCentroidInstantiated =
                    Instantiate(CentroidPrefab, _adaptationDownCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _adaptationDownColor;
            }
            if (_washoutUpTrials != 0)
            {
                var currentCentroidInstantiated = Instantiate(CentroidPrefab, _washoutUpCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _washoutUpColor;
            }
            if (_washoutCentreTrials != 0)
            {
                var currentCentroidInstantiated =
                    Instantiate(CentroidPrefab, _washoutCentreCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _washoutCentreColor;
            }
            if (_washoutDownTrials != 0)
            {
                var currentCentroidInstantiated =
                    Instantiate(CentroidPrefab, _washoutDownCentroid, Quaternion.identity);
                //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
                currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
                currentCentroidInstantiated.GetComponent<RawImage>().color = _washoutDownColor;
            }
        }
        else if (_centroidType == 3)
        {
            _baselineUpCentroid = new Vector3(_baselineUpCentroid.x/_numberOfBaselineTrials, 1.85f, -0.005f);
            _adaptationUpCentroid = new Vector3(_adaptationUpCentroid.x/_numberOfAdaptationTrials, 1.85f, -0.005f);
            _washoutUpCentroid = new Vector3(_washoutUpCentroid.x/_numberOfWashoutTrials, 1.85f, -0.005f);

            var currentCentroidInstantiated = Instantiate(CentroidPrefab, _baselineUpCentroid, Quaternion.identity);
            //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
            currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
            currentCentroidInstantiated.GetComponent<RawImage>().color = _baselineUpColor;

            currentCentroidInstantiated = Instantiate(CentroidPrefab, _adaptationUpCentroid, Quaternion.identity);
            //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
            currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
            currentCentroidInstantiated.GetComponent<RawImage>().color = _adaptationUpColor;

            currentCentroidInstantiated = Instantiate(CentroidPrefab, _washoutUpCentroid, Quaternion.identity);
            //currentCentroidInstantiated.transform.localScale = new Vector3(_pointSize, _pointSize, _pointSize);
            currentCentroidInstantiated.transform.parent = _pointCloudScreen.transform;
            currentCentroidInstantiated.GetComponent<RawImage>().color = _washoutUpColor;
        }

        _pointCloudScreen.SetActive(true);
        if (_centroidType == 0)
        {
            GameObject.Find("CentroidLegend").SetActive(false);
        }
        yield return new WaitForSeconds(3);
        while (!_buttonOneDown)
        {
            yield return null;
        }
        _restartFlag = 1;
    }

    private IEnumerator DisplayCursor(int block)
    {
        var cursorPosition = _touchPosition;
        if (block == 2)
        {
            cursorPosition += new Vector3(_handOffsetInCm / 100f, 0f, 0f);
        }
        var cursor = Instantiate(Cursor, cursorPosition, Quaternion.identity);
        yield return new WaitForSeconds(_cursorDisplayDuration);
        Destroy(cursor);
    }

    public void ChangeLightToRed()
    {
        if (!_withinBlock) return;
        GameObject.Find("MainLight").GetComponent<Light>().color = new Color(0.6f, 0.3f, 0.3f);
    }

    public void ChangeLightToWhite()
    {
        GameObject.Find("MainLight").GetComponent<Light>().color = Color.white;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DigHi2
{
    class DigHi2
    {
        static void Main(string[] args)
        {
            DigHi2 prog = new DigHi2();
            //relative path to folder containing "experiment-i" "experiment-ii" subfolders 
            prog.ReadInput("..\\..\\..\\..\\");


        }

        Experiment _experiment1 = new Experiment();
        Experiment _experiment2 = new Experiment();

        void ReadInput(string dirPath)
        {
            _experiment1._experimentIndex = 1;
            _experiment2._experimentIndex = 2;

            Console.WriteLine("reading input...");
            
            //_experiment1.ReadInput(dirPath + "experiment-i");
            _experiment2.ReadInput(dirPath + "experiment-ii");

            Console.WriteLine("reading input finished.");
        }
    }

    //represents one experiment
    class Experiment
    {
        List<Subject> _subjects = new List<Subject>();
        public int _experimentIndex;

        public void ReadInput(string dirPath)
        {
            string[] subjectDirs = Directory.GetDirectories(dirPath, "S*", SearchOption.TopDirectoryOnly);
            foreach(string subdir in subjectDirs)
            {
                int index = int.Parse(subdir.Remove(0, subdir.LastIndexOf("S") + 1));

                //dirty way to prevent OutOfBoundsException
                while (_subjects.Count < index)
                    _subjects.Add(new Subject());

                _subjects[index - 1] = new Subject(subdir, index -1, _experimentIndex);
            }
        }
    }

    //represents one experiment subject
    class Subject
    {
        List<DataSet> _dataSets = new List<DataSet>();
        SubjectProperties _properties;
        bool _isEmpty = false;

        public Subject()
        {
            _isEmpty = true;
        }
        
        public Subject(string dirPath, int subIndex, int expIndex)
        {
            _properties = new SubjectProperties(expIndex, subIndex);

            string[] dataSetFiles;
            if (expIndex == 1)
                dataSetFiles = Directory.GetFiles(dirPath, "*", SearchOption.TopDirectoryOnly);
            else
                dataSetFiles = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);


            foreach (string file in dataSetFiles)
            {
                int index = CalculateIndex(file, expIndex);

                //dirty way to prevent OutOfBoundsException again
                while (_dataSets.Count < index)
                    _dataSets.Add(new DataSet());

                _dataSets[index-1]  = new DataSet(file, expIndex, index-1);
            }
        }

        private int CalculateIndex(string file, int expIndex)
        {
            int index = 0;

            //experiment 1: just take the number from the string
            if(expIndex == 1)
            {
                string indexStr = file.Remove(0, file.LastIndexOf("\\") + 1);
                indexStr = indexStr.Remove(indexStr.LastIndexOf("."));
                index = int.Parse(indexStr);
            }
            //experiment 2: calculate offset from mmat type and letter
            else
            {
                string indexStr = file.Remove(0, file.LastIndexOf("\\") + 1);
                indexStr = Regex.Match(indexStr, @"\d+").Value;
                indexStr.Trim();
                index = int.Parse(indexStr);

                //range 1-10
                if (file.Contains("B"))
                {
                    
                }
                //range 11-13
                else if (file.Contains("C"))
                {
                    index += 10;
                }
                //range 14-16
                else if (file.Contains("D"))
                {
                    index += 13;
                }
                //range 17-22
                else if (file.Contains("E"))
                {
                    index += 16;
                }
                //range 23-29
                else if (file.Contains("F"))
                {
                    index += 22;
                }

                //increase for airmat
                if(file.Contains("Air_Mat"))
                {
                    index += 29;
                }
            }

            return index;
        }
    }

    public enum Posture { Supine, Left, Right, LeftFetus, RightFetus };

    //represents properties of a subject
    class SubjectProperties
    {
        public int _age;
        public int _height;
        public int _weight;

        private static int[] ex1Age = { 19, 23, 23, 24, 24, 26, 27, 27, 30, 30, 30, 33, 34 };
        private static int[] ex1Height = { 175, 183, 183, 177, 172, 169, 179, 186, 174, 174, 176, 170, 174 };
        private static int[] ex1Weight= { 87, 85, 100, 70, 66, 83, 96, 63, 74, 79, 91, 78, 74 };

        private static int[] ex2Age = { 19, 23, 23, 24, 27, 27, 30, 30};
        private static int[] ex2Height = { 175, 183, 183, 172, 179, 186, 174, 174};
        private static int[] ex2Weight = { 87, 85, 100, 66, 96, 63, 74, 79};

        public SubjectProperties(int expIndex, int subIndex)
        {
            //experiment 1
            if(expIndex == 1)
            {
                _age = ex1Age[subIndex];
                _height = ex1Height[subIndex];
                _weight = ex1Weight[subIndex];
            }
            //experiment 2
            else
            {
                _age = ex2Age[subIndex];
                _height = ex2Height[subIndex];
                _weight = ex2Weight[subIndex];
            }
        }
    }

    //represents one data set (one file of messurements)
    class DataSet
    {
        DataSetProperties _properties;
        
        List<int[]> _data = new List<int[]>();
        int _frameLength;
        bool _isEmpty = false;

        private void SetFrameLength(int expIndex)
        {
            if (expIndex == 1)
                _frameLength = 2048;
            else
                _frameLength = 1728;
        }

        public DataSet()
        {
            _isEmpty = true;
        }

        public DataSet(string file, int expIndex, int dataIndex)
        {
            SetFrameLength(expIndex);

            string[] lines = File.ReadAllLines(file);
            if(expIndex == 1)
            {
                _properties = new DataSetProperties(dataIndex);
            }
            //in experimant 2 only one frame per file and use other props constructor
            else
            {
                _properties = new DataSetProperties(file);
                
                while(lines.Length > 1)
                {
                    string last = " " + lines[lines.Length - 1];
                    lines[0] += last;
                    Array.Resize(ref lines, lines.Length - 1);
                }
            }

            foreach(string line in lines)
            {
                int[] frame = new int[_frameLength];
                for (int i=0; i<_frameLength; ++i)
                {
                    string numString = Regex.Match(line, @"\d+").Value;
                    if(numString.Length < 1)
                    {
                        frame[i] = 0;
                    }
                    else
                    {
                        line.Remove(0, numString.Length);
                        frame[i] = int.Parse(numString.Trim());
                    }
                }

                _data.Add(frame);
            }
        }
    }

    class DataSetProperties
    {
        public Posture _posture;
        public int _bedInclination;
        public int _bodyRoll;

        private static Posture[] _ex1posture = { Posture.Supine, Posture.Right, Posture.Left, Posture.Right, Posture.Right, Posture.Left, Posture.Left,
            Posture.Supine, Posture.Supine, Posture.Supine, Posture.Supine, Posture.Supine, Posture.RightFetus, Posture.LeftFetus, Posture.Supine,
            Posture.Supine, Posture.Supine};
        private static int[] _ex1Inc = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30, 45, 60 };
        private static int[] _ex1Roll = { 0, 0, 0, 30, 60, 30, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        //for exp 1
        public DataSetProperties(int index)
        {
            _posture = _ex1posture[index];
            _bedInclination = _ex1Inc[index];
            _bodyRoll = _ex1Roll[index];
        }

        //for exp 2
        public DataSetProperties(string file)
        {
            if(file.Contains("B"))
            {
                _posture = Posture.Supine;
                _bedInclination = 0;
                _bodyRoll = 0;
            }
            else if(file.Contains("C"))
            {
                _posture = Posture.Right;
                _bedInclination = 0;
                _bodyRoll = 0;
            }
            else if(file.Contains("D"))
            {
                _posture = Posture.Left;
                _bedInclination = 0;
                _bodyRoll = 0;
            }
            else if(file.Contains("E"))
            {
                if (file.Contains("E1") || file.Contains("E2") || file.Contains("E5"))
                    _posture = Posture.Right;
                else
                    _posture = Posture.Left;

                if (file.Contains("E2") || file.Contains("E4"))
                    _bodyRoll = 40;
                else
                    _bodyRoll = 20;

                _bedInclination = 0;
            }
            else if(file.Contains("F"))
            {
                string indexStr = file.Remove(0, file.LastIndexOf("\\") + 1);
                indexStr = Regex.Match(indexStr, @"\d+").Value;
                indexStr.Trim();
                int index = int.Parse(indexStr);
                _bedInclination = (index - 1) * 10;

                _posture = Posture.Supine;
                _bodyRoll = 0;
            }
        }
    }
}

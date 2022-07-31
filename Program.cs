using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

using LSL;

namespace PainLabDeviceLSLCompatialeLayer
{
    [Serializable]
    class LSLDataFrame
    {
        public double[] CZ;
        public double[] Ch2;
        public double[] Ch3;
        public double[] Ch4;
        public double[] Ch5;
        public double[] Ch6;
        public double[] Ch7;
        public double[] Ch8;

        public LSLDataFrame(float[][] sample)
        {
            CZ = new double[sample.Length];
            Ch2 = new double[sample.Length];
            Ch3 = new double[sample.Length];
            Ch4 = new double[sample.Length];
            Ch5 = new double[sample.Length];
            Ch6 = new double[sample.Length];
            Ch7 = new double[sample.Length];
            Ch8 = new double[sample.Length];
            for (int i = 0; i < sample.Length; i++)
            {
                CZ[i] = (double)sample[i][0];
                Ch2[i] = (double)sample[i][1];
                Ch3[i] = (double)sample[i][2];
                Ch4[i] = (double)sample[i][3];
                Ch5[i] = (double)sample[i][4];
                Ch6[i] = (double)sample[i][5];
                Ch7[i] = (double)sample[i][6];
                Ch8[i] = (double)sample[i][7];
            }
        }
    }
    class PainlabLSLCompatiblilityProtocol : PainlabProtocol
    {
        static string descriptorPath = "Resources/device-descriptor.json";

        protected override void RegisterWithDescriptor()
        {
            string descriptorString = File.ReadAllText(descriptorPath);
            SendString(descriptorString);

            return;
        }
        public byte[] PrepareDataFrameBytes(float[][] sample)
        {
            LSLDataFrame dataFrame = new LSLDataFrame(sample);
            byte[] byteData = StringToBytes(JsonConvert.SerializeObject(dataFrame, Formatting.None));

            return byteData;
        }
    }
    class Program
    {
        static string networkConfigPath = "Resources/network-config.json";
        static int subFramePerFrame = 20;
        static void Main(string[] args)
        {
            PainlabLSLCompatiblilityProtocol protocol = new PainlabLSLCompatiblilityProtocol();
            string networkJsonString = File.ReadAllText(networkConfigPath);
            NetworkConfig netConf = JsonConvert.DeserializeObject<NetworkConfig>(networkJsonString);

            protocol.Init(netConf);

            // wait until an EEG stream shows up
            StreamInfo[] results = LSL.LSL.resolve_stream("type", "EEG");

            // open an inlet and print some interesting info about the stream (meta-data, etc.)
            StreamInlet inlet = new StreamInlet(results[0]);
            results.DisposeArray();
            //System.Console.Write(inlet.info().as_xml());

            float[][] sample = new float[subFramePerFrame][];
            for (int i = 0; i < subFramePerFrame; i++)
            {
                sample[i] = new float[8];
            }
            int subframe_counter = 0;
            // read samples
            while (true)
            {
                inlet.pull_sample(sample[subframe_counter]);
                subframe_counter++;
                if (subframe_counter >= subFramePerFrame)
                {
                    protocol.UpdateFrameData(protocol.PrepareDataFrameBytes(sample));
                    subframe_counter = 0;
                }
                
            }
        }
    }
}

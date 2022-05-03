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
        public double[] EEG;

        public LSLDataFrame(float[] sample)
        {
            EEG = new double[sample.Length];
            for (int i = 0; i < sample.Length; i++)
            {
                EEG[i] = (double)sample[i];
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
        public byte[] PrepareDataFrameBytes(float[] sample)
        {
            LSLDataFrame dataFrame = new LSLDataFrame(sample);
            byte[] byteData = StringToBytes(JsonConvert.SerializeObject(dataFrame, Formatting.None));

            return byteData;
        }
    }
    class Program
    {
        static string networkConfigPath = "Resources/network-config.json";
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
            System.Console.Write(inlet.info().as_xml());

            // read samples
            while (true)
            {
                float[] sample = new float[8];
                inlet.pull_sample(sample);
                protocol.UpdateFrameData(protocol.PrepareDataFrameBytes(sample));
            }
        }
    }
}

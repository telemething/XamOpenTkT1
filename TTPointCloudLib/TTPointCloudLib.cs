using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

    namespace RosSharp.RosBridgeClient.Messages.Standard
    {
        public class Time : Message
        {
            [JsonIgnore]
            public const string RosMessageName = "std_msgs/Time";
            public uint secs;
            public uint nsecs;
            public Time()
            {
                secs = 0;
                nsecs = 0;
            }
        }
    }
    namespace RosSharp.RosBridgeClient.Messages.Standard
    {
        public class Header : Message
        {
            [JsonIgnore]
            public const string RosMessageName = "std_msgs/Header";
            public uint seq;
            public Time stamp;
            public string frame_id;
            public Header()
            {
                seq = 0;
                stamp = new Time();
                frame_id = "";
            }
        }
    }
    namespace RosSharp.RosBridgeClient
    {
        public abstract class Message
        {
        }
    }
    namespace RosSharp.RosBridgeClient.Messages.Sensor
    {
        public class PointField : Message
        {
            [JsonIgnore]
            public const string RosMessageName = "sensor_msgs/PointField";
            public const byte INT8 = 1;
            public const byte UINT8 = 2;
            public const byte INT16 = 3;
            public const byte UINT16 = 4;
            public const byte INT32 = 5;
            public const byte UINT32 = 6;
            public const byte FLOAT32 = 7;
            public const byte FLOAT64 = 8;
            public byte datatype;
            public string name;
            public uint offset;
            public uint count;
            public PointField()
            {
                datatype = 0;
                name = "";
                offset = 0;
                count = 0;
            }
        }
    }

    namespace RosSharp.RosBridgeClient.Messages.Sensor
    {
        public class PointCloud2 : Message
        {
            [JsonIgnore]
            public const string RosMessageName = "sensor_msgs/PointCloud2";
            public Standard.Header header;
            public uint height;
            public uint width;
            public PointField[] fields;
            public bool is_bigendian;
            public uint point_step;
            public uint row_step;

            public byte[] data;
            public bool is_dense;
            public PointCloud2()
            {
                header = new Standard.Header();
                height = 0;
                width = 0;
                fields = new PointField[0];
                is_bigendian = false;
                point_step = 0;
                row_step = 0;
                is_dense = false;
                data = new byte[0];
            }
        }
    }

namespace TTPointCloudLib
{
    public class TTPointCloud
    {
        //*********************************************************************
        ///
        /// <summary>
        /// Read a Pointcloud2 file
        /// </summary>
        /// <param name="filePath"></param>
        ///
        //*********************************************************************

        public static RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2
            ReadPointCloudFromStream(Stream stream)
        {
            var pc = new RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2();

            using (var reader = new System.IO.BinaryReader(stream))
            {
                var header = reader.ReadString();
                pc = JsonConvert.DeserializeObject<RosSharp.
                    RosBridgeClient.Messages.Sensor.PointCloud2>(header);
                pc.data = reader.ReadBytes((int)pc.row_step);
            }

            return pc;
        }
        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        public static RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 
            ReadPointCloudFile(Assembly assembly, string fileName)
        {
            var pc = new RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2();
            string filePath = string.Empty;

            foreach (var res in assembly.GetManifestResourceNames())
            {
                if (res.Contains(fileName))
                    filePath = res;
                //System.Diagnostics.Debug.WriteLine("found resource: " + res);
            }

            if (filePath.Length < 1)
            {
                throw new FileNotFoundException($"resource '{fileName}' could not be located");
            }

            var stream = assembly.GetManifestResourceStream(filePath);

            if (filePath.Length < 1)
            {
                throw new FileNotFoundException($"resource '{fileName}' was located but could not be loaded");
            }

            return ReadPointCloudFromStream(stream);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Save a Pointcloud2 file
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="filePath"></param>
        ///
        //*********************************************************************

        public static void SavePointCloudFile(
            RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pc, string filePath)
        {
            var data = pc.data;
            pc.data = null;
            var header = JsonConvert.SerializeObject(pc);

            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(
                System.IO.File.Open(filePath, System.IO.FileMode.Create)))
            {
                writer.Write(header);
                writer.Write(data, 0, (int)pc.row_step);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Read a Pointcloud2 file
        /// </summary>
        /// <param name="filePath"></param>
        ///
        //*********************************************************************

        public static RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 
            ReadPointCloudFile(string filePath)
        {
            var pc = new RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2();

            if (System.IO.File.Exists(filePath))
            {
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(
                    System.IO.File.Open(filePath, System.IO.FileMode.Open)))
                {
                    var header = reader.ReadString();
                    pc = JsonConvert.DeserializeObject
                        <RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2>(header);
                    pc.data = reader.ReadBytes((int)pc.row_step);
                }
            }

            return pc;
        }

    }
}

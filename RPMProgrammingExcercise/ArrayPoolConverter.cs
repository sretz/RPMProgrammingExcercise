using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RPMProgrammingExcercise
{
	internal static class ArrayPoolConverter
	{
		#region Methods
		public static void ConfigBuffers(XmlNode config, out int buffersLength, out Converter<int, byte[]> buffersRent, out Action<byte[], bool> buffersReturn)
		{
			ArrayPool<byte> buffers;
			if (config == null)
			{
				buffersLength = 1024 * 1024;
				buffers = ArrayPool<byte>.Shared;
			}
			else
			{
				XmlNode node;

				//Length
				node = config.SelectSingleNode("Length");
				if (node == null || int.TryParse(node.InnerText, out buffersLength) ||
					buffersLength <= 0)
				{
					buffersLength = 1024 * 1024;
				}

				//Bucket
				int bucket;
				node = config.SelectSingleNode("Bucket");
				if (node == null || int.TryParse(node.InnerText, out bucket) ||
					bucket <= 0)
				{
					bucket = 50;
				}

				if (buffersLength <= 1024 * 1024 && bucket <= 50)
				{
					buffers = ArrayPool<byte>.Shared;
				}
				else
				{
					buffers = ArrayPool<byte>.Create(buffersLength, bucket);
				}
			}

			buffersRent = buffers.Rent;
			buffersReturn = buffers.Return;
		}
		#endregion //Methods
	}
}
